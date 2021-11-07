using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private UnitType unitType;

    private Vector3 targetPosition = Vector3.zero;
    private Quaternion targetRotation = Quaternion.identity;

    private bool isMoving = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            if (targetPosition != transform.position)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, unitType.moveSpeed * Time.deltaTime);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, unitType.turnSpeed * Time.deltaTime);
                
                Debug.DrawLine(transform.position, targetPosition, Color.blue);
            }
            else
            {
                isMoving = false;
            }
        }
    }

    public void Move(Vector3 targetPosition, Quaternion targetRotation)
    {
        Debug.Log("Spawned, moving enemy");
        isMoving = true;

        this.targetPosition = targetPosition;
        this.targetRotation = targetRotation;
    }
}
