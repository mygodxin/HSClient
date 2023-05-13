using System.IO;
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

        /// <summary>
        /// ��ʹ��OnInit������������Awake����ʹ��base.Awake
        /// </summary>
        private void Start()
        {
            this.OnInit();
        }
        /// <summary>
        /// ��ʹ��OnShow������������OnEnable����ʹ��base.OnEnable
        /// </summary>
        private void OnEnable()
        {
            this.OnShow();
        }
        /// <summary>
        /// ��ʹ��OnHide������������OnDisable����ʹ��base.OnDisable
        /// </summary>
        private void OnDisable()
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

        public virtual void Hide()
        {
            this.gameObject.SetActive(false);
        }
    }
}