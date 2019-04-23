using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AW
{
    /*
     * A Note in the Arcweave context.
     * It's totally useless for the runner, only useful for the designer.
     * Maybe never load them in-game.
     * For now, just load them too in order to have a precise import.
     */
    public class Note
    {
        // Arcweave imported data
        public string id { get; protected set; }
        public string content { get; protected set; }

        /*
         * Construct with id, which is read before construction.
         */
        public Note(string id)
        {
            this.id = id;
        }

        /*
         * Read from json.
         */
        public void FromJSON(JSONNode root)
        {
            content = root["content"];
        }
    } // class Note
} // namespace AW
