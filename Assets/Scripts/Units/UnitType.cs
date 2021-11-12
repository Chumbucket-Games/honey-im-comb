using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Unit Type")]
public class UnitType : ColonyObject
{
    [SerializeField] public float moveSpeed;
    [SerializeField] public float turnSpeed;
    [SerializeField] public float attackRate = 1;
    [SerializeField] public int baseDamage = 5;
}