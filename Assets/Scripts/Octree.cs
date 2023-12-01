using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Octree
    {
        // Root node of the octree
        // Region encapsulating the entire octant
        public OctreeNode Root { get; set; }


        public Octree(Vector3 position)
        {
            Root = new OctreeNode(position);
        }

        
        /// Add an child nodes.
        /// <param name="parentNode">Parent node.</param>
        /// <param name="position">Position of the centre of the initial node.</param>
        public void AddChildrenLevelTwo(OctreeNode parentNode, Vector3 position, int node)
        {
            if (parentNode.Children.Count < 8)
            {
                var newNode = new OctreeNode(position);
                parentNode.Children.Add(newNode);
            }
            else
                throw new Exception("Parent already has 8 children");
        }


        public void AddChildrenLevelThree(OctreeNode childNode, Vector3 position, int node)
        {
            if (childNode.Children.Count < 8)
            {
                var newNode = new OctreeNode(position);
                childNode.Children.Add(newNode);
            }
            else
                throw new Exception("Parent already has 8 children");
        }

        // when no parent is root node
        // find parent
        //public OctreeNode FindParentNode(OctreeNode childNode, OctreeNode rootNode)
        //{
        //    if (rootNode == null)
        //    {
        //        return null;
        //    }

        //    if (rootNode == childNode) // the root node has no child
        //    {
        //        return null;
        //    }

        //    foreach (OctreeNode node in rootNode.Children)
        //    {
        //        if (node == childNode)
        //            return rootNode;

        //        OctreeNode parent = FindParentNode(childNode, node);
        //        if (parent != null)
        //            return parent;
        //    }

        //    return null;
        //}


        // find children nodes

        //public List<OctreeNode> FindChildrenNodes(OctreeNode parent)
        //{
        //    Console.WriteLine("child ");
        //    List<OctreeNode> children = new List<OctreeNode>();

        //    //Console.WriteLine("child " + parent.Data.Max);

        //    foreach (OctreeNode child in parent.Children)
        //    {
        //        Console.WriteLine("child " + child);
        //        children.Add(child);
        //        //children.AddRange(FindChildrenNodes(child));
        //    }
        //    return children;
        //}


        // sub divide

        public List<Vector3> SplitNodes(Vector3 position, Vector3 scale)
        {
            List<Vector3> childrenPos = new List<Vector3>();

            childrenPos.Add(new Vector3(position.x - scale.x, position.y + scale.y, position.z - scale.z));
            childrenPos.Add(new Vector3(position.x + scale.x, position.y + scale.y, position.z - scale.z));
            childrenPos.Add(new Vector3(position.x - scale.x, position.y - scale.y, position.z - scale.z));
            childrenPos.Add(new Vector3(position.x + scale.x, position.y - scale.y, position.z - scale.z));
            childrenPos.Add(new Vector3(position.x - scale.x, position.y + scale.y, position.z + scale.z));
            childrenPos.Add(new Vector3(position.x + scale.x, position.y + scale.y, position.z + scale.z));
            childrenPos.Add(new Vector3(position.x - scale.x, position.y - scale.y, position.z + scale.z));
            childrenPos.Add(new Vector3(position.x + scale.x, position.y - scale.y, position.z + scale.z));

            return childrenPos;
        }
    }
}
