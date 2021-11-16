using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour, ISelectable
{
    public float moveSpeed;
    Vector3 target = Vector3.zero;
    static Vector3 hivePosition;
    bool moving = false;
    float health;
    public UnitType type;
    GameObject targetObject;
    static HexGrid hiveGrid;
    static SquareGrid overworldGrid;
    bool harvestMode = false;
    bool returningToHive = false;
    bool returningToNode = false;
    ResourceStack stack;
    Rigidbody rb;
    public bool IsDead { get; private set; } = false;
    bool IsAttacking = false;

    Coroutine attackRoutine;
    Coroutine harvestRoutine;

    // Use this for initialization
    void Start()
    {
        health = type.maxHealth;
        if (hivePosition == null)
        {
            hivePosition = GameObject.FindGameObjectWithTag("Hive") ? GameObject.FindGameObjectWithTag("Hive").transform.position : Vector3.zero;
        }
        
        if (hiveGrid == null)
        {
            hiveGrid = GameObject.FindGameObjectWithTag("HexGrid") ? GameObject.FindGameObjectWithTag("HexGrid").GetComponent<HexGrid>() : null;
        }

        if (overworldGrid == null)
        {
            overworldGrid = GameObject.Find("Game Grid").GetComponent<SquareGrid>();
        }
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsDead)
        {
            if (moving)
            {
                if (targetObject != null && targetObject.GetComponent<Unit>())
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
                        harvestRoutine = StartCoroutine(WaitToLeave(5));
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
                        harvestRoutine = StartCoroutine(WaitToReturn(5));
                    }
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
            target.y = 3;
        }
        
        targetObject = info.transform.gameObject;
        moving = true;
    }

    public void MoveToPosition(Vector3 position, bool IsHiveMode)
    {
        harvestMode = false;
        returningToHive = false;
        returningToNode = false;
        targetObject = null;
        StopAllCoroutines();
        var targetCell = overworldGrid.GetClosestAvailableCellToPosition(position, 1, 5);
        targetCell.MarkCellAsOccupied();
        target = targetCell.Position;
        if (IsHiveMode)
        {
            // Maintain same position on the XY plane.
            target.z = transform.position.z;
        }
        else
        {
            // Maintain same position on the XZ plane.
            target.y = 3;
        }

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
        // Dismiss UI for selected unit and remove selection ring.
    }

    public void DidReachDestination()
    {
        Debug.Log($"{type.label} has reached its destination.");
        
        if (targetObject != null)
        {
            if (targetObject.GetComponent<Building>())
            {
                // Interact with the building.
                Debug.Log($"Interacting with {targetObject.GetComponent<Building>().type.label}!");
                transform.forward = (targetObject.transform.position - transform.position).normalized;

                if (targetObject.GetComponent<Building>().type.label == "Hive")
                {
                    // Move the bee to the exit cell of the hive grid.
                    transform.position = hiveGrid.HexCellToWorld(hiveGrid.width / 2, 0);
                    transform.forward = hiveGrid.transform.forward;
                }
                else if (targetObject.GetComponent<Building>().type.label == "Hive Exit")
                {
                    // Move the bee to the overworld.
                    transform.position = new Vector3(hivePosition.x + 10, hivePosition.y + 3, hivePosition.z + 10);
                    transform.forward = Vector3.forward;
                    MoveToPosition(GameObject.FindGameObjectWithTag("Hive").transform.GetChild(0).transform.position, false);
                }
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
                harvestRoutine = StartCoroutine(WaitToReturn(5));
            }
            else if (targetObject.GetComponent<EnemySpawner>() || targetObject.GetComponent<Enemy>())
            {
                IsAttacking = true;
                StartCoroutine(Attack());
            }
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

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(type.attackRate);
        
        if (targetObject.GetComponent<Unit>())
        {
            Debug.Log("Attacking enemy!");
            targetObject.GetComponent<Enemy>().TakeDamage(type.baseDamage);
            if (!targetObject.GetComponent<Enemy>().IsDead)
            {
                attackRoutine = StartCoroutine(Attack());
            }
        }
        else if (targetObject.GetComponent<EnemySpawner>())
        {
            Debug.Log("Attacking spawner!");
            targetObject.GetComponent<EnemySpawner>().TakeDamage(type.baseDamage);
            if (!targetObject.GetComponent<EnemySpawner>().IsDead)
            {
                attackRoutine = StartCoroutine(Attack());
            }
        }
    }

    public void TakeDamage(GameObject source, float dmg)
    {
        health = Mathf.Max(0, health - dmg);

        if (health <= 0)
        {
            OnDie();
        }

        // If the bee is not currently attacking anything, attack the enemy that attacked it.
        if (!IsAttacking)
        {
            targetObject = source;
            IsAttacking = true;
        }
    }

    void OnDie()
    {
        if (harvestRoutine != null)
        {
            StopCoroutine(harvestRoutine);
        }

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
        }
        
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        IsDead = true;
        
        StartCoroutine(DelayDestroy(5));
    }

    IEnumerator DelayDestroy(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
}
