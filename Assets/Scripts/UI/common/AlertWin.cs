using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework
{
    public class AlertParam
    {
        public string Content;
        public Action LeftAction;
        public Action RightAction;
        public Action CloseAction;
    }

    public partial class AlertWin : BaseWindow
    {
        public override UILayer Layer => UILayer.Top;
        public static string Path = "Assets/GamePackage/UI/Common/AlertWin.prefab";

        //由 BindComponent 自动生成，请勿直接修改。
        //__FIELD_BEGIN__
        public Button CloseButton;
        public TextMeshProUGUI ContentTText;
        public Button LeftButton;
        public Button RightButton;
        //__FIELD_END__

        private AlertParam alertParam;

        protected override string[] EventList()
        {
            return Array.Empty<string>();

        }
        protected override void OnEvent(string eventName, object data)
        {
        }

        protected override void OnInit()
        {
            this.LeftButton.onClick.AddListener(this.OnClickLeft);
            this.RightButton.onClick.AddListener(this.OnClickRight);
        }

        protected override void OnShow()
        {
            this.alertParam = (AlertParam)this.Data;
            if (this.alertParam == null)
            {
                Debug.LogWarning("请确认AlertWin的param");
            }
            this.ContentTText.text = this.alertParam.Content;
        }

        //隐藏界面
        protected override void OnHide()
        {
            this.alertParam.CloseAction?.Invoke();
        }
        //点击左侧男女
        private void OnClickLeft()
        {
            this.alertParam.LeftAction?.Invoke();
            this.Hide();
        }
        //点击右侧按钮
        private void OnClickRight()
        {
            this.alertParam.RightAction?.Invoke();
            this.Hide();
        }
    }
}
