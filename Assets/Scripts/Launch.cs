using HS;
using UnityEngine;

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
