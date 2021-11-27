using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Unit Type")]
public class UnitType : ColonyObject
{
    [SerializeField] public float overworldMoveSpeed;
    [SerializeField] public float hiveMoveSpeed;
    [SerializeField] public float turnSpeed;
    [SerializeField] public float attackRate = 1;
    [SerializeField] public int baseDamage = 5;
    [SerializeField] public bool isBuilder = false;
    [SerializeField] public BuildingType buildingType;

    public void PerformAction(int actionID, Unit instance)
    {
        if (isBuilder)
        {
            if (actionID <= 5)
            {
                instance.SelectBuilding(actionID - 1);
            }
            else
            {
                switch (actionID)
                {
                    case 6:
                    case 7:
                        // Attach to first available building.
                        instance.ExchangeUnit(actionID - 4);
                        break;
                    default:
                        break;
                }
            }
            
        }
        else
        {
            instance.PerformSpecialAction(actionID);
        }
    }
}