using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Editor.AssetBundleBuilder
{
    class AssetRefrenceNode
    {
        public string AssetPath { get; set; }
        public string MainAssetName { get; set; }

        public List<string> depenceOnMe = new List<string>();
        public List<string> depence = new List<string>();

        public List<string> incluedDepReference = new List<string>();
    }
}
