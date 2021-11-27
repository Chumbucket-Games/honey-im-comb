using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] AudioClip attackSound;
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
    [SerializeField] AudioSource persistentAudioSource; // This plays persistent audio that should not change often eg. buzzing for wings
    [SerializeField] AudioSource dynamicAudioSource; // This plays audio that often changes.

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
    }

    void OverworldMove()
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
    }

    void HiveMove()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation((target - transform.position).normalized, Vector3.back), Time.deltaTime * type.turnSpeed);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
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
                    OverworldMove();
                    if (Mathf.Round(transform.position.x) == Mathf.Round(currentWaypoint.Position.x) && Mathf.Round(transform.position.z) == Mathf.Round(currentWaypoint.Position.z))
                    {
                        // If all waypoints have been iterated through, stop movement.
                        if (!waypoints.TryPop(out currentWaypoint))
                        {
                            moving = false;
                            animator.SetBool(Constants.Animations.BeeMoving, false);
                            DidReachDestination();
                        }
                    }
                }
                else
                {
                    HiveMove();
                    if (transform.position == target)
                    {
                        target = Vector3.zero;
                        moving = false;
                        animator.SetBool(Constants.Animations.BeeMoving, false);
                        DidReachDestination();
                    }
                }
            }
            else if (harvestMode)
            {
                if (returningToHive)
                {
                    OverworldMove();
                    if (Mathf.Round(transform.position.x) == Mathf.Round(currentWaypoint.Position.x) && Mathf.Round(transform.position.z) == Mathf.Round(currentWaypoint.Position.z))
                    {
                        if (!waypoints.TryPop(out currentWaypoint))
                        {
                            returningToHive = false;
                            harvestRoutine = StartCoroutine(WaitToLeave(5));
                        }
                    }
                }
                else if (returningToNode)
                {
                    OverworldMove();
                    
                    if (Mathf.Round(transform.position.x) == Mathf.Round(currentWaypoint.Position.x) && Mathf.Round(transform.position.z) == Mathf.Round(currentWaypoint.Position.z))
                    {
                        if (!waypoints.TryPop(out currentWaypoint))
                        {
                            dynamicAudioSource.clip = targetObject.GetComponent<ResourceNode>().harvestSound;
                            dynamicAudioSource.loop = true;
                            dynamicAudioSource.Play();
                            returningToNode = false;
                            
                            harvestRoutine = StartCoroutine(WaitToReturn(5));
                        }
                    }
                }
            }
            else if (SwitchingRole)
            {
                if (returningToHive)
                {
                    OverworldMove();

                    if (Mathf.Round(transform.position.x) == Mathf.Round(currentWaypoint.Position.x) && Mathf.Round(transform.position.z) == Mathf.Round(currentWaypoint.Position.z))
                    {
                        if (!waypoints.TryPop(out currentWaypoint))
                        {
                            Vector3 exitPosition = hiveGrid.HexCellToWorld(hiveGrid.width / 2, 0);
                            animator.SetBool(Constants.Animations.BeeFlying, false);
                            persistentAudioSource.Stop();
                            exitPosition.z = Constants.HiveUnitOffset;
                            transform.position = exitPosition;
                            transform.forward = hiveGrid.transform.forward;
                            InHiveMode = true;
                            returningToHive = false;
                            movingToBuilding = true;
                        }
                    }
                }
                else if (movingToBuilding)
                {
                    Vector3 buildingPosition = AssociatedBuilding.transform.position;
                    buildingPosition.z = Constants.HiveUnitOffset;
                    Vector3 newPosition = Vector3.MoveTowards(transform.position, buildingPosition, moveSpeed * Time.deltaTime);

                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation((buildingPosition - transform.position).normalized, Vector3.back), Time.deltaTime * type.moveSpeed);

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
                transform.position = new Vector3(transform.position.x, hit.point.y + Constants.OverworldUnitOffset, transform.position.z);
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

    public void MoveToPosition(Vector3 position, RaycastHit info, bool IsHiveMode, bool emptyStartCell = true)
    {
        harvestMode = false;
        returningToHive = false;
        returningToNode = false;
        StopAllCoroutines();
        target = position;
        if (IsHiveMode)
        {
            // Maintain same position on the XY plane.
            animator.SetBool(Constants.Animations.BeeFlying, false);
            animator.SetBool(Constants.Animations.BeeMoving, true);
            persistentAudioSource.Stop();
            if (info.transform.gameObject.GetComponent<HexCell>().IsOccupied)
            {
                return;
            }
            target.z = Constants.HiveUnitOffset;
        }
        else
        {
            if (currentWaypoint != null && currentWaypoint.IsOccupied)
            {
                currentWaypoint.EmptyCell();
            }
            animator.SetBool(Constants.Animations.BeeFlying, true);
            animator.SetBool(Constants.Animations.BeeMoving, true);
            persistentAudioSource.Play();
            Pathfind(null, emptyStartCell);
            currentWaypoint = waypoints.Pop();
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

    void Pathfind(Cell cell = null, bool emptyStartCell = true)
    {
        Cell targetCell;
        if (cell == null)
        {
            targetCell = overworldGrid.GetClosestAvailableCellToPosition(target);
        }
        else
        {
            targetCell = cell;
        }

        Cell startCell = overworldGrid.GetCell(overworldGrid.WorldToCell(transform.position));

        Node startNode = new Node(startCell);
        Node endNode = new Node(targetCell);

        // Find the shortest path to the target.
        waypoints = SquareGrid.FindPath(startNode, endNode);
        if (waypoints.Count > 0)
        {
            targetCell.OccupyCell();
            if (emptyStartCell)
            {
                startCell.EmptyCell();
            }
        }
    }

    void Pathfind(Vector3 target, bool emptyStartCell = true)
    {
        Cell targetCell = overworldGrid.GetClosestAvailableCellToPosition(target);
        Cell startCell = overworldGrid.GetCell(overworldGrid.WorldToCell(transform.position));

        Node startNode = new Node(startCell);
        Node endNode = new Node(targetCell);

        // Find the shortest path to the target.
        waypoints = SquareGrid.FindPath(startNode, endNode);
        if (waypoints.Count > 0)
        {
            targetCell.OccupyCell();
            if (emptyStartCell)
            {
                startCell.EmptyCell();
            }
        }
    }

    public void MoveToPosition(Vector3 position, bool IsHiveMode, bool emptyStartCell = true)
    {
        harvestMode = false;
        returningToHive = false;
        returningToNode = false;
        targetObject = null;
        StopAllCoroutines();
        
        if (IsHiveMode)
        {
            // Maintain same position on the XY plane.
            animator.SetBool(Constants.Animations.BeeFlying, false);
            animator.SetBool(Constants.Animations.BeeMoving, true);
            persistentAudioSource.Stop();
            target.z = Constants.HiveUnitOffset;
            moving = true;
        }
        else
        {
            if (currentWaypoint != null && currentWaypoint.IsOccupied)
            {
                currentWaypoint.EmptyCell();
            }
            // Maintain same position on the XZ plane.
            animator.SetBool(Constants.Animations.BeeFlying, true);
            animator.SetBool(Constants.Animations.BeeMoving, true);
            persistentAudioSource.Play();
            var targetCell = overworldGrid.GetClosestAvailableCellToPosition(position);
            target = targetCell.Position;
            target.y = position.y;
            Pathfind(targetCell, emptyStartCell);
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
        HUDManager.GetInstance().SetSelectedObjectDetails(type.label, (int)health, stack.resource == pebbleResource ? stack.quantity : 0, stack.resource == honeyResource ? stack.quantity : 0);
        HUDManager.GetInstance().SetActionImages(type.actionSprites);
        HUDManager.GetInstance().ShowSelectedObjectDetails(selectionView);
        
    }

    public void OnDeselect()
    {
        // Dismiss UI for selected unit and remove selection ring.
        unitSelected = false;
        if (selectionRing != null)
        {
            selectionRing.gameObject.SetActive(false);
        }
        HUDManager.GetInstance().SetActionImages(null);
        HUDManager.GetInstance().HideSelectedObjectDetails(selectionView);
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
                

                if (targetObject.CompareTag(Constants.Tags.Hive))
                {
                    // Move the bee to the exit cell of the hive grid.
                    Vector3 exitPosition = hiveGrid.HexCellToWorld(hiveGrid.width / 2, 0);
                    exitPosition.z = Constants.HiveUnitOffset;
                    transform.position = exitPosition;
                    transform.forward = hiveGrid.transform.up;
                    InHiveMode = true;
                    animator.SetBool(Constants.Animations.BeeFlying, false);
                    persistentAudioSource.Stop();
                    dynamicAudioSource.Stop();
                    if (SwitchingRole)
                    {
                        returningToHive = false;
                        movingToBuilding = true;
                    }
                }
                else if (targetObject.GetComponent<Building>().type.label == "Hive Exit")
                {
                    // Move the bee to the overworld.
                    transform.position = new Vector3(hivePosition.x + 10, Constants.OverworldUnitOffset, hivePosition.z + 10);
                    transform.forward = Vector3.forward;
                    InHiveMode = false;
                    MoveToPosition(GameObject.FindGameObjectWithTag(Constants.Tags.Hive).transform.GetChild(0).transform.position, false, false);
                }
            }
            else if (targetObject.CompareTag(Constants.Tags.ResourceNode) && type.isBuilder)
            {
                // Start harvesting the resource node.
                harvestMode = true;
                Debug.Log($"Let the {targetObject.GetComponent<ResourceNode>().resource.displayName} harvest begin!");
                target = targetObject.transform.position;
                Vector3 forward = (targetObject.transform.position - transform.position).normalized;
                forward.y = 0;
                transform.forward = forward;
                dynamicAudioSource.clip = targetObject.GetComponent<ResourceNode>().harvestSound;
                dynamicAudioSource.loop = true;
                dynamicAudioSource.Play();
                harvestRoutine = StartCoroutine(WaitToReturn(5));
            }
            else if (targetObject.GetComponent<EnemySpawner>() || targetObject.GetComponent<Enemy>() && !IsAttacking)
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
            if (unitSelected)
            {
                HUDManager.GetInstance().SetSelectedObjectResources(stack.resource == pebbleResource ? stack.quantity : 0, stack.resource == honeyResource ? stack.quantity : 0);
            }
            dynamicAudioSource.loop = false;
            dynamicAudioSource.Stop();
            Pathfind(hivePosition);
            currentWaypoint = waypoints.Pop();
            //if (waypoints.TryPop(out currentWaypoint))
            //{
            returningToHive = true;
            //}
        }
        else
        {
            harvestMode = false;
            stack.resource = null;
            stack.quantity = 0;
            currentWaypoint = null;
            if (unitSelected)
            {
                HUDManager.GetInstance().SetSelectedObjectResources(0, 0);
            }
        }
    }

    IEnumerator WaitToLeave(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        Debug.Log("Materials deposited. Returning to resource node.");

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

        if (targetObject.activeInHierarchy)
        {
            Pathfind(target);
            currentWaypoint = waypoints.Pop();
            returningToNode = true;
        }
        else
        {
            harvestMode = false;
            currentWaypoint = null;
        }
    }

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(type.attackRate);
        
        if (targetObject != null)
        {
            if (targetObject.GetComponent<Unit>())
            {
                animator.SetTrigger(Constants.Animations.BeeAttacking);
                dynamicAudioSource.PlayOneShot(attackSound);

                targetObject.GetComponent<Enemy>().TakeDamage(type.baseDamage, gameObject);
                if (!targetObject.GetComponent<Enemy>().IsDead)
                {
                    attackRoutine = StartCoroutine(Attack());
                }
            }
            else if (targetObject.GetComponent<EnemySpawner>())
            {
                animator.SetTrigger(Constants.Animations.BeeAttacking);
                dynamicAudioSource.PlayOneShot(attackSound);

                targetObject.GetComponent<EnemySpawner>().TakeDamage(type.baseDamage);
                if (!targetObject.GetComponent<EnemySpawner>().IsDead)
                {
                    attackRoutine = StartCoroutine(Attack());
                }
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

        animator.SetBool(Constants.Animations.BeeFlying, false);
        animator.SetBool(Constants.Animations.BeeMoving, false);
        persistentAudioSource.Stop();
        dynamicAudioSource.Stop();

        IsDead = true;
        HexGrid.DecreaseTotalUnits(1);

        if (currentWaypoint != null && currentWaypoint.IsOccupied)
        {
            currentWaypoint.EmptyCell();
        }
        while (waypoints.TryPop(out var cell))
        {
            // Unoccupy all remaining cells in the path.
            if (cell.IsOccupied)
            {
                cell.EmptyCell();
            }
        }

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
                if (hit.collider.CompareTag(Constants.Tags.Building) && hit.transform.gameObject.GetComponent<Building>().type == emptyCell)
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
        Building currentBuilding = AssociatedBuilding;
        if (buildingIndex != -1)
        {
            Building targetBuilding = HexGrid.FindFirstUnallocatedBuildingOfType(buildings[buildingIndex]);
            if (targetBuilding)
            {
                AssociatedBuilding = targetBuilding;
            }
            else
            {
                HUDManager.GetInstance().DisplayErrorMessage("Cannot reassign selected unit");
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
        animator.SetBool(Constants.Animations.BeeMoving, true);
        
        if (InHiveMode)
        {
            movingToBuilding = true;
        }
        else
        {
            Cell targetCell = overworldGrid.GetClosestAvailableCellToPosition(hivePosition);
            Pathfind(targetCell);
            if (waypoints.TryPop(out currentWaypoint))
            {
                returningToHive = true;
            }
            else
            {
                Debug.Log("Unable to enter hive. Unit not reassigned.");
                AssociatedBuilding = currentBuilding;
            }
        }
    }

    public void PerformSpecialAction(int actionIndex)
    {
        if (actionIndex == 1)
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

    public GameObject GetGameObject()
    {
        if (gameObject != null)
            return gameObject;
        return null;
    }
}
