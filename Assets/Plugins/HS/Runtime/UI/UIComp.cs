using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HS
{
    /// <summary>
    /// 组件基类，继承自MonoBehaviour
    /// </summary>
    public abstract class UIComp : MonoBehaviour
    {
        public virtual void Init()
        {
            Debug.LogWarning($"{name} not Init");
        }
    }
}