
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UnityFramework
{
    /// <summary>
    /// UI����
    /// </summary>
    public abstract class GComponent : EventTarget
    {
        /// <summary>
        /// ������ʾ����
        /// </summary>
        public GameObject view;
        /// <summary>
        /// ��Դ·��
        /// </summary>
        protected abstract string path();
        /// <summary>
        /// ��������
        /// </summary>
        public object data;

        protected bool _inited = false;
        protected bool _loading = false;
        public GComponent(GameObject view = null)
        {
            this.view = view;
            On("onAddedToStage", onAddedToStage);
            On("onRemovedFromStage", onRemovedFromStage);
        }

        protected virtual void onAddedToStage(object data)
        {
        }
        protected virtual void onRemovedFromStage(object data)
        {
        }
        protected Transform GetTransform(string path)
        {
            return view.transform.Find("Canvas/" + path);
        }
        protected Image GetImage(string path)
        {
            return this.GetTransform(path).GetComponent<Image>();
        }
        protected RawImage GetRawImage(string path)
        {
            return this.GetTransform(path).GetComponent<RawImage>();
        }
        protected Mask GetMask(string path)
        {
            return this.GetTransform(path).GetComponent<Mask>();
        }
        protected Shadow GetShadow(string path)
        {
            return this.GetTransform(path).GetComponent<Shadow>();
        }
        protected Outline GetOutline(string path)
        {
            return this.GetTransform(path).GetComponent<Outline>();
        }
        protected Button GetButton(string path)
        {
            return this.GetTransform(path).GetComponent<Button>();
        }
        protected Toggle GetToggle(string path)
        {
            return this.GetTransform(path).GetComponent<Toggle>();
        }
        protected ToggleGroup GetToggleGroup(string path)
        {
            return this.GetTransform(path).GetComponent<ToggleGroup>();
        }
        protected Slider GetSlider(string path)
        {
            return this.GetTransform(path).GetComponent<Slider>();
        }
        protected Scrollbar GetScrollbar(string path)
        {
            return this.GetTransform(path).GetComponent<Scrollbar>();
        }
        protected Dropdown GetDropdown(string path)
        {
            return this.GetTransform(path).GetComponent<Dropdown>();
        }
        protected InputField GetInputField(string path)
        {
            return this.GetTransform(path).GetComponent<InputField>();
        }
        protected ScrollRect GetScrollRect(string path)
        {
            return this.GetTransform(path).GetComponent<ScrollRect>();
        }
        protected LayoutElement GetLayoutElement(string path)
        {
            return this.GetTransform(path).GetComponent<LayoutElement>();
        }
        protected ContentSizeFitter GetContentSizeFitter(string path)
        {
            return this.GetTransform(path).GetComponent<ContentSizeFitter>();
        }
        protected AspectRatioFitter GetAspectRatioFitter(string path)
        {
            return this.GetTransform(path).GetComponent<AspectRatioFitter>();
        }
        protected HorizontalLayoutGroup GetHorizontalLayoutGroup(string path)
        {
            return this.GetTransform(path).GetComponent<HorizontalLayoutGroup>();
        }
        protected VerticalLayoutGroup GetVerticalLayoutGroup(string path)
        {
            return this.GetTransform(path).GetComponent<VerticalLayoutGroup>();
        }
        protected GridLayoutGroup GetGridLayoutGroup(string path)
        {
            return this.GetTransform(path).GetComponent<GridLayoutGroup>();
        }
        protected TMP_Text GetTextTMP(string path)
        {
            return this.GetTransform(path).GetComponent<TMP_Text>();
        }
        protected GList GetList(string path)
        {
            return this.GetTransform(path).GetComponent<GList>();
        }
    }

}