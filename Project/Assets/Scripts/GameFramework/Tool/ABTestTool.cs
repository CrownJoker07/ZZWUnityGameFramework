using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class ABTestGroup
{
    // 实验 ID
    public string experimentID;

    public List<ABTestData> ABTestDataList = new List<ABTestData>();
    
    // 实验内容
    public string content;

    // 应用实验组
    public int? ApplyExperimentGroup = null;

    private int? _tempGroup = null;

    private int HitExperimentGroup()
    {
        int group = 0;
        
        int randomInt = Random.Range(0, 100);

        int total = 0;
        foreach (var abTestData in ABTestDataList)
        {
            total += abTestData.percent;
            
            if (randomInt < total)
            {
                group = abTestData.group;
                break;
            }
        }
        
        PlayerPrefs.SetInt(experimentID, group);

        return group;
    }

    public int GetExperimentGroup()
    {
        if (_tempGroup == null)
        {
            if (!PlayerPrefs.HasKey(experimentID))
            {
                _tempGroup = HitExperimentGroup();
            }
            else
            {
                _tempGroup = PlayerPrefs.GetInt(experimentID);
            } 
        }
        
        return _tempGroup.Value;
    }
}

[Serializable]
public class ABTestData
{
    // 分组, 0: 默认分组, 1: 实验分组1, 2: 实验分组2, 3: 实验分组3，所有实验组加起来的概率不能超过 100%
    public int group;
    // 流量占比
    public int percent;
    // 实验描述
    public string description;
}

public class ABTestTool
{
    private List<ABTestGroup> _ABTestGroupList = new List<ABTestGroup>();
    
    public void InitABTestGroup(List<ABTestGroup> ABTestGroupList)
    {
        _ABTestGroupList = ABTestGroupList;
    }
    
    // 手动应用实验组
    public void ApplyExperimentGroup(string experimentID, int group)
    {
        ABTestGroup abTestGroup = GetABTestGroup(experimentID);

        if (abTestGroup != null)
        {
            abTestGroup.ApplyExperimentGroup = group;
        }
    }

    public int GetExperimentGroup(string experimentID)
    {
        int group = 0;

        ABTestGroup abTestGroup = GetABTestGroup(experimentID);

        // 已经应用的实验
        if (abTestGroup.ApplyExperimentGroup != null)
        {
            group = abTestGroup.ApplyExperimentGroup.Value;
        }
        else
        {
            group = abTestGroup.GetExperimentGroup();
        }

        return group;
    }

    private ABTestGroup GetABTestGroup(string experimentID)
    {
        foreach (var abTestGroup in _ABTestGroupList)
        {
            if (abTestGroup.experimentID == experimentID)
            {
                return abTestGroup;
            }
        }

        return null;
    }
}
