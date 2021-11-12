using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private UnitType unitType;
    [SerializeField] float attackScanRadius; // This is used to determine if there are any units or buildings within range to start attacking (if not already targeting a unit or building).
    
    public bool IsDead { get; private set; } = false;

    private Vector3 targetPosition = Vector3.zero;
    private Quaternion targetRotation = Quaternion.identity;

    private GameObject targetObject;
    private Rigidbody rb;
    private EnemyWave wave;

    private bool isMoving = false;
    private float health;
    private bool registeredToWave = false;
    private bool isAttacking = false;

    Coroutine attackRoutine;

    static Vector3 hivePosition;

    // Start is called before the first frame update
    void Start()
    {
        health = unitType.maxHealth;
        rb = GetComponent<Rigidbody>();
        if (hivePosition == null)
        {
            hivePosition = GameObject.FindGameObjectWithTag("Hive") ? GameObject.FindGameObjectWithTag("Hive").transform.position : Vector3.zero;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsDead)
        {
            CheckNearbyTargets();
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

    void CheckNearbyTargets()
    {
        GameObject closestTarget = null;
        float distanceToClosestTarget = 999;
        foreach (var collider in Physics.OverlapSphere(transform.position, attackScanRadius))
        {
            // Ignore any collisions that aren't units, buildings or the hive
            if (collider.gameObject != gameObject && collider.gameObject != targetObject &&
                (
                    (collider.gameObject.GetComponent<Unit>() && !collider.gameObject.GetComponent<Unit>().IsDead) ||
                    (collider.gameObject.GetComponent<Building>() /* && !collider.gameObject.GetComponent<Building>().IsDead*/)
                )
            )
            {
                float distance = (transform.position - collider.transform.position).magnitude;

                if (distance < distanceToClosestTarget)
                {
                    distanceToClosestTarget = distance;
                    closestTarget = collider.gameObject;
                }
            }
        }

        if (closestTarget != null && closestTarget != targetObject)
        {
            Vector3 faceDirection = (closestTarget.transform.position - transform.position).normalized;
            Move(closestTarget.transform.position, Quaternion.FromToRotation(transform.forward, faceDirection), closestTarget);
        }
    }

    public void Move(Vector3 targetPosition, Quaternion targetRotation, GameObject targetObject = null)
    {
        isMoving = true;

        this.targetPosition = targetPosition;
        this.targetRotation = targetRotation;
        this.targetObject = targetObject;
    }

    public void AssignToWave(EnemyWave wave)
    {
        this.wave = wave;
    }

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(unitType.attackRate);
        if (targetObject.GetComponent<Unit>())
        {
            Debug.Log("Attacking bee!");
            targetObject.GetComponent<Unit>().TakeDamage(unitType.baseDamage);
            if (!targetObject.GetComponent<Unit>().IsDead)
            {
                attackRoutine = StartCoroutine(Attack());
            }
            else
            {
                // Retarget the hive.
                targetObject = null;
                Debug.Log("Re-targeting the hive!");
                TargetHive();
            }
        }
        else if (targetObject.CompareTag("Building"))
        {
            /*targetObject.GetComponent<Building>().TakeDamage(unitType.baseDamage);
            if (!targetObject.GetComponent<Building>().IsDead)
            {
                attackRoutine = Attack();
                StartCoroutine(attackRoutine);
            }*/
        }
    }

    public void TargetHive()
    {
        hivePosition.y = transform.position.y;
        Vector3 lookDirection = -(transform.position - hivePosition).normalized;
        lookDirection.y = 0;
        Vector3 target = hivePosition;
        foreach (var hit in Physics.RaycastAll(transform.position, lookDirection, 100))
        {
            if (hit.collider.CompareTag("Hive"))
            {
                target = hit.point - Vector3.one; // Set the target to one unit away from the collision point.
                target.y = transform.position.y;
                break;
            }
        }
        Move(target, Quaternion.FromToRotation(transform.forward, lookDirection));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackScanRadius);
    }

    public void TakeDamage(float dmg)
    {
        health = Mathf.Max(0, health - dmg);

        if (health <= 0)
        {
            OnDie();
        }
    }

    void OnDie()
    {
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
