using System;

namespace AW
{
    /*
     * A folder of Arcweave assets.
     */
    [Serializable]
    public class AssetFolder
        : IAssetEntry
    {
        // Arcweave imported data
        public string[] childIds;
    } // class AssetFolder
} // namespace AW
