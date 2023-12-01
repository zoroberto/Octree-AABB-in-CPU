using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class OctreeNode
    {

        public Vector3 Position { get; set; } // Centre/ Position of this node
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }

        public OctreeNode Parent { get; set; }

        public List<OctreeNode> Children { get; set; } //Child octrees.

        public List<int> LevelTwoIndex { get; set; } //Child collided.

        public OctreeNode() 
        {
            
        }

        public OctreeNode(Vector3 center)
        {
            Position = center;
            Parent = null;
            Children = new List<OctreeNode>();
            LevelTwoIndex = new List<int>();

        }
    }
}
