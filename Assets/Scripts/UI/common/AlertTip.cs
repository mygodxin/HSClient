using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework
{
    public partial class AlertTip : BaseWindow
    {
        public static string Path = "Assets/GamePackage/UI/Common/AlertTip/AlertTip.prefab";

        public override UILayer Layer => UILayer.Top;

        //由 BindComponent 自动生成，请勿直接修改。
        //__FIELD_BEGIN__
        public Image BgImage;
        public TextMeshProUGUI ContentTText;
        //__FIELD_END__

        public CanvasGroup CanvasGroup;
        private Sequence sequence;

        protected override string[] EventList()
        {
            return Array.Empty<string>();
        }
        protected override void OnEvent(string eventName, object data)
        {
        }

        protected override void OnInit()
        {
        }

        protected override void OnShow()
        {
        }

        protected override void OnHide()
        {
        }

        public void Play(string content, Action<AlertTip> callback)
        {
            this.ContentTText.text = content;

            if (sequence != null)
            {
                sequence.Kill();
            }
            sequence = DOTween.Sequence();
            transform.localPosition = new Vector3(0, 450);
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            CanvasGroup.alpha = 1;
            sequence.Append(this.transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f));
            //sequence.Append(this.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.1f));
            sequence.AppendInterval(1.5f);
            sequence.Append(this.transform.DOLocalMoveY(550, 0.2f));
            sequence.Join(CanvasGroup.DOFade(0, 0.2f));
            sequence.AppendCallback(() => { callback(this); });
            sequence.Play();
        }
    }
}
