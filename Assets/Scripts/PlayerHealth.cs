using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int healths = 3;
    private bool _isPlaying = true;

    [Header("Hearts UI")]
    public Image[] heartIcons;

    [Header("Distance UI")]
    public Text distanceText;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public Text gameOverText;

    private void Start()
    {
        UpdateHeartsUI();
        gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        distanceText.text = "Distance traveled: " + Mathf.Max(0, Mathf.FloorToInt(transform.position.x)) + "m";
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage();
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (!_isPlaying) return;

        healths -= damage;
        Debug.Log("Zycia: " + healths);
        UpdateHeartsUI();

        if (healths <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _isPlaying = false;
        gameOverText.text = "   Distance: " + Mathf.FloorToInt(transform.position.x);
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void UpdateHeartsUI()
    {
        for (int i = 0; i < heartIcons.Length; i++)
        {
            heartIcons[i].enabled = i < healths;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}