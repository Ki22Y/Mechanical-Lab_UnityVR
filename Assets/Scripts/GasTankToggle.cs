using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Simple global gas toggle (on/off) for welding VR trainers.
/// Handles interactive state, material change, and UI warning.
/// </summary>
public class GasTankToggle : MonoBehaviour
{
    [Header("Visuals & UI")]
    [Tooltip("Material (color/effect) to indicate GAS ON.")]
    public Material onMaterial;

    [Tooltip("Material (color/effect) to indicate GAS OFF.")]
    public Material offMaterial;

    [Tooltip("Assign the MeshRenderer for the tank's outline or main body")]
    public MeshRenderer outlineRenderer;

    [Tooltip("Optional. UI Panel or warning graphic to show if gas is off")]
    public GameObject gasOffWarningUI;

    // Global state: "Is gas ON?" This is checked by other objects (eg, gun/controller scripts).
    public static bool isGasOn = false;

    // Local reference to XR Toolkit's interactable component
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    // Called first: Get interactable reference, connect toggle event, set visuals
    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnTogglePressed);
        }

        // At game start, make sure visuals match default gas state
        UpdateVisual();

        // At start, also set/hide the warning UI if needed
        if (gasOffWarningUI != null)
        {
            gasOffWarningUI.SetActive(!isGasOn);
        }
    }

    // Clean up listener if object is destroyed (prevents memory leaks)
    void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnTogglePressed);
        }
    }

    /// <summary>
    /// This function is called by XR Interaction Toolkit when the switch/tank is "grabbed".
    /// Toggles gas on/off. Updates visuals and displays warning UI if gas is off.
    /// </summary>
    /// <param name="args">Interaction arguments from XR Toolkit</param>
    private void OnTogglePressed(SelectEnterEventArgs args)
    {
        isGasOn = !isGasOn; // Toggle state

        UpdateVisual();

        Debug.Log("Gas is now " + (isGasOn ? "ON ✅" : "OFF ❌"));

        // If warning UI is set, show it only if gas is OFF
        if (gasOffWarningUI != null)
        {
            gasOffWarningUI.SetActive(!isGasOn);
        }
    }

    /// <summary>
    /// Visually update the tank or associated outline with color/material representing ON/OFF state
    /// </summary>
    private void UpdateVisual()
    {
        if (outlineRenderer != null)
        {
            outlineRenderer.material = isGasOn ? onMaterial : offMaterial;
        }
    }
}
