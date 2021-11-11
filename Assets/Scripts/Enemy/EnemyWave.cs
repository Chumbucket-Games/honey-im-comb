using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyWave
{
    [SerializeField] private List<Enemy> enemies;
    
    public bool isAttacking { get; private set; } = false;

    private uint waveSize = 0;

    // Use this for initialization
    public EnemyWave(uint waveSize)
    {
        this.waveSize = waveSize;
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
            e.TargetHive();
        }

        isAttacking = true;
    }
}
