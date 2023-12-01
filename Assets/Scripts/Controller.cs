using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Controller : MonoBehaviour
{
    [Header("Particle Parameters")]
    public float dt = 0.001f;
    public Vector3 gravity = new Vector3(0, -10, 0);
    public GameObject floor;

    [Header("Material")]
    public Material material;

    //==============================
    //   Private Fields
    //==============================

    // clone object
    private Vector3[] vertices;
    private List<Transform> meshTransform = new List<Transform>(); // used to store objects after cloing

    // Mesh bounding
    private BoundingBox planeBounding = new BoundingBox();
    private List<BoundingBox> customMesh = new List<BoundingBox>();

    // collision pair
    private List<BoundingboxPair> collisionPairs = new List<BoundingboxPair>();
    private List<BoundingboxPair> meshesPair = new List<BoundingboxPair>();
    public List<int> collidablePairIndex = new List<int>();
    private bool overlap = false;

    // vertex DATA
    private List<Vector3> verticesRangeList = new List<Vector3>(); // store all combinded vertices of all objects
    private List<List<Vector3>> listOfVerticesList = new List<List<Vector3>>(); // list inside list, to find index of each list
    private List<int> indicesListRange = new List<int>(); // store index range of each object

    // physic particle
    private List<ParticleList> particleData = new List<ParticleList>();
    private List<Particle> particleRangeList = new List<Particle>();

    // octree and octree node
    private Octree[] octrees;
    private List<OctreeNode> parentRoot = new List<OctreeNode>();
    private List<Vector3> parentCenter = new List<Vector3>();
    private List<Vector3> parentSize = new List<Vector3>();
    private List<Vector3> childLevelTwoCenter;


    void Start()
    {
        ReadPositionFromExcel();
        FindPlaneMinMax();
        InitializeData();
        InitializeParticles();

        AddCollisionPair();
        print(" pair " + collisionPairs.Count);
    }

    // Import excel data
    private void ReadPositionFromExcel()
    {
        meshTransform = GetComponent<CSVImportPos>().ReadPositionFromExcel();
    }

    // Find min and max of floor
    private void FindPlaneMinMax()
    {
        GetMeshVertex.Getvertices(floor);
        planeBounding.Min = GetMeshVertex.minPos;
        planeBounding.Max = GetMeshVertex.maxPos;
        planeBounding.Max.y += 0.1f; // add offset to plane bounding
    }

    // Initialize the positions and velocities if needed
    private void InitializeData()
    {
        for (int i = 0; i < meshTransform.Count; i++)
        {

            GetMeshVertex.Getvertices(meshTransform[i].gameObject);
            vertices = GetMeshVertex.vertices;

            BoundingBox mesh = new BoundingBox();
            mesh.Vertices = new List<Vector3>();
            foreach (Vector3 v in vertices) mesh.Vertices.Add(v);
            customMesh.Add(mesh);
        }

        // add vertices of all objects to one list, so list inside list and can find index of each list
        foreach (BoundingBox vert in customMesh) listOfVerticesList.Add(vert.Vertices.ToList());

        for (int i = 0; i < meshTransform.Count; i++)
        {
            for (int j = 0; j < customMesh[i].Vertices.Count; j++)
            {
                //print(" world "+ j+ " " + meshTransform[1].gameObject.transform.TransformPoint(vertices[j]));

                customMesh[i].Vertices[j] = meshTransform[i].gameObject.transform.TransformPoint(vertices[j]);

            }
            verticesRangeList.AddRange(customMesh[i].Vertices); // add all vertices of object from vertex 1 -> n
        }

    }

    // Initialize particle
    private void InitializeParticles()
    {
        for (int i = 0; i < verticesRangeList.Count; i++)
        {
            Particle p = new Particle();
            p.POS = verticesRangeList[i];
            particleRangeList.Add(p);
        }
    }

    /// <summary>
    /// Add collision pair
    /// - collision pair
    /// - add bounding pair
    /// - check collision pair
    /// </summary>
    private void AddCollisionPair()
    {
        octrees = new Octree[customMesh.Count];

        // iterate by length of clone objects
        for (int i = 0; i < meshTransform.Count; i++)
        {
            customMesh[i] = new BoundingBox();

            for (int j = i + 1; j < meshTransform.Count; j++)
            {
                //print($" {i} {j}");

                customMesh[j] = new BoundingBox();
                AddBoundingPair(customMesh[i], customMesh[j], i, j);
            }
        }
    }

    // Add bounding pair by objects list => [0,1], [0,2], [1,2] ...
    private void AddBoundingPair(BoundingBox b1, BoundingBox b2, int i, int j)
    {

        // in pair needs to store array of bounding and add b1 and b2 to list
        List<BoundingBox> boundings = new List<BoundingBox>();
        boundings.Add(b1);
        boundings.Add(b2);

        List<int> indices = new List<int>();
        indices.Add(i);
        indices.Add(j);

        BoundingboxPair pair = new BoundingboxPair(boundings, indices);
        collisionPairs.Add(pair);
    }


    // Update is called once per frame
    void Update()
    {
        UpdateAllVerticesPos();
        FindVerticesListByIndexRange();
        UpdateMeshVertices();
        FindMeshMinMax();

        UpdateBoundingBoxMeshPair();
        UpdateBoundingCollisionPairs();

        // octree
        CreateParent();
        ImplementParentRoot();

        ImplementRootLevelTwo();
        // AddParentRootPair();

        // check collision
        CheckCollisionPairIntersection();
        CheckCollisionFloorWithObj();

        // reverse velocity
        UpdateReverseVelocity();
    }

    private void UpdateAllVerticesPos()
    {
        for (int i = 0; i < verticesRangeList.Count; i++)
        {
            //print(" p " + i + " " + particleRangeList[i].POS); 
            //print(" ls " + i + " " + verticesRangeList[i]);
            particleRangeList[i].UpdatePosition(dt, gravity); // TransformPoint => world pos
            verticesRangeList[i] = particleRangeList[i].POS; // => mesh vertex pos
        }
    }

    private void FindVerticesListByIndexRange()
    {
        // find indices range in listOfVerticesList or ...
        indicesListRange.Clear();
        indicesListRange = CalculateCombinedIndices(listOfVerticesList);

        particleData.Clear();
        for (int i = 0; i < indicesListRange.Count - 1; i++)
        {

            ParticleList p = new ParticleList();
            p.particle = new List<Particle>();

            BoundingBox mesh = new BoundingBox();
            mesh.Vertices = new List<Vector3>();

            int start = indicesListRange[i];
            int end = indicesListRange[i + 1];

            //print("st " + start);
            //print("end " + end);

            for (int s = start; s < end; s++)
            {
                mesh.Vertices.Add(verticesRangeList[s]);
                p.particle.Add(particleRangeList[s]);

            }
            customMesh[i].Vertices = mesh.Vertices;
            particleData.Add(p);
        }
    }

    static List<int> CalculateCombinedIndices(List<List<Vector3>> lists)
    {
        List<int> combinedIndices = new List<int>();
        int currentIndex = 0;

        // Start with 0 as the initial index.
        combinedIndices.Add(currentIndex);

        foreach (List<Vector3> list in lists)
        {
            int listLength = list.Count;
            //print($"listLength vert { listLength}");

            currentIndex += listLength;
            //print($"currentIndex { currentIndex}");

            combinedIndices.Add(currentIndex);
        }

        return combinedIndices;
    }

    private void UpdateMeshVertices()
    {

        for (int i = 0; i < meshTransform.Count; i++)
        {
            if (!customMesh[i].IsCollide)
            {
                meshTransform[i].gameObject.GetComponent<Renderer>().material = material;
                meshTransform[i].gameObject.GetComponent<MeshFilter>().mesh.vertices = customMesh[i].Vertices.ToArray();
            }
        }
    }

    private void FindMeshMinMax()
    {
        for (int i = 0; i < meshTransform.Count; i++)
        {
            GetMeshVertex.Getvertices(meshTransform[i].gameObject);

            customMesh[i].Min = GetMeshVertex.minPos;
            customMesh[i].Max = GetMeshVertex.maxPos;
            customMesh[i].Center = (customMesh[i].Max + customMesh[i].Min) / 2;

        }
    }

    // Add bounding box pair by game objects list => [0,1], [0,2], [1,2] ... 
    private void UpdateBoundingBoxMeshPair()
    {

        // *** mesh pair 
        // - add bounding pair
        // - check collision pair
        // iterate by length of clone objects

        meshesPair.Clear();
        for (int i = 0; i < meshTransform.Count; i++)
        {
            for (int j = i + 1; j < meshTransform.Count; j++)
            {
                //print($" {i} {j}");
                AddMeshesPair(customMesh[i], customMesh[j]);
            }
        }
    }

    // Add bounding pair of meshes
    private void AddMeshesPair(BoundingBox m1, BoundingBox m2)
    {
        //print("min m" + m1.minPos);
        List<BoundingBox> meshes = new List<BoundingBox>();
        meshes.Add(m1);
        meshes.Add(m2);

        BoundingboxPair pair = new BoundingboxPair(meshes);
        meshesPair.Add(pair);

        //print("pair " + meshesPair.Count);
    }

    // Updatet collision pair min and max
    private void UpdateBoundingCollisionPairs()
    {
        for (int i = 0; i < collisionPairs.Count; i++)
        {
            for (int j = 0; j < collisionPairs[i].Bound.Count; j++)
            {
                collisionPairs[i].Bound[j].Max = meshesPair[i].Bound[j].Max;
                collisionPairs[i].Bound[j].Min = meshesPair[i].Bound[j].Min;
                collisionPairs[i].Bound[j].Center = (collisionPairs[i].Bound[j].Max + collisionPairs[i].Bound[j].Min) / 2;

                //print($" min meshes {i} {collisionPairs[i].Bound[j].Min} ");
                //print($" max meshes {i} {collisionPairs[i].Bound[j].Max} ");
            }
        }
    }

  
    /// <summary>
    /// octree implementation
    /// </summary>
    private void CreateParent()
    {
        parentCenter.Clear();
        parentSize.Clear();
        for (int i = 0; i < meshTransform.Count; i++)
        {
            parentCenter.Add((customMesh[i].Min + customMesh[i].Max) / 2);
            parentSize.Add(customMesh[i].Max - customMesh[i].Min);

            //print("p center" + i + " " + parentCenter[i]);
            //print("p size" + i + " " + parentSize[i]);
        }
    }

    private void ImplementParentRoot()
    {
        // Creating octrees with root nodes
        parentRoot.Clear();
        for (int i = 0; i < customMesh.Count; i++)
        {
            octrees[i] = new Octree(parentCenter[i]); // 2, instance obj
            parentRoot.Add(octrees[i].Root);  // 2
        }

        //for (int i = 0; i < parentRoot.Count; i++)
        //{
        //    //print(" oct " + i + " " + octrees[i].Root.Position);
        //    //print(" root " + i + " " + parentRoot[i].Position);
        //}
    }

    private void ImplementRootLevelTwo()
    {
        for (int i = 0; i < parentRoot.Count; i++)
        {
            childLevelTwoCenter = new List<Vector3>();
            childLevelTwoCenter = octrees[0].SplitNodes(parentCenter[i], parentSize[i] / 4); // p 2 <= c 8

            AddChildLevelTwo(i);

        }
    }

    

    /*
     *  add child level two, find each children level 1 min and max by parent position and size
     */
    private void AddChildLevelTwo(int i)
    {
        for (int j = 0; j < childLevelTwoCenter.Count; j++)
        {
            AddChildrenLevelTwoCenter(i, j);
            FindChildLevelTwoMinMax(i, j);
        }

        //print("children min " + i + " " + parentRoot[i].Children[0].Min);
    }

    private void AddChildrenLevelTwoCenter(int i, int j)
    {
        octrees[i].AddChildrenLevelTwo(parentRoot[i], childLevelTwoCenter[j], j);
        //print($"- Child Node: {i} {j} {parentRoot[i].Children[j].Position}");
    }

    private void FindChildLevelTwoMinMax(int i, int j)
    {
        parentRoot[i].Children[j].Min = customMesh[0].Minimum(parentRoot[i].Children[j].Position, parentSize[i] / 2);
        parentRoot[i].Children[j].Max = customMesh[0].Maximum(parentRoot[i].Children[j].Position, parentSize[i] / 2);

        //print("children min " + i + " " + j + " " + parentRoot[i].Children[j].Min);
        //print("children max " + i + " " + j + " " + parentRoot[i].Children[j].Max);
    }


    //private void AddParentRootPair()
    //{
    //    parentRootPairs.Clear();
    //    for (int i = 0; i < customMesh.Count; i++)
    //    {
    //        for (int j = i + 1; j < customMesh.Count; j++)
    //        {
    //            AddRootPair(parentRoot[i], parentRoot[j]);
    //        }
    //    }

    //    //print(" root pair " + parentRootPairs.Count);
    //    //for (int i = 0; i < parentRootPairs.Count; i++)
    //    //{
    //    //    //print(" 0 pair min " + i + " " + parentRootPairs[i].Nodes[0].Children[0].Min);
    //    //    //print(" 1 pair min " + i + " " + parentRootPairs[i].Nodes[1].Children[0].Min);
    //    //    //print(" pair max " + i + " " + parentRootPairs[i].Nodes[0].Max);
    //    //}
    //}


    //private void AddRootPair(OctreeNode r1, OctreeNode r2)
    //{
    //    List<OctreeNode> parentRoots = new List<OctreeNode>();
    //    parentRoots.Add(r1);
    //    parentRoots.Add(r2);

    //    ParentRootPair pair = new ParentRootPair(parentRoots);
    //    parentRootPairs.Add(pair);
    //}



    // TODO: 
    private void ImplementChildLevelThree()
    {

    }

    
    // check collision pair collision by pair 
    private void CheckCollisionPairIntersection()
    {
        // Create a new list to store indices to remove
        List<int> indicesToRemove = new List<int>();
        List<int> indicesParentToRemove = new List<int>();

        // collision
        collidablePairIndex.Clear();

        indicesToRemove.Clear();
        indicesParentToRemove.Clear();
        for (int i = 0; i < collisionPairs.Count; i++) // pair
        {
            if (Intersection.AABB(collisionPairs[i].Bound[0], collisionPairs[i].Bound[1])) // bounding, aabb, i
            {
                //print("true " + i);
                overlap = true;
                collidablePairIndex.Add(i);
                //  i = a and b, index => i = 0 [0,1], i = 1 [0,2]

                // implement collision detection for level two, i = index
                CheckCollisionLevelTwo(collisionPairs[i].Indices);
            }
        }

        //print("count  " + collidablePairIndex.Count);
        //for (int i = 0; i < collidablePairIndex.Count; i++)
        //{
        //    print("allIndices  " + collidablePairIndex[i]);
        //}

    }


    // Check collision level two
    private void CheckCollisionLevelTwo(List<int> pairs)
    {
        
        for (int i = 0; i < 8; i++)
        {
            BoundingBox[] box1 = new BoundingBox[8]; // node 1
            box1[i] = new BoundingBox();
            box1[i].Min = parentRoot[pairs[0]].Children[i].Min;
            box1[i].Max = parentRoot[pairs[0]].Children[i].Max;

            for (int j = 0; j < 8; j++)
            {
                BoundingBox[] box2 = new BoundingBox[8]; // node 2
                box2[j] = new BoundingBox();
                box2[j].Min = parentRoot[pairs[1]].Children[j].Min;
                box2[j].Max = parentRoot[pairs[1]].Children[j].Max;

                if (Intersection.AABB(box1[i], box2[j]))
                {
                    //print(" index i " + i);
                    //print(" index j " + j);
                    if (!parentRoot[pairs[0]].LevelTwoIndex.Contains(i))
                        parentRoot[pairs[0]].LevelTwoIndex.Add(i);

                    if (!parentRoot[pairs[1]].LevelTwoIndex.Contains(j))
                        parentRoot[pairs[1]].LevelTwoIndex.Add(j);
                }
            }
        }
    }


    private void CheckCollisionFloorWithObj()
    {
        for (int i = 0; i < meshTransform.Count; i++)
            customMesh[i].IsCollide = Intersection.AABB(planeBounding, customMesh[i]);
    }


    private void UpdateReverseVelocity()
    {
        for (int i = 0; i < meshTransform.Count; i++)
        {
            for (int j = 0; j < customMesh[0].Vertices.Count; j++)
            {
                if (customMesh[i].IsCollide)
                {
                    particleData[i].particle[j].UpdateReverseVelocity(dt);
                    customMesh[i].Vertices[j] = particleData[i].particle[j].POS;
                    //print(" rev VELO " + particleData[i].particle[j].VELO);
                    //print(" rev cus " + customMesh[i].Vertices[j]);
                }
            }

            meshTransform[i].gameObject.GetComponent<Renderer>().material = material;
            meshTransform[i].gameObject.GetComponent<MeshFilter>().mesh.vertices = customMesh[i].Vertices.ToArray();
        }
    }


    private void OnDrawGizmos()
    {
        for (int i = 0; i < meshTransform.Count; i++)
        {
            Gizmos.color = Color.green; ;

            for (int j = 0; j < parentRoot[i].LevelTwoIndex.Count; j++)
            {
                Gizmos.DrawWireCube(parentRoot[i].Children[parentRoot[i].LevelTwoIndex[j]].Position, parentSize[i] / 2);
            }
        }


        if (overlap)
        {
            for (int i = 0; i < collisionPairs.Count; i++)
            {
                for (int j = 0; j < collisionPairs[i].Bound.Count; j++)
                {
                    Vector3 size = new Vector3(
                       Mathf.Abs(collisionPairs[i].Bound[j].Max.x - collisionPairs[i].Bound[j].Min.x),
                       Mathf.Abs(collisionPairs[i].Bound[j].Max.y - collisionPairs[i].Bound[j].Min.y),
                       Mathf.Abs(collisionPairs[i].Bound[j].Max.z - collisionPairs[i].Bound[j].Min.z));

                    collisionPairs[i].Bound[j].Center = (collisionPairs[i].Bound[j].Max + collisionPairs[i].Bound[j].Min) / 2;


                    for (int c = 0; c < collidablePairIndex.Count; c++)
                    {
                        if (i == collidablePairIndex[c])
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawWireCube(collisionPairs[collidablePairIndex[c]].Bound[j].Center, size);
                        }
                    }

                }

            }
        }
    }
}
