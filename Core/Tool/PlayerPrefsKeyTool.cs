using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/*
 * 持久化数据工具 版本: V1.0.0，设计思路：
 * 1. 为了实现完美无缺的存档功能，由于PlayerPrefs 无法直接获取所有 Key 是因其设计限制，因此另外设计一个工具进行所有的 Key
 */
public class PlayerPrefsKeyTool : Singleton<PlayerPrefsKeyTool>
{
    internal const string KeyTag = "PlayerPrefsKeyTool_AllKey";
    private readonly List<string> _keyList;

    public PlayerPrefsKeyTool()
    {
        string json = PlayerPrefs.GetString(KeyTag);
        
        _keyList = JsonConvert.DeserializeObject<List<string>>(json);

        if (_keyList == null)
        {
            _keyList = new List<string>();
        }
    }
    
    public void AddKey(string key)
    {
        if (_keyList.Contains(key))
        {
            return;
        }
        
        _keyList.Add(key);

        Save();
    }

    public List<string> GetAllKey()
    {
        return _keyList;
    }

    private void Save()
    {
        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };
        
        if (!_keyList.Contains(KeyTag))
        {
            _keyList.Add(KeyTag);
        }
        
        // 将数据转换为 JSON 格式
        string json = JsonConvert.SerializeObject(_keyList, jsonSerializerSettings);
        
        PlayerPrefs.SetString(KeyTag, json);
    }
}
