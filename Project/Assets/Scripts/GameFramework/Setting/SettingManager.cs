using UnityEngine;

public class SettingManager : Singleton<SettingManager>
{
    public void Save()
    {
        PlayerPrefs.Save();
    }

    public bool HasSetting(string settingName)
    {
        return PlayerPrefs.HasKey(settingName);
    }

    public void RemoveSetting(string settingName)
    {
        PlayerPrefs.DeleteKey(settingName);
    }

    public void RemoveAllSettings()
    {
        PlayerPrefs.DeleteAll();
    }

    public int GetInt(string settingName, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(settingName, defaultValue);
    }

    public void SetInt(string settingName, int value)
    {
        PlayerPrefs.SetInt(settingName, value);
    }

    public float GetFloat(string settingName, float defaultValue = 0)
    {
        return PlayerPrefs.GetFloat(settingName, defaultValue);
    }

    public void SetFloat(string settingName, float value)
    {
        PlayerPrefs.SetFloat(settingName, value);
    }

    public string GetString(string settingName, string defaultValue = null)
    {
        return PlayerPrefs.GetString(settingName, defaultValue);
    }

    public void SetString(string settingName, string value)
    {
        PlayerPrefs.SetString(settingName, value);
    }
    
    public bool GetBool(string settingName, bool defaultValue = false)
    {
        return GetInt(settingName, defaultValue ? 1 : 0) != 0;
    }
    
    public void SetBool(string settingName, bool value)
    {
        SetInt(settingName, value ? 1 : 0);
    }
}