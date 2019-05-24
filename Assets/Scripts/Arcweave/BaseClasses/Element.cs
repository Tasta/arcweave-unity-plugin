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
        public string content;
        public Component[] components;
        public Board linkedBoard;

        // Computed data
        public Connection[] inConnections;
        public Connection[] outConnections;
        
        /*
         * Basic constructor.
         */
        public Element()
        {
            inConnections = new Connection[0];
            outConnections = new Connection[0];
        }

        /*
         * Add given connection as input.
         */
        public void AddInConnection(Connection conn)
        {
            Connection[] old = inConnections;
            inConnections = new Connection[old.Length + 1];
            for (int i = 0; i < old.Length; i++)
                inConnections[i] = old[i];
            inConnections[old.Length] = conn;
        }

        /*
         * Add given connection as output.
         */
        public void AddOutConnection(Connection conn)
        {
            Connection[] old = outConnections;
            outConnections = new Connection[old.Length + 1];
            for (int i = 0; i < old.Length; i++)
                outConnections[i] = old[i];
            outConnections[old.Length] = conn;
        }

        /*
         * Get the in neighbour for given index.
         */
        public Element GetInNeighbour(int connectionIdx, Project project)
        {
            if (inConnections.Length == 0)
                return null;

            if (connectionIdx < 0 || connectionIdx >= inConnections.Length)
                return null;

            Connection conn = inConnections[connectionIdx];
            try {
                return project.GetElement(conn.sourceElementId);
            } catch (Exception) {
                return null;
            }
        }

        /*
         * Get out element for given index.
         */
        public Element GetOutNeighbour(int connectionIdx, Project project)
        {
            if (outConnections.Length == 0)
                return null;

            if (connectionIdx < 0 || connectionIdx >= outConnections.Length)
                return null;

            Connection conn = outConnections[connectionIdx];
            try {
                return project.GetElement(conn.targetElementId);
            } catch (Exception) {
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
			if (inConnections.Length == 0) {
				Debug.LogWarning("[Arcweave] Cannot GoBack. No inConnections.");
				return null;
			}

			Element parent = this;
			do {
				if (parent.inConnections.Length == 0)
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
         * Get displayable string for this node.
         * Used when action pointing to it has no label.
         * Used in Editor Utility.
         */
        public string GetTitle()
        {
            const int maxDisplayChar = 12;
            if (!string.IsNullOrEmpty(title) && title != "null") {
                if (title.Length > maxDisplayChar)
                    return title.Substring(0, maxDisplayChar) + "...";
                else
                    return title;
            } else if (!string.IsNullOrEmpty(content) && content != "null") {
                if (content.Length > maxDisplayChar)
                    return content.Substring(0, maxDisplayChar) + "...";
                else
                    return content;
            } else {
                return "Empty Element";
            }
        }

		/*
		 * Parse the HTML contents and resolve the links.
		 */
		public void ParseHTML(Project project) {
			// Parse the title, while looking for the linked board reference
			string linkedBoardId = null;
			title = Utils.ParseHTML(title, ref linkedBoardId);
			if (!string.IsNullOrEmpty(linkedBoardId) && linkedBoardId != "null") {
                Board board = project.GetBoard(linkedBoardId);

				if (board == null) {
					Debug.LogWarning("[Arcweave] Cannot find linked board of id: " + linkedBoardId);
				} else {
                    IBoardEntry boardEntry = project.GetBoard(linkedBoardId);
					if (!(boardEntry is Board)) {
						Debug.LogWarning("[Arcweave] Board of id " + linkedBoardId + " found but it's a BoardFolder.");
					} else {
						linkedBoard = boardEntry as Board;
					}
				}
			}

			// Parse the content
			content = Utils.ParseHTML(content, ref linkedBoardId);
		}
    } // class Node
} // namespace AW
