using System.Collections.Generic;
using UnityEngine;

public class ShaderTool
{
#if UNITY_EDITOR
    private static List<string> _defaultShaderList = new List<string>()
    {
        "Legacy Shaders/Diffuse",
        "Hidden/CubeBlur",
        "Hidden/CubeCopy",
        "Hidden/CubeBlend",
        "Sprites/Default",
        "UI/Default",
    };

    public static void SetIncludedShaders(List<string> includedShaderPaths)
    {
        UnityEditor.SerializedObject serializedObject =
            new UnityEditor.SerializedObject(
                UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
        UnityEditor.SerializedProperty serializedProperty = serializedObject.GetIterator();
        
        while (serializedProperty.NextVisible(true))
        {
            if (serializedProperty.name != "m_AlwaysIncludedShaders") continue;
            
            serializedProperty.ClearArray();

            int index = 0;
            for (int i = 0; i < _defaultShaderList.Count; i++)
            {
                serializedProperty.InsertArrayElementAtIndex(index);
                UnityEditor.SerializedProperty dataPoint = serializedProperty.GetArrayElementAtIndex(index);

                string shaderName = _defaultShaderList[i];
                dataPoint.objectReferenceValue = UnityEngine.Shader.Find(shaderName);
                index++;
            }

            for (int i = 0; i < includedShaderPaths.Count; i++)
            {
                serializedProperty.InsertArrayElementAtIndex(index);
                UnityEditor.SerializedProperty dataPoint = serializedProperty.GetArrayElementAtIndex(index);
                
                string shaderPath = includedShaderPaths[i];
                dataPoint.objectReferenceValue = UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);

                index++;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}