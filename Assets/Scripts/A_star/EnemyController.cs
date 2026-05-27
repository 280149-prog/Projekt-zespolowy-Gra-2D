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

    [Header("Typ wroga")]
    public EEnemyType EnemyType = EEnemyType.Ground;

    [Header("Pathfinding")]
    public AStarPathfinder Pathfinder;
    public Transform PlayerTransform;

    [Header("Ruch przeciwnika")]
    public float MoveSpeed = 3f;
    public float WaypointThreshold = 0.2f;

    [Header("Sila skoku")]
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

    // Skok - TODO
    private bool _jumpRequested = false;
    private float _jumpCooldown = 0f;
    private const float JumpCooldownTime = 0.4f;

    private bool CanFly => EnemyType == EEnemyType.Flying;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Wylacz grawitacje dla latajacego przeciwnika
        if (CanFly)
        {
            _rb.gravityScale = 0f;
        }

        _currentSpeed = MoveSpeed;
    }

    private void Update()
    {
        if (PlayerTransform == null) return;

        float dist = Vector2.Distance(transform.position, PlayerTransform.position);
        if (DetectionRange > 0f && dist > DetectionRange) return;

        _pathRefreshTimer += Time.deltaTime;
        if (_pathRefreshTimer >= PathRefreshRate)
        {
            _pathRefreshTimer = 0f;
            RecalculatePath();
        }

        if (_jumpCooldown > 0f)
            _jumpCooldown -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (CanFly)
            FollowPath_Flying();
        else
            FollowPath_Ground();
    }

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
                // Dla przeciwnika latajacego sprawdzamy pelny dystans 2D oraz wektor kierunku
                if (Vector2.Distance(currentPos, targetPos) < WaypointThreshold)
                {
                    skipFirstNode = true;
                }
                else
                {
                    Vector2 toTarget = (targetPos - currentPos).normalized;
                    Vector2 toNext = (nextTargetPos - targetPos).normalized;

                    // Jesli iloczyn skalarny jest ujemny -> targetPos za nami
                    if (Vector2.Dot(toTarget, toNext) < 0f)
                        skipFirstNode = true;
                }
            }
            else
            {
                // Dla przeciwnika nielatajacego os X
                if (Mathf.Abs(currentPos.x - targetPos.x) < WaypointThreshold)
                {
                    skipFirstNode = true;
                }
                // Jesli znak kierunku do obecnego punktu jest inny niż do nastepnego -> miniety
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

    private void FollowPath_Flying()
    {
        if (_path == null || _pathIndex >= _path.Count)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        var (targetPos, isCorner) = _path[_pathIndex];
        Vector2 target = new Vector2(targetPos.x, targetPos.y);
        Vector2 current = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = (target - current).normalized;

        // Corner slowdown: oblicz docelowa prędkosc
        float targetSpeed = ComputeTargetSpeed(isCorner, direction);
        _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedVelocity, AccelerationSmoothTime);

        _rb.MovePosition(current + direction * _currentSpeed * Time.fixedDeltaTime);

        FlipSprite(direction.x);

        if (Vector2.Distance(current, target) < WaypointThreshold)
            _pathIndex++;
    }
    
    private void FollowPath_Ground()
    {
        if (_path == null || _pathIndex >= _path.Count)
        {
            // Wygaszaj predkosc pozioma (grawitacja zatrzyma pionowy ruch)
            _rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(_rb.linearVelocity.x, 0f, MoveSpeed * 4f * Time.fixedDeltaTime),
                _rb.linearVelocity.y
            );
            return;
        }

        var (targetPos, isCorner) = _path[_pathIndex];
        Vector2 target = new Vector2(targetPos.x, targetPos.y);
        Vector2 current = new Vector2(transform.position.x, transform.position.y);

        float horizontalDir = Mathf.Sign(target.x - current.x);

        // Corner slowdown (tylko dla zmiany kierunku poziomego)
        float targetSpeed = ComputeTargetSpeed(isCorner, new Vector2(horizontalDir, 0f));
        _currentSpeed = Mathf.SmoothDamp(
            _currentSpeed, targetSpeed, ref _speedVelocity, AccelerationSmoothTime);

        // Ustaw velocity X, zachowaj Y (grawitacja, skok)
        _rb.linearVelocity = new Vector2(horizontalDir * _currentSpeed, _rb.linearVelocity.y);

        FlipSprite(horizontalDir);

        // Skok - TODO
        bool nextIsHigher = (target.y - current.y) > 0.3f;

        if (nextIsHigher && IsGrounded() && _jumpCooldown <= 0f)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f); // reset Y przed skokiem
            _rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            _jumpCooldown = JumpCooldownTime;
        }

        // Przejscie do nastepnego waypointa — dla naziemnego sprawdzamy tylko X
        if (Mathf.Abs(current.x - target.x) < WaypointThreshold)
            _pathIndex++;
    }

    private float ComputeTargetSpeed(bool isCorner, Vector2 currentDir)
    {
        if (!isCorner || _pathIndex + 1 >= _path.Count)
            return MoveSpeed;

        Vector2 nextTarget = new Vector2(
            _path[_pathIndex + 1].position.x,
            _path[_pathIndex + 1].position.y);
        Vector2 cornerTarget = new Vector2(
            _path[_pathIndex].position.x,
            _path[_pathIndex].position.y);
        Vector2 dirAfter = (nextTarget - cornerTarget).normalized;

        // dot: 1 = prosto, 0 = 90deg, -1 = zawroc
        float dot = Vector2.Dot(currentDir, dirAfter);

        // Normalizuj: dot 1 -> 0 slowdown, dot -1 -> max slowdown
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
        // Zasieg detekcji
        if (DetectionRange > 0f)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
            Gizmos.DrawWireSphere(transform.position, DetectionRange);
        }

        // Punkt groundChecka
        if (EnemyType == EEnemyType.Ground)
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawWireSphere(
                (Vector2)transform.position + GroundCheckOffset,
                GroundCheckRadius);
        }

        if (!DrawPathGizmos || _path == null || _path.Count == 0) return;

        // Sciezka
        for (int i = 0; i < _path.Count - 1; i++)
        {
            Gizmos.color = _path[i].isCorner
                ? new Color(1f, 0.8f, 0f)   // zolty = zakret
                : new Color(1f, 0.2f, 0.2f); // czerwony = normalny

            Gizmos.DrawLine(_path[i].position, _path[i + 1].position);
        }

        // Aktualny cel
        if (_pathIndex < _path.Count)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_path[_pathIndex].position, 0.18f);
        }

        // Koniec sciezki
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_path[_path.Count - 1].position, 0.22f);
    }
}