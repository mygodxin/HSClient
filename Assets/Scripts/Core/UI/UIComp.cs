using UnityEngine;

namespace HS
{
    //public interface IPath
    //{
    //    public static abstract string path();
    //}
    /// <summary>
    /// ������࣬�̳���MonoBehaviour��ʹ�ñ��븲��path��Show���Զ�����
    /// </summary>
    public abstract class UIComp : MonoBehaviour//,IPath
    {
        /// <summary>
        /// ��Դ·����c# 11����static abstract �ȴ�����
        /// </summary>
        //public static string path
        //{
        //    get
        //    {
        //        throw new System.Exception("δʵ��path");
        //    }
        //}
        /// <summary>
        /// ����
        /// </summary>
        public object data;

        protected void Awake()
        {
        }

        protected void Start()
        {
            this.OnInit();
        }

        internal virtual void OnAddedToStage()
        {
            this.OnShow();
        }

        internal virtual void OnRemovedFromStage()
        {
            this.OnHide();
        }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        protected virtual void OnInit()
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
        /// <summary>
        /// �ر�
        /// </summary>
        public virtual void Hide()
        {
            this.OnRemovedFromStage();
            this.gameObject.SetActive(false);
        }
    }
}