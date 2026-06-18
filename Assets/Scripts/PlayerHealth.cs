using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("Health")]
    public int healths = 3;
    [SerializeField] private LayerMask instantDeathMask;

    private bool _isPlaying = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            healths--;
            Debug.Log("Zycia: " + healths);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsInLayerMask(other.gameObject.layer, instantDeathMask))
        {
            healths = 0;
            Debug.Log("Instant death hazard!");
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

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
}
