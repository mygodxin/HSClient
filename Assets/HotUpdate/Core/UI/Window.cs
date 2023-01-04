
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
    GameObject view;
    /// <summary>
    /// ��Դ·��
    /// </summary>
    protected abstract string path();
    /// <summary>
    /// ��������
    /// </summary>
    public object Data;

    protected EventTarget eventTarget;

    bool inited = false;
    bool loading = false;
    protected bool isModal = true;
    protected bool clickClose = true;

    public Window()
    {
        On("onAddedToStage", onAddedToStage);
    }

    public void onAddedToStage(object data)
    {
        Data = data;
        if (!inited)
        {
            init();
        }
        else
        {
            DoShowAnimation();
        }
    }

    public void Show(object data = null)
    {
        GRoot.Instance.ShowWindow(this, data);
    }

    virtual protected void DoShowAnimation()
    {
        OnShow();
    }

    void init()
    {
        if (loading)
            return;
        view = Addressables.LoadAssetAsync<GameObject>(path()).WaitForCompletion();
        Object.Instantiate(view);
        view.SetActive(true);

        OnInit();

        registerEvent();

        DoShowAnimation();
    }

    public void Hide()
    {
        removeEvent();
    }

    Dictionary<string, EventCallback> callbackDic;
    protected virtual string[] eventList()
    {
        return null;
    }
    protected void registerEvent()
    {
        string[] eventList = this.eventList();
        if(eventList != null)
        {
            callbackDic = new Dictionary<string, EventCallback>();
            foreach (var str in eventList)
            {
                EventCallback callback = delegate (object data)
                {
                    onEvent(str, data);
                };
                Facade.Inst.On(str, callback);
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
                Facade.Inst.Off(str.Key, str.Value);
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
