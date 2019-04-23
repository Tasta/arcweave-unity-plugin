using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AW
{
    /*
     * A component in the Arcweave system.
     */
    public class Component
        : IComponentEntry
    {
        // Arcweave imported data
        public string name { get; protected set; }
        public Sprite image { get; protected set; }
        public Dictionary<int, Attribute> attributes { get; protected set; }

        /*
         * Read component from JSON entry.
         */
        public IEnumerator FromJSON(JSONNode root, string projectPath)
        {
            name = root["name"];

            // Attempt to load the image
            string imgPath = root["image"];
            if (!string.IsNullOrEmpty(imgPath) && imgPath != "null") {
                // Remove extension
                imgPath = System.IO.Path.ChangeExtension(imgPath, null);

                // Load sprite at given path
                string fullPath = projectPath + "assets/" + imgPath;
                ResourceRequest imgReq = Resources.LoadAsync<Sprite>(fullPath);
                yield return imgReq;
                if (imgReq.asset == null) {
                    Debug.LogWarning("Could not load image at path: " + fullPath + " for component " + name);
                } else {
                    image = imgReq.asset as Sprite;
                }
            }

            // Load the attributes
            attributes = new Dictionary<int, Attribute>();
            JSONArray attributeArray = root["attributes"].AsArray;
            for (int i = 0; i < attributeArray.Count; i++) {
                Attribute a = new Attribute();
                a.FromJSON(attributeArray[i]);

                int id = attributeArray[i]["id"].AsInt;
                attributes[id] = a;
            }
        }
    } // class Component
} // namespace AW
