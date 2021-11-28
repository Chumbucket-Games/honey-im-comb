using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyWave
{
    private List<Enemy> enemies;
    
    public bool isAttacking { get; private set; } = false;

    public Vector2Int WaveDimensions { get; private set; } = Vector2Int.zero;
    private int waveSize = 0;

    // Use this for initialization
    public EnemyWave(Vector2Int waveDimensions)
    {
        this.WaveDimensions = waveDimensions;
        this.waveSize = waveDimensions.x * waveDimensions.y;
        enemies = new List<Enemy>();
    }

    // Update is called once per frame
    public void AddUnitToWave(Enemy e)
    {
        enemies.Add(e);
        if (enemies.Count == waveSize)
        {
            LaunchAttack();
        }
    }

    void LaunchAttack()
    {
        foreach (var e in enemies)
        {
            if (e != null && e.gameObject != null)
            {
                e.TargetHive();
            }
        }

        isAttacking = true;
    }
}
