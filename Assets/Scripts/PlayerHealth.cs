using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 3;
    private bool _isPlaying = true;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask enemyLayer;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            health--;
            Debug.Log("Zycia: " + health);
        }
    }

    void Update()
    {
        if (health <= 0 && _isPlaying)
        {
            Debug.Log("Koniec gry!");
            _isPlaying = false;
        }
    }
}
