using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private UnitType unitType;
    [SerializeField] float attackScanRadius; // This is used to determine if there are any units or buildings within range to start attacking (if not already targeting a unit or building).
    [SerializeField] Image healthBar;
    [SerializeField] ParticleSystem explosionVFX;
    [SerializeField] AudioSource persistentAudioSource;
    [SerializeField] AudioSource dynamicAudioSource;
    [SerializeField] AudioClip attackSound;

    public bool IsDead { get; private set; } = false;

    private Vector3 targetPosition = Vector3.zero;
    private Quaternion targetRotation = Quaternion.identity;

    private ISelectable targetObject;
    private Rigidbody rb;
    private EnemyWave wave;
    SquareGrid grid;
    Stack<Cell> waypoints;
    Cell currentWaypoint;

    private bool isMoving = false;
    private float health;
    private bool registeredToWave = false;
    private bool isAttacking = false;

    Coroutine attackRoutine;

    static Vector3 hivePosition = Vector3.zero;
    static Building hiveObject;

    Animator animator;

    

    // Start is called before the first frame update

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        animator.SetBool(Constants.Animations.EnemyFlying, true);
        persistentAudioSource.Play();
        if (hiveObject == null)
        {
            hiveObject = GameObject.FindGameObjectWithTag(Constants.Tags.Hive).GetComponent<Building>();
        }
    }

    void Start()
    {
        health = unitType.maxHealth;
        healthBar.type = Image.Type.Filled;
        healthBar.fillMethod = Image.FillMethod.Horizontal;
        healthBar.fillOrigin = (int)Image.OriginHorizontal.Left;
        rb = GetComponent<Rigidbody>();
        if (hivePosition == Vector3.zero)
        {
            hivePosition = GameObject.FindGameObjectWithTag("Hive").transform.position;
            hivePosition.y = 3;
        }
    }

    public void SetGrid(SquareGrid grid)
    {
        this.grid = grid;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsDead)
        {
            healthBar.fillAmount = Mathf.Clamp01(health / unitType.maxHealth);
            CheckNearbyTargets();
            if (isMoving)
            {
                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.Position, unitType.moveSpeed * Time.deltaTime);
                CorrectYPosition();
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, unitType.turnSpeed * Time.deltaTime);

                Debug.DrawLine(transform.position, currentWaypoint.Position, Color.blue);
                if (Mathf.Round(currentWaypoint.Position.x) == Mathf.Round(transform.position.x) && Mathf.Round(currentWaypoint.Position.z) == Mathf.Round(transform.position.z))
                {
                    if (!waypoints.TryPop(out currentWaypoint))
                    {
                        Debug.Log(targetObject);
                        isMoving = false;
                        animator.SetBool(Constants.Animations.EnemyMoving, false);

                        // Add to the associated wave once movement has ended.
                        if (!registeredToWave)
                        {
                            wave.AddUnitToWave(this);
                            registeredToWave = true;
                        }

                        // If a target object has been set, start attacking the object.
                        if (targetObject != null && !isAttacking)
                        {
                            isAttacking = true;
                            attackRoutine = StartCoroutine(Attack());
                        }
                    }
                }
            }
        }
    }

    void CorrectYPosition()
    {
        Vector3 rayStart = new Vector3(transform.position.x, transform.position.y + 10, transform.position.z);
        var hits = Physics.RaycastAll(rayStart, Vector3.down, 15); // Using RaycastAll as the ray should ignore everything except the terrain collider.
        foreach (var hit in hits)
        {
            if (hit.transform.gameObject.GetComponent<TerrainCollider>())
            {
                transform.position = new Vector3(transform.position.x, hit.point.y + 3, transform.position.z);
                break;
            }
        }
    }

    void Pathfind(bool emptyStartCell = true)
    {
        Cell targetCell = grid.GetClosestAvailableCellToPosition(targetPosition);
        Cell startCell = grid.GetCell(grid.WorldToCell(transform.position));

        targetPosition = targetCell.Position;

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

    void CheckNearbyTargets()
    {
        ISelectable closestTarget = null;
        float distanceToClosestTarget = 999;
        foreach (var collider in Physics.OverlapSphere(transform.position, attackScanRadius, Constants.UnitScanLayerMask))
        {
            // Ignore collisions on the following:
            // 1. This unit.
            // 2. The unit's spawner.
            // 3. The unit's current target (if they have one).
            // 4. Any dead units or buildings.
            if (collider.gameObject != gameObject &&
                (
                    (collider.gameObject.GetComponent<Unit>() && !collider.gameObject.GetComponent<Unit>().IsDead) ||
                    (collider.gameObject.GetComponent<Building>() && !collider.gameObject.GetComponent<EnemySpawner>() && !collider.gameObject.GetComponent<Building>().IsDead)
                ) &&
                (targetObject == null || collider.gameObject != targetObject.GetGameObject())
            )
            {
                float distance = (transform.position - collider.transform.position).magnitude;

                if (distance < distanceToClosestTarget)
                {
                    distanceToClosestTarget = distance;
                    if (collider.gameObject.GetComponent<Unit>() != null)
                    {
                        closestTarget = collider.gameObject.GetComponent<Unit>();
                    }
                    else if (collider.gameObject.GetComponent<Building>() != null)
                    {
                        closestTarget = collider.gameObject.GetComponent<Building>();
                    }
                }
            }
        }

        if (closestTarget != null && closestTarget != targetObject)
        {
            Debug.Log("New target identified!");
            Vector3 faceDirection = (closestTarget.GetGameObject().transform.position - transform.position).normalized;

            Move(closestTarget.GetGameObject().transform.position, Quaternion.FromToRotation(transform.forward, faceDirection), closestTarget);
        }
    }

    public void Move(Cell target, Quaternion targetRotation, ISelectable targetObject = null, bool emptyStartCell = true)
    {
        animator.SetBool(Constants.Animations.EnemyMoving, true);
        targetPosition = target.Position;
        this.targetRotation = targetRotation;
        this.targetObject = targetObject;

        Pathfind(emptyStartCell);
        if (waypoints.TryPop(out currentWaypoint))
        {
            isMoving = true;
        }
    }

    public void Move(Vector3 targetPosition, Quaternion targetRotation, ISelectable targetObject = null, bool emptyStartCell = true)
    {
        animator.SetBool(Constants.Animations.EnemyMoving, true);
        this.targetPosition = targetPosition;
        this.targetRotation = targetRotation;
        this.targetObject = targetObject;

        Pathfind(emptyStartCell);
        if (waypoints.TryPop(out currentWaypoint))
        {
            isMoving = true;
        }
    }

    public void AssignToWave(EnemyWave wave)
    {
        this.wave = wave;
    }

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(unitType.attackRate);
        if (targetObject != null && targetObject.GetGameObject() != null && isAttacking)
        {
            if (targetObject.GetGameObject().GetComponent<Unit>())
            {
                if (!targetObject.GetGameObject().GetComponent<Unit>().IsDead)
                {
                    animator.SetTrigger(Constants.Animations.EnemyAttacking);
                    dynamicAudioSource.PlayOneShot(attackSound);
                    targetObject.GetGameObject().GetComponent<Unit>().TakeDamage(gameObject, unitType.baseDamage);
                    attackRoutine = StartCoroutine(Attack());
                }
                else
                {
                    // Retarget the hive.
                    targetObject = null;
                    isAttacking = false;
                    Debug.Log("Re-targeting the hive!");
                    TargetHive();
                }
            }
            else if (targetObject.GetGameObject().CompareTag(Constants.Tags.Hive))
            {
                if (!targetObject.GetGameObject().GetComponent<Building>().IsDead)
                {
                    animator.SetTrigger(Constants.Animations.EnemyAttacking);
                    dynamicAudioSource.PlayOneShot(attackSound);
                    targetObject.GetGameObject().GetComponent<Building>().TakeDamage(unitType.baseDamage);
                    attackRoutine = StartCoroutine(Attack());
                }
            }
        }
    }

    public void TargetHive()
    {
        hivePosition.y = 3;
        Vector3 lookDirection = -(transform.position - hivePosition).normalized;
        lookDirection.y = 0;
        Vector3 target = hivePosition + new Vector3(1, 0, 1);
        
        Move(target, Quaternion.FromToRotation(transform.forward, lookDirection));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackScanRadius);
    }

    public void TakeDamage(float dmg, GameObject source)
    {
        health = Mathf.Max(0, health - dmg);

        if (source.CompareTag(Constants.Tags.Hive))
        {
            explosionVFX.Play();
        }

        if (health <= 0)
        {
            OnDie();
        }
    }

    void OnDie()
    {
        animator.SetBool(Constants.Animations.EnemyFlying, false);
        animator.SetBool(Constants.Animations.EnemyMoving, false);
        persistentAudioSource.Stop();
        dynamicAudioSource.Stop();
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            isAttacking = false;
        }

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }
        IsDead = true;
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
}
