using HS;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace GameFramework
{

    public class UIManager
    {
        private BaseScene curScene;
        private MovieClip clickMC;  // 鼠标点击动效
        private GameObject ReconnectGO; // 断线重连动画
        private ObjectPool<AlertTip> tipPool;
        private WaitModelWin _waitModelWin;

        public static UIManager Inst = new UIManager();

        public void Init()
        {
            UIRoot.Inst.Init();
            InitAlertTip();
            LoadWaitModel();
        }
        private async void LoadWaitModel()
        {
            var path = typeof(WaitModelWin).GetField("Path", BindingFlags.Public | BindingFlags.Static).GetValue(null).ToString();
            var obj = await ResLoader.LoadAssetAsync<GameObject>(path);
            _waitModelWin = UnityEngine.Object.Instantiate(obj).GetComponent<WaitModelWin>();
            _waitModelWin.transform.SetParent(UIRoot.Inst.Canvas.transform, false);
            _waitModelWin.gameObject.SetActive(false);
        }
        public void ShowScene<T>(object data = null) where T : BaseScene
        {
            ShowScene(typeof(T), data);
        }

        public async void ShowScene(Type type, object data = null)
        {
            if (this.curScene != null)
            {
                this.curScene.Hide();
            }
            this.curScene = (BaseScene)await this.ShowWindow(type, data);
        }

        public BaseScene Scene
        {
            get
            {
                return curScene;
            }
        }

        public async void ShowWindow<T>(object data = null)
        {
            await ShowWindow(typeof(T), data);
        }

        public async Task<BaseWindow> ShowWindow(Type type, object data = null)
        {
            return (BaseWindow)await UIRoot.Inst.ShowWindow(type, data);
        }
        public async void AddComponent<T>(object data = null, Transform trans = null)
        {
            await this.AddComponent(typeof(T), data, trans);
        }
        public async Task<UIComp> AddComponent(Type type, object data = null, Transform trans = null)
        {
            return (UIComp)await UIRoot.Inst.AddComponent(type, data, trans);
        }
        /// <summary>
        /// 打开弹窗
        /// </summary>
        /// <param name="param"></param>
        public void ShowAlert(AlertParam param)
        {
            ShowWindow<AlertWin>(param);
        }

        public void ShowAlert(string content, Action right = null, Action left = null)
        {
            var alertParam = new AlertParam();
            alertParam.Content = content;
            alertParam.LeftAction = left;
            alertParam.RightAction = right;
            ShowWindow<AlertWin>(alertParam);
        }

        public void ShowWaitModel()
        {
            _waitModelWin.gameObject.SetActive(true);
        }
        public void HideWaitModel()
        {
            if (_waitModelWin != null)
            {
                _waitModelWin.gameObject.SetActive(false);
            }
        }

        public async void InitAlertTip()
        {
            var path = typeof(AlertTip).GetField("Path", BindingFlags.Public | BindingFlags.Static).GetValue(null).ToString();
            var obj = await ResLoader.LoadAssetAsync<GameObject>(path);
            tipPool = new UnityEngine.Pool.ObjectPool<AlertTip>(() =>
            {
                return UnityEngine.Object.Instantiate(obj).GetComponent<AlertTip>();
            }, (go) => go.gameObject.SetActive(true), (go) => go.gameObject.SetActive(false));
        }

        public void ShowTip(string content)
        {
            var tip = tipPool.Get();
            var parent = UIRoot.Inst.UILayers[UILayer.Popup];
            tip.transform.SetParent(parent, false);
            tip.Play(content, (AlertTip t) =>
            {
                tipPool.Release(t);
            });
        }

        public void ShowAward()
        {

        }

        public async void ShowConnectAni()
        {
            if (ReconnectGO == null)
            {
                var go = await ResLoader.LoadAssetAsync<GameObject>("Assets/GamePackage/UI/Common/ConnectFrame/ConnectFrame.prefab");
                ReconnectGO = UnityEngine.Object.Instantiate(go);
                ReconnectGO.GetComponent<MovieClip>().SetPlaySettings();
                ReconnectGO.transform.SetParent(UIRoot.Inst.Canvas.transform, false);
                ReconnectGO.transform.localPosition = new Vector3(0, 500);
            }
            ReconnectGO.SetActive(true);
        }

        public void HideConnectAni()
        {
            if (ReconnectGO != null)
            {
                ReconnectGO.gameObject.SetActive(false);
            }
        }

        //public async void ShowItemTip(int id, object Other, RectTransform rect)
        //{
        //    if (itemTip == null)
        //    {
        //        //itemTip = default;
        //        var go = await ResLoader.LoadAssetAsync<GameObject>("Assets/GamePackage/UI/Comp/ItemTip.prefab");
        //        itemTip = UnityEngine.Object.Instantiate<GameObject>(go).GetComponent<ItemTip>();
        //        var parent = UIRoot.Inst.UILayers[UILayer.Popup];
        //        itemTip.transform.SetParent(parent, false);
        //    }
        //    itemTip.gameObject.SetActive(true);
        //    itemTip.SetData(id, Other, rect);
        //}

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                PlayClickAnimation();

#if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_IOS)
                if (EventSystem.current.IsPointerOverGameObject())
#else
                if (1 == Input.touchCount && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
#endif
                {
                    GraphicRaycaster[] graphicRaycasters = UnityEngine.Object.FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);

                    PointerEventData eventData = new PointerEventData(EventSystem.current);
                    eventData.pressPosition = Input.mousePosition;
                    eventData.position = Input.mousePosition;
                    List<RaycastResult> list = new List<RaycastResult>();

                    foreach (var item in graphicRaycasters)
                    {
                        item.Raycast(eventData, list);
                        if (list.Count > 0)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                var obj = list[i].gameObject;
                                if (obj.name != "itemTip")
                                {
                                    //if (itemTip != null)
                                    //    itemTip.gameObject.SetActive(false);
                                }
                            }
                        }
                    }
                }
            }
        }

        #region 鼠标点击动效
        private async void PlayClickAnimation()
        {
            // 鼠标左键点击事件处理代码
            Vector3 mousePosition = Input.mousePosition;

            if (this.clickMC == null)
            {
                var click = await ResLoader.LoadAssetAsync<GameObject>("Assets/GamePackage/Effect/Click/Click.prefab");
                var prefab = UnityEngine.Object.Instantiate(click);
                //prefab.GetComponent<Image>().raycastTarget = false;
                prefab.transform.SetParent(UIRoot.Inst.Canvas.transform, false);
                this.clickMC = prefab.GetComponent<MovieClip>();
                //clickMC.transform.SetSiblingIndex(0);
            }
            Vector2 newPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UIRoot.Inst.Canvas.GetComponent<RectTransform>(), mousePosition, null, out newPosition);
            this.clickMC.GetComponent<RectTransform>().localPosition = newPosition;

            // 播放动画
            PlayAnimationOnce();
        }
        private void PlayAnimationOnce()
        {
            // 从头开始播放动画
            this.clickMC.gameObject.SetActive(true);
            clickMC.SetPlaySettings(0, -1, 1, -1, () =>
            {
                clickMC.gameObject.SetActive(false);
            });
        }
        #endregion
    }
}
