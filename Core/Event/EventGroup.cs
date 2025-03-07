using System.Collections.Generic;
using UnityEngine;

public class EventGroup
{
    private readonly Dictionary<int, List<EventManager.EventDelegate>> _eventDelegateDictionary =
        new Dictionary<int, List<EventManager.EventDelegate>>();

    /// <summary>
    /// 添加一个监听
    /// </summary>
    public void AddListener(int eventID, EventManager.EventDelegate eventDelegate)
    {
        if (!_eventDelegateDictionary.TryGetValue(eventID, out List<EventManager.EventDelegate> eventDelegateList))
        {
            eventDelegateList = new List<EventManager.EventDelegate>();
            _eventDelegateDictionary.Add(eventID, eventDelegateList);
        }

        if (eventDelegateList.Contains(eventDelegate))
        {
            Debug.LogError("Event: " + eventID + " is already added");
            return;
        }
        else
        {
            eventDelegateList.Add(eventDelegate);
        }
            
        EventManager.Instance.AddListener(eventID, eventDelegate);
    }

    /// <summary>
    /// 移除所有缓存的监听
    /// </summary>
    public void RemoveAllListener()
    {
        foreach (var eventDelegateKeyValue in _eventDelegateDictionary)
        {
            foreach (var eventDelegate in eventDelegateKeyValue.Value)
            {
                EventManager.Instance.RemoveListener(eventDelegateKeyValue.Key, eventDelegate);
            }
        }
        
        _eventDelegateDictionary.Clear();
    } 
}