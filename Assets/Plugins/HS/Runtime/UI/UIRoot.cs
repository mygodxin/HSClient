using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HS
{
    /// <summary>
    /// UI根节点
    /// </summary>
    public class UIRoot
    {
        /// <summary>
        /// 设计宽
        /// </summary>
        public static float DesignWidth = 1334;
        /// <summary>
        /// 设计高
        /// </summary>
        public static float DesignHeight = 750;
        /// <summary>
        /// UI缓存
        /// </summary>
        public Dictionary<string, UIView> UICache;
        /// <summary>
        /// 界面半透明背景层
        /// </summary>
        private GameObject _modalLayer;
        /// <summary>
        /// 画布对象
        /// </summary>
        private Canvas _canvas;
        /// <summary>
        /// 根节点
        /// </summary>
        public Dictionary<UILayer, RectTransform> UILayers;
        /// <summary>
        /// 根节点
        /// </summary>
        public RectTransform ViewRoot;

        public static UIRoot Inst = new UIRoot();

        public UIRoot()
        {
            UICache = new Dictionary<string, UIView>();
            UILayers = new Dictionary<UILayer, RectTransform>();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            var uiRoot = CreateRectTransformByName("UIRoot", Canvas.transform);
            ViewRoot = uiRoot;

            var scene = CreateRectTransformByName("Scene", uiRoot);
            UILayers.Add(UILayer.Scene, scene);

            var fight = CreateRectTransformByName("Fight", uiRoot);
            UILayers.Add(UILayer.Fight, fight);

            var window = CreateRectTransformByName("Window", uiRoot);
            UILayers.Add(UILayer.Window, window);

            var popup = CreateRectTransformByName("Popup", uiRoot);
            UILayers.Add(UILayer.Popup, popup);

            var top = CreateRectTransformByName("Top", Canvas.transform);
            UILayers.Add(UILayer.Top, top);
        }

        private RectTransform CreateRectTransformByName(string name, Transform parent)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            return rect;
        }

        /// <summary>
        /// 画布
        /// </summary>
        public Canvas Canvas
        {
            get
            {
                if (!_canvas)
                    _canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
                return _canvas;
            }
        }

        private Camera _camera;
        /// <summary>
        /// 摄像机
        /// </summary>
        public Camera Camera
        {
            get
            {
                if (_camera == null)
                    _camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
                return _camera;
            }
        }
        private async Task<UIView> GetComponent(Type type)
        {
            if (!UICache.TryGetValue(type.Name, out var comp))
            {
                comp = await LoadComp(type);

                var key = type.Name;
                if (this.UICache.ContainsKey(key))
                {
                    return this.UICache[key];
                }
                else
                {
                    this.UICache.Add(key, comp);
                }
            }
            return comp;
        }

        /// <summary>
        /// 根据类型加载组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private async Task<UIView> LoadComp(Type type)
        {
            //根据组件的Path去加载
            var property = type.GetField("Path", BindingFlags.Public | BindingFlags.Static);
            var path = property.GetValue(null);
            var go = await ResLoader.LoadAssetAsync<GameObject>((string)path);

            var comp = (UIView)UnityEngine.Object.Instantiate(go).GetComponent(type);
            return comp;
        }

        public async Task<T> RenderComp<T>(object data = null, Transform parent = null) where T : UIView
        {
            var comp = await this.GetComponent(typeof(T));
            var render = UnityEngine.Object.Instantiate(comp);
            render.Data = data;
            render.transform.SetParent(parent, false);

            render.OnAddedToStage();

            return (T)render;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public async Task<UIComp> AddComponent(Type type, object data = null, Transform trans = null)
        {
            var comp = await this.GetComponent(type);
            comp.Data = data;
            if (trans == null) trans = UILayers[UILayer.Scene].transform;
            comp.transform.SetParent(trans, false);
            comp.gameObject.SetActive(true);
            comp.OnAddedToStage();
            return comp;
        }

        public void AddCompToLayer(RectTransform rectTransform, UILayer layer)
        {
            rectTransform.SetParent(UILayers[layer].transform, false);
        }

        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<UIView> ShowWindow(Type type, object data = null, Transform trans = null)
        {
            var view = (UIView)await this.GetComponent(type);
            var layer = view.Layer;
            view.transform.SetParent(UILayers[layer].transform, false);
            view.Data = data;
            view.gameObject.SetActive(true);
            view.OnAddedToStage();

            //新打开窗口放到最上面
            view.transform.SetAsLastSibling();

            AdjustModalLayer();
            return view;
        }

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        /// <param name="view"></param>
        public void HideWindow(UIView view)
        {
            view.Hide();
        }
        /// <summary>
        /// 立即隐藏窗口
        /// </summary>
        /// <param name="view"></param>
        /// <param name="dispose"></param>
        public void HideWindowImmediately(UIView view, bool dispose = false)
        {
            view.OnRemovedFromStage();

            if (dispose)
            {
                UnityEngine.Object.DestroyImmediate(view.gameObject);
            }
            else
            {
                view.gameObject.SetActive(false);
            }

            AdjustModalLayer();
        }

        /// <summary>
        /// 屏幕适配
        /// </summary>
        /// <param name="scaleUI"></param>
        public void ScreenUISelfAdptation(Transform scaleUI)
        {
            float widthrate = UIRoot.DesignWidth / Screen.width;
            float heightrate = UIRoot.DesignHeight / Screen.height;
            float postion_x = scaleUI.GetComponent<RectTransform>().anchoredPosition.x * widthrate;
            float postion_y = scaleUI.GetComponent<RectTransform>().anchoredPosition.y * heightrate;
            scaleUI.localScale = new Vector3(widthrate, heightrate, 1);

            scaleUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(postion_x, postion_y);
        }

        /// <summary>
        /// 界面半透明背景层
        /// </summary>
        public GameObject ModalLayer
        {
            get
            {
                if (_modalLayer == null)
                    CreateModalLayer();

                return _modalLayer;
            }
            private set
            {
                _modalLayer = value;
            }
        }

        private void CreateModalLayer()
        {
            _modalLayer = new GameObject("ModalLayer");
            var parent = UILayers[UILayer.Window].transform;
            var img = _modalLayer.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.9f);
            var rectTran = _modalLayer.GetComponent<RectTransform>();
            rectTran.sizeDelta = new Vector2(Screen.width, Screen.height);
            _modalLayer.transform.SetParent(parent, false);
            _modalLayer.transform.SetAsFirstSibling();
            _modalLayer.transform.localPosition = Vector3.zero;
            rectTran.anchorMin = new Vector2(0, 0);
            rectTran.anchorMax = new Vector2(1, 1);
            rectTran.offsetMax = Vector2.zero;
            rectTran.offsetMin = Vector2.zero;
            _modalLayer.AddComponent<Button>();
        }

        private void AdjustModalLayer()
        {
            if (_modalLayer == null)
                CreateModalLayer();
            var parent = UILayers[UILayer.Window].transform;
            int cnt = parent.childCount;

            _modalLayer.transform.SetSiblingIndex(cnt - 1);
            var btn = _modalLayer.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();

            for (int i = cnt - 1; i >= 0; i--)
            {
                var go = parent.GetChild(i);
                var name = go.name.Replace("(Clone)", "");
                this.UICache.TryGetValue(name, out var comp);
                var win = (UIView)comp;
                if (win != null && win.IsModal && go.gameObject.activeInHierarchy)
                {
                    if (win.IsClickVoidClose)
                    {
                        btn.onClick.AddListener(() =>
                        {
                            win.Hide();
                        });
                    }
                    _modalLayer.SetActive(true);
                    _modalLayer.transform.SetSiblingIndex(i);
                    return;
                }
            }
            _modalLayer.SetActive(false);
        }
    }
}