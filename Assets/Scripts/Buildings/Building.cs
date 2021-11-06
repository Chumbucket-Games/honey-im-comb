using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour, ISelectable
{
    float health;
    public BuildingType type;
    public bool IsMovable()
    {
        return false;
    }

    public void MoveToPosition(Vector3 position, RaycastHit info)
    {
        throw new System.InvalidOperationException();
    }

    public void OnSelect()
    {
        Debug.Log($"{type.label} selected.");
    }

    // Use this for initialization
    void Start()
    {
        health = type.MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
