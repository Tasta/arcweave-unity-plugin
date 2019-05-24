using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AW
{
    /*
     * A component in the Arcweave system.
     */
    [Serializable]
    public class Component
        : IComponentEntry
    {
        // Arcweave imported data
        public Sprite image;
        public Attribute[] attributes;
    } // class Component
} // namespace AW
