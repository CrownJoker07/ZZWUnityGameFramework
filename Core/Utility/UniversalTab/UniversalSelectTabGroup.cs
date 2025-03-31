using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalSelectTabGroup : MonoBehaviour
{
    protected List<UniversalTab> _universalTabs = new List<UniversalTab>();

    public List<UniversalTab> GetAllUniversalTabs
    {
        get
        {
            if (_isDynamic)
            {
                gameObject.GetComponentsInChildren(true, _universalTabs);
            }

            return _universalTabs;
        }
    }

    private bool _isDynamic;

    public void Init(bool isDynamic = false)
    {
        _isDynamic = isDynamic;
        if (!isDynamic)
        {
            gameObject.GetComponentsInChildren(true, _universalTabs);
        }
    }

    public void Spawn()
    {
        foreach (var universalTab in GetAllUniversalTabs)
        {
            universalTab.Spawn();
        }
    }

    public void DeSpawn()
    {
        foreach (var universalTab in GetAllUniversalTabs)
        {
            universalTab.SetState(UniversalTab.State.UnSelect);
        }

        foreach (var universalTab in GetAllUniversalTabs)
        {
            universalTab.DeSpawn();
        }
    }

    public void SwitchTab(int tabType)
    {
        UniversalTab selectUniversalTab = null;
        foreach (var universalTab in GetAllUniversalTabs)
        {
            if (tabType != universalTab.TabType)
            {
                universalTab.SetState(UniversalTab.State.UnSelect);
            }
            else
            {
                selectUniversalTab = universalTab;
            }
        }

        if (selectUniversalTab == null) return;

        selectUniversalTab.SetState(UniversalTab.State.Select);
    }

    public UniversalTab GetTabByTabType(int tabType)
    {
        foreach (var universalTab in GetAllUniversalTabs)
        {
            if (tabType == universalTab.TabType)
            {
                return universalTab;
            }
        }

        return null;
    }

    public UniversalTab GetCurrentSelectTab()
    {
        foreach (var universalTab in GetAllUniversalTabs)
        {
            if (universalTab.CurrentState == UniversalTab.State.Select)
            {
                return universalTab;
            }
        }

        return null;
    }

    public int GetMaxTabType()
    {
        int tabType = -1;
        foreach (var universalTab in GetAllUniversalTabs)
        {
            if (universalTab.TabType > tabType)
            {
                tabType = universalTab.TabType;
            }
        }

        return tabType;
    }

    public int GetMinTabType()
    {
        int tabType = Int32.MaxValue;
        foreach (var universalTab in GetAllUniversalTabs)
        {
            if (universalTab.TabType < tabType)
            {
                tabType = universalTab.TabType;
            }
        }

        return tabType;
    }
}