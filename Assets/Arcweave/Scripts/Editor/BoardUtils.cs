using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace AW.Editor
{
    /*
     * A collection of static utilities for managing Boards.
     */
    public static class BoardUtils
    {
        /*
         * Read boards from JSON entry.
         */
        public static void ReadBoards(Project project, JSONClass boardRoot)
        {
            string boardsPath = "Assets" + ProjectUtils.projectResourceFolder + "Boards/";

            List<Board> tmp = new List<Board>();
            List<BoardFolder> tmpFolders = new List<BoardFolder>();

            IEnumerator children = boardRoot.GetEnumerator();
            while (children.MoveNext()) {
                // Get current
                KeyValuePair<string, JSONNode> current = (children.Current != null) ?
                    (KeyValuePair<string, JSONNode>)children.Current : default(KeyValuePair<string, JSONNode>);
                JSONNode child = current.Value;

                // Get its ID
                string id = current.Key;

                bool isFolder = child["children"] != null;

                if (isFolder) {
                    BoardFolder folder = ScriptableObject.CreateInstance<BoardFolder>();
                    folder.id = id;
                    ReadBoardFolder(folder, project, child);
                    tmpFolders.Add(folder);
                    AssetDatabase.CreateAsset(folder, boardsPath + folder.name + ".asset");
                } else {
                    Board board = ScriptableObject.CreateInstance<Board>();
                    board.id = id;
                    ReadBoard(board, project, child);
                    tmp.Add(board);
                    AssetDatabase.CreateAsset(board, boardsPath + board.name + ".asset");
                }
            }

            project.boards = tmp.ToArray();
            project.boardFolders = tmpFolders.ToArray();
        }

        /*
         * Read board folder.
         */
        private static void ReadBoardFolder(BoardFolder bf, Project project, JSONNode node)
        {
            bf.name = bf.id;
            bf.realName = node["name"];

            JSONArray hashArray = node["children"].AsArray;
            if (hashArray.Count == 0) {
                bf.childIds = new string[0];
                return;
            }

            bf.childIds = new string[hashArray.Count];

            for (int i = 0; i < hashArray.Count; i++)
                bf.childIds[i] = hashArray[i];
        }

        /*
         * Read board.
         */
        private static void ReadBoard(Board b, Project project, JSONNode node)
        {
            b.name = b.id;
            b.realName = node["name"];

            // Notes
            JSONArray noteArray = node["notes"].AsArray;
            b.noteIds = new string[noteArray.Count];
            for (int i = 0; i < noteArray.Count; i++) {
                string noteID = noteArray[i].Value;
                b.noteIds[i] = noteID;
            }

            // Elements
            JSONArray elementArray = node["elements"].AsArray;
            b.elementIds = new string[elementArray.Count];
            for (int i = 0; i < elementArray.Count; i++) {
                string elementId = elementArray[i];
                b.elementIds[i] = elementId;
            }

            // Search each jumper and set self as board
            JSONArray jumperArray = node["jumpers"].AsArray;
            for (int i = 0; i < jumperArray.Count; i++) {
                var jumper = project.GetJumper(jumperArray[i]);
                if (jumper == null) {
                    Debug.LogWarning("[Arcweave] Cannot find jumper of ID: " + jumperArray[i]);
                    continue;
                }
                jumper.boardID = b.id;
            }
        }

        /*
         * Setup default root for given board.
         */
        public static void SetDefaultRoot(Board board)
        {
            // Compute potential roots
            List<Element> potentialRoots = ComputeRoots(board);
            if (potentialRoots.Count > 0) {
                board.rootElementId = potentialRoots[0].id;
            }
        }

        /*
         * Compute the list of possible roots to be set
         * for the given Board.
         */
        public static List<Element> ComputeRoots(Board board)
        {
            List<Element> potentialRoots = new List<Element>();

            if (board == null) {
                Debug.LogWarning("[Arcweave] Cannot compute roots for null board.");
                return potentialRoots;
            }

            if (board.elements == null) {
                Debug.LogWarning("[Arcweave] Board elements are not set. The board was not linked before computing roots.");
                return potentialRoots;
            }

            for (int i = 0; i < board.elements.Count; i++) {
                Element e = board.elements[i];
                if (e.inConnections.Count == 0)
                    potentialRoots.Add(e);
            }

            if (potentialRoots.Count == 0)
                potentialRoots.AddRange(board.elements);

            if (potentialRoots.Count == 0) {
                // Still
                Debug.LogWarning("[Arcweave] Given board (" + board.realName + ") has no elements. Cannot compute root for it.");
            }

            return potentialRoots;
        }
    } // class BoardUtils
} // namespace AW.Editor
