using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AW
{
    /*
     * A connection between two elements in the Arcweave context.
     */
    public class Connection
    {
        // Arcweave imported data
        public string label { get; protected set; }
        public int sourceElementIdx { get; protected set; }
        public int targetElementIdx { get; protected set; }

        /*
         * Load from json.
         */
        public void FromJSON(JSONNode root, Project project)
        {
            // Read data
            label = root["label"];

			// Read source & target
            sourceElementIdx = root["sourceid"].AsInt;
            targetElementIdx = root["targetid"].AsInt;

            // Add in connection to element
            if (project.elements.ContainsKey(targetElementIdx)) {
                Element target = project.elements[targetElementIdx];
                target.AddInConnection(this);
            } else {
                Debug.LogWarning("[Arcweave] Cannot find target element of id " + targetElementIdx + ".");
            }

            // Add out connection to element
            if (project.elements.ContainsKey(sourceElementIdx)) {
                Element source = project.elements[sourceElementIdx];
                source.AddOutConnection(this);
            } else {
                Debug.LogWarning("[Arcweave] Cannot find source element of id " + sourceElementIdx + ".");
            }
        }

		/*
		 * Parse the HTML contents and resolve the links.
		 */
		public void ParseHTML(Project project) {
			int linkedBoardId = -1;
			label = Utils.ParseHTML(label, ref linkedBoardId);
		}
    } // class Connection
} // namespace AW
