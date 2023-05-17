using HS;
using PureMVC.Patterns.Facade;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ��������
/// </summary>
public class Launch : MonoBehaviour
{
    void Start()
    {
        //����pureMVC

        //��ʼ������
        ConfigManager.Inst.Init();

        //������ʼ����
        UIManager.Inst.ShowScene<LoginScene>("��loginScene");
    }
}
