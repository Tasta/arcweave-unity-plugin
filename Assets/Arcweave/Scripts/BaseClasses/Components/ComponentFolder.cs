using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AW
{
    /*
     * A component folder in the Arcweave Component context.
     */
    [Serializable]
    public class ComponentFolder
        : IComponentEntry
    {
        // Arcweave imported data
        public int[] childIds;
    } // class ComponentFolder
} // namespace AW
