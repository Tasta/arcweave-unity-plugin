using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AW
{
    /*
     * A Board in the Arcweave context.
     *
     * Load from JSON and reconstruct the graph from the data there.
     */
    public class Board
        : IBoardEntry
    {
        // Arcweave imported data
        public Note[] notes;
        public Element[] elements;

        // Runtime data
        public Element root;

        /*
         * Get element by id.
         */
        public Element GetElement(string id)
        {
            for (int eIdx = 0; eIdx < elements.Length; eIdx++) {
                if (elements[eIdx].id == id) {
                    return elements[eIdx];
                }
            }

            return null;
        }
    } // class Board
} // namespace Aw
