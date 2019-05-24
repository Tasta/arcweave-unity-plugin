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
    public class Project : ScriptableObject
    {    
        [Header("The Arcweave Data")]
        public string project;
        public IComponentEntry[] components;
        public Element[] elements;
        public Connection[] connections;
        public Note[] notes;
        public Board[] boards;
        public BoardFolder[] boardFolders;

        [Space(15.0f)]
        [Header("User Preferences")]
        public int startingBoardIdx = 0;
        public int boardRootId = 0;

        [Space(15.0f)]
        [Header("Editor Preferences")]
        public string folderPath = null;

        /*
         * Get board with given id.
         */
        public Board GetBoard(int id)
        {
            for (int i = 0; i < boards.Length; i++) {
                if (boards[i].id == id)
                    return boards[i] as Board;
            }

            Debug.LogWarning("[Arcweave] Cannot find board with given id: " + id);
            return null;
        }

        /*
         * Get board with given name.
         */
        public Board GetBoard(string boardName)
        {
            for (int i = 0; i < boards.Length; i++) {
                if (boards[i].name == boardName)
                    return boards[i] as Board;
            }
            
            Debug.LogWarning("[Arcweave] Cannot find board with given name: " + boardName);
            return null;
        }

        /*
         * Get all boards. (used to exclude folders which are stored in the same array)
         */
        public List<Board> GetAllBoards()
        {
            List<Board> subset = new List<Board>();
            for (int i = 0; i < boards.Length; i++) {
                if (boards[i] is Board)
                    subset.Add(boards[i] as Board);
            }
            return subset;
        }

        /*
         * Get element by id.
         */
        public Element GetElement(int id)
        {
            for (int eIdx = 0; eIdx < elements.Length; eIdx++) {
                if (elements[eIdx].id == id) {
                    return elements[eIdx];
                }
            }

            return null;
        }

        /*
         * Get note by id.
         */
        public Note GetNote(string id)
        {
            for (int nIdx = 0; nIdx < notes.Length; nIdx++) {
                if (notes[nIdx].id == id) {
                    return notes[nIdx];
                }
            }

            return null;
        }
    } // class Project
} // namespace AW
