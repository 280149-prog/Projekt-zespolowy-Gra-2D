using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    public float speed = 4f;
    private bool _isMovingRight = true;


    void Update()
    {
        if (rb.position.x >= 3)
        {
            _isMovingRight = false;
        }

        if (rb.position.x <= -1)
        {
            _isMovingRight = true;
        }
    }

    private void FixedUpdate()
    {
        if (_isMovingRight)
        {
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
        }
    }
}
