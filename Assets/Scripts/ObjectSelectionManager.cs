using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class ObjectSelectionManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The camera that has the controller script.")]
    [SerializeField] private CinemachineCamera activeCamera;
    [Tooltip("The controller script itself (drag the camera here too).")]
    [SerializeField] private ThirdPersonCameraController cameraController;
    [SerializeField] private CanvasGroup faderPanel;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [Tooltip("List of objects. Index 0 = Dropdown Option 0.")]
    [SerializeField] private GameObject[] availableObjects;

    private bool isSwitching = false;

    private void Start()
    {
        // Ensure starting state is correct
        InitializeScene();
    }

    public void OnDropdownChanged(int index)
    {
        if (!isSwitching && index >= 0 && index < availableObjects.Length)
        {
            StartCoroutine(SwitchSequence(index));
        }
    }

    private IEnumerator SwitchSequence(int index)
    {
        isSwitching = true;
        
        // 1. Lock Camera Input
        if (cameraController != null) cameraController.IsInputLocked = true;

        // 2. Fade Out
        yield return StartCoroutine(Fade(0f, 1f));

        // 3. Swap Objects
        for (int i = 0; i < availableObjects.Length; i++)
        {
            // Only activate the one that matches the index
            if(availableObjects[i] != null) 
                availableObjects[i].SetActive(i == index);
        }

        // 4. Update Camera Target
        if (activeCamera != null && availableObjects[index] != null)
        {
            activeCamera.Follow = availableObjects[index].transform;
            
            // Optional: Tell camera to reset momentum/physics
            if (cameraController != null) cameraController.ResetCameraState();
        }

        // Wait a tiny bit while screen is black
        yield return new WaitForSeconds(0.2f);

        // 5. Fade In
        yield return StartCoroutine(Fade(1f, 0f));

        // 6. Unlock Camera
        if (cameraController != null) cameraController.IsInputLocked = false;
        isSwitching = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            faderPanel.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            yield return null;
        }
        faderPanel.alpha = endAlpha;
    }

    private void InitializeScene()
    {
        // Ensure only the object the camera is currently looking at is active
        if (activeCamera != null && activeCamera.Follow != null)
        {
            foreach (var obj in availableObjects)
            {
                obj.SetActive(obj.transform == activeCamera.Follow);
            }
        }
    }
}