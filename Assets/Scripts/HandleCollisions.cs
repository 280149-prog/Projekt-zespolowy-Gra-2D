using UnityEngine;

public class HandleCollisions : MonoBehaviour
{
    private PlayerHealth _playerHealth;
    private PlayerMovement _playerMovement;

    [Header("Layer Masks")]
    public LayerMask hazardLayer;

    private float _normalSpeed;
    private float _normalGravity;

    private void Awake()
    {
        _playerHealth = GetComponent<PlayerHealth>();
        _playerMovement = GetComponent<PlayerMovement>();

        if (_playerMovement != null)
        {
            _normalSpeed = _playerMovement.moveSpeed;
            _normalGravity = _playerMovement.baseGravity;
        }
    }

    public void HandleWaterEnter()
    {
        Debug.Log("Gracz wszed³ do wody");

        if (_playerMovement != null)
        {
            _playerMovement.moveSpeed = _normalSpeed * 0.4f;        // Spowolnienie o 60%
            _playerMovement.baseGravity = _normalGravity * 0.3f;    // Wypornoœæ

            // TODO: Przytrzymanie W w wodzie powinno ca³y czas zwiêkszaæ prêdkoœæY i nie liczyæ siê jako skok
        }
    }

    public void HandleWaterExit()
    {
        Debug.Log("Gracz wyszed³ z wody");

        if (_playerMovement != null)
        {
            _playerMovement.moveSpeed = _normalSpeed;
            _playerMovement.baseGravity = _normalGravity;
        }
    }

    public void HandleIceEnter()
    {
        Debug.Log("Gracz wszed³ na lód");

        if (_playerMovement != null)
        {
            _playerMovement.moveSpeed = _normalSpeed * 0.4f;        // Spowolnienie o 60%
            _playerMovement.baseGravity = _normalGravity * 0.3f;    // Wolniejsze spadanie (wypornoœæ)
        }
    }

    public void HandleIceExit()
    {
        Debug.Log("Gracz wyszed³ z lodu.");

        if (_playerMovement != null)
        {
            _playerMovement.moveSpeed = _normalSpeed;
            _playerMovement.baseGravity = _normalGravity;
        }
    }

    public void HandleLavaEnter()
    {
        Debug.Log("Gracz wszed³ wskoczy³ do lawy");
        if (_playerHealth != null) { _playerHealth.TakeDamage(3); }     // Natychmiastowa œmieræ
    }

    public void HandleSpikesEnter()
    {
        Debug.Log("Gracz wszed³ w kolce");
        if (_playerHealth != null) { _playerHealth.TakeDamage(3); }     // Natychmiastowa œmieræ
    }

    public void HandleHazardEnter() 
    {
        Debug.Log("Gracz znalaz³ siê w niebezpieczeñstwie!!!");
        if (_playerHealth != null) { _playerHealth.TakeDamage(3); }     // Natychmiastowa œmieræ
    }
}