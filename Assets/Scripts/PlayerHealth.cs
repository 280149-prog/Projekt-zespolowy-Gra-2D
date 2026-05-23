using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("Health")]
    public int healths = 3;
    private bool _isPlaying = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            healths--;
            Debug.Log("Zycia: " + healths);
        }
    }

    void Update()
    {
        // Testowo
        if (healths <= 0 && _isPlaying)
        {
            Debug.Log("Koniec gry!");
            _isPlaying = false;
        }
    }
}
