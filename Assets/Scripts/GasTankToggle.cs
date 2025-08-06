using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GasTankToggle : MonoBehaviour
{
    [Header("Visuals & UI")]
    public Material onMaterial;
    public Material offMaterial;
    public MeshRenderer outlineRenderer; // Assign your mesh here
    public GameObject gasOffWarningUI;   // Assign in Inspector (for UI warning)

    // Static - global accessible gas switch state
    public static bool isGasOn = false;

    // Internal XR Interactable reference
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnTogglePressed);
        }
    }

    void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnTogglePressed);
        }
    }

    // This runs when user "grabs" or triggers the switch in VR
    private void OnTogglePressed(SelectEnterEventArgs args)
    {
        isGasOn = !isGasOn;   // Toggle gas state

        UpdateVisual();

        Debug.Log("Gas is now " + (isGasOn ? "ON ✅" : "OFF ❌"));

        // Show or hide gas warning UI if assigned
        if (gasOffWarningUI != null)
        {
            gasOffWarningUI.SetActive(!isGasOn); // Show UI when gas is OFF
        }
    }

    // Switches outline material for a visual ON/OFF effect
    private void UpdateVisual()
    {
        if (outlineRenderer != null)
        {
            outlineRenderer.material = isGasOn ? onMaterial : offMaterial;
        }
    }
}
