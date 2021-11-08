using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] AnimationCurve spawnRate;
    [SerializeField] Enemy objectToSpawn;
    [SerializeField] BuildingType buildingType;
    [SerializeField] SquareGrid rallyPoint;

    private bool spawningEnabled = false;
    private float nextSpawnTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        EnableSpawning();
    }

    // Update is called once per frame
    void Update()
    {
        if (spawningEnabled && Time.time >= nextSpawnTime)
        {
            Spawn();
            float currentSpawnRate = spawnRate.Evaluate(Time.time / 60f);
            nextSpawnTime = Time.time + (60f / currentSpawnRate);
        }
    }

    private void Spawn()
    {
        var spawnedInstance = Instantiate(objectToSpawn, transform.position, Quaternion.identity);

        if (!rallyPoint.IsGridFull)
        {
            var rallyGridCell = rallyPoint.GetNextAvailableCell();
            rallyGridCell.OccupyCell();
            spawnedInstance.Move(rallyGridCell.Position, rallyPoint.transform.rotation);
        }
        else
        {
            // create a new grid or send the wave?
        }
    }

    public void EnableSpawning()
    {
        spawningEnabled = true;
    }

    public void DisableSpawning()
    {
        spawningEnabled = false;
    }
}
