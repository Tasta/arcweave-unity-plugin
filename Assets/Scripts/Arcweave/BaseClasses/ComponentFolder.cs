using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AW
{
    /*
     * A component folder in the Arcweave Component context.
     */
    public class ComponentFolder
        : IComponentEntry
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
    } // class ComponentFolder
} // namespace AW
