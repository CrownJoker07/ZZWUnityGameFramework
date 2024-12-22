using UnityEngine;
using UnityEditor;

namespace RingToss.Editor.Postprocess {
    public class AudioPostprocessor : AssetPostprocessor {
        public AudioPostprocessor() {

        }

        public void OnPreprocessAudio() {
            AudioImporter importer = (AudioImporter) assetImporter;
            importer.forceToMono = true;
        }
    }
}
