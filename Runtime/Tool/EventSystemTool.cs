using UnityEngine.EventSystems;

public static class EventSystemTool
{
    private static EventSystem _disableEventSystem = null;
        
    public static void EnableEventSystem()
    {
        if (_disableEventSystem != null)
        {
            _disableEventSystem.enabled = true;
            _disableEventSystem = null;
        }
    }

    public static void DisableEventSystem()
    {
        // 已经关闭点击事件
        if(_disableEventSystem != null) return;
            
        _disableEventSystem = EventSystem.current;
        _disableEventSystem.enabled = false;
    }
}
