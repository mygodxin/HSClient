using HS;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ��������
/// </summary>
public class Launch : MonoBehaviour
{
    void Start()
    {
        //��ʼ������
        ConfigManager.Inst.Init();

        //������ʼ����
        UIManager.Inst.ShowScene<LoginScene>("��loginScene");
    }
}
