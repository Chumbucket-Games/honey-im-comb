using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyWave
{
    public List<Enemy> enemies;
    public uint waveSize;
    public bool Attacking { get; private set; } = false;
    // Use this for initialization
    public EnemyWave(uint size)
    {
        waveSize = size;
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

        Attacking = true;
    }
}
