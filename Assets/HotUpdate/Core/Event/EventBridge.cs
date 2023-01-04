using System.Collections.Generic;

/// <summary>
/// �¼��ص�
/// </summary>
/// <param name="param"></param>
public delegate void EventCallback(object param = null);

/// <summary>
/// �¼��Ž�
/// </summary>
public class EventBridge
{
    /// <summary>
    /// �ص�����
    /// </summary>
    public EventCallback eventcallback;

    private static Stack<EventBridge> pool = new Stack<EventBridge>();
    internal static EventBridge Get()
    {
        if(pool.Count > 0)
        {
            EventBridge eventCallback = pool.Pop();
            return eventCallback;
        }
        else
        {
            return new EventBridge();
        }
    }
    internal static void Put(EventBridge eventCallback)
    {
        pool.Push(eventCallback);
    }
}
