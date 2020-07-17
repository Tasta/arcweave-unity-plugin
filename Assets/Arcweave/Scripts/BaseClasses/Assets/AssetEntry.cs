using System;
using UnityEditor;
using UnityEngine;

namespace AW {
    /*
     * An Asset Entry in the Arcweave project.
     * At the moment, it just references images.
     */
    [Serializable]
    public class AssetEntry
        : IAssetEntry
    {
        /*
         * Types of assets.
         */
        public enum Type
        {
            CoverImage,
            TemplateImage,
            Icon,
        } // enum Type

        // Type of this asset
        public Type type;

        // Link to references sprite
        public Sprite sprite;

        /*
         * Parse type from JSON string.
         */
        public void ParseType(string typeStr) {
            switch (typeStr) {
                case "template-image":
                    type = Type.TemplateImage;
                    break;
                case "cover":
                    type = Type.CoverImage;
                    break;
                case "icon":
                    type = Type.Icon;
                    break;
                default:
                    Debug.LogWarning("Unhandled Asset Type in project: " + typeStr);
                    break;
            }
        }

        /*
         * Load resource using given project base path.
         */
        public void LinkAsset(string projectBasePath) {
            string fullPath = projectBasePath + "/assets/" + name;
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
            if (sprite == null) {
                Debug.LogWarning("Cannot load asset at path: " + fullPath);
            }
        }
    } // class AssetEntry
} // namespace AW
