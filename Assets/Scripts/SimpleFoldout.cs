using UnityEngine;
using UnityEngine.UI;

public class SimpleFoldout : MonoBehaviour
{
    public Button headerButton;
    public GameObject contentObject; 
    
    void Start()
    {
        headerButton.onClick.AddListener(ToggleFoldout);
    }

    void ToggleFoldout()
    {
        bool isActive = !contentObject.activeSelf;
        contentObject.SetActive(isActive);

        // --- THE FIX ---
        // We act like a "Layout Rebuilder"
        // We notify the LayoutGroup on this object (and its parent) to recalculate
        if (isActive) 
        {
            // Force the layout to update immediately to fix the overlap
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            
            // If this is nested inside another Layout Group (like the Master Settings), 
            // we might need to rebuild that one too:
            if(transform.parent != null)
            {
                 LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
            }
        }
    }
}