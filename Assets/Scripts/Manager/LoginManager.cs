using KHCore.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using HS;
using DuiChongServerCommon.ClientProtocol;

public class ClientServerInfo : GameServerInfo
{
    public int UserId;
    public ClientServerInfo(GameServerInfo info) : base()
    {
        this.GateIp = info.GateIp;
        this.ConnectCode = info.ConnectCode;
        this.OpenDate = info.OpenDate;
        this.ServerID = info.ServerID;
        this.ServerName = info.ServerName;
        this.ServerState = info.ServerState;
        this.UserInfo = info.UserInfo;
    }
}
/// <summary>
/// ��¼����
/// </summary>
public class LoginManager
{
    private LoginAccount _loginAccount;
    public LoginAccount loginAccount
    {
        get
        {
            return _loginAccount;
        }
    }
    private List<GameServerInfo> _serverList;
    private ClientServerInfo _curServerInfo;
    private static LoginManager _inst = null;

    public static LoginManager Inst
    {
        get
        {
            if (_inst == null)
                _inst = new LoginManager();
            return _inst;
        }
    }

    public async void Login(LoginAccount data, bool switchServer = false)
    {
        this._loginAccount = data;
        if (this._serverList == null)
        {
            //У��sdk��¼�˺�
            var loginResult = await this.LoginAccount(data);
            if (loginResult.returnCode == ReturnCode.Scuess)
            {
                var login = loginResult.bufferReader.ReadSerializable<AccountLoginResponse>();
                //�����
                if (login.HsitoryLoginCount == 1)
                {
                    //�������
                }

                this._serverList = login.GameServers;
            }
            else
            {
                //return "��ȡ�������б�ʧ�ܣ�";
            }
        }
        var info = await this.GetLoginServer(switchServer);
        if (info != null)
        {
            this._curServerInfo = info;
            ConnectToGate(info.GateIp);
        }
    }
    //��ȡ��¼������
    private async Task<ClientServerInfo> GetLoginServer(bool switchServer)
    {
        ClientServerInfo info = null;
        if (switchServer)
        {
            //�ȴ��û�UIѡ�������
            info = await Task.Run(() =>
            {
                ClientServerInfo server = null;
                Action<ClientServerInfo> action = (ClientServerInfo s) =>
                {
                    server = s;
                };
                //UIManager.inst.ShowWindow<ServerWin>(action);
                return server;
            });
        }
        else
        {
            ClientServerInfo lastServer = LocalStorage.Read<ClientServerInfo>(StorageDef.LastServer);
            if (lastServer != null)
            {
                for (int i = 0; i < this._serverList.Count; i++)
                {
                    var cur = this._serverList[i];
                    if (cur.ServerID == lastServer.ServerID)
                    {
                        if (cur.ServerState != ServerStatus.�ر� && cur.ServerState != ServerStatus.�ܾ�����)
                            info = new ClientServerInfo(cur);
                        break;
                    }
                }
            }
            if (info == null)
            {
                this._serverList.Sort((a, b) =>
                {
                    return DateTime.Compare(b.OpenDate, a.OpenDate);
                });
                for (int i = 0; i < this._serverList.Count; i++)
                {
                    var s = this._serverList[i];
                    if (s.ServerState != ServerStatus.�ر� && s.ServerState != ServerStatus.�ܾ�����)
                    {
                        info = new ClientServerInfo(s);
                        break;
                    }
                }
            }
        }
        if (info != null)
            LocalStorage.Save(StorageDef.LastServer, info);
        return info;
    }
    private async Task<ResponseData> LoginAccount(LoginAccount data)
    {
        var req = new RequestData(RequestCode.LoginAccount, data);
        var buffer = EncryptTool.EncryptBinary(req.Buffer, ConfigManager.Inst.encryptKeys);
        var writer = new BufferWriter(buffer.Length + 1);
        writer.Write((byte)RequestCode.LoginAccount);
        writer.Joint(buffer);
        var str = Convert.ToBase64String(writer.GetBuffer);
        var result = await HttpRequest.Inst.Post(ConfigManager.Inst.connectURL, str);
        if (result == null)
        {
            var buffer1 = new BufferWriter(128);
            buffer1.Write("������ά���У����Ժ����ԣ�");
            var reader = new BufferReader(buffer1.GetBuffer);
            return new ResponseData(ReturnCode.Filed, reader, req.code);
        }
        else
        {
            var reader = new BufferReader(result);
            reader.ReadByte();
            var requestCode = (RequestCode)reader.ReadByte();
            var returnCode = (ReturnCode)reader.ReadByte();
            return new ResponseData(returnCode, reader, requestCode);
        }
    }
    //��������
    private void ConnectToGate(string ip)
    {
        WSClient.inst.Connect(ip);
    }

    public void GateHandHake()
    {
        var req = new RequestData(RequestCode.GateHandShake);
        WSClient.inst.SendPackage(req);
    }
    public void OnGateHandShake(ResponseData responseData)
    {
        if (responseData.returnCode == ReturnCode.Scuess)
        {
            var hand = responseData.bufferReader.ReadSerializable<GateHandShake>();
            RequestData.sign = new Dictionary<RequestCode, int>();
            RequestData.keys = hand.EncryptKeys.ToArray();
            foreach (var item in hand.RequestEncrytSign)
            {
                RequestData.sign.Add((RequestCode)item.Key, item.Value);
            }
            this.LoginGame(this._curServerInfo.ConnectCode, this._curServerInfo.UserId);
        }
    }
    public void LoginGame(string connectCode, int userID)
    {
        var req = new RequestData(RequestCode.LoginGame);
        req.BufferWriter.Write(connectCode);
        req.BufferWriter.Write(0);
        req.BufferWriter.Write("test");
        req.BufferWriter.Write("test");
        WSClient.inst.SendPackage(req);
    }
    public void OnLoginGame(ResponseData responseData)
    {
        if (responseData.returnCode == ReturnCode.Scuess)
        {
            var buffer = new BufferReader(responseData.bufferReader.UnReadSpan.ToArray());
            buffer.Buffer.DeCompress();
            var user = buffer.ReadSerializable<User>();
            Debug.Log(user);
        }
        else
        {
            Debug.Log(responseData.bufferReader.ReadString());
        }
    }
}
