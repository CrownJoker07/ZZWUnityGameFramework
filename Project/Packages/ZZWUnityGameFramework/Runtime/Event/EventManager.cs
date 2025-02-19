using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * EventManager 版本: V1.0.0，设计思路：
 * 1. 通过通知的形式，解耦合
 * 2. 只实现添加、移除、通知三种API，逻辑简单，便于理解和后续修改
 * 3. 事件传递参数使用C#原生EventArgs，特殊对象传递通过继承来进行定制，比params object[]这种通过封包解包的形式性能好，且更易理解
 */
public class EventManager : Singleton<EventManager>
{
    public delegate void EventDelegate(EventArgs eventArgs = null);

    private readonly Dictionary<string, List<EventDelegate>> _eventDelegateDictionary =
        new Dictionary<string, List<EventDelegate>>();
    
    public void AddListener(string eventName, EventDelegate eventDelegate)
    {
        if (!_eventDelegateDictionary.TryGetValue(eventName, out List<EventDelegate> eventDelegateList))
        {
            eventDelegateList = new List<EventDelegate>();
            _eventDelegateDictionary.Add(eventName, eventDelegateList);
        }

        if (eventDelegateList.Contains(eventDelegate))
        {
            Debug.LogError("Event: " + eventName + " is already added");
        }
        else
        {
            eventDelegateList.Add(eventDelegate);
        }
    }

    public void RemoveListener(string eventName, EventDelegate eventDelegate)
    {
        if (!_eventDelegateDictionary.TryGetValue(eventName, out List<EventDelegate> eventDelegateList))
        {
            Debug.LogError("Event: " + eventName + " is not added");
            return;
        }

        if (!eventDelegateList.Contains(eventDelegate))
        {
            Debug.LogError("Event: " + eventName + " is not added this eventDelegate");
            return;
        }
        
        eventDelegateList.Remove(eventDelegate);
    }

    public void NotifyEvent(string eventName, EventArgs eventArgs = null)
    {
        if (!_eventDelegateDictionary.TryGetValue(eventName, out List<EventDelegate> eventDelegateList)) return;
        
        foreach (var eventDelegate in eventDelegateList)
        {
            try
            {
                eventDelegate.Invoke(eventArgs);
            }
            catch (Exception e)
            {
                Debug.LogError($"Event: {eventName} have exception, some eventDelegate error : {e.Message}");
            }
        }
    }
}