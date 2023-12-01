using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class ParentRootPair
    {
        public List<OctreeNode> Nodes { get; set; }
        public List<int> collidedLV2 { get; set; }

        public ParentRootPair(List<OctreeNode> n)
        {
            Nodes = n;
        }

        public ParentRootPair(List<int> c)
        {
            collidedLV2 = c;
        }
    }
}
