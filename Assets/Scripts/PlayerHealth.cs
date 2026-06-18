using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int healths = 3;
    private bool _isPlaying = true;

    public void TakeDamage(int damage = 1)
    {
        if (!_isPlaying) { return; }

        healths -= damage;
        Debug.Log("Zycia: " + healths);

        if (healths <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _isPlaying = false;
        // Game over screen
    }
}
