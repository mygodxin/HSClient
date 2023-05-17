using UnityEngine;
using UnityEngine.UI;

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
        private GameObject _clickCloseLayer;

        /// <summary>
        /// ��ӵ���̨ʱ����
        /// </summary>
        public override void OnAddedToStage(object obj = null)
        {
            this.data = obj;
            this.DoShowAnimation();
        }
        /// <summary>
        /// ���򿪶���
        /// </summary>
        protected virtual void DoShowAnimation()
        {
            this.OnShow();
        }
        /// <summary>
        /// ��ʹ��OnHide������������OnDisable����ʹ��base.OnDisable
        /// </summary>
        public override void OnRemovedFromStage()
        {
            this.OnHide();
        }
        /// <summary>
        /// �ر�
        /// </summary>
        public override void Hide()
        {
            if (_clickCloseLayer != null)
                _clickCloseLayer.SetActive(false);
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
            base.Hide();
            UIRoot.Inst.HideWindowImmediately(this);
        }
    }
}