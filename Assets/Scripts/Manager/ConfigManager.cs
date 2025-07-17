using HS;
using Luban;
using System.IO;
using UnityEngine;


/// <summary>
/// 配置管理类，使用了luban
/// </summary>
public class ConfigManager
{
    public string connectURL = "http://192.168.1.184:88";
    public string resoucesURL = "http://192.168.1.3:88";
    public int[] encryptKeys = new int[4]{
           56,
           23123,
           -34332,
           -7878441
       };
    private static ConfigManager _inst = null;
    public static ConfigManager Inst
    {
        get
        {
            if (_inst == null)
                _inst = new ConfigManager();
            return _inst;
        }
    }

    public void Init()
    {
        // var tables = new cfg.Tables(Loader);
        // Debug.Log(tables.Tbitem.DataList);
        // foreach (var k in tables.Tbitem.DataList)
        // {
        //     Debug.Log($"ID:{k.Id},名称:{k.Name},描述:{k.Desc}");
        // }
    }

    // private ByteBuf Loader(string path)
    // {
    //     var json = ResLoader.LoadAssetAsync<TextAsset>("Assets/Config/" + path + ".bytes");
    //     json.Wait();
    //     return new ByteBuf(File.ReadAllBytes(json.Result.text));
    // }
}
