using UnityEngine;
using UnityEditor;

public class AudioPostprocessor : AssetPostprocessor {
    public void OnPreprocessAudio() {
        AudioImporter importer = (AudioImporter) assetImporter;
        importer.forceToMono = true;
    }
}
