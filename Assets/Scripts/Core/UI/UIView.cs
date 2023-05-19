using System.Collections.Generic;
using UnityEngine;

namespace HS
{
    /// <summary>
    /// �����࣬�̳���UIComp��ʹ�ñ��븲��path��Show���Զ�����
    /// </summary>
    public class UIView : UIComp
    {
        /// <summary>
        /// �Ƿ�ģ̬��
        /// </summary>
        public bool isModal = true;
        /// <summary>
        /// �Ƿ����հ״��ر�
        /// </summary>
        public bool isClickVoidClose = true;

        internal override void OnAddedToStage()
        {
            this.DoShowAnimation();
        }
        /// <summary>
        /// ���򿪶���
        /// </summary>
        protected virtual void DoShowAnimation()
        {
            this.OnShow();
        }

        internal override void OnRemovedFromStage()
        {
            this.OnHide();
        }
        /// <summary>
        /// �ر�
        /// </summary>
        public override void Hide()
        {
            this.DoHideAnimation();
        }
        /// <summary>
        /// �رն���
        /// </summary>
        protected virtual void DoHideAnimation()
        {
            this.HideImmediately();
        }
        /// <summary>
        /// �����رգ���ִ�йرն���
        /// </summary>
        public virtual void HideImmediately()
        {
            UIRoot.Inst.HideWindowImmediately(this);
        }
    }
}