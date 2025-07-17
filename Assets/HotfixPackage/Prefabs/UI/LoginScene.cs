using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Spine.Unity;
using EnhancedUI.EnhancedScroller;

namespace GameFramework
{
    public class LoginScene : BaseScene
    {
        public static string Path = "Assets/HotfixPackage/Prefabs/UI/LoginScene.prefab";

        //由 BindComponent 自动生成，请勿直接修改。
        //__FIELD_BEGIN__
        public Button ExitButton;
        public Button StartButton;
        public TextMeshProUGUI StartTText;
        public Button SettingButton;
        //__FIELD_END__

        public override void Init()
        {

        }

        protected override string[] EventList()
        {
            return base.EventList();
        }

        protected override void OnShow()
        {

        }

        public override void Hide()
        {

        }
    }
}
