using System;
using System.ComponentModel;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine.Networking;

public class HttpRequest
{

    private static HttpRequest inst = null;
    public static HttpRequest Inst
    {
        get
        {
            if (inst == null)
                inst = new HttpRequest();
            return inst;
        }
    }

    public void Request()
    {
        UnityWebRequest wr = new UnityWebRequest(); // ��ȫΪ��
        UnityWebRequest wr2 = new UnityWebRequest("https://www.mysite.com"); // ����Ŀ�� URL

        // �����ṩ������������� Web ������������
        wr.url = "https://www.mysite.com";
        wr.method = UnityWebRequest.kHttpVerbGET;   // ������Ϊ�κ��Զ��巽�����ṩ�˹�������

        wr.useHttpContinue = false;
        wr.chunkedTransfer = false;
        wr.redirectLimit = 0;  // �����ض���
        wr.timeout = 60;       // �����ò�Ҫ̫С��Web ������ҪһЩʱ��


        var handler = wr.SendWebRequest();
        handler.completed += (a)=>
        {
            if(wr.result == UnityWebRequest.Result.Success)
            {

            }
        };
    }
}
