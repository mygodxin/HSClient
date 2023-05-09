using System;
using System.Collections.Generic;

namespace HS
{
    /// <summary>
    /// �¼�
    /// </summary>
    public class EventTarget
    {
        public readonly Dictionary<string, EventBridge> observerMap = new Dictionary<string, EventBridge>();

        /// <summary>
        /// �ɷ�
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        public void Emit(string name, object data = null)
        {
            if (observerMap.TryGetValue(name, out var observerRef))
            {
                if (observerRef.eventcallback != null)
                    observerRef.eventcallback.Invoke(data);
            }
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="name"></param>
        /// <param name="eventCallback"></param>
        public void On(string name, EventCallback eventCallback)
        {
            if (observerMap.TryGetValue(name, out var observers))
            {
                observers.eventcallback -= eventCallback;
                observers.eventcallback += eventCallback;
            }
            else
            {
                var evt = EventBridge.Get();
                evt.eventcallback -= eventCallback;
                evt.eventcallback += eventCallback;
                observerMap.Add(name, evt);
            }
        }

        /// <summary>
        /// �Ƴ�
        /// </summary>
        /// <param name="name"></param>
        /// <param name="eventCallback"></param>
        public void Off(string name, EventCallback eventCallback)
        {
            if (observerMap.TryGetValue(name, out var observers))
            {
                observers.eventcallback -= eventCallback;
                if (observers.eventcallback == null)
                    EventBridge.Put(observers);
            }
        }

        public void Clear()
        {
            if (observerMap.Count > 0)
            {
                var iter = observerMap.GetEnumerator();
                while (iter.MoveNext())
                {
                    var observer = iter.Current.Value;
                    while (observer.eventcallback != null)
                    {
                        observer.eventcallback -= observer.eventcallback;
                    }
                    EventBridge.Put(observer);
                }
            }
        }

        /// <summary>
        /// �Ƿ�����¼�
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Has(string name)
        {
            observerMap.TryGetValue(name, out var value);
            return value.eventcallback == null;
        }
    }

}