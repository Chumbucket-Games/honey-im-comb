using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour, ISelectable
{
    public float moveSpeed;
    Vector3 target = Vector3.zero;
    bool moving = false;
    float health;
    public UnitType type;

    // Use this for initialization
    void Start()
    {
        health = type.MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            transform.position = newPosition;

            if (transform.position == target)
            {
                target = Vector3.zero;
                moving = false;
                DidStopMoving();
            }
        }
    }

    public bool IsMovable()
    {
        return true;
    }

    public void MoveToPosition(Vector3 position)
    {
        target = position;
        target.z = transform.position.z;
        moving = true;
    }

    public void OnSelect()
    {
        Debug.Log($"{type.label} selected");
    }

    public void DidStopMoving()
    {
        Debug.Log($"{type.label} has reached its destination.");
    }
}
