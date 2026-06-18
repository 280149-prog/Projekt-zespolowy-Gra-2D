using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    public enum EEnemyType
    {
        Ground,
        Flying
    }
    public enum EEnemyState
    {
        Patrolling,
        Chasing
    }

    [Header("Typ i Stan wroga")]
    public EEnemyType EnemyType = EEnemyType.Ground;
    public EEnemyState CurrentState = EEnemyState.Patrolling;

    [Header("Pathfinding (Flying Chase Only)")]
    public AStarPathfinder Pathfinder;
    public Transform PlayerTransform;

    [Header("Ruch przeciwnika (Baza dla Chase)")]
    public float MoveSpeed = 3f;
    public float WaypointThreshold = 0.2f;

    [Header("Sila skoku (Baza dla Chase)")]
    public float JumpForce = 7f;

    [Tooltip("Warstwa podloza do detekcji IsGrounded.")]
    public LayerMask GroundMask;

    [Tooltip("Promien kola do detekcji IsGrounded pod stopami przeciwnika.")]
    public float GroundCheckRadius = 0.15f;

    [Tooltip("Offset punktu sprawdzania podloza.")]
    public Vector2 GroundCheckOffset = new Vector2(0f, -0.5f);

    [Header("Wygladzanie zakretow")]
    [Tooltip("Wspolczynnik zwalniania na zakrecie (0 = brak, 1 = zatrzymanie).")]
    [Range(0f, 0.85f)]
    public float CornerSlowdownFactor = 0.45f;

    [Tooltip("Jak szybko wrog przyspiesza po zakrecie.")]
    public float AccelerationSmoothTime = 0.12f;

    [Header("Poscig")]
    public float PathRefreshRate = 0.5f;
    public float DetectionRange = 15f;

    [Header("Patrol (Modyfikatory)")]
    [Tooltip("Mnoznik predkosci MoveSpeed podczas patrolu.")]
    public float PatrolSpeedMultiplier = 0.6f;
    [Tooltip("Mnoznik sily skoku JumpForce podczas patrolu (tylko Ground).")]
    public float PatrolJumpMultiplier = 0.8f;
    [Tooltip("Czas w sekundach marszu/skakania w jednym kierunku podczas patrolu.")]
    public float PatrolDirectionTime = 1.0f;

    [Header("Wizualizacja")]
    public SpriteRenderer SpriteRenderer;

    [Header("Debug")]
    public bool DrawPathGizmos = true;

    private Rigidbody2D _rb;

    // Sciezka z flagami zakretow
    private List<(Vector3 position, bool isCorner)> _path = new List<(Vector3, bool)>();

    private int _pathIndex = 0;
    private float _pathRefreshTimer = 0f;

    // Wygladzanie predkosci
    private float _currentSpeed;
    private float _speedVelocity; // ref dla SmoothDamp

    // Skok i cooldowny
    private float _jumpCooldown = 0f;
    private const float JumpCooldownTime = 0.4f;

    // Zmienne pomocnicze dla patrolu
    private float _patrolTimer = 0f;
    private float _patrolDirectionX = -1f;

    private bool CanFly => EnemyType == EEnemyType.Flying;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (CanFly)
        {
            _rb.gravityScale = 0f;
        }

        _currentSpeed = MoveSpeed;
    }

    private void Update()
    {
        if (_jumpCooldown > 0f)
            _jumpCooldown -= Time.deltaTime;

        if (PlayerTransform != null && DetectionRange > 0f)
        {
            float dist = Vector2.Distance(transform.position, PlayerTransform.position);

            if (dist <= DetectionRange)
            {
                if (CurrentState == EEnemyState.Patrolling)
                {
                    CurrentState = EEnemyState.Chasing;
                    _pathRefreshTimer = PathRefreshRate;
                }
            }
            else
            {
                if (CurrentState == EEnemyState.Chasing)
                {
                    CurrentState = EEnemyState.Patrolling;
                    _patrolTimer = 0f;
                    _patrolDirectionX = -1f;
                }
            }
        }

        if (CurrentState == EEnemyState.Chasing && CanFly && PlayerTransform != null)
        {
            _pathRefreshTimer += Time.deltaTime;
            if (_pathRefreshTimer >= PathRefreshRate)
            {
                _pathRefreshTimer = 0f;
                RecalculatePath();
            }
        }

        if (CurrentState == EEnemyState.Patrolling)
        {
            _patrolTimer += Time.deltaTime;
            if (_patrolTimer >= PatrolDirectionTime)
            {
                _patrolTimer = 0f;
                _patrolDirectionX *= -1f;
            }
        }
    }

    private void FixedUpdate()
    {
        if (CanFly)
        {
            if (CurrentState == EEnemyState.Chasing)
                FollowPath_Flying();
            else
                Patrol_Flying();
        }
        else
        {
            if (CurrentState == EEnemyState.Chasing)
                FollowPath_Ground();
            else
                Patrol_Ground();
        }
    }

    #region LOGIKA POŚCIGU (CHASE MODE)

    private void FollowPath_Flying()
    {
        if (_path == null || _pathIndex >= _path.Count)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 current = transform.position;
        Vector2 target = _path[_pathIndex].position;

        float distance = Vector2.Distance(current, target);
        bool isCorner = _path[_pathIndex].isCorner;
        float currentThreshold = isCorner ? 0.06f : WaypointThreshold;

        if (distance < currentThreshold)
        {
            _pathIndex++;

            if (isCorner)
            {
                _rb.linearVelocity = Vector2.zero;
            }

            if (_pathIndex >= _path.Count) return;
            target = _path[_pathIndex].position;
        }

        Vector2 direction = (target - current).normalized;
        float speedMod = isCorner ? (1f - CornerSlowdownFactor) : 1f;

        _rb.linearVelocity = direction * MoveSpeed * speedMod;
        FlipSprite(direction.x);
    }

    private void FollowPath_Ground()
    {
        if (PlayerTransform == null)
        {
            StopHorizontalMovement();
            return;
        }

        Vector2 current = transform.position;
        Vector2 playerPos = PlayerTransform.position;

        float distanceX = playerPos.x - current.x;
        float horizontalDir = Mathf.Abs(distanceX) > 0.15f ? Mathf.Sign(distanceX) : 0f;

        if (horizontalDir != 0f)
        {
            FlipSprite(horizontalDir);
        }

        if (IsGrounded() && _jumpCooldown <= 0f && horizontalDir != 0f)
        {
            _rb.linearVelocity = Vector2.zero;

            bool isPlayerHigher = (playerPos.y - current.y) > 0.5f;
            float jumpForceUp = isPlayerHigher ? JumpForce * 1.25f : JumpForce;
            float jumpForceForward = MoveSpeed * 1.5f;

            Vector2 jumpImpulse = new Vector2(horizontalDir * jumpForceForward, jumpForceUp);
            _rb.AddForce(jumpImpulse, ForceMode2D.Impulse);

            _jumpCooldown = JumpCooldownTime > 0f ? JumpCooldownTime : 0.6f;
        }
        else if (IsGrounded())
        {
            _rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(_rb.linearVelocity.x, 0f, MoveSpeed * 8f * Time.fixedDeltaTime),
                _rb.linearVelocity.y
            );
        }
    }

    #endregion

    #region LOGIKA PATROLU (PATROL MODE)

    private void Patrol_Flying()
    {
        float patrolSpeed = MoveSpeed * PatrolSpeedMultiplier;

        _rb.linearVelocity = new Vector2(_patrolDirectionX * patrolSpeed, 0f);

        FlipSprite(_patrolDirectionX);
    }

    private void Patrol_Ground()
    {
        FlipSprite(_patrolDirectionX);

        if (IsGrounded() && _jumpCooldown <= 0f)
        {
            _rb.linearVelocity = Vector2.zero;

            float jumpForceUp = JumpForce * PatrolJumpMultiplier;
            float jumpForceForward = (MoveSpeed * PatrolSpeedMultiplier) * 1.2f;

            Vector2 jumpImpulse = new Vector2(_patrolDirectionX * jumpForceForward, jumpForceUp);
            _rb.AddForce(jumpImpulse, ForceMode2D.Impulse);

            _jumpCooldown = (JumpCooldownTime > 0f ? JumpCooldownTime : 0.6f) + 0.2f;
        }
        else if (IsGrounded())
        {
            _rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(_rb.linearVelocity.x, 0f, MoveSpeed * 8f * Time.fixedDeltaTime),
                _rb.linearVelocity.y
            );
        }
    }

    #endregion

    private void RecalculatePath()
    {
        if (PlayerTransform == null || Pathfinder == null) return;

        List<Vector3> raw = Pathfinder.FindPath(
            transform.position,
            PlayerTransform.position,
            CanFly
        );

        _path = AStarPathfinder.GetSmoothedPathWithFlags(raw);
        _pathIndex = 0;

        if (_path.Count > 1)
        {
            Vector2 currentPos = transform.position;
            Vector2 targetPos = _path[0].position;
            Vector2 nextTargetPos = _path[1].position;

            bool skipFirstNode = false;

            if (CanFly)
            {
                if (Vector2.Distance(currentPos, targetPos) < WaypointThreshold)
                {
                    skipFirstNode = true;
                }
                else
                {
                    Vector2 toTarget = (targetPos - currentPos).normalized;
                    Vector2 toNext = (nextTargetPos - targetPos).normalized;

                    if (Vector2.Dot(toTarget, toNext) < 0f)
                        skipFirstNode = true;
                }
            }
            else
            {
                if (Mathf.Abs(currentPos.x - targetPos.x) < WaypointThreshold)
                {
                    skipFirstNode = true;
                }
                else if (Mathf.Sign(targetPos.x - currentPos.x) != Mathf.Sign(nextTargetPos.x - targetPos.x))
                {
                    skipFirstNode = true;
                }
            }

            if (skipFirstNode)
            {
                _pathIndex = 1;
            }
        }
    }

    public void SetCustomTarget(Vector3 targetWorldPos)
    {
        List<Vector3> raw = Pathfinder.FindPath(transform.position, targetWorldPos, CanFly);
        _path = AStarPathfinder.GetSmoothedPathWithFlags(raw);
        _pathIndex = 0;
    }

    private void StopHorizontalMovement()
    {
        if (IsGrounded())
        {
            _rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(_rb.linearVelocity.x, 0f, MoveSpeed * 4f * Time.fixedDeltaTime),
                _rb.linearVelocity.y
            );
        }
    }

    private float ComputeTargetSpeed(bool isCorner, Vector2 currentDir)
    {
        if (!isCorner || _pathIndex + 1 >= _path.Count)
            return MoveSpeed;

        Vector2 nextTarget = new Vector2(_path[_pathIndex + 1].position.x, _path[_pathIndex + 1].position.y);
        Vector2 cornerTarget = new Vector2(_path[_pathIndex].position.x, _path[_pathIndex].position.y);
        Vector2 dirAfter = (nextTarget - cornerTarget).normalized;

        float dot = Vector2.Dot(currentDir, dirAfter);
        float slowdown = Mathf.InverseLerp(1f, -1f, dot) * CornerSlowdownFactor;

        return MoveSpeed * (1f - slowdown);
    }

    private bool IsGrounded()
    {
        Vector2 checkPos = (Vector2)transform.position + GroundCheckOffset;
        return Physics2D.OverlapCircle(checkPos, GroundCheckRadius, GroundMask);
    }

    private void FlipSprite(float directionX)
    {
        if (SpriteRenderer != null && Mathf.Abs(directionX) > 0.01f)
            SpriteRenderer.flipX = directionX < 0f;
    }

    private void OnDrawGizmos()
    {
        if (DetectionRange > 0f)
        {
            Gizmos.color = (CurrentState == EEnemyState.Chasing) ? new Color(1f, 0f, 0f, 0.4f) : new Color(1f, 0.5f, 0f, 1f);
            Gizmos.DrawWireSphere(transform.position, DetectionRange);
        }

        if (EnemyType == EEnemyType.Ground)
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawWireSphere((Vector2)transform.position + GroundCheckOffset, GroundCheckRadius);
        }

        if (CurrentState == EEnemyState.Patrolling || !DrawPathGizmos || _path == null || _path.Count == 0) return;

        for (int i = 0; i < _path.Count - 1; i++)
        {
            Gizmos.color = _path[i].isCorner ? new Color(1f, 0.8f, 0f) : new Color(1f, 0.2f, 0.2f);
            Gizmos.DrawLine(_path[i].position, _path[i + 1].position);
        }

        if (_pathIndex < _path.Count)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_path[_pathIndex].position, 0.18f);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_path[_path.Count - 1].position, 0.22f);
    }
}