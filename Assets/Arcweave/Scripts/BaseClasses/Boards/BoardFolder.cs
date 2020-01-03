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
        public string[] childIds;
    } // class BoardFolder
} // namespace AW
