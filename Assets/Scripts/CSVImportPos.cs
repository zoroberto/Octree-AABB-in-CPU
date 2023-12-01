using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSVImportPos : MonoBehaviour
{
    [Header("Clone Object")]
    public GameObject objectToClone;
    public string csvFileName = "object_positions.csv";

    //   Private Fields
    private GameObject clone;
    private List<Transform> meshTransform = new List<Transform>();
    private List<GameObject> games = new List<GameObject>();

    // Import excel data
    public List<Transform> ReadPositionFromExcel()
    {
        // view by column of data as csvData
        List<List<string>> csvData = ExporterAndImporter.ReadCSVFile(csvFileName);

        // Create a HashSet to store unique positions
        HashSet<Vector3> uniquePositions = new HashSet<Vector3>();

        meshTransform.Clear();
        for (int i = 1; i < csvData.Count; i++) // count = 3 x,y,z
        {
            List<string> row = csvData[i];

            for (int j = 1; j < row.Count; j++)
            {
                //string cellValue = row[j];

                // Use the cell value as needed
                //Debug.Log($"Row {i}, Column {j}: {cellValue}");


                float x = float.Parse(row[1]);
                float y = float.Parse(row[2]);
                float z = float.Parse(row[3]);

                Vector3 randomPosition = new Vector3(x, y, z);
                //Vector3 randomPosition = new Vector3(
                //        Random.Range(-x, x),
                //        Random.Range(-y, y),
                //        Random.Range(-z, z));

                if (!uniquePositions.Contains(randomPosition))
                {
                    uniquePositions.Add(randomPosition);


                    clone = Instantiate(objectToClone, randomPosition, Quaternion.identity);
                    clone.transform.SetParent(transform);
                    meshTransform.Add(clone.transform);
                    //print("clone " + meshTransform.Count);
                }

            }
        }

        return meshTransform;
    }

    public List<GameObject> GetGameObjects()
    {
        return games;
    }

    void Start()
    {
        ReadPositionFromExcel();
    }
}
