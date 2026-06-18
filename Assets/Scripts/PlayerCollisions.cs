using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    [Header("Components")]
    private PlayerHealth _playerHealth;
    private HandleCollisions _effects;

    [Header("Water")]
    public LayerMask waterLayer;

    [Header("Ice")]
    public LayerMask iceLayer;

    [Header("Lava")]
    public LayerMask lavaLayer;

    [Header("Spikes")]
    public LayerMask spikesLayer;

    [Header("Hazard")]
    public LayerMask hazardLayer;

    public void Awake()
    {
        _playerHealth = GetComponent<PlayerHealth>();
        _effects = GetComponent<HandleCollisions>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Sprawdzanie kolizji z przeciwnikiem
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (_playerHealth != null)
            {
                _playerHealth.TakeDamage(1);
            }
            return;
        }

        if (_effects == null) { return; }

        if ((iceLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            _effects.HandleIceEnter();
        }
        else if ((spikesLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            _effects.HandleSpikesEnter();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (_effects == null) { return; }

        if ((iceLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            _effects.HandleIceExit();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_effects == null) { return; }

        if ((waterLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            _effects.HandleWaterEnter();
        }
        else if ((lavaLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            _effects.HandleLavaEnter();
        } else if ((hazardLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            _effects.HandleHazardEnter();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_effects == null) { return; }

        if ((waterLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            _effects.HandleWaterExit();
        }
    }
}