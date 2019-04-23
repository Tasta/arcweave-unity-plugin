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
     * An element in the Arcweave context.
     */
    public class Element
    {
        // Arcweave imported data
        public string title { get; protected set; }
        public string content { get; protected set; }
        public List<Component> components { get; protected set; }
        public Board linkedBoard { get; protected set; }

        // Runtime instantiated data
        public List<Connection> inConnections { get; protected set; }
        public List<Connection> outConnections { get; protected set; }

        /*
         * Basic constructor.
         */
        public Element()
        {
            inConnections = new List<Connection>();
            outConnections = new List<Connection>();
        }

        /*
         * Add given connection as input.
         */
        public void AddInConnection(Connection con)
        {
            inConnections.Add(con);
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
                return project.elements[conn.sourceElementIdx];
            } catch (Exception) {
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
                return project.elements[conn.targetElementIdx];
            } catch (Exception) {
                return null;
            }
        }

        /*
         * Read from json.
         */
        public void FromJSON(JSONNode root, Project project)
        {
            title = root["title"];
            content = root["content"];

            components = new List<Component>();
            JSONArray componentArray = root["components"].AsArray;
            for (int i = 0; i < componentArray.Count; i++) {
                int compId = componentArray[i].AsInt;
                if (!project.components.ContainsKey(compId)) {
                    Debug.LogWarning("[Arcweave] Cannot find component for given id: " + compId);
                    continue;
                }

                IComponentEntry cEntry = project.components[compId];
                if (!(cEntry is Component)) {
                    Debug.LogWarning("[Arcweave] Requested component id=" + compId + " found, but not valid.");
                    continue;
                }

                Component c = cEntry as Component;
                components.Add(c);
            }

            string linkedBoardID = root["linkedBoard"];
            if (linkedBoard != null) {
                Debug.LogWarning("[Arcweave] Linked board became available for reading!");
            }
        }
    } // class Node
} // namespace AW
