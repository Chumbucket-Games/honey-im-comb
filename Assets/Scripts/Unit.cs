using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour, ISelectable
{
    public string unitType;
    public float moveSpeed;
    Vector3 target = Vector3.zero;
    bool moving = false;

    // Use this for initialization
    void Start()
    {

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
        Debug.Log($"{unitType} selected");
    }

    public void DidStopMoving()
    {
        Debug.Log($"{unitType} has reached its destination.");
    }
}
