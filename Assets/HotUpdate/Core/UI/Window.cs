using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����
/// </summary>
public class Window : GComponent
{
    public GameObject contentPanel;
    public string path = "";
    private bool inited = false;

    /// <summary>
    /// ��ʼ��
    /// </summary>
    public virtual void OnInit()
    {

    }
    /// <summary>
    /// ��
    /// </summary>
    public virtual void OnShow()
    {

    }
    /// <summary>
    /// �ر�
    /// </summary>
    public virtual void OnHide()
    {

    }
}
