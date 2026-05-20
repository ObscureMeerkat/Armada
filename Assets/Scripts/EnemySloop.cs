using UnityEngine;

public class EnemySloop : MonoBehaviour
{
    public enum EnemyState { Idle, Patrol, Combat }

    [Header("State Settings")]
    public EnemyState currentState = EnemyState.Idle;
    public float idleToPatrolChance = 0.3f;
    public float idleToPatrolCheckInterval = 5f;
    public float detectionRange = 10f;
    public float combatExitRange = 12f;

    [Header("Patrol Settings")]
    public float patrolSpeed = 1f;
    public float patrolRotationSpeed = 30f;
    public float patrolRadius = 5f;
    private Vector2 patrolCentre;
    private float patrolAngle = 0f;

    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float rotationSpeed = 150f;
    public float orbitDistance = 8f;
    public float orbitSpeed = 3f;

    [Header("Separation Settings")]
    public float separationRadius = 2.5f;
    public float separationForce = 3f;

    [Header("Combat Settings")]
    public GameObject cannonBallPrefab;
    public float fireRate = 3f;
    public float forwardArcDegrees = 30f;
    public float leadPredictionStrength = 0.5f;
    public float accuracySpread = 15f;

    [Header("Audio")]
    public AudioClip cannonFireSound;
    private AudioSource audioSource;

    private float nextFireTime = 0f;
    private float nextIdleCheckTime = 0f;
    private int orbitDirection = 1;

    private Rigidbody2D rb;
    private Transform player;
    private Rigidbody2D playerRb;
    private Camera mainCamera;
    private bool isAlerted = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody2D>();
        }

        audioSource = GetComponent<AudioSource>();

        // Set patrol centre to spawn position
        patrolCentre = transform.position;
        patrolAngle = Random.Range(0f, 360f);
        orbitDirection = Random.value > 0.5f ? 1 : -1;
        orbitDistance += Random.Range(-1f, 1f);

        // Randomly start in idle or patrol
        currentState = Random.value > 0.5f ? EnemyState.Idle : EnemyState.Patrol;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        CheckDetection();

        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Combat:
                UpdateCombat();
                break;
        }
    }

    // ── DETECTION ─────────────────────────────────────────────

    void CheckDetection()
    {
        if (currentState == EnemyState.Combat)
        {
            // Check if player has left combat range
            float distToPlayer = Vector2.Distance(transform.position,
                                                   player.position);
            if (distToPlayer > combatExitRange && !isAlerted)
            {
                currentState = EnemyState.Patrol;
                patrolCentre = transform.position;
            }
            return;
        }

        // Fire raycasts from both sides
        bool detected = CheckRaycast(transform.right) ||
                        CheckRaycast(-transform.right);

        if (detected)
        {
            EnterCombat();
        }
    }

    bool CheckRaycast(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            detectionRange,
            LayerMask.GetMask("Player")
        );

        return hit.collider != null;
    }

    public void AlertEnemy()
    {
        isAlerted = true;
        EnterCombat();
    }

    void EnterCombat()
    {
        currentState = EnemyState.Combat;
    }

    // ── IDLE ──────────────────────────────────────────────────

    void UpdateIdle()
    {
        // Gently slow to a stop
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 0.05f);

        // Occasionally switch to patrol
        if (Time.time >= nextIdleCheckTime)
        {
            nextIdleCheckTime = Time.time + idleToPatrolCheckInterval;
            if (Random.value < idleToPatrolChance)
            {
                currentState = EnemyState.Patrol;
                patrolCentre = transform.position;
            }
        }
    }

    // ── PATROL ────────────────────────────────────────────────

    void UpdatePatrol()
    {
        // Slowly circle around patrol centre
        patrolAngle += patrolRotationSpeed * Time.fixedDeltaTime * orbitDirection;

        Vector2 targetPos = patrolCentre + new Vector2(
            Mathf.Cos(patrolAngle * Mathf.Deg2Rad) * patrolRadius,
            Mathf.Sin(patrolAngle * Mathf.Deg2Rad) * patrolRadius
        );

        Vector2 directionToTarget = (targetPos - (Vector2)transform.position).normalized;
        float targetAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x)
                            * Mathf.Rad2Deg - 90f;
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle,
                            patrolRotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);
        rb.linearVelocity = transform.up * patrolSpeed;

        // Occasionally switch back to idle
        if (Time.time >= nextIdleCheckTime)
        {
            nextIdleCheckTime = Time.time + idleToPatrolCheckInterval;
            if (Random.value < idleToPatrolChance * 0.5f)
            {
                currentState = EnemyState.Idle;
            }
        }
    }

    // ── COMBAT ────────────────────────────────────────────────

    void UpdateCombat()
    {
        // Slow to a stop
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 0.05f);

        // Rotate side toward player
        RotateSideToPlayer();
        TryFire();

        // Check if player has left range
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer > combatExitRange && !isAlerted)
        {
            currentState = EnemyState.Patrol;
            patrolCentre = transform.position;
            isAlerted = false;
        }
    }

    void RotateSideToPlayer()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x)
                            * Mathf.Rad2Deg - 90f;

        // Offset by 90 degrees to present the side rather than the front
        float sideAngle = targetAngle + 90f;

        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, sideAngle,
                            rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);
    }

    void Move(float distanceToPlayer)
    {
        if (distanceToPlayer > orbitDistance)
        {
            rb.linearVelocity = transform.up * moveSpeed;
        }
        else
        {
            Vector2 directionToPlayer = (player.position -
                                         transform.position).normalized;
            Vector2 orbitTangent = new Vector2(
                -directionToPlayer.y * orbitDirection,
                directionToPlayer.x * orbitDirection
            );
            rb.linearVelocity = orbitTangent * orbitSpeed;
        }
    }

    void ApplySeparation()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position,
                                separationRadius, LayerMask.GetMask("Enemy"));

        Vector2 separationVector = Vector2.zero;

        foreach (Collider2D col in nearby)
        {
            if (col.gameObject == gameObject) continue;

            Vector2 awayFromNeighbour = (Vector2)(transform.position -
                                                   col.transform.position);
            float distance = awayFromNeighbour.magnitude;

            if (distance > 0)
                separationVector += awayFromNeighbour.normalized / distance;
        }

        if (separationVector != Vector2.zero)
            rb.AddForce(separationVector * separationForce);
    }

    bool IsOnScreen()
    {
        Vector3 screenPos = mainCamera.WorldToViewportPoint(transform.position);
        return screenPos.x >= 0f && screenPos.x <= 1f &&
               screenPos.y >= 0f && screenPos.y <= 1f;
    }

    void TryFire()
    {
        if (Time.time < nextFireTime)
        {
            return;
        }

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Mathf.Min(
            Vector2.Angle(transform.right, directionToPlayer),
            Vector2.Angle(-transform.right, directionToPlayer)
        );
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer > detectionRange)
        {
            return;
        }

        if (angleToPlayer <= forwardArcDegrees)
        {
            Fire();
        }
    }

    void Fire()
    {
        if (cannonBallPrefab == null) return;

        Vector2 playerVelocity = playerRb != null ? playerRb.linearVelocity
                                                   : Vector2.zero;
        Vector2 predictedPosition = (Vector2)player.position
                                    + playerVelocity * leadPredictionStrength;
        Vector2 fireDirection = (predictedPosition -
                                 (Vector2)transform.position).normalized;

        float spreadAngle = Random.Range(-accuracySpread, accuracySpread);
        fireDirection = Quaternion.Euler(0, 0, spreadAngle) * fireDirection;

        Vector3 spawnPos = transform.position +
                           (Vector3)(Vector2)transform.up * 0.6f;

        GameObject ball = Instantiate(cannonBallPrefab, spawnPos,
                                      Quaternion.identity);
        ball.layer = LayerMask.NameToLayer("EnemyCannonBall");
        CannonBall cb = ball.GetComponent<CannonBall>();
        if (cb != null) cb.Launch(fireDirection);

        if (audioSource != null && cannonFireSound != null)
            audioSource.PlayOneShot(cannonFireSound);

        nextFireTime = Time.time + fireRate;
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Vector2 pushDirection = (transform.position -
                                     other.transform.position).normalized;
            rb.AddForce(-pushDirection * 5f);
        }
    }
}