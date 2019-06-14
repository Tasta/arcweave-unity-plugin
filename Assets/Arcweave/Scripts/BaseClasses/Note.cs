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
    [Serializable]
    public class Note
    {
        // Arcweave imported data
        public string id;
        public string content;

        /*
         * Construct with id, which is read before construction.
         */
        public Note(string id, string content)
        {
            this.id = id;
            this.content = content;
        }
    } // class Note
} // namespace AW
