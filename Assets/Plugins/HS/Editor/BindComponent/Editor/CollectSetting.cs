using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class CollectSetting
{
    public CollectSetting()
    {
        WindowCodeTemp = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Plugins/HS/Editor/BindComponent/Editor/Config/WindowCodeTemp.txt");
        ComponentCodeTemp = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Plugins/HS/Editor/BindComponent/Editor/Config/ComponentCodeTemp.txt");
    }
    [Serializable]
    public class ComponentType
    {
        public string TypeKey;
        public string TypeName;
        public ComponentType(string typeKey, string typeName)
        {
            this.TypeKey = typeKey;
            this.TypeName = typeName;
        }
    }
    [Space(4)]
    [Tooltip("收集代码名称")]
    [HideInInspector]
    public string CollectorCodeName = "Collect";
    [Header("Collect")]
    [Tooltip("组件类型映射列表")]
    public List<ComponentType> ComponentList = new List<ComponentType>
        {
            new ComponentType("Obj","GameObject"),
            new ComponentType("Tran", "Transform"),
            new ComponentType("Rect", "RectTransform"),

            new ComponentType("Canvas", "Canvas"),
            new ComponentType("CGroup", "CanvasGroup"),

            new ComponentType("Anim", "Animation"),
            new ComponentType("Animator", "Animator"),

            new ComponentType("Text", "Text"),
            new ComponentType("Image", "Image"),
            new ComponentType("RawImage", "RawImage"),
            new ComponentType("Mask", "Mask"),
            new ComponentType("RectMask", "RectMask2D"),

            new ComponentType("Button", "Button"),
            new ComponentType("Toggle", "Toggle"),
            new ComponentType("TGroup", "ToggleGroup"),
            new ComponentType("Slider", "Slider"),
            new ComponentType("Scrollbar", "Scrollbar"),
            new ComponentType("Dropdown", "Dropdown"),
            new ComponentType("InputField", "InputField"),
            new ComponentType("ScrollRect", "ScrollRect"),

            new ComponentType("HLayout", "HorizontalLayoutGroup"),
            new ComponentType("VLayout", "VerticalLayoutGroup"),
            new ComponentType("GLayout", "GridLayoutGroup"),

            new ComponentType("TText","TMP_Text"),
            new ComponentType("TDropdown","TMP_Dropdown"),
            new ComponentType("TInputField","TMP_InputField"),
            new ComponentType("SG","SkeletonGraphic"),
            new ComponentType("SR","SkeletonRenderer"),
            new ComponentType("SA","SkeletonAnimation"),

            new ComponentType("Scroller","EnhancedScroller"),
            new ComponentType("CImage","CircleImage"),
            new ComponentType("MC","MovieClip"),
            new ComponentType("HSList","HSList"),
            new ComponentType("HSBar","HSProgressBar"),
            new ComponentType("HSTabView","HSTabView"),
            new ComponentType("HSButton","HSButton")
        };
    [Tooltip("默认字段前缀")]
    public string FieldNamePrefix = "";
    [Tooltip("字段名是否使用组件类型 默认值")]
    public bool FieldNameUseType = false;
    private Dictionary<string, string> componentDic;

    [Space(4)]
    [Header("Generate")]
    [Tooltip("默认命名空间")]
    public string Namespace = "GameFramework";
    [Tooltip("默认代码保存地址")]
    public string CodeSavePath = "Assets/Scripts";
    [Tooltip("窗口代码模板")]
    public TextAsset WindowCodeTemp;
    [Tooltip("组件代码模板")]
    public TextAsset ComponentCodeTemp;
    [Tooltip("组件名包含该字段时其自身和所有子组件都将被收集器忽略")]
    public string ExcludeName = "Comp";
    [Space(4)]

    public Regex DefaultNameRegex = new Regex(@"^[A-Za-z][A-Za-z0-9_]*$");
    public Regex FieldNameRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");

    public Dictionary<string, string> ComponentMapDict
    {
        get
        {
            if (componentDic == null)
            {
                componentDic = new Dictionary<string, string>();
                for (int i = ComponentList.Count - 1; i >= 0; i--)
                {
                    ComponentType map = ComponentList[i];
                    if (componentDic.ContainsKey(map.TypeKey))
                    {
                        ComponentList.RemoveAt(i);
                        continue;
                    }
                    componentDic.Add(map.TypeKey, map.TypeName);
                }
            }

            return componentDic;
        }
    }
}
