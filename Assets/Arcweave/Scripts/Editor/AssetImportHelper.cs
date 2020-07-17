using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace AW
{
    /*
     * An utility to force project textures to import as Sprites.
     */
    public static class AssetImportHelper
    {
        public static void SetupSprites(string prjPath) {
            string[] allGUIDs = AssetDatabase.FindAssets("t:Texture2D");
            for (int i = 0; i < allGUIDs.Length; i++) {
                string assetPath = AssetDatabase.GUIDToAssetPath(allGUIDs[i]);
                if (!assetPath.Contains(prjPath))
                    continue;

                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                importer.textureType = TextureImporterType.Sprite;
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }
        }
    } // class AssetImportHelper
} // namespace
