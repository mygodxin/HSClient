using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI���ڵ�
/// </summary>
public class GRoot : GComponent
{

    public GRoot()
    {

    }

    public void ShowWindow(Window win)
    {

    }

    public void HideWindow(Window win)
    {
        win.OnHide();
    }
}
