using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace HS
{
    /// <summary>
    /// UI���ڵ�
    /// </summary>
    public class UIRoot
    {
        /// <summary>
        /// ��ƿ��
        /// </summary>
        public static float designWidth = 1280;
        /// <summary>
        /// ��Ƹ߶�
        /// </summary>
        public static float designHeight = 720;
        /// <summary>
        /// UI�����ֵ�
        /// </summary>
        public Dictionary<string, UIView> cacheDict;
        /// <summary>
        /// �Ѵ򿪴����б�
        /// </summary>
        public List<UIView> openList;
        /// <summary>
        /// modal��
        /// </summary>
        private GameObject _modalLayer;

        private static UIRoot _inst = null;
        public static UIRoot Inst
        {
            get
            {
                if (_inst == null)
                    _inst = new UIRoot();
                return _inst;
            }
        }
        public UIRoot()
        {
            cacheDict = new Dictionary<string, UIView>();
            openList = new List<UIView>();
        }
        /// <summary>
        /// ��ȡ��ǰ������������
        /// </summary>
        public Transform Canvas
        {
            get
            {
                return GameObject.Find("Canvas").transform;
            }
        }
        /// <summary>
        /// �򿪴���
        /// </summary>
        /// <param name="type">����</param>
        /// <param name="data">���ݵ�����</param>
        /// <returns></returns>
        public async void ShowWindow(Type type, object data = null)
        {
            cacheDict.TryGetValue(type.Name, out var view);
            if (view == null)
            {
                //���ز�ʵ����
                //����wait��΢��С��Ϸ��ƽ̨���ñ����Ե�ʣ���ΪĿǰ���첽�ص�
                var path = ResManager.UIPath + type.GetFields().FirstOrDefault(field => field.Name == "path").GetValue(null) + ".prefab";
                var go = await Loader.LoadAssetAsync<GameObject>(path);

                view = (UIView)UnityEngine.Object.Instantiate(go).GetComponent(type);
                var key = type.Name;
                if (this.cacheDict.ContainsKey(key))
                {
                    this.cacheDict[key] = view;
                }
                else
                {
                    this.cacheDict.Add(key, view);
                }
            }

            view.transform.SetParent(this.Canvas, false);

            this.InitWindow(view, data);
        }
        private void InitWindow(UIView view, object data)
        {
            view.gameObject.SetActive(true);

            view.data = data;

            view.OnAddedToStage();

            //�¿�������������
            view.transform.SetAsLastSibling();

            openList.Add(view);

            AdjustModalLayer();
        }
        /// <summary>
        /// ���ش���
        /// </summary>
        /// <param name="view"></param>
        public void HideWindow(UIView view)
        {
            view.Hide();
        }
        /// <summary>
        /// �������ش���
        /// </summary>
        /// <param name="view"></param>
        /// <param name="dispose"></param>
        public void HideWindowImmediately(UIView view, bool dispose = false)
        {
            openList.Remove(view);
            view.gameObject.SetActive(false);

            AdjustModalLayer();
        }
        /// <summary>
        /// UI��Ļ�ߴ�����
        /// </summary>
        /// <param name="scaleUI">Ҫ����UI��Transform</param>
        public void ScreenUISelfAdptation(Transform scaleUI)
        {
            float widthrate = UIRoot.designWidth / Screen.width;
            float heightrate = UIRoot.designHeight / Screen.height;
            float postion_x = scaleUI.GetComponent<RectTransform>().anchoredPosition.x * widthrate;
            float postion_y = scaleUI.GetComponent<RectTransform>().anchoredPosition.y * heightrate;
            scaleUI.localScale = new Vector3(widthrate, heightrate, 1);

            scaleUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(postion_x, postion_y);
        }
        /// <summary>
        /// modal��
        /// </summary>
        public GameObject modalLayer
        {
            get
            {
                if (_modalLayer == null)
                    CreateModalLayer();

                return _modalLayer;
            }
        }

        private void CreateModalLayer()
        {
            _modalLayer = new GameObject("modalLayer");
            var canvas = GameObject.Find("Canvas").transform;
            var img = _modalLayer.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.5f);
            var rectTran = _modalLayer.GetComponent<RectTransform>();
            rectTran.sizeDelta = new Vector2(Screen.width, Screen.height);
            _modalLayer.transform.SetParent(canvas, false);
            _modalLayer.transform.SetAsFirstSibling();
            _modalLayer.transform.localPosition = Vector3.zero;
            rectTran.anchorMin = new Vector2(0, 0);
            rectTran.anchorMax = new Vector2(1, 1);
            _modalLayer.AddComponent<Button>();
        }

        private void AdjustModalLayer()
        {
            if (_modalLayer == null)
                CreateModalLayer();
            var canvas = GameObject.Find("Canvas").transform;
            int cnt = canvas.childCount;
            //modalLayerĬ�Ϸ�������
            _modalLayer.transform.SetSiblingIndex(cnt - 1);
            var btn = _modalLayer.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            //����ҪmodalLayerʱ�ı�㼶
            for (int i = cnt - 1; i >= 0; i--)
            {
                var go = canvas.GetChild(i);
                var name = go.name.Replace("(Clone)", "");
                this.cacheDict.TryGetValue(name, out var win);
                if (win != null && win.isModal && go.gameObject.activeInHierarchy)
                {
                    if (win.isClickVoidClose)
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