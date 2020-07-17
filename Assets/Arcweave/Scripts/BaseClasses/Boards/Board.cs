using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AW
{
    /*
     * A Board in the Arcweave context.
     *
     * Load from JSON and reconstruct the graph from the data there.
     */
    public class Board
        : IBoardEntry
    {
        // Arcweave imported data
        public string[] noteIds;
        public string[] elementIds;
        public string rootElementId;

        // Runtime data
        [NonSerialized] public List<Note> notes;
        [NonSerialized] public List<Element> elements;

        /*
         * Relink notes and elements.
         */
        public void Relink(Project prj)
        {
            notes = new List<Note>();
            for (int i = 0; i < noteIds.Length; i++) {
                Note n = prj.GetNote(noteIds[i]);
                if (n == null) {
                    Debug.LogWarning("[Arcwewave] Cannot find note of given id: " + noteIds[i]);
                    continue;
                }

                notes.Add(n);
            }

            elements = new List<Element>();
            for (int i = 0; i < elementIds.Length; i++) {
                Element e = prj.GetElement(elementIds[i]);
                if (e == null) {
                    Debug.LogWarning("[Arcewave] Cannot find element of given id: " + elementIds[i]);
                    continue;
                }

                elements.Add(e);
                e.ownerBoard = this;
                if (!string.IsNullOrEmpty(e.coverID)) {
                    var asset = prj.GetAsset(e.coverID);
                    if (asset != null) {
                        e.cover = asset.sprite;
                    } else {
                        Debug.LogWarning("Missing asset: " + asset.name);
                    }
                }
            }
        }

        /*
         * Get element by id.
         */
        public Element GetElement(string id)
        {
            for (int eIdx = 0; eIdx < elements.Count; eIdx++) {
                if (elements[eIdx].id == id) {
                    return elements[eIdx];
                }
            }

            return null;
        }
    } // class Board
} // namespace Aw
