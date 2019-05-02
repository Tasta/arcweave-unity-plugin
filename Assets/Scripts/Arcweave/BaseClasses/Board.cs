using SimpleJSON;
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
        public string name { get; protected set; }
        public Note[] notes { get; protected set; }
        public Element[] elements { get; protected set; }

        // Runtime data
        public Element root { get; protected set; }

        /*
         * Find the root of this board.
         */
        public void ComputeRoot()
        {
			// Reset current root
			root = null;

			// Find first root
            for (int i = 0; i < elements.Length; i++) {
                if (elements[i].inConnections.Count == 0) {
                    if (root != null) {
                        Debug.LogWarning("[Arcweave] More than one root that could be roots found in board: " + name);
                    } else {
                        root = elements[i];
                    }
                }
            }
        }

        /*
         * Read this folder.
         */
        public void FromJSON(JSONNode node, Project project)
        {
            name = node["name"];

            // Notes
            JSONArray noteArray = node["notes"].AsArray;
            notes = new Note[noteArray.Count];
            for (int i = 0; i < noteArray.Count; i++) {
                string noteID = noteArray[i].Value;

                if (project.notes.ContainsKey(noteID)) {
                    notes[i] = project.notes[noteID];
                } else {
                    Debug.LogWarning("[Arcweave] Cannot find note " + noteID + " for board " + name + ".");
                }
            }

            // Elements
            JSONArray elementArray = node["elements"].AsArray;
            elements = new Element[elementArray.Count];
            for (int i = 0; i < elementArray.Count; i++) {
                int elementId = elementArray[i].AsInt;

                if (project.elements.ContainsKey(elementId)) {
                    elements[i] = project.elements[elementId];
                } else {
                    Debug.LogWarning("[Arcweave] Cannot find element " + elementId + " for board " + name + ".");
                }
            }
        }
    } // class Board
} // namespace Aw
