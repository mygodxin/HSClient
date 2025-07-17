
using DG.Tweening;
using HS;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameFramework
{
    /// <summary>
    /// 窗口
    /// </summary>
    public class BaseWindow : UIView
    {
        Dictionary<string, Action<object>> callbackDic;
        public AudioClip OpenAudioClip;

        protected override void DoShowAnimation()
        {
            this.RegisterEvent();
            PlayAudio();
            OnShow();
            //this.transform.localScale.Set(.2f, .2f, .2f);
            //var tween = this.transform.DOScale(Vector3.one, 0.25f);
            //tween.SetEase(Ease.OutBack);
            //tween.onComplete += OnShow;
        }

        protected virtual void PlayAudio()
        {
            if (OpenAudioClip != null)
            {
                AudioManager.Inst.PlayEffect(OpenAudioClip);
            }
            else
                AudioManager.Inst.PlayEffect("Assets/GamePackage/Audios/通用/通用_一级弹窗.mp3");
        }

        protected override void DoHideAnimation()
        {
            //var tween = this.transform.DOScale(new Vector3(.1f, .1f, .1f), 0.25f);
            //tween.SetEase(Ease.InBack);
            //tween.onComplete += () => { HideImmediately(); };
            this.RemoveEvent();
            HideImmediately();
        }

        protected virtual string[] EventList()
        {
            return null;
        }
        protected void RegisterEvent()
        {
            string[] eventList = this.EventList();
            if (eventList != null)
            {
                callbackDic = new Dictionary<string, Action<object>>();
                foreach (var str in eventList)
                {
                    Action<object> callback = (object data) =>
                    {
                        OnEvent(str, data);
                    };
                    EventManager.Inst.On(str, callback);
                    callbackDic.Add(str, callback);
                }
            }
        }
        protected void RemoveEvent()
        {
            string[] eventList = this.EventList();
            if (eventList != null)
            {
                foreach (var str in callbackDic)
                {
                    EventManager.Inst.Off(str.Key, str.Value);
                }
            }
        }
        protected virtual void OnEvent(string eventName, object data)
        {
        }
    }

}