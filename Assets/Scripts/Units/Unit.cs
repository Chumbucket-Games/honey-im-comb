using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Unit : MonoBehaviour, ISelectable, IMoveable, PlayerControls.IHiveManagementActions
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
    bool warnedOfAttack = false;
    [SerializeField] float warningCooldown = 10;
    [SerializeField] BuildingType[] buildings;
    [SerializeField] BuildingType emptyCell;
    [SerializeField] ResourceType honeyResource;
    [SerializeField] ResourceType pebbleResource;
    [SerializeField] GameObject selectionRing;
    [SerializeField] Camera selectionView;
    [SerializeField] Image healthBar;
    [SerializeField] Notification reassignmentNotification;
    [SerializeField] Notification underAttackNotification;
    [SerializeField] Notification buildingCompleteNotification;
    static MapController mapController;
    BuildingType selectedBuilding;
    bool unitSelected = false;
    Vector2 cursorPosition;

    Coroutine attackRoutine;
    Coroutine harvestRoutine;

    PlayerControls playerControls;
    bool IsBuildMode = false;

    Stack<Cell> waypoints;
    Cell currentWaypoint;

    Animator animator;

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

    public void SetTargetObject(GameObject targetObject)
    {
        this.targetObject = targetObject;
    }

    // Use this for initialization
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
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
        transform.forward = Vector3.up;

        healthBar.type = Image.Type.Filled;
        healthBar.fillMethod = Image.FillMethod.Horizontal;
        healthBar.fillOrigin = (int)Image.OriginHorizontal.Left;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsDead)
        {
            healthBar.fillAmount = Mathf.Clamp01(health / type.maxHealth);
            if (moving)
            {
                if (targetObject != null && targetObject.GetComponent<Unit>())
                {
                    // If targeting a unit, the unit is likely moving so keep the target vector aligned with the unit's position and recalc pathfinding.
                    target = targetObject.transform.position;
                    Pathfind();
                }
                
                
                if (!InHiveMode)
                {
                    Vector3 newPosition = Vector3.MoveTowards(transform.position, currentWaypoint.Position, moveSpeed * Time.deltaTime);
                    Vector3 forward = (currentWaypoint.Position - transform.position).normalized;
                    forward.y = 0;
                    if (forward != Vector3.zero)
                    {
                        transform.forward = Vector3.Slerp(transform.forward, forward, Time.deltaTime * type.turnSpeed);
                    }
                    
                    transform.position = newPosition;
                    CorrectYPosition();
                    if (Mathf.Round(transform.position.x) == Mathf.Round(currentWaypoint.Position.x) && Mathf.Round(transform.position.z) == Mathf.Round(currentWaypoint.Position.z))
                    {
                        // If all waypoints have been iterated through, stop movement.
                        if (!waypoints.TryPop(out currentWaypoint))
                        {
                            moving = false;
                            animator.SetBool("Moving", false);
                            DidReachDestination();
                        }
                    }
                }
                else
                {
                    Vector3 newPosition = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation((target - transform.position).normalized, Vector3.back), Time.deltaTime * type.turnSpeed);
                    transform.position = newPosition;
                    if (transform.position == target)
                    {
                        target = Vector3.zero;
                        moving = false;
                        animator.SetBool("Moving", false);
                        DidReachDestination();
                    }
                }
            }
            else if (harvestMode)
            {
                if (returningToHive)
                {
                    Vector3 newPosition = Vector3.MoveTowards(transform.position, hivePosition, moveSpeed * Time.deltaTime);

                    Vector3 forward = (hivePosition - transform.position).normalized;
                    forward.y = 0;
                    transform.forward = Vector3.Slerp(transform.forward, forward, Time.deltaTime * type.turnSpeed);

                    transform.position = newPosition;
                    CorrectYPosition();

                    if (Mathf.Floor(transform.position.x) == Mathf.Floor(hivePosition.x) && Mathf.Floor(transform.position.z) == Mathf.Floor(hivePosition.z))
                    {
                        returningToHive = false;
                        harvestRoutine = StartCoroutine(WaitToLeave(5));
                    }
                }
                else if (returningToNode)
                {
                    Vector3 newPosition = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                    Vector3 forward = (targetObject.transform.position - transform.position).normalized;
                    forward.y = 0;
                    transform.forward = Vector3.Slerp(transform.forward, forward, Time.deltaTime * type.moveSpeed);

                    transform.position = newPosition;
                    CorrectYPosition();

                    if (Mathf.Floor(transform.position.x) == Mathf.Floor(target.x) && Mathf.Floor(transform.position.z) == Mathf.Floor(target.z))
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

                    Vector3 forward = Vector3.Slerp(transform.position, (hivePosition - transform.position).normalized, Time.deltaTime * type.moveSpeed);
                    forward.y = 0;
                    transform.forward = forward;

                    transform.position = newPosition;
                    CorrectYPosition();

                    if (Mathf.Floor(transform.position.x) == Mathf.Floor(hivePosition.x) && Mathf.Floor(transform.position.z) == Mathf.Floor(hivePosition.z))
                    {
                        Vector3 exitPosition = hiveGrid.HexCellToWorld(hiveGrid.width / 2, 0);
                        animator.SetBool("Flying", false);
                        exitPosition.z = GameConstants.HiveUnitOffset;
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
                    buildingPosition.z = GameConstants.HiveUnitOffset;
                    Vector3 newPosition = Vector3.MoveTowards(transform.position, buildingPosition, moveSpeed * Time.deltaTime);

                    transform.forward = Vector3.Slerp(transform.forward, (buildingPosition - transform.position).normalized, Time.deltaTime * type.moveSpeed);

                    transform.position = newPosition;

                    if (transform.position == buildingPosition)
                    {
                        returningToHive = false;
                        movingToBuilding = false;
                        BuildingType.SwitchUnit(this);
                        HUDManager.GetInstance().CreateNotification(reassignmentNotification);
                        SwitchingRole = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Maintains the unit's height above the ground, even when the ground height varies.
    /// </summary>
    void CorrectYPosition()
    {
        Vector3 rayStart = new Vector3(transform.position.x, transform.position.y + 10, transform.position.z);
        var hits = Physics.RaycastAll(rayStart, Vector3.down, 15); // Using RaycastAll as the ray should ignore everything except the terrain collider.
        foreach (var hit in hits)
        {
            if (hit.transform.gameObject.GetComponent<TerrainCollider>())
            {
                transform.position = new Vector3(transform.position.x, hit.point.y + GameConstants.OverworldUnitOffset, transform.position.z);
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 rayStart = new Vector3(transform.position.x, transform.position.y + 10, transform.position.z);
        Gizmos.DrawLine(rayStart, rayStart + (Vector3.down * 15));
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
            animator.SetBool("Flying", false);
            animator.SetBool("Moving", true);
            if (info.transform.gameObject.GetComponent<HexCell>().IsOccupied)
            {
                return;
            }
            target.z = GameConstants.HiveUnitOffset;
        }
        else
        {
            if (currentWaypoint != null && currentWaypoint.IsOccupied)
            {
                currentWaypoint.EmptyCell();
            }
            animator.SetBool("Flying", true);
            animator.SetBool("Moving", true);
            Pathfind();
            currentWaypoint = waypoints.Pop();
            Debug.Log(waypoints.Count);
        }
        if (targetObject != null && targetObject.GetComponent<HexCell>())
        {
            // Mark the current cell as unoccupied.
            targetObject.GetComponent<HexCell>().IsOccupied = false;
        }

        targetObject = info.transform.gameObject;
        if (targetObject.GetComponent<HexCell>() && targetObject.GetComponent<Building>().type.canOccupy)
        {
            // Mark the destination as occupied.
            targetObject.GetComponent<HexCell>().IsOccupied = true;
        }
        moving = true;
    }

    void Pathfind(Cell cell = null)
    {
        Cell targetCell;
        if (cell == null)
        {
            targetCell = overworldGrid.GetClosestAvailableCellToPosition(target, 5, 5);
        }
        else
        {
            targetCell = cell;
        }
        
        Cell startCell = overworldGrid.GetClosestAvailableCellToPosition(transform.position, 5, 5);

        Node startNode = new Node(startCell);
        Node endNode = new Node(targetCell);

        // Find the shortest path to the target.
        waypoints = SquareGrid.FindPath(startNode, endNode);
        targetCell.OccupyCell();
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
            animator.SetBool("Flying", false);
            animator.SetBool("Moving", true);
            target.z = GameConstants.HiveUnitOffset;
            moving = true;
        }
        else
        {
            if (currentWaypoint != null && currentWaypoint.IsOccupied)
            {
                currentWaypoint.EmptyCell();
            }
            // Maintain same position on the XZ plane.
            animator.SetBool("Flying", true);
            animator.SetBool("Moving", true);
            var targetCell = overworldGrid.GetClosestAvailableCellToPosition(position, 1, 5);
            target = targetCell.Position;
            target.y = position.y;
            Pathfind(targetCell);
            if (waypoints.TryPop(out currentWaypoint))
            {
                moving = true;
            }
        }
    }

    public void OnSelect()
    {
        string resource = stack.resource ? stack.resource.displayName + " - " + stack.quantity : "None";
        Debug.Log($"{type.label} selected. Current health: {health}. Current resources collected: {resource}");
        // Bring up the UI for the selected unit.
        unitSelected = true;
        if (selectionRing != null)
        {
            selectionRing.gameObject.SetActive(true);
        }
        //HUDManager.GetInstance().SetSelectedObjectImage(type.image);
        if (selectionView != null)
        {
           selectionView.gameObject.SetActive(true);
        }
        HUDManager.GetInstance().SetSelectedObjectDetails(type.label, (int)health, stack.resource == pebbleResource ? stack.quantity : 0, stack.resource == honeyResource ? stack.quantity : 0);
        HUDManager.GetInstance().SetActionImages(type.actionSprites);
    }

    public void OnDeselect()
    {
        // Dismiss UI for selected unit and remove selection ring.
        unitSelected = false;
        if (selectionRing != null)
        {
            selectionRing.gameObject.SetActive(false);
        }
        if (selectionView != null)
        {
           selectionView.gameObject.SetActive(false);
        }
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
                if (!InHiveMode)
                {
                    Vector3 forward = (targetObject.transform.position - transform.position).normalized;
                    forward.y = 0;
                    transform.forward = forward;
                }
                

                if (targetObject.GetComponent<Building>().type.label == "Hive")
                {
                    // Move the bee to the exit cell of the hive grid.
                    Vector3 exitPosition = hiveGrid.HexCellToWorld(hiveGrid.width / 2, 0);
                    exitPosition.z = GameConstants.HiveUnitOffset;
                    transform.position = exitPosition;
                    transform.forward = hiveGrid.transform.up;
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
                    transform.position = new Vector3(hivePosition.x + 10, GameConstants.OverworldUnitOffset, hivePosition.z + 10);
                    transform.forward = Vector3.forward;
                    InHiveMode = false;
                    MoveToPosition(GameObject.FindGameObjectWithTag("Hive").transform.GetChild(0).transform.position, false);
                }
            }
            else if (targetObject.GetComponent<ResourceNode>() && type.isBuilder)
            {
                // Start harvesting the resource node.
                harvestMode = true;
                target = targetObject.transform.position;
                target.y = transform.position.y;
                Debug.Log($"Let the {targetObject.GetComponent<ResourceNode>().resource.displayName} harvest begin!");
                Vector3 forward = (targetObject.transform.position - transform.position).normalized;
                forward.y = 0;
                transform.forward = forward;
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
            if (unitSelected)
            {
                HUDManager.GetInstance().SetSelectedObjectResources(stack.resource == pebbleResource ? stack.quantity : 0, stack.resource == honeyResource ? stack.quantity : 0);
            }
            returningToHive = true;
        }
        else
        {
            harvestMode = false;
            stack.resource = null;
            stack.quantity = 0;
            if (unitSelected)
            {
                HUDManager.GetInstance().SetSelectedObjectResources(0, 0);
            }
        }
    }

    IEnumerator WaitToLeave(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        // Deposit the collected resource, leave the hive and return to the resource node.
        if (stack.resource == honeyResource)
        {
            mapController.ChangeHoney(stack.quantity, false);
        }
        else if (stack.resource == pebbleResource)
        {
            mapController.ChangePebbles(stack.quantity, false);
        }

        if (unitSelected)
        {
            HUDManager.GetInstance().SetSelectedObjectResources(0, 0);
        }

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
            animator.SetTrigger("Attack");

            targetObject.GetComponent<Enemy>().TakeDamage(type.baseDamage);
            if (!targetObject.GetComponent<Enemy>().IsDead)
            {
                attackRoutine = StartCoroutine(Attack());
            }
        }
        else if (targetObject.GetComponent<EnemySpawner>())
        {
            Debug.Log("Attacking spawner!");
            animator.SetTrigger("Attack");
            targetObject.GetComponent<EnemySpawner>().TakeDamage(type.baseDamage);
            if (!targetObject.GetComponent<EnemySpawner>().IsDead)
            {
                attackRoutine = StartCoroutine(Attack());
            }
        }
    }

    IEnumerator ResetWarnedStatus(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        warnedOfAttack = false;
    }


    public void TakeDamage(GameObject source, float dmg)
    {
        health = Mathf.Max(0, health - dmg);

        if (unitSelected)
        {
            HUDManager.GetInstance().SetSelectedObjectHealth((int)health);
        }
        else if (!warnedOfAttack)
        {
            warnedOfAttack = true;
            HUDManager.GetInstance().CreateNotification(underAttackNotification);

            // Reset the warned status after a fixed duration. This ensures that units raise the alarm if they are attacked more than once.
            StartCoroutine(ResetWarnedStatus(warningCooldown));
        }

        if (health <= 0)
        {
            if (unitSelected)
            {
                OnDeselect();
            }
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

        animator.SetBool("Flying", false);
        animator.SetBool("Moving", false);

        IsDead = true;
        HexGrid.DecreaseTotalUnits(1);
        
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
                        mapController.ChangePebbles(selectedBuilding.pebbles.quantity);
                        mapController.ChangeHoney(selectedBuilding.honey.quantity);

                        HUDManager.GetInstance().CreateNotification(buildingCompleteNotification);

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
        movingToBuilding = false;
        moving = false;
        StopAllCoroutines();
        SwitchingRole = true;
        animator.SetBool("Moving", true);
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

    public System.Type GetObjectType()
    {
        return this.GetType();
    }
}
