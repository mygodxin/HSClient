using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace KcpTransport
{
    public static class TimeExtension
    {
        public static TimeSpan GetElapsedTime(long startTimestamp)
        {
            long elapsed = Stopwatch.GetTimestamp() - startTimestamp;
            return TimeSpan.FromTicks(elapsed * TimeSpan.TicksPerSecond / Stopwatch.Frequency);
        }

        public static TimeSpan GetElapsedTime(long startTimestamp, long endTimestamp)
        {
            long elapsed = endTimestamp - startTimestamp;
            return TimeSpan.FromTicks(elapsed * TimeSpan.TicksPerSecond / Stopwatch.Frequency);
        }
    }

    internal static class SynCookie
    {
        public static (uint Cookie, long Timestamp) Generate(ReadOnlySpan<byte> hashKey, IPEndPoint remoteAddress)
        {
            var timestamp = Stopwatch.GetTimestamp();
            var hash = GenerateCore(hashKey, remoteAddress, timestamp);

            return (hash, timestamp);
        }

        public static bool Validate(ReadOnlySpan<byte> hashKey, TimeSpan timeout, uint cookie, IPEndPoint remoteAddress, long timestamp)
        {
            var cookie2 = GenerateCore(hashKey, remoteAddress, timestamp);
            if (cookie != cookie2)
            {
                return false;
            }

            var elapsed = TimeExtension.GetElapsedTime(timestamp);
            if (elapsed < timeout)
            {
                return true;
            }

            return false;
        }

        static uint GenerateCore(ReadOnlySpan<byte> hashKey, IPEndPoint remoteAddress, long timestamp)
        {
            // 1. 准备缓冲区（IPv4: 16+8=24字节，IPv6: 16+2+8=26字节）
            byte[] buffer = new byte[26]; // 按最大需求分配

            // 2. 写入IP地址和端口
            if (remoteAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                // IPv4地址 (4字节)
                remoteAddress.Address.GetAddressBytes().CopyTo(buffer, 0);
                // 端口 (2字节，大端序)
                buffer[4] = (byte)(remoteAddress.Port >> 8);
                buffer[5] = (byte)remoteAddress.Port;
                // 时间戳位置从16开始
                Buffer.BlockCopy(BitConverter.GetBytes(timestamp), 0, buffer, 16, 8);
            }
            else
            {
                // IPv6地址 (16字节)
                remoteAddress.Address.GetAddressBytes().CopyTo(buffer, 0);
                // 端口 (2字节，大端序)
                buffer[16] = (byte)(remoteAddress.Port >> 8);
                buffer[17] = (byte)remoteAddress.Port;
                // 时间戳位置从18开始
                Buffer.BlockCopy(BitConverter.GetBytes(timestamp), 0, buffer, 18, 8);
            }

            // 3. 计算HMAC-SHA256
            using (var hmac = new HMACSHA256(hashKey.ToArray()))
            {
                byte[] hash = hmac.ComputeHash(buffer);
                return BitConverter.ToUInt32(hash, 0); // 取前4字节
            }
        }
    }
}