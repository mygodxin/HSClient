using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotUpdate : MonoBehaviour
{
    public Button btnClick;
    public TextMeshProUGUI txt;
    private int i = 0;
    // Start is called before the first frame update
    void Start()
    {
        btnClick.onClick.AddListener(this.OnClick);

        Debug.Log("����DLL");

        Debug.Log("����");
        gameObject.AddComponent<GameScene>();

        Debug.Log("=======����������־������ɹ�������ʾ����Ŀ���ȸ��´���1=======");
    }

    private void OnClick()
    {
        txt.text = "firstsss" + i;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
