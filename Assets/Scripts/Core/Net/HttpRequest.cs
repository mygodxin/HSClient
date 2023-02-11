using System;
using System.Collections;
using System.ComponentModel;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine.Networking;

public class HttpRequest
{

    private static HttpRequest _inst = null;
    public static HttpRequest inst
    {
        get
        {
            if (_inst == null)
                _inst = new HttpRequest();
            return _inst;
        }
    }

    //public async void Request()
    //{
    //    //return await Post();
    //}

    IEnumerator Post()
    {
        UnityWebRequest wr = new UnityWebRequest(); // ��ȫΪ��
        UnityWebRequest wr2 = new UnityWebRequest("https://www.mysite.com"); // ����Ŀ�� URL

        // �����ṩ������������� Web ������������
        wr.url = "https://www.mysite.com";
        wr.method = UnityWebRequest.kHttpVerbGET;   // ������Ϊ�κ��Զ��巽�����ṩ�˹�������
        wr.SetRequestHeader("Content-Type", "application/octet-stream");
        wr.useHttpContinue = false;
        wr.redirectLimit = 0;  // �����ض���
        wr.timeout = 60;       // �����ò�Ҫ̫С��Web ������ҪһЩʱ��
           
        yield return wr.SendWebRequest();
        //var handler = wr.SendWebRequest();
        //handler.completed += (a) =>
        //{
        //    if (wr.result == UnityWebRequest.Result.Success)
        //    {
        //        wr.downloadHandler.text;
        //    }
        //};
    }
}
