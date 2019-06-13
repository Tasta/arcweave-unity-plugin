using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AW
{
    /*
     * Just a base for components and component folders.
     */
    [Serializable]
    public abstract class IComponentEntry : ScriptableObject
    {
        public string id;
        public string realName;
    } // interface IComponentEntry
} // namespace AW
