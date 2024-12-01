using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : Singleton<EventManager>
{
    public delegate void EventDelegate(params object[] objs);

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

    public void NotifyEvent(string eventName, params object[] objs)
    {
        if (!_eventDelegateDictionary.TryGetValue(eventName, out List<EventDelegate> eventDelegateList)) return;
        
        foreach (var eventDelegate in eventDelegateList)
        {
            try
            {
                eventDelegate.Invoke(objs);
            }
            catch (Exception e)
            {
                Debug.LogError($"Event: {eventName} have exception, some eventDelegate error : {e.Message}");
            }
        }
    }
}