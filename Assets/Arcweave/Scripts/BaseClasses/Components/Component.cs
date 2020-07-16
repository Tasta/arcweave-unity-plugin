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
    [Serializable]
    public class Component
        : IComponentEntry
    {
        // Arcweave imported data
        public string[] attributeIDs;
        public string coverID;

        // Populate upon relink
        [NonSerialized] public List<Attribute> attributes;
        [NonSerialized] public Sprite cover;

        /*
         * Relink attributes
         */
        public void RelinkAttributes(Project prj)
        {
            attributes = new List<Attribute>();
            for (int i = 0; i < attributeIDs.Length; i++) {
                Attribute a = prj.GetAttribute(attributeIDs[i]);
                if (a == null) {
                    Debug.LogWarning("[Arcweave] Cannot find attribute of id: " + attributeIDs[i]);
                    continue;
                }

                attributes.Add(a);
            }
        }
    } // class Component
} // namespace AW
