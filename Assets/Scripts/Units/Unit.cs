using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Unit : MonoBehaviour, ISelectable, PlayerControls.IHiveManagementActions
{
    public float moveSpeed;
    Vector3 target = Vector3.zero;
    static Vector3 hivePosition = Vector3.zero;
    bool moving = false;
    float health;
    public UnitType type;
    GameObject targetObject;
    public Building AssociatedBuilding { get; private set; }
    static HexGrid hiveGrid;
    static SquareGrid overworldGrid;
    bool harvestMode = false;
    bool returningToHive = false;
    bool returningToNode = false;
    bool movingToBuilding = false;
    public bool InHiveMode { get; private set; } = true;
    ResourceStack stack;
    Rigidbody rb;
    public bool IsDead { get; private set; } = false;
    bool IsAttacking = false;
    bool SwitchingRole = false;
    [SerializeField] BuildingType[] buildings;
    [SerializeField] BuildingType emptyCell;
    static MapController mapController;
    BuildingType selectedBuilding;
    bool unitSelected = false;
    Vector2 cursorPosition;

    Coroutine attackRoutine;
    Coroutine harvestRoutine;

    PlayerControls playerControls;
    bool IsBuildMode = false;

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.HiveManagement.SetCallbacks(this);
        }
        playerControls.HiveManagement.Enable();
    }

    private void OnDisable()
    {
        playerControls.HiveManagement.Disable();
    }

    // Use this for initialization
    void Start()
    {
        if (mapController == null)
        {
            mapController = GameObject.Find("Kingdom Manager").GetComponent<MapController>();
        }
        health = type.maxHealth;
        if (hivePosition == Vector3.zero)
        {
            hivePosition = GameObject.FindGameObjectWithTag("Hive") ? GameObject.FindGameObjectWithTag("Hive").transform.position : Vector3.zero;
            hivePosition.y = 3;
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
            else if (SwitchingRole)
            {
                if (returningToHive)
                {
                    Vector3 newPosition = Vector3.MoveTowards(transform.position, hivePosition, moveSpeed * Time.deltaTime);

                    transform.forward = (hivePosition - transform.position).normalized;

                    transform.position = newPosition;

                    if (transform.position == hivePosition)
                    {
                        Vector3 exitPosition = hiveGrid.HexCellToWorld(hiveGrid.width / 2, 0);
                        exitPosition.z = -3.2f;
                        transform.position = exitPosition;
                        transform.forward = hiveGrid.transform.forward;
                        InHiveMode = true;
                        returningToHive = false;
                        movingToBuilding = true;
                    }
                }
                else if (movingToBuilding)
                {
                    Vector3 buildingPosition = AssociatedBuilding.transform.position;
                    buildingPosition.z = -3.2f;
                    Vector3 newPosition = Vector3.MoveTowards(transform.position, buildingPosition, moveSpeed * Time.deltaTime);

                    transform.forward = (buildingPosition - transform.position).normalized;

                    transform.position = newPosition;

                    if (transform.position == buildingPosition)
                    {
                        returningToHive = false;
                        movingToBuilding = false;
                        BuildingType.SwitchUnit(this);
                        SwitchingRole = false;
                    }
                }
            }
        }
    }

    public void SelectUnit()
    {
        unitSelected = true;
    }

    public void DeselectUnit()
    {
        unitSelected = false;
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
        
        if (IsHiveMode)
        {
            // Maintain same position on the XY plane.
            target.z = -3.2f;
        }
        else
        {
            // Maintain same position on the XZ plane.
            var targetCell = overworldGrid.GetClosestAvailableCellToPosition(position, 1, 5);
            targetCell.MarkCellAsOccupied();
            target = targetCell.Position;
            target.y = 3;
        }

        moving = true;
    }

    public void OnSelect()
    {
        string resource = stack.resource ? stack.resource.displayName + " - " + stack.quantity : "None";
        Debug.Log($"{type.label} selected. Current health: {health}. Current resources collected: {resource}");
        unitSelected = true;
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
                    Vector3 exitPosition = hiveGrid.HexCellToWorld(hiveGrid.width / 2, 0);
                    exitPosition.z = -3.2f;
                    transform.position = exitPosition;
                    transform.forward = hiveGrid.transform.forward;
                    InHiveMode = true;
                    if (SwitchingRole)
                    {
                        returningToHive = false;
                        movingToBuilding = true;
                    }
                }
                else if (targetObject.GetComponent<Building>().type.label == "Hive Exit")
                {
                    // Move the bee to the overworld.
                    transform.position = new Vector3(hivePosition.x + 10, hivePosition.y + 3, hivePosition.z + 10);
                    transform.forward = Vector3.forward;
                    InHiveMode = false;
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
        if (targetObject.activeSelf)
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

        if (targetObject.activeSelf)
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

    public void OnAction1(InputAction.CallbackContext context)
    {
        if (context.performed && unitSelected)
        {
            type.PerformAction(1, this);
        }
    }

    public void OnAction2(InputAction.CallbackContext context)
    {
        if (context.performed && unitSelected)
        {
            type.PerformAction(2, this);
        }
    }

    public void OnAction3(InputAction.CallbackContext context)
    {
        if (context.performed && unitSelected)
        {
            type.PerformAction(3, this);
        }
    }

    public void OnAction4(InputAction.CallbackContext context)
    {
        if (context.performed && unitSelected)
        {
            type.PerformAction(4, this);
        }
    }

    public void OnAction5(InputAction.CallbackContext context)
    {
        if (context.performed && unitSelected)
        {
            type.PerformAction(5, this);
        }
    }

    public void OnAction6(InputAction.CallbackContext context)
    {
        if (context.performed && unitSelected)
        {
            type.PerformAction(6, this);
        }
    }

    public void OnAction7(InputAction.CallbackContext context)
    {
        if (context.performed && unitSelected)
        {
            type.PerformAction(7, this);
        }
    }

    public void OnAction8(InputAction.CallbackContext context)
    {
        if (context.performed && unitSelected)
        {
            type.PerformAction(8, this);
        }
    }

    public void OnAction9(InputAction.CallbackContext context)
    {
        if (context.performed && unitSelected)
        {
            type.PerformAction(9, this);
        }
    }

    public void OnPlaceBuilding(InputAction.CallbackContext context)
    {
        if (context.performed && selectedBuilding != null && IsBuildMode && mapController.IsHiveMode)
        {
            Ray ray = Camera.main.ScreenPointToRay(cursorPosition);
            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.collider.CompareTag("Building") && hit.transform.gameObject.GetComponent<Building>().type == emptyCell)
                {
                    if (selectedBuilding.PlaceBuilding(hiveGrid, hit.transform.gameObject.GetComponent<HexCell>().Index))
                    {
                        Debug.Log($"{selectedBuilding.label} has been built.");
                        // Spend resources on the building. In a future commit, the allocated cells will require a worker bee present to construct the building over time.

                        // Switch off build mode.
                        IsBuildMode = false;
                        mapController.EnableInput();
                        selectedBuilding = null;
                    }
                    else
                    {
                        Debug.Log("Cannot place building here.");
                    }
                }
            }
        }
    }

    public void OnCancelBuild(InputAction.CallbackContext context)
    {
        if (context.performed && IsBuildMode)
        {
            IsBuildMode = false;
            mapController.EnableInput();
            selectedBuilding = null;
            Debug.Log("Build mode cancelled.");
        }
    }

    public void OnCursor(InputAction.CallbackContext context)
    {
        cursorPosition = context.ReadValue<Vector2>();
    }

    public void SelectBuilding(int buildingIndex)
    {
        if (InHiveMode)
        {
            IsBuildMode = true;
            mapController.DisableInput();
            selectedBuilding = buildings[buildingIndex];
            Debug.Log($"Build mode active. {type.label} selected.");
        }
    }

    public void ExchangeUnit(int buildingIndex)
    {
        if (buildingIndex != -1)
        {
            Building target = HexGrid.FindFirstUnallocatedBuildingOfType(buildings[buildingIndex]);
            if (target)
            {
                AssociatedBuilding = target;
            }
            else
            {
                Debug.Log("Could not change unit role.");
                return;
            }
        }
        harvestMode = false;
        returningToHive = false;
        returningToNode = false;
        moving = false;
        StopAllCoroutines();
        SwitchingRole = true;
        if (InHiveMode)
        {
            movingToBuilding = true;
        }
        else
        {
            returningToHive = true;
        }
    }

    public void PerformSpecialAction(int actionIndex)
    {
        if (actionIndex == 6)
        {
            // Revert to worker bee.
            ExchangeUnit(-1);

            Destroy(gameObject);
        }
    }

    public void AttachBuilding(Building building)
    {
        AssociatedBuilding = building;
    }

    public void DetachCurrentBuilding()
    {
        AssociatedBuilding = null;
    }
}
