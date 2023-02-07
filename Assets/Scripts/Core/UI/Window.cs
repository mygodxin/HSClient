
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

/// <summary>
/// ����
/// </summary>
public abstract class Window : GComponent
{
    /// <summary>
    /// ������ʾ����
    /// </summary>
    public GameObject view;
    /// <summary>
    /// ��Դ·��
    /// </summary>
    protected abstract string path();
    /// <summary>
    /// ��������
    /// </summary>
    public object data;

    protected EventTarget eventTarget;

    bool inited = false;
    bool loading = false;
    public bool modal = true;
    protected bool clickClose = true;

    public Window()
    {
    }

    protected override void onAddedToStage(object data)
    {
        this.data = data;
        if (!inited)
        {
            init();
        }
        else
        {
            registerEvent();

            DoShowAnimation();
        }
    }

    protected override void onRemovedFromStage(object data)
    {
        OnHide();
        DoHideAnimation();
    }

    public void Show(object data = null)
    {
        GRoot.inst.ShowWindow(this, data);
    }

    virtual protected void DoShowAnimation()
    {
        OnShow();
    }

    void init()
    {
        if (loading)
            return;
        if (view == null)
        {
            var gameObject = Addressables.LoadAssetAsync<GameObject>(path()).WaitForCompletion();
            view = Object.Instantiate(gameObject);
        }
        view.SetActive(true);

        OnInit();

        registerEvent();

        DoShowAnimation();
    }

    public void Hide()
    {
        GRoot.inst.HideWindow(this);
    }

    protected virtual void DoHideAnimation()
    {

        HideImmediately();
    }

    public void HideImmediately()
    {
        removeEvent();
        //view.transform.parent = null;
        //Object.Destroy(view);
        view.SetActive(false);
    }


    Dictionary<string, EventCallback> callbackDic;
    protected virtual string[] eventList()
    {
        return null;
    }
    protected void registerEvent()
    {
        string[] eventList = this.eventList();
        if (eventList != null)
        {
            callbackDic = new Dictionary<string, EventCallback>();
            foreach (var str in eventList)
            {
                EventCallback callback = delegate (object data)
                {
                    onEvent(str, data);
                };
                Facade.inst.On(str, callback);
                callbackDic.Add(str, callback);
            }
        }
    }
    protected void removeEvent()
    {
        string[] eventList = this.eventList();
        if (eventList != null)
        {
            foreach (var str in callbackDic)
            {
                Facade.inst.Off(str.Key, str.Value);
            }
        }
    }
    protected virtual void onEvent(string eventName, object data)
    {
    }

    /// <summary>
    /// ��ʼ��
    /// </summary>
    public virtual void OnInit()
    {

    }
    /// <summary>
    /// ��
    /// </summary>
    protected virtual void OnShow()
    {

    }
    /// <summary>
    /// �ر�
    /// </summary>
    protected virtual void OnHide()
    {

    }
}
