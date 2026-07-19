using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;
using UnityGameFramework.Runtime;

public static class PlayerPrefsUtility
{
    [Serializable]
    public class PlayerPrefsData
    {
        public string type;
        public string content;
    }

    private static Dictionary<string, PlayerPrefsData> GetAllData()
    {
        Dictionary<string, PlayerPrefsData> playerPrefsData = new Dictionary<string, PlayerPrefsData>();

        // 获取所有的键
        List<string> keys = PlayerPrefsKeyTool.Instance.GetAllKey();

        // 获取每个键对应的数据
        foreach (string key in keys)
        {
            if (!PlayerPrefs.HasKey(key)) continue;

            // 根据键的类型获取对应的数据
            int intValue = PlayerPrefs.GetInt(key, int.MinValue);
            if (intValue != int.MinValue)
            {
                playerPrefsData[key] = new PlayerPrefsData()
                {
                    type = "Int",
                    content = intValue.ToString(),
                };
                continue;
            }

            float floatValue = PlayerPrefs.GetFloat(key, float.MinValue);
            if (Math.Abs(floatValue - float.MinValue) > 0.0001f)
            {
                playerPrefsData[key] = new PlayerPrefsData()
                {
                    type = "Float",
                    content = floatValue.ToString(CultureInfo.InvariantCulture),
                };
                continue;
            }

            string stringValue = PlayerPrefs.GetString(key);
            if (!string.IsNullOrEmpty(stringValue))
            {
                if (PlayerPrefsStringEncryptTool.TryDecryptString(stringValue, out string decryptValue))
                {
                    stringValue = decryptValue;
                }

                playerPrefsData[key] = new PlayerPrefsData()
                {
                    type = "String",
                    content = stringValue,
                };

                continue;
            }
        }

        return playerPrefsData;
    }

    // 将游戏数据转为 Json 文件
    public static string GetDataJson()
    {
        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };
        // 将数据转换为 JSON 格式
        string json = JsonConvert.SerializeObject(GetAllData(), jsonSerializerSettings);
        return json;
    }

    public static string GetDataJson_Base64()
    {
        return StringConvertUtility.StringToBase64(GetDataJson());
    }

    public static void LoadDataJson_Base64(string jsonBase64)
    {
        LoadDataJson(StringConvertUtility.Base64ToString(jsonBase64));
    }

    public static void LoadDataJson(string json)
    {
        DeleteAllData();

        Dictionary<string, PlayerPrefsData> datas = JsonConvert.DeserializeObject<Dictionary<string, PlayerPrefsData>>(json);

        foreach (var data in datas)
        {
            string key = data.Key;
            string type = data.Value.type;
            string content = data.Value.content;

            switch (type)
            {
                case "Int":
                {
                    if (int.TryParse(content, out int resultInt))
                    {
                        PlayerPrefs.SetInt(key, resultInt);
                    }
                    break;
                }
                case "Float":
                {
                    if (float.TryParse(content, out float resultFloat))
                    {
                        PlayerPrefs.SetFloat(key, resultFloat);
                    }
                    break;
                }
                default:
                {
                    if (key != PlayerPrefsKeyTool.KeyTag && !PlayerPrefsStringEncryptTool.TryDecryptString(content, out _))
                    {
                        content = PlayerPrefsStringEncryptTool.EncryptString(content);
                    }

                    PlayerPrefs.SetString(key, content);
                    break;
                }
            }
        }

        PlayerPrefs.Save();
    }

    public static void DeleteAllData()
    {
#if UNITY_EDITOR
        int deviceIdSuffix = UnityEditor.EditorPrefs.GetInt("ArchivingTool_DeviceIdSuffix", 0);
#endif

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        try
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            DeleteDirectory(Application.persistentDataPath);
#endif

#if UNITY_ANDROID || UNITY_IOS
            DeleteDirectory(Application.temporaryCachePath);
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete local data: {e}");
        }

#if UNITY_EDITOR
        UnityEditor.EditorPrefs.SetInt("ArchivingTool_DeviceIdSuffix", deviceIdSuffix);
#endif
    }

    private static void DeleteDirectory(string directoryPath)
    {
        if (!System.IO.Directory.Exists(directoryPath)) return;

        System.IO.Directory.Delete(directoryPath, true);
    }
}
