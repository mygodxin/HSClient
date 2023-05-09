using System;

namespace HS
{
    /// <summary>
    /// ʱ����ع�����
    /// </summary>
    public class DateUtil
    {
        /// <summary>
        /// ��ȡ��ǰʱ���(ms)
        /// </summary>
        public static long Now
        {
            get
            {
                return (long)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
            }
        }
        /**��ʽ������ xx��xxʱxx��xx�� */
        public static string FormatDate(double time)
        {
            var str = "";
            if (time <= 0) { }
            else if (time > 0 && time <= 60)
            {
                str = Math.Floor(time / 60) + '��' + str;
            }
            else if (time > 60 && time <= 60 * 60)
            {
                str = Math.Floor(time / (60 * 60)) + '��' + str;
            }
            else if (time > 60 * 60 && time <= 60 * 60 * 24)
            {
                str = Math.Floor(time / (60 * 60 * 24)) + 'ʱ' + str;
            }
            else if (time > 60 * 60 * 24)
            {
                str = Math.Floor(time / (60 * 60 * 24)) + '��' + str;
            }
            return str;
        }
        /// <summary>
        /// ��ת��Ϊ00:00:00��ʽ�ĸ�ʽ�ַ���
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        public static string FormatHMS(int duration)
        {
            return new TimeSpan(0,0,duration).ToString(@"hh\:mm\:ss");
        }
    }
}
