using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HS
{
    /// <summary>
    /// 面板基类，继承自UIComp，使用必须覆盖path，Show会自动加载
    /// </summary>
    public class UIView : UIComp
    {
        public virtual UILayer Layer => UILayer.Window;
        /// <summary>
        /// 是否模态窗
        /// </summary>
        public bool IsModal = true;
        /// <summary>
        /// 是否点击空白处关闭
        /// </summary>
        public bool IsClickVoidClose = true;
        /// <summary>
        /// 绑定数据
        /// </summary>
        public object Data;

        /// <summary>
        /// 是否初始化
        /// </summary>
        protected bool _isInit = false;

        internal virtual void OnAddedToStage()
        {
            if (!_isInit)
            {
                OnInit();
                InitComp();
                _isInit = true;
            }
            DoShowAnimation();
        }

        private void InitComp()
        {
            var uiComps = transform.GetComponentsInChildren<UIComp>(true);
            foreach (var uiComp in uiComps)
                uiComp.Init();
        }

        /// <summary>
        /// 面板打开动画
        /// </summary>
        protected virtual void DoShowAnimation()
        {
            OnShow();
        }

        internal virtual void OnRemovedFromStage()
        {
            OnHide();
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public virtual void Hide()
        {
            DoHideAnimation();

        }
        /// <summary>
        /// 关闭动画
        /// </summary>
        protected virtual void DoHideAnimation()
        {
            HideImmediately();
        }
        /// <summary>
        /// 立即关闭，不执行关闭动画
        /// </summary>
        public virtual void HideImmediately(bool dispose = false)
        {
            UIRoot.Inst.HideWindowImmediately(this, dispose);
        }


        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void OnInit()
        {

        }

        /// <summary>
        /// 打开
        /// </summary>
        protected virtual void OnShow()
        {

        }

        /// <summary>
        /// 关闭
        /// </summary>
        protected virtual void OnHide()
        {

        }
    }
}