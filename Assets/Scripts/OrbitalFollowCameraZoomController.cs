using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitalFollowCameraZoomController : MonoBehaviour
{
    [SerializeField] CinemachineOrbitalFollow _cinemachineOrbitalFollow;
    [SerializeField] float minRadius = 2f;
    [SerializeField] float maxRadius = 10f;
    [SerializeField] float scrollSensitivity = 2f;
    [SerializeField] float lerpSpeed = 5f;

    private float targetRadius;

    void Start()
    {
        if (_cinemachineOrbitalFollow != null)
            targetRadius = Mathf.Clamp(_cinemachineOrbitalFollow.Radius, minRadius, maxRadius);
    }

    void Update()
    {
        if (_cinemachineOrbitalFollow == null) return;

        Vector2 scrollDelta = Mouse.current.scroll.ReadValue();

        if (scrollDelta.y != 0)
        {
            targetRadius -= scrollDelta.y * scrollSensitivity * 0.001f;
            targetRadius = Mathf.Clamp(targetRadius, minRadius, maxRadius);
        }

        float currentRadius = _cinemachineOrbitalFollow.Radius;
        float newRadius = Mathf.Lerp(currentRadius, targetRadius, lerpSpeed * Time.deltaTime);
        _cinemachineOrbitalFollow.Radius = newRadius;
    }
}
