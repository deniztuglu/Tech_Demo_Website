using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Ships")]
    [SerializeField] private SpaceshipController[] _spaceships;
    private int _activeSpaceShipIndex = 0;

    public static GameManager Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Application.targetFrameRate = 120;

        Initialize();
    }

    void Update()
    {
        HandleSelectShip();
    }

    void Initialize()
    {
        if (_spaceships.Length < 1) return;

        DisableAllShips();
        ActivateShip(0);
    }

    void DisableAllShips()
    {
        foreach (var spaceShip in _spaceships)
        {
            spaceShip.gameObject.SetActive(false);
        }
    }

    void ActivateShip(int index)
    {
        DisableAllShips();
        _spaceships[index].gameObject.SetActive(true);
    }

    void HandleSelectShip()
    {
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            _activeSpaceShipIndex = (_activeSpaceShipIndex - 1 + _spaceships.Length) % _spaceships.Length;
            ActivateShip(_activeSpaceShipIndex);
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            _activeSpaceShipIndex = (_activeSpaceShipIndex + 1) % _spaceships.Length;
            ActivateShip(_activeSpaceShipIndex);
        }
    }
}
