using System.IO;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders.Simulation;

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

        private void Awake()
        {
        }

        /// <summary>
        /// ��ʹ��OnInit������������Awake����ʹ��base.Awake
        /// </summary>
        private void Start()
        {
            this.OnInit();
        }

        public virtual void OnAddedToStage(object obj = null)
        {
            this.data = obj;
            this.OnShow();
        }
        /// <summary>
        /// ��ʹ��OnHide������������OnDisable����ʹ��base.OnDisable
        /// </summary>
        public virtual void OnRemovedFromStage()
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
            this.OnRemovedFromStage();
            this.gameObject.SetActive(false);
        }
    }
}