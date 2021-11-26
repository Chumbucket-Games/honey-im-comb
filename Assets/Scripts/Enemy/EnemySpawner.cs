using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] AnimationCurve spawnRate;
    [SerializeField] AnimationCurve waveRowSize;
    [SerializeField] AnimationCurve waveColumnSize;
    [SerializeField] Enemy objectToSpawn;
    [SerializeField] BuildingType buildingType;
    [SerializeField] GameObject rallyPoint;
    [SerializeField] SquareGrid gameGrid;
    [SerializeField] Image healthBar;

    private bool spawningEnabled = false;
    private float nextSpawnTime = 0.0f;
    private EnemyWave currentWave;
    float health;
    public bool IsDead { get; private set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        EnableSpawning();
        health = buildingType.maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.fillAmount = Mathf.Clamp01(health / buildingType.maxHealth);
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
        spawnedInstance.SetGrid(gameGrid);
        spawnedInstance.AssignToWave(currentWave);

        if (!gameGrid.IsGridFull)
        {
            var rallyGridCell = gameGrid.GetClosestAvailableCellToPosition(rallyPoint.transform.position);

            if (rallyGridCell != null)
            {
                // This rotation will turn the enemy to face the direction it is moving rather than having it face the same way as the rally point grid.
                Vector3 faceDirection = (rallyGridCell.Position - spawnedInstance.transform.position).normalized;
                faceDirection.y = 0;
                spawnedInstance.Move(rallyGridCell, Quaternion.FromToRotation(spawnedInstance.transform.forward, faceDirection), null, true);
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

    public void TakeDamage(float dmg)
    {
        health = Mathf.Max(0, health - dmg);

        if (health <= 0)
        {
            OnDestroyed();
        }
        // Destroying the game object is handled by the map controller to prevent any broken references.
    }

    public void OnDestroyed()
    {
        // Update the game state manager to indicate that a spawner has been destroyed. Win the game when all spawners are destroyed.
        IsDead = true;
        DisableSpawning();
    }
}
