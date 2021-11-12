using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] AnimationCurve spawnRate;
    [SerializeField] AnimationCurve waveRowSize;
    [SerializeField] AnimationCurve waveColumnSize;
    [SerializeField] Enemy objectToSpawn;
    [SerializeField] BuildingType buildingType;
    [SerializeField] GameObject rallyPoint;
    [SerializeField] SquareGrid gameGrid;

    private bool spawningEnabled = false;
    private float nextSpawnTime = 0.0f;
    private EnemyWave currentWave;

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
            float currentSpawnRate = spawnRate.Evaluate(Time.time / 60f);
            nextSpawnTime = Time.time + (60f / currentSpawnRate);
            Spawn();
        }
        if (currentWave.isAttacking)
        {
            var waveDimensions = GetWaveDimensions();
            currentWave = new EnemyWave(waveDimensions);
        }
    }

    private void Spawn()
    {
        var spawnedInstance = Instantiate(objectToSpawn, transform.position, Quaternion.identity);
        spawnedInstance.AssignToWave(currentWave);

        if (!gameGrid.IsGridFull)
        {
            var rallyGridCell = gameGrid.GetClosestAvailableCellToPosition(rallyPoint.transform.position, 
                Mathf.CeilToInt(currentWave.WaveDimensions.x / 2f), Mathf.CeilToInt(currentWave.WaveDimensions.y / 2f));

            if (rallyGridCell != null)
            {
                rallyGridCell.MarkCellAsOccupied();

                // This rotation will turn the enemy to face the direction it is moving rather than having it face the same way as the rally point grid.
                Vector3 faceDirection = (rallyGridCell.Position - spawnedInstance.transform.position).normalized;
                faceDirection.y = 0;
                spawnedInstance.Move(rallyGridCell.Position, Quaternion.FromToRotation(spawnedInstance.transform.forward, faceDirection));
            }
        }
    }

    public void EnableSpawning()
    {
        spawningEnabled = true;

        var waveDimensions = GetWaveDimensions();
        currentWave = new EnemyWave(waveDimensions);
    }

    public void DisableSpawning()
    {
        spawningEnabled = false;
    }

    private Vector2Int GetWaveDimensions()
    {
        var rowSize = Mathf.FloorToInt(waveRowSize.Evaluate(Time.time / 60f));
        var colSize = Mathf.FloorToInt(waveColumnSize.Evaluate(Time.time / 60f));

        return new Vector2Int(rowSize, colSize);
    }
}
