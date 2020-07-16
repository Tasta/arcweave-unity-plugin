using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AW
{
    /*
     * Just a base for boards and board folders.
     */
    public abstract class IBoardEntry : ScriptableObject
    {
        public string id;
        public string realName;
    } // class IBoardEntry
} // namespace AW
