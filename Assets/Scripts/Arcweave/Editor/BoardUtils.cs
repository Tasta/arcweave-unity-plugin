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
                    // Async operation because it might load images
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
            bf.name = node["name"];

            JSONArray idxArray = node["children"].AsArray;
            if (idxArray.Count == 0)
                return;

            bf.childIds = new int[idxArray.Count];

            for (int i = 0; i < idxArray.Count; i++)
                bf.childIds[i] = idxArray[i].AsInt;
        }

        /*
         * Read board.
         */
        private static void ReadBoard(Board b, Project project, JSONNode node)
        {
            b.name = node["name"];

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
        }

        /*
         * Compute the list of possible roots to be set
         * for the given Board.
         */
        public static List<Element> ComputeRoots(Board board)
        {
            List<Element> potentialRoots = new List<Element>();

            for (int i = 0; i < board.elements.Count; i++) {
                Element e = board.elements[i];
                if (e.inConnections.Count == 0)
                    potentialRoots.Add(e);
            }

            if (potentialRoots.Count == 0)
                potentialRoots.AddRange(board.elements);

            if (potentialRoots.Count == 0) {
                // Still
                Debug.LogWarning("[Arcweave] Given board has no elements. Cannot compute root for it.");
            }

            return potentialRoots;
        }
    } // class BoardUtils
} // namespace AW.Editor
