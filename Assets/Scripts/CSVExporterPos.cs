using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSVExporterPos : MonoBehaviour
{
    [Header("Clone Object")]
    public GameObject objectToClone;
    public GameObject floor;
    public int numberOfClones;
    public Vector3 positionRange; // Range for random positions

    private GameObject clone;
    private List<Vector3> clonedObjects = new List<Vector3>();

    private Vector3 minP, maxP;
    private Vector3 minPos, maxPos;

    void Start()
    {
        CreateCloneObject();
        PositionCSVExporter();
    }

    // Create clone objects
    private void CreateCloneObject()
    {
        for (int i = 0; i < numberOfClones; i++)
        {
            Vector3 randomPosition = GenerateRandomPosition();


            clone = Instantiate(objectToClone, randomPosition, transform.rotation);

            clone.transform.SetParent(transform);

            clonedObjects.Add(clone.transform.localPosition);
        }

        //print("num " + " " + numberOfClones);


        //while (clonedObjects.Count < numberOfClones)
        //{

        //    //i++;
        //    Vector3 randomPosition = GenerateRandomPosition();

        //    //print("i " + " " + i);

        //    // Check for collisions before adding to the list
        //    bool hasCollision = false;
        //    foreach (Vector3 player in clonedObjects)
        //    {
        //        if (IsPositionIntersect(randomPosition, player))
        //        {
        //            hasCollision = true;
        //            //Destroy(clone); // Destroy the clone if there's a collision
        //            break;
        //        }
        //    }

        //    if (!hasCollision)
        //    {
        //        clone = Instantiate(objectToClone, randomPosition, transform.rotation);
        //        clone.transform.SetParent(transform);
        //        clonedObjects.Add(clone.transform.localPosition);
        //    }
        //}

    }

    bool IsPositionIntersect(Vector3 position, Vector3 other)
    {
        minPos = Minimum(position, objectToClone.transform.localScale);
        maxPos = Maximum(position, objectToClone.transform.localScale);

        //print("minPos " + " " + minPos);
        //print("maxPos " + " " + maxPos);


        minP = Minimum(other, objectToClone.transform.localScale);
        maxP = Maximum(other, objectToClone.transform.localScale);
        //print("min Pos i " + i + " " + clonedObjects[i]);
        //print("max Pos i " + i + " " + clonedObjects[i]);

        if (
               minPos.x <= maxP.x &&
               maxPos.x >= minP.x &&
               minPos.y <= maxP.y &&
               maxPos.y >= minP.y &&
               minPos.z <= maxP.z &&
               maxPos.z >= minP.z
               )
        {
            //print("i " + i);
            return true;
        }


        return false; // No overlapping
    }


    private Vector3 GenerateRandomPosition()
    {
        return
                transform.position + new Vector3(
                    Random.Range(-positionRange.x, positionRange.x),
                    Random.Range(floor.transform.position.y + positionRange.y / 2, positionRange.y),
                    Random.Range(-positionRange.z, positionRange.z)
                );
    }

    public Vector3 Minimum(Vector3 position, Vector3 scale)
    {
        Vector3 min = new Vector3(position.x - scale.x / 2, position.y - scale.y / 2, position.z - scale.z / 2);
        return min;
    }

    public Vector3 Maximum(Vector3 position, Vector3 scale)
    {
        Vector3 max = new Vector3(position.x + scale.x / 2, position.y + scale.y / 2, position.z + scale.z / 2);
        return max;
    }

    // Export Excel data
    private void PositionCSVExporter()
    {
        ExporterAndImporter exporter = new ExporterAndImporter(clonedObjects.ToArray());
        exporter.ExportPositionsToExcel();
    }
}
