using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AW
{
    /*
     * A Jumper is a new type of entity which represents a Board:Element address in order
     * to pass control of the flow between boards, at given Dialogue points.
     *
     * Note: At the moment, the export does not have the 'Element' part of the address,
     *       so this jumper can only jump to the board.
     */
    [Serializable]
    public class Jumper
    {
        public string id;
        public string boardID;
        public string elementID;
    } // class Jumper
} // namespace AW
