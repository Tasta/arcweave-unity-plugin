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
        public string label;
        public int sourceElementIdx;
        public int targetElementIdx;

        /*
         * Attribute connections
         */
        public void AttributeConnections(Project project)
        {
            // Add in connection to element
            Element target = project.GetElement(targetElementIdx);
            if (target != null) {
                target.AddInConnection(this);
            } else {
                Debug.LogWarning("[Arcweave] Cannot find target element of id " + targetElementIdx + ".");
            }

            // Add out connection to element
            Element source = project.GetElement(sourceElementIdx);
            if (source != null) {
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
