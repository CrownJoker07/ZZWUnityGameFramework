using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : Singleton<EventManager>
{
    public delegate void EventDelegate(EventArgs eventArgs);

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

    public void NotifyEvent(string eventName, EventArgs eventArgs)
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