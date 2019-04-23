using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AW
{
    /* 
     * A component Attribute in the Arcweave context.
     */
    public class Attribute
    {
        // Arcweave imported data
        public string label { get; protected set; }
        public string content { get; protected set; }

        /*
         * Load from JSON.
         */
        public void FromJSON(JSONNode root)
        {
            label = root["label"];
            content = root["content"];
        }
    } // class Attribute
} // namespace AW
