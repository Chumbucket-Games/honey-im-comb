using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Unit Type")]
public class UnitType : ColonyObject
{
    [SerializeField] public float moveSpeed;
    [SerializeField] public float turnSpeed;

    public void AttackEnemy(/*Unit self, Enemy e*/)
    {

    }

    public void InteractWithBuilding(Unit self, Building target)
    {

    }
}