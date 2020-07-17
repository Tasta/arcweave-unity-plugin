using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AW
{
    /*
     * A component Attribute in the Arcweave context.
     */
    [Serializable]
    public class Attribute
    {
        // Arcweave imported data
        public string id;
        public string label;
        public string labelNoStyle;
        public string content;
        public string contentNoStyle;
    } // class Attribute
} // namespace AW
