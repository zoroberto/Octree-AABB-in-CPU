using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public static class GetMeshVertex
    {

        public static Vector3[] vertices;
        public static Vector3 minPos;
        public static Vector3 maxPos;

        // Find mesh vertices
        public static void Getvertices(GameObject g)
        {
            // get all vertices from all game objects
            vertices = g.GetComponent<MeshFilter>().mesh.vertices;

            // get only first index of vertex
            minPos = g.transform.TransformPoint(vertices[0]);
            maxPos = g.transform.TransformPoint(vertices[0]);


            for (int i = 0; i < vertices.Length; i++)
            {
                // get all vertices and TransformPoint to the world coor
                Vector3 allVerts = g.transform.TransformPoint(vertices[i]);


                minPos.x = Mathf.Min(minPos.x, allVerts.x);
                minPos.y = Mathf.Min(minPos.y, allVerts.y);
                minPos.z = Mathf.Min(minPos.z, allVerts.z);

                maxPos.x = Mathf.Max(maxPos.x, allVerts.x);
                maxPos.y = Mathf.Max(maxPos.y, allVerts.y);
                maxPos.z = Mathf.Max(maxPos.z, allVerts.z);
            }

        }
    }
}
