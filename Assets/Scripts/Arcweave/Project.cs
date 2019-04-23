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
     * An Arcweave project. With all the references inside.
     * Contains:
     *  - Components
     *  - Connections
     *  - Elements
     *  - Boards
     *  - Notes (which are, at the moment, ignored)
     */
    [Serializable]
    public class Project
    {
        // The arcweave data
        public string name { get; protected set; }
        public Dictionary<int, IComponentEntry> components { get; protected set; }
        public Dictionary<int, Element> elements { get; protected set; }
        public Connection[] connections { get; protected set; }
        public Dictionary<string, Note> notes { get; protected set; }
        public Dictionary<int, IBoardEntry> boards { get; protected set; }

        /*
         * Get board with given name.
         */
        public Board GetBoardByName(string boardName)
        {
            foreach (var item in boards) {
                if (item.Value is Board) {
                    Board board = item.Value as Board;
                    if (board.name == boardName)
                        return board;
                }
            }

            Debug.LogWarning("[Arcweave] Cannot find board with given name: " + boardName);
            return null;
        }

        /*
         * Read the Arcweave data from disk.
         */
        public IEnumerator Read(string projectFolder = "ArcweaveData/")
        {
            // Attempt to load project
            ResourceRequest rrRequest = Resources.LoadAsync<TextAsset>(projectFolder + "project_settings");
            yield return rrRequest;
            if (rrRequest.asset == null) {
                Debug.LogWarning("[Arcweave] Cannot load project. 'project_settings' TextAsset not found at project path: " + projectFolder);
                yield break;
            }

            // Parse contents
            TextAsset projectAsset = rrRequest.asset as TextAsset;
            string projectContents = Encoding.UTF8.GetString(projectAsset.bytes);
            JSONNode root = JSONNode.Parse(projectContents);

            // Read name
            name = root["name"];

            // Read components
            yield return ReadComponents(root["components"].AsObject, projectFolder);

            // Read elements
            yield return ReadElements(root["elements"].AsObject);

            // Read connections
            yield return ReadConnections(root["connections"].AsArray);

            // Read notes
            yield return ReadNotes(root["notes"].AsObject);

            // Read boards
            yield return ReadBoards(root["boards"].AsObject);

            Debug.Log("[Arcweave] Load successful!");
        }

        /*
         * Read components.
         */
        public IEnumerator ReadComponents(JSONClass componentRoot, string projectFolder)
        {
            // Create new container
            components = new Dictionary<int, IComponentEntry>();

            IEnumerator children = componentRoot.GetEnumerator();
            while (children.MoveNext()) {
                // Get current
                KeyValuePair<string, JSONNode> current = (children.Current != null) ?
                    (KeyValuePair<string, JSONNode>)children.Current : default(KeyValuePair<string, JSONNode>);
                JSONNode child = current.Value;

                // Get its ID
                int id = int.Parse(current.Key);

                bool isFolder = child["children"] != null;

                if (isFolder) {
                    ComponentFolder folder = new ComponentFolder();
                    folder.FromJSON(child);
                    components[id] = folder;
                } else {
                    // Async operation because it might load images
                    Component component = new Component();
                    yield return component.FromJSON(child, projectFolder);
                    components[id] = component;
                }
            }
        }

        /*
         * Read elements
         */
        public IEnumerator ReadElements(JSONClass elementRoot)
        {
            // Create new container
            elements = new Dictionary<int, Element>();

            IEnumerator children = elementRoot.GetEnumerator();
            while (children.MoveNext()) {
                // Get current
                KeyValuePair<string, JSONNode> current = (children.Current != null) ?
                    (KeyValuePair<string, JSONNode>)children.Current : default(KeyValuePair<string, JSONNode>);
                JSONNode child = current.Value;

                // Get id
                int id = int.Parse(current.Key);

                // Create element
                Element element = new Element();
                element.FromJSON(current.Value, this);

                // Add
                elements[id] = element;
            }

            // Done
            yield break;
        }

        /*
         * Read connections.
         */
        public IEnumerator ReadConnections(JSONArray connectionArray)
        {
            connections = new Connection[connectionArray.Count];
            for (int i = 0; i < connectionArray.Count; i++) {
                connections[i] = new Connection();
                connections[i].FromJSON(connectionArray[i], this);
            }

            // Done
            yield break;
        }

        /*
         * Read notes.
         */
        public IEnumerator ReadNotes(JSONClass noteRoot)
        {
            notes = new Dictionary<string, Note>();

            IEnumerator children = noteRoot.GetEnumerator();
            while (children.MoveNext()) {
                // Get current
                KeyValuePair<string, JSONNode> current = (children.Current != null) ?
                    (KeyValuePair<string, JSONNode>)children.Current : default(KeyValuePair<string, JSONNode>);
                JSONNode child = current.Value;

                // Get id
                string id = current.Key;

                // Create element
                Note note = new Note(id);
                note.FromJSON(current.Value);

                // Add
                notes[id] = note;
            }

            // Done
            yield break;
        }

        /*
         * Read boards.
         */
        public IEnumerator ReadBoards(JSONClass boardRoot)
        {
            // Create new container
            boards = new Dictionary<int, IBoardEntry>();

            IEnumerator children = boardRoot.GetEnumerator();
            while (children.MoveNext()) {
                // Get current
                KeyValuePair<string, JSONNode> current = (children.Current != null) ?
                    (KeyValuePair<string, JSONNode>)children.Current : default(KeyValuePair<string, JSONNode>);
                JSONNode child = current.Value;

                // Get its ID
                int id = int.Parse(current.Key);

                bool isFolder = child["children"] != null;

                if (isFolder) {
                    BoardFolder folder = new BoardFolder();
                    folder.FromJSON(child);
                    boards[id] = folder;
                } else {
                    // Async operation because it might load images
                    Board board = new Board();
                    board.FromJSON(child, this);
                    boards[id] = board;
                }
            }

            // Done
            yield break;
        }
    } // class Project
} // namespace AW
