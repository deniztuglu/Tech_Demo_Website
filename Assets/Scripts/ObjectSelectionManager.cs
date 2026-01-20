using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class ObjectSelectionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera activeCamera;
    [SerializeField] private ThirdPersonCameraController cameraController;
    [SerializeField] private CanvasGroup faderPanel;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private GameObject[] availableObjects;

    private bool isSwitching = false;

    private void Start()
    {
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
    GameObject newShipObj = null;

    for (int i = 0; i < availableObjects.Length; i++)
    {
        bool isSelected = (i == index);
        if(availableObjects[i] != null) 
        {
            availableObjects[i].SetActive(isSelected);
            if (isSelected) newShipObj = availableObjects[i];
        }
    }

    // 4. Update Camera Target & Distance
    if (activeCamera != null && newShipObj != null)
    {
        activeCamera.Follow = newShipObj.transform;
        
        // --- NEW CODE: Adjust Distance based on Ship Size ---
        // Try to find the controller on the new ship
        var shipController = newShipObj.GetComponent<SpaceshipController>();
        
        if (shipController != null && cameraController != null)
        {
            // Apply the custom distance defined on the ship
            cameraController.ForceZoom(shipController.startingCameraDistance);
        }
        else if (cameraController != null)
        {
            // If no controller found, just reset to a default
            cameraController.ResetCameraState(); 
        }
    }

    // Wait a tiny bit
    yield return new WaitForSeconds(0.2f);

    // 5. Fade In
    yield return StartCoroutine(Fade(1f, 0f));

    // 6. Unlock Camera
    if (cameraController != null) cameraController.IsInputLocked = false;
    isSwitching = false;
}

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (faderPanel == null)
        {
            Debug.LogError("Fader Panel is not assigned in ObjectSelectionManager!");
            yield break;
        }

        // Ensure we block clicks while the screen is covering the game
        faderPanel.blocksRaycasts = true;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            faderPanel.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            yield return null;
        }
        
        // Force exact end value
        faderPanel.alpha = endAlpha;

        // Only allow clicking through the panel if it is invisible (alpha 0)
        faderPanel.blocksRaycasts = (endAlpha > 0);
    }

    private void InitializeScene()
    {
        if (faderPanel != null)
        {
            faderPanel.alpha = 0f;
            faderPanel.blocksRaycasts = false;
        }

        if (activeCamera != null && activeCamera.Follow != null)
        {
            foreach (var obj in availableObjects)
            {
                if(obj != null) obj.SetActive(obj.transform == activeCamera.Follow);
            }
        }
    }
}