using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace AW
{
    /*
     * An element in the Arcweave context.
     */
    [Serializable]
    public class Element
    {
        // Arcweave imported data
        public string id;
        public string title;
        public string titleNoStyle;
        public string content;
        public string contentNoStyle;
        public Component[] components;
        public string linkedBoardId;
        public string coverID;

        // Computed data
        [NonSerialized] public Board ownerBoard;
        [NonSerialized] public List<Connection> inConnections;
        [NonSerialized] public List<Connection> outConnections;
        [NonSerialized] public Sprite cover;

        /*
         * Basic constructor.
         */
        public Element()
        {
            ResetConnections();
        }

        /*
         * Add given connection as input.
         */
        public void AddInConnection(Connection conn)
        {
            inConnections.Add(conn);
        }

        /*
         * Add given connection as output.
         */
        public void AddOutConnection(Connection conn)
        {
            outConnections.Add(conn);
        }

        /*
         * Get the in neighbour for given index.
         */
        public Element GetInNeighbour(int connectionIdx, Project project)
        {
            if (inConnections.Count == 0)
                return null;

            if (connectionIdx < 0 || connectionIdx >= inConnections.Count)
                return null;

            Connection conn = inConnections[connectionIdx];
            try {
                return conn.Get(false, project);
            } catch (Exception ex) {
                Debug.LogWarning("[Arcweave] Exception " + ex.Message + " whhen trying to get next element from connection.");
                return null;
            }
        }

        /*
         * Get out element for given index.
         */
        public Element GetOutNeighbour(int connectionIdx, Project project)
        {
            if (outConnections.Count == 0)
                return null;

            if (connectionIdx < 0 || connectionIdx >= outConnections.Count)
                return null;

            Connection conn = outConnections[connectionIdx];
            try {
                return conn.Get(true, project);
            } catch (Exception ex) {
                Debug.LogWarning("[Arcweave] Exception " + ex.Message + " whhen trying to get next element from connection.");
                return null;
            }
        }

		/*
		 * Propagate back to the last element that had a connection with labels.
		 * Used to simulate a root. It's a hack!
		 *
		 * ToDo: With a more fixed structure in Arcweave, this should return to
		 * 		 the last Element in the RootStack. And shouldn't be isolated here.
		 *
		 * Return element that acts as root.
		 */
		public Element GoBack(Project project) {
			if (inConnections.Count == 0) {
				Debug.LogWarning("[Arcweave] Cannot GoBack. No inConnections.");
				return null;
			}

			Element parent = this;
			do {
				if (parent.inConnections.Count == 0)
					break;

				string label = parent.inConnections[0].label;
				if (!string.IsNullOrEmpty(label) && label != "null")
					return parent.GetInNeighbour(0, project);
				else
					parent = parent.GetInNeighbour(0, project);
			} while (true);

			Debug.LogWarning("[Arcweave] Found no root to go back to.");
			return null;
		}

        /*
         * Reset connections.
         */
        public void ResetConnections() {
            inConnections = new List<Connection>();
            outConnections = new List<Connection>();
        }

        /*
         * Get displayable string for this node.
         * Used when action pointing to it has no label.
         * Used in Editor Utility.
         */
        public string GetActionLabel()
        {
            const int maxDisplayChar = 40;

            if (!string.IsNullOrEmpty(titleNoStyle) && titleNoStyle != "null") {
                if (titleNoStyle.Length > maxDisplayChar)
                    return titleNoStyle.Substring(0, maxDisplayChar) + "...";
                else
                    return titleNoStyle;
            } else if (!string.IsNullOrEmpty(contentNoStyle) && contentNoStyle != "null") {
                if (contentNoStyle.Length > maxDisplayChar)
                    return contentNoStyle.Substring(0, maxDisplayChar) + "...";
                else
                    return contentNoStyle;
            } else {
                return "Empty Element";
            }
        }

		/*
		 * Parse the HTML contents and resolve the links.
		 */
		public void ParseHTML(Project project) {
            // Parse the title, while looking for the linked board reference
            string tmpTitle = title;
			Utils.ParseHTML(tmpTitle, ref title, ref titleNoStyle, ref linkedBoardId);

            // Parse the content
            string tmpContent = content;
			Utils.ParseHTML(tmpContent, ref content, ref contentNoStyle, ref linkedBoardId);
		}
    } // class Node
} // namespace AW
