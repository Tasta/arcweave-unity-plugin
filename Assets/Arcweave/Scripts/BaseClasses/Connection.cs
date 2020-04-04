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
        public string labelNoStyle;
        public string sourceElementId;
        public string targetElementId;

        /*
         * Get source/destination element.
         * Handles jumpers.
         */
        public Element Get(bool destination, Project project) {
            string id = destination ? targetElementId : sourceElementId;

            // Search for Jumper first (they're fewer)
            Jumper jumper = project.GetJumper(id);
            if (jumper != null) {
                // ToDo: In case one day we will care about board changes, trigger it here.
                return project.GetElement(jumper.elementID);
            } else {
                return project.GetElement(id);
            }
        }

        /*
         * Attribute connections
         */
        public void Relink(Project project)
        {
            // Add in connection to element
            Element target = Get(true, project);
            if (target != null) {
                target.AddInConnection(this);
            } else {
                Debug.LogWarning("[Arcweave] Cannot find target element of id " + targetElementId + ".");
            }

            // Add out connection to element
            Element source = Get(false, project);
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
            string tmpLabel = label;
			Utils.ParseHTML(tmpLabel, ref label, ref labelNoStyle, ref linkedBoardId);
		}
    } // class Connection
} // namespace AW
