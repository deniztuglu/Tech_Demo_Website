using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using FMODUnity;

public class GameManager : MonoBehaviour
{
    [Header("Ships")]
    [SerializeField] private SpaceshipController[] _spaceships;
    private int _activeSpaceShipIndex = 0;

    [Header("Playables")]
    [SerializeField] private PlayableDirector _transitionPlayable;

    [Header("Audio")]
    [SerializeField] private EventReference _transitionSound;

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

        // Pass 'false' here so the sound doesn't play on start
        ActivateShip(0, false);
    }

    void DisableAllShips()
    {
        foreach (var spaceShip in _spaceships)
        {
            spaceShip.gameObject.SetActive(false);
        }
    }

    // Default is true, so normal gameplay calls will play sound
    void ActivateShip(int index, bool playSound = true)
    {
        if (playSound && !_transitionSound.IsNull)
        {
            RuntimeManager.PlayOneShot(_transitionSound, transform.position);
        }

        _transitionPlayable.Play();
        DisableAllShips();
        _spaceships[index].gameObject.SetActive(true);
    }

    void HandleSelectShip()
    {
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
        {
            _activeSpaceShipIndex = (_activeSpaceShipIndex - 1 + _spaceships.Length) % _spaceships.Length;
            ActivateShip(_activeSpaceShipIndex); // Defaults to true
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
        {
            _activeSpaceShipIndex = (_activeSpaceShipIndex + 1) % _spaceships.Length;
            ActivateShip(_activeSpaceShipIndex); // Defaults to true
        }
    }
}