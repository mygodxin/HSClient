using DG.Tweening;
using GameFramework;
using HS;
using UnityEngine;

/// <summary>
/// 启动场景
/// </summary>
public class Launch : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Launch Start");
        //初始化配置
        ConfigManager.Inst.Init();

        //启动开始场景
        // UIManager.Inst.ShowScene<Login>("打开loginScene");
    }
}