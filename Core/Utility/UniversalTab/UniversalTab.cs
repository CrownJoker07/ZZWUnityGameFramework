using System;
using UnityEngine;
using UnityEngine.UI;


public abstract class UniversalTab : MonoBehaviour
{
    public enum State
    {
        None = 0,
        Select = 1,
        UnSelect = 2,
    }

    private UniversalSelectTabGroup _universalSelectTabGroup;
    private int _tabType;
    public int TabType => _tabType;

    private Button _button;
    private State _currentState;
    public State CurrentState => _currentState;
    private Action<int> _selectAction;
    private Action<int> _unSelectAction;
    private IUniversalISelectTabPanel _universalISelectTabPanel;
    public IUniversalISelectTabPanel UniversalISelectTabPanel => _universalISelectTabPanel;

    protected virtual void Awake()
    {
        _button = GetOrAddComponent<Button>(gameObject);
        _button.onClick.AddListener(Click_Button);
    }

    public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }

    public virtual void Init(UniversalSelectTabGroup albumSelectTabGroup, int temTabType,
        IUniversalISelectTabPanel albumSelectTabPanel, Action<int> selectAction = null,
        Action<int> unSelectAction = null)
    {
        _universalSelectTabGroup = albumSelectTabGroup;
        _tabType = temTabType;
        _universalISelectTabPanel = albumSelectTabPanel;
        _currentState = State.None;

        _selectAction = selectAction;
        _unSelectAction = unSelectAction;
    }

    public virtual void Spawn()
    {
    }

    public virtual void DeSpawn()
    {
    }

    public virtual void Click_Button()
    {
        _universalSelectTabGroup.SwitchTab(_tabType);
    }

    public void SetState(State state)
    {
        if (_currentState == state) return;

        _currentState = state;

        switch (state)
        {
            case State.Select:
            {
                SelectEvent();
                break;
            }
            case State.UnSelect:
            {
                UnSelectEvent();
                break;
            }
        }
    }

    protected virtual void SelectEvent()
    {
        _selectAction?.Invoke(_tabType);
        _button.enabled = false;
        _universalISelectTabPanel?.Select(_tabType);
    }

    protected virtual void UnSelectEvent()
    {
        _unSelectAction?.Invoke(_tabType);
        _button.enabled = true;
        _universalISelectTabPanel?.UnSelect(_tabType);
    }
}