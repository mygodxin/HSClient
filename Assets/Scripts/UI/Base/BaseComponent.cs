using HS;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class BaseComponent : UIComp
    {
        public virtual void Refresh()
        {
            Debug.LogError($"{name} not Refresh");
        }
    }
}