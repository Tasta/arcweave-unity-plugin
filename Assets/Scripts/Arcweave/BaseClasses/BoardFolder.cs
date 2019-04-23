using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AW
{
    /*
     * A folder of boards, in the Arcweave board context.
     */
    public class BoardFolder
        : IBoardEntry
    {
        // Arcweave imported data
        public string name { get; protected set; }
        public int[] childIds { get; protected set; }

        /*
         * Read this folder.
         */
        public void FromJSON(JSONNode node)
        {
            name = node["name"];

            JSONArray idxArray = node["children"].AsArray;
            if (idxArray.Count == 0)
                return;

            childIds = new int[idxArray.Count];

            for (int i = 0; i < idxArray.Count; i++)
                childIds[i] = idxArray[i].AsInt;
        }
    } // class BoardFolder
} // namespace AW
