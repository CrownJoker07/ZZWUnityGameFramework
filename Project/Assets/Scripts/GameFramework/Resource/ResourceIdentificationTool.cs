using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;

public partial class ResourceIdentificationTool : Singleton<ResourceIdentificationTool>
{
    private static List<string> paths = new List<string>()
    {
        "Assets/GameAsset",
    };
    
    private const string ResourceIdentificationTypeScriptPath =
        "Assets/Scripts/Definition/ResourceIdentificationType.cs";

    private const string XMLFilePath = "Assets/GameAsset/Configs/ResourcesIdentification.xml"; // XML文件路径
    
    private struct ResourceIdentificationInfo
    {
        public int AssetID;
        public string AssetName;
        public string AssetPath;
    }
    
    private Dictionary<int, ResourceIdentificationInfo> _resourceIdentificationInfoMaps =
        new Dictionary<int, ResourceIdentificationInfo>();

    private Action _initSuccessEvent;

    public void InitResourceIdentificationInfos(Action initSuccessEvent)
    {
        _initSuccessEvent = initSuccessEvent;
        _resourceIdentificationInfoMaps.Clear();
        
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(XMLFilePath);
            LoadAssetSuccessCallback(textAsset);
            return;
        }
#endif
        
        ResourceManager.instance.LoadAssetAsync<TextAsset>(XMLFilePath, LoadAssetSuccessCallback);
    }

    private void LoadAssetSuccessCallback(UnityEngine.Object asset)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(((TextAsset)asset).text);
        
        // 读取XML文件中的数据
        XmlNodeList selectNodes = xmlDoc.SelectNodes("//Resource"); // 替换为你的XML文件中的节点路径

        if (selectNodes != null)
        {
            foreach (XmlNode node in selectNodes)
            {
                ResourceIdentificationInfo resourceIdentificationInfo = new ResourceIdentificationInfo()
                {
                    AssetID = int.Parse(node.Attributes["AssetID"].Value),
                    AssetName = node.Attributes["AssetName"].Value,
                    AssetPath = node.Attributes["AssetPath"].Value,
                };

                _resourceIdentificationInfoMaps[resourceIdentificationInfo.AssetID] = resourceIdentificationInfo;
            }
        } 
        
        _initSuccessEvent?.Invoke();
        _initSuccessEvent = null;
        
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }
#endif
        
        ResourceManager.instance.UnloadAsset(asset);
    }

    public string GetAssetPathById(int resourceIdentificationTypeId)
    {
        if (_resourceIdentificationInfoMaps.TryGetValue((int)resourceIdentificationTypeId,
                out ResourceIdentificationInfo resourceIdentificationInfo))
        {
            return resourceIdentificationInfo.AssetPath;
        }
        else
        {
            Debug.LogError($"No resource identification type found with id: {resourceIdentificationTypeId}");
        }

        return String.Empty;
    }
}

#if UNITY_EDITOR
public partial class ResourceIdentificationTool
{
    private const string ResourceIdentificationTypeScriptTemplate =
        @"public enum ResourceIdentificationType
{
**Content**
}";
    
    private static Dictionary<int, ResourceIdentificationInfo> _resourceIdentificationInfoMaps_Static =
        new Dictionary<int, ResourceIdentificationInfo>();

    [UnityEditor.MenuItem("Tools/ResourceTool/InitResourcesIdentification")]
    public static void InitResourceIdentificationInfoMapInProject()
    {
        List<FileInfo> assetsFile = new List<FileInfo>();
        foreach (var path in paths)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            assetsFile.AddRange(directoryInfo.GetFiles("*#*", SearchOption.AllDirectories).ToList());
        }

        _resourceIdentificationInfoMaps_Static.Clear();

        // 遍历所有资源并检查名称中是否包含"#"符号
        foreach (FileInfo prefabfile in assetsFile)
        {
            if(prefabfile.Name.Contains(".meta")) continue;
            if (!prefabfile.Name.Contains("#")) continue;

            string fileName = prefabfile.Name;

            int assetID = int.Parse(GetNumbersFromString(fileName));
            string assetName = GetFileNameLettersWithoutExtensionInPath(fileName);
            string assetPath = GetPathInAsset(prefabfile.FullName);

            if (_resourceIdentificationInfoMaps_Static.ContainsKey(assetID))
            {
                Debug.LogError(
                    "ZZW" + "ResourceIdentificationTool日志" + ":" +
                    $"assetID:{assetID}, assetPath:{assetPath}, 存在相同ID，请重新设置资源ID",
                    UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath));
                return;
            }

            ResourceIdentificationInfo resourceIdentificationInfo = new ResourceIdentificationInfo()
            {
                AssetID = assetID,
                AssetName = assetName,
                AssetPath = assetPath,
            };

            _resourceIdentificationInfoMaps_Static[resourceIdentificationInfo.AssetID] = resourceIdentificationInfo;
        }

        // 按键升序排序
        _resourceIdentificationInfoMaps_Static =
            _resourceIdentificationInfoMaps_Static.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

        InitXMLFile();
        InitResourceIdentificationType();

        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
    }

    public static bool CheckIsNeedInitResourceIdentificationInfo(string assetPath)
    {
        if (!assetPath.Contains("#"))
        {
            return false;
        }
        
        foreach (var path in paths)
        {
            if (assetPath.Contains(path))
            {
                return true;
            }
        }
        
        return false;
    }

    private static void InitXMLFile()
    {
        // 创建XML文档
        XmlDocument xmlDoc = new XmlDocument();

        XmlElement rootElement = xmlDoc.CreateElement("ResourcesIdentification");
        xmlDoc.AppendChild(rootElement);

        foreach (var resourceIdentificationInfoMap in _resourceIdentificationInfoMaps_Static)
        {
            ResourceIdentificationInfo resourceIdentificationInfo = resourceIdentificationInfoMap.Value;
            // 创建XML元素并设置属性
            XmlElement resourceElement = xmlDoc.CreateElement("Resource");
            resourceElement.SetAttribute("AssetID", resourceIdentificationInfo.AssetID.ToString());
            resourceElement.SetAttribute("AssetName", resourceIdentificationInfo.AssetName);
            resourceElement.SetAttribute("AssetPath", resourceIdentificationInfo.AssetPath);

            // 将资源元素添加到根元素
            rootElement.AppendChild(resourceElement);
        }

        // 保存XML文档到文件
        xmlDoc.Save(XMLFilePath);
    }

    private static void InitResourceIdentificationType()
    {
        string enumType = string.Empty;

        int index = 0;
        foreach (var resourceIdentificationInfoMap in _resourceIdentificationInfoMaps_Static)
        {
            ResourceIdentificationInfo resourceIdentificationInfo = resourceIdentificationInfoMap.Value;
            enumType +=
                $"        {resourceIdentificationInfo.AssetName + resourceIdentificationInfo.AssetID} = {resourceIdentificationInfo.AssetID},";

            index++;
            if (index < _resourceIdentificationInfoMaps_Static.Count)
            {
                enumType += "\n";
            }
        }

        string temScript = ResourceIdentificationTypeScriptTemplate;
        temScript = temScript.Replace("**Content**", enumType);
        File.WriteAllText($"{ResourceIdentificationTypeScriptPath}", temScript,
            Encoding.UTF8);
    }

    public static string GetNumbersFromString(string input)
    {
        // 使用正则表达式匹配所有数字
        Regex regex = new Regex(@"#(\d+)");
        MatchCollection matches = regex.Matches(input);

        // 将匹配到的数字拼接成一个字符串
        string result = "";
        foreach (Match match in matches)
        {
            result += match.Groups[1].Value;
        }

        return result;
    }

    private static string GetFileNameLettersWithoutExtensionInPath(string filePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string letters = Regex.Replace(fileName, @"[^a-zA-Z]", "");
        return letters;
    }

    private static string GetPathInAsset(string fullPath)
    {
        string assetPath = fullPath.Replace("\\", "/");
        assetPath = assetPath.Substring(assetPath.IndexOf("Assets", StringComparison.Ordinal));

        return assetPath;
    }

    public static int GetNotSameAssetIDByRange(int startIndex)
    {
        instance.InitResourceIdentificationInfos(() =>
        {

        });
        
        int temAssetID = startIndex + 1;
        foreach (var resourceIdentificationInfo in instance._resourceIdentificationInfoMaps)
        {
            ResourceIdentificationInfo info = resourceIdentificationInfo.Value;
            int assetID = info.AssetID;

            if (assetID < startIndex + 1)
            {
                continue;
            }

            if (temAssetID != assetID)
            {
                return temAssetID;
            }

            temAssetID++;
        }

        return temAssetID;
    }
}
#endif