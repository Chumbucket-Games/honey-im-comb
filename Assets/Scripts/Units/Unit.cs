using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour, ISelectable
{
    public float moveSpeed;
    Vector3 target = Vector3.zero;
    Vector3 hivePosition;
    bool moving = false;
    float health;
    public UnitType type;
    GameObject targetObject;
    bool harvestMode = false;
    bool returningToHive = false;
    bool returningToNode = false;
    ResourceStack stack;

    // Use this for initialization
    void Start()
    {
        health = type.MaxHealth;
        hivePosition = GameObject.FindGameObjectWithTag("Hive") ? GameObject.FindGameObjectWithTag("Hive").transform.position : Vector3.zero;
        hivePosition.y = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            if (targetObject.GetComponent<Unit>())
            {
                // If targeting a unit, the unit is likely moving so keep the target vector aligned with the unit's position.
                target = targetObject.transform.position;
            }
            Vector3 newPosition = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            transform.forward = (target - transform.position).normalized;

            transform.position = newPosition;

            if (transform.position == target)
            {
                target = Vector3.zero;
                moving = false;
                DidReachDestination();
            }
        }
        else if (harvestMode)
        {
            if (returningToHive)
            {
                Vector3 newPosition = Vector3.MoveTowards(transform.position, hivePosition, moveSpeed * Time.deltaTime);

                transform.forward = (hivePosition - transform.position).normalized;

                transform.position = newPosition;

                if (transform.position == hivePosition)
                {
                    returningToHive = false;
                    StartCoroutine(WaitToLeave(5));
                }
            }
            else if (returningToNode)
            {
                Vector3 newPosition = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                transform.forward = (target - transform.position).normalized;

                transform.position = newPosition;

                if (transform.position == target)
                {
                    returningToNode = false;
                    StartCoroutine(WaitToReturn(5));
                }
            }
        }
    }

    public bool IsMovable()
    {
        return true;
    }

    public void MoveToPosition(Vector3 position, RaycastHit info, bool IsHiveMode)
    {
        harvestMode = false;
        returningToHive = false;
        returningToNode = false;
        StopAllCoroutines();
        target = position;
        if (IsHiveMode)
        {
            // Maintain same position on the XY plane.
            target.z = transform.position.z;
        }
        else
        {
            // Maintain same position on the XZ plane.
            target.y = transform.position.y;
        }
        
        targetObject = info.transform.gameObject;
        moving = true;
    }

    public void OnSelect()
    {
        string resource = stack.resource ? stack.resource.displayName + " - " + stack.quantity : "None";
        Debug.Log($"{type.label} selected. Current health: {health}. Current resources collected: {resource}");
        // Bring up the UI for the selected unit.
    }

    public void OnDeselect()
    {

    }

    public void DidReachDestination()
    {
        Debug.Log($"{type.label} has reached its destination.");
        
        if (targetObject.GetComponent<Building>())
        {
            // Interact with the building.
            Debug.Log($"Interacting with {targetObject.GetComponent<Building>().type.label}!");
            transform.forward = (targetObject.transform.position - transform.position).normalized;
        }
        else if (targetObject.GetComponent<Unit>())
        {
            // Interact with the unit.
            Debug.Log($"Interacting with {targetObject.GetComponent<Unit>().type.label}!");
            transform.forward = (targetObject.transform.position - transform.position).normalized;
        }
        else if (targetObject.GetComponent<ResourceNode>())
        {
            // Start harvesting the resource node.
            harvestMode = true;
            target = targetObject.transform.position;
            target.y = transform.position.y;
            Debug.Log($"Let the {targetObject.GetComponent<ResourceNode>().resource.displayName} harvest begin!");
            transform.forward = (targetObject.transform.position - transform.position).normalized;
            StartCoroutine(WaitToReturn(5));
        }
    }

    IEnumerator WaitToReturn(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        // Extract the collected resource based on the collection rate, then return to the hive with it.
        if (targetObject.activeInHierarchy)
        {
            stack.resource = targetObject.GetComponent<ResourceNode>().resource;
            stack.quantity = targetObject.GetComponent<ResourceNode>().HarvestResources(seconds);
            returningToHive = true;
        }
        else
        {
            harvestMode = false;
            stack.resource = null;
            stack.quantity = 0;
        }
    }

    IEnumerator WaitToLeave(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        // Deposit the collected resource, leave the hive and return to the resource node.
        stack.resource = null;
        stack.quantity = 0;

        if (targetObject.activeInHierarchy)
        {
            returningToNode = true;
        }
        else
        {
            harvestMode = false;
        }
    }
}
