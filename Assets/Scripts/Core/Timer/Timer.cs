using System.Collections.Generic;
using UnityEngine;

namespace HS
{
    public delegate void TimerCallback();
    /// <summary>
    /// ��ʱ��
    /// </summary>
    public class Timer
    {
        Dictionary<TimerCallback, TimerNode> curTimers;
        Dictionary<TimerCallback, TimerNode> waitAdd;
        List<TimerNode> waitRemove;
        private static Timer _inst = null;
        public static Timer Inst
        {
            get
            {
                if (_inst == null)
                    _inst = new Timer();
                return _inst;
            }
        }

        public Timer()
        {
            curTimers = new Dictionary<TimerCallback, TimerNode>();
            waitAdd = new Dictionary<TimerCallback, TimerNode>();
            waitRemove = new List<TimerNode>();

            var _gameObject = new GameObject();
            _gameObject.hideFlags = HideFlags.HideInHierarchy;
            _gameObject.SetActive(true);
            Object.DontDestroyOnLoad(_gameObject);
            _gameObject.AddComponent<TimerMono>();
        }
        /// <summary>
        /// ���
        /// </summary>
        /// <param name="interval">���(s)</param>
        /// <param name="repeat">ѭ������</param>
        /// <param name="callback">�ص�����</param>
        public void Add(float interval, int repeat, TimerCallback callback)
        {
            Add(interval, repeat, callback, null);
        }
        /// <summary>
        /// ��Ӽ�ʱ��
        /// </summary>
        /// <param name="interval">���(s)</param>
        /// <param name="repeat">ѭ������</param>
        /// <param name="callback">�ص�����</param>
        /// <param name="objects">�ص�����</param>
        public void Add(float interval, int repeat, TimerCallback callback, params object[] objects)
        {
            if (callback == null)
            {
                return;
            }

            TimerNode t;

            if (curTimers.TryGetValue(callback, out t))
            {
                t.Set(interval, repeat, callback, objects);
                t.elapsed = 0;
                t.deleted = false;
                return;
            }

            if (waitAdd.TryGetValue(callback, out t))
            {
                t.Set(interval, repeat, callback, objects);
                return;
            }

            t = new TimerNode();
            t.interval = interval;
            t.repeat = repeat;
            t.callback = callback;
            t.param = objects;
            waitAdd.Add(callback, t);
        }
        /// <summary>
        /// �Ƿ���ڸü�ʱ���ص�
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool IsExist(TimerCallback callback)
        {
            if (waitAdd.ContainsKey(callback))
            {
                return true;
            }

            if (curTimers.TryGetValue(callback, out var tn))
            {
                return !tn.deleted;
            }

            return false;
        }
        /// <summary>
        /// �Ƴ���ʱ��
        /// </summary>
        /// <param name="callback"></param>
        public void Remove(TimerCallback callback)
        {
            TimerNode t;
            if (waitAdd.TryGetValue(callback, out t))
            {
                waitAdd.Remove(callback);
            }

            if (curTimers.TryGetValue(callback, out t))
            {
                t.deleted = true;
            }
        }
        /// <summary>
        /// ��ʱ��ˢ�£���monobehaviour��update�е���
        /// </summary>
        public void Update()
        {
            float dt = Time.unscaledDeltaTime;
            Dictionary<TimerCallback, TimerNode>.Enumerator iter;

            if (curTimers.Count > 0)
            {
                iter = curTimers.GetEnumerator();
                while (iter.MoveNext())
                {
                    TimerNode i = iter.Current.Value;

                    if (i.deleted)
                    {
                        waitRemove.Add(i);
                        continue;
                    }

                    i.elapsed += dt;
                    if (i.elapsed < i.interval)
                        continue;

                    i.elapsed -= i.interval;
                    if (i.elapsed < 0 || i.elapsed > 0.03f)
                        i.elapsed = 0;

                    if (i.repeat > 0)
                    {
                        i.repeat--;
                        if (i.repeat == 0)
                        {
                            i.deleted = true;
                            waitRemove.Add(i);
                        }
                    }

                    if (i.callback != null)
                    {
                        //i.callback(i.param);
                        i.callback.Invoke();
                    }
                }
                iter.Dispose();
            }

            int removeLen = waitRemove.Count;
            if (removeLen > 0)
            {
                for (int i = 0; i < removeLen; i++)
                {
                    TimerNode node = waitRemove[i];
                    if (node.deleted && node.callback != null)
                    {
                        curTimers.Remove(node.callback);
                    }
                }
                waitRemove.Clear();
            }
            if (waitAdd.Count > 0)
            {
                iter = waitAdd.GetEnumerator();
                while (iter.MoveNext())
                {
                    curTimers.Add(iter.Current.Key, iter.Current.Value);
                }
                iter.Dispose();
                waitAdd.Clear();
            }
        }
    }

    class TimerNode
    {
        public int repeat;
        public float interval;
        public float elapsed;
        public bool deleted;

        public TimerCallback callback;
        public object[] param;

        public void Set(float interval, int repeat, TimerCallback callback, params object[] objects)
        {
            this.interval = interval;
            this.repeat = repeat;
            this.callback = callback;
            this.param = objects;
        }
    }

    class TimerMono : MonoBehaviour
    {
        private void Update()
        {
            Timer.Inst.Update();
        }
    }

}