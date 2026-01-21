using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using FMODUnity;

public class GameManager : MonoBehaviour
{
    [Header("Ships")]
    [SerializeField] private SpaceshipController[] _spaceships;
    private int _activeSpaceShipIndex = 0;

    [Header("Skybox")]
    [SerializeField] private Material[] _skyboxes;
    private int _activeSkyboxIndex = 0;

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
        HandleChangeSkybox();
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
        PlayTransitionPlayable();
        DisableAllShips();
        _spaceships[index].gameObject.SetActive(true);
    }

    void HandleSelectShip()
    {
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
        {
            _activeSpaceShipIndex = (_activeSpaceShipIndex - 1 + _spaceships.Length) % _spaceships.Length;
            ActivateShip(_activeSpaceShipIndex);

        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
        {
            _activeSpaceShipIndex = (_activeSpaceShipIndex + 1) % _spaceships.Length;
            ActivateShip(_activeSpaceShipIndex);
        }
    }

    void HandleChangeSkybox()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            _activeSkyboxIndex = (_activeSkyboxIndex + 1) % _skyboxes.Length;
            RenderSettings.skybox = _skyboxes[_activeSkyboxIndex];
            PlayTransitionPlayable();
        }
    }

    void PlayTransitionPlayable()
    {
        PlayTransitionSound();
        _transitionPlayable.Play();
    }

    void PlayTransitionSound()
    {
        if (_transitionSound.IsNull) return;
        RuntimeManager.PlayOneShot(_transitionSound, transform.position);
    }
}