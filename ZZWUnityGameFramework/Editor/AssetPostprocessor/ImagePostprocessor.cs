using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;

public class ImagePostprocessor : AssetPostprocessor
{
    private const string IgnoreTag = "_Ignore";

    const string platformStandalone = "Standalone";
    const string PlatformIphone = "iPhone";
    const string PlatformAndroid = "Android";

    private static readonly List<int> SizeList = new List<int>() { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048 };

    private List<string> _spinePath = new List<string>()
    {
        "Assets/RawAsset/Spine",
    };

    private List<string> _spritePath = new List<string>()
    {
        "Assets/RawAsset/Sprite",
    };

    void OnPreprocessTexture()
    {
        if (assetPath.Contains(IgnoreTag)) return;

        if (ContainAssetPath(assetPath, _spritePath))
        {
            ProcessSprite(assetPath);
        }
        else if (ContainAssetPath(assetPath, _spinePath))
        {
            ProcessNormalTexture(assetPath);
        }
        else if (assetPath.Contains("Assets/RawAsset"))
        {
            ProcessNormalTexture(assetPath, (maxSize) => { return Mathf.Min(maxSize, 512); });
        }
    }

    void OnPostprocessTexture(Texture2D texture)
    {
    }

    private bool ContainAssetPath(string assetPath, List<string> curFormat)
    {
        bool contain = false;
        foreach (var item in curFormat)
        {
            contain = assetPath.Contains(item);
            if (contain) return true;
        }

        return false;
    }

    private static int AdapterSize(int sorcesize)
    {
        if (sorcesize < SizeList[0]) return SizeList[0];
        if (sorcesize > SizeList[SizeList.Count - 1]) return SizeList[SizeList.Count - 1];

        for (int i = 0; i < SizeList.Count; i++)
        {
            int cursize = SizeList[i];

            if (sorcesize <= cursize)
            {
                //find first
                float average = (cursize * 0.3f + SizeList[i - 1] * 0.7f);

                if (sorcesize >= average)
                {
                    return cursize;
                }
                else
                {
                    return SizeList[i - 1];
                }
                // return cursize;
            }
        }

        return SizeList[SizeList.Count - 1];
    }

    private static TextureImporterPlatformSettings TextureImportSettings(string platform, int maxTextSize,
        TextureImporterFormat platformFormat)
    {
        TextureImporterPlatformSettings tips = new TextureImporterPlatformSettings();
        tips.maxTextureSize = maxTextSize;
        tips.textureCompression = TextureImporterCompression.Compressed;
        tips.format = platformFormat;
        tips.name = platform;
        tips.overridden = true;
        return tips;
    }

    private void ProcessSprite(string assetPath) //, Texture2D texture
    {
        TextureImporter textureImport = (TextureImporter)assetImporter;
        object[] args = new object[2] { 0, 0 };
        MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        mi.Invoke(textureImport, args);

        int maxSize = Mathf.Max((int)args[0], (int)args[1]);
        int newSize = AdapterSize(maxSize);
        bool sourceHasAlpha = textureImport.DoesSourceTextureHaveAlpha();

        TextureImporterFormat formatTexture = TextureImporterFormat.ASTC_4x4;
        if (newSize <= 128 || assetPath.Contains("_HD"))
        {
            formatTexture = TextureImporterFormat.RGBA32;
        }

        if (assetPath.Contains("_5x5"))
        {
            formatTexture = TextureImporterFormat.ASTC_5x5;
        }

        if (assetPath.Contains("_6x6"))
        {
            formatTexture = TextureImporterFormat.ASTC_6x6;
        }

        TextureImporterPlatformSettings androidPlatNew = TextureImportSettings(PlatformAndroid, newSize, formatTexture);
        TextureImporterPlatformSettings iOSPlatNew = TextureImportSettings(PlatformIphone, newSize, formatTexture);
        TextureImporterPlatformSettings pcPlatNew =
            TextureImportSettings(platformStandalone, newSize, TextureImporterFormat.DXT5);


        textureImport.SetPlatformTextureSettings(androidPlatNew);
        textureImport.SetPlatformTextureSettings(iOSPlatNew);
        textureImport.SetPlatformTextureSettings(pcPlatNew);

        textureImport.mipmapEnabled = false;
        textureImport.isReadable = false;
        textureImport.textureType = TextureImporterType.Sprite;
    }


    private void ProcessNormalTexture(string assetPath, Func<int, int> customMaxSizeFunc = null,
        Func<TextureImporterFormat> customFormatFunc = null) //, Texture2D texture
    {
        TextureImporter textureImport = (TextureImporter)assetImporter;

        object[] args = new object[2] { 0, 0 };
        MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        mi.Invoke(textureImport, args);

        int maxSize = Mathf.Max((int)args[0], (int)args[1]);

        int newSize = 0;
        if (customMaxSizeFunc != null)
        {
            newSize = customMaxSizeFunc.Invoke(maxSize);
        }
        else
        {
            newSize = AdapterSize(maxSize);
        }

        bool sourceHasAlpha = textureImport.DoesSourceTextureHaveAlpha();

        TextureImporterFormat formatTexture = TextureImporterFormat.ASTC_5x5;

        if (customFormatFunc != null)
        {
            formatTexture = customFormatFunc.Invoke();
        }
        else
        {
            if (newSize <= 128 || assetPath.Contains("_HD") ||
                textureImport.textureType == TextureImporterType.NormalMap)
            {
                formatTexture = TextureImporterFormat.ASTC_4x4;
            }
            else
            {
                formatTexture = sourceHasAlpha ? TextureImporterFormat.ASTC_5x5 : TextureImporterFormat.ASTC_8x8;
            }
        }

        TextureImporterPlatformSettings androidPlatNew = TextureImportSettings(PlatformAndroid, newSize, formatTexture);
        TextureImporterPlatformSettings iOSPlatNew = TextureImportSettings(PlatformIphone, newSize, formatTexture);
        TextureImporterPlatformSettings pcPlatNew = TextureImportSettings(platformStandalone, newSize,
            sourceHasAlpha ? TextureImporterFormat.DXT5 : TextureImporterFormat.DXT1);

        textureImport.SetPlatformTextureSettings(androidPlatNew);
        textureImport.SetPlatformTextureSettings(iOSPlatNew);
        textureImport.SetPlatformTextureSettings(pcPlatNew);

        textureImport.mipmapEnabled = false;
        textureImport.isReadable = false;
        // textureImport.textureType = TextureImporterType.Default;
        textureImport.SaveAndReimport();
    }
}