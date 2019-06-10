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
    [Serializable]
    public class Connection
    {
        // Arcweave imported data
        public string id;
        public string label;
        public string sourceElementId;
        public string targetElementId;

        /*
         * Attribute connections
         */
        public void Relink(Project project)
        {
            // Add in connection to element
            Element target = project.GetElement(targetElementId);
            if (target != null) {
                target.AddInConnection(this);
            } else {
                Debug.LogWarning("[Arcweave] Cannot find target element of id " + targetElementId + ".");
            }

            // Add out connection to element
            Element source = project.GetElement(sourceElementId);
            if (source != null) {
                source.AddOutConnection(this);
            } else {
                Debug.LogWarning("[Arcweave] Cannot find source element of id " + sourceElementId + ".");
            }
        }

		/*
		 * Parse the HTML contents and resolve the links.
		 */
		public void ParseHTML(Project project) {
			string linkedBoardId = null;
			label = Utils.ParseHTML(label, ref linkedBoardId);
		}
    } // class Connection
} // namespace AW
