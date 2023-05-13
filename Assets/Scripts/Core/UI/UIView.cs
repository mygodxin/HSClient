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
        /// ��ʹ��OnShow������������OnEnable����ʹ��base.OnEnable
        /// </summary>
        protected virtual void OnEnable()
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
        /// <summary>
        /// ��ʹ��OnHide������������OnDisable����ʹ��base.OnDisable
        /// </summary>
        protected virtual void OnDisable()
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