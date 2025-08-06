using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class DisplayClickIncrement : MonoBehaviour
{
    public TextMeshPro displayText; 
    public SwitchToggle switchToggle; // Assign in Inspector!
    public int value = 0;
    public int minValue = 0;
    public int maxValue = 200;
    public int step = 5;

    void Awake()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        interactable.selectEntered.AddListener(OnDisplayClicked);
    }

    void OnDestroy()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        interactable.selectEntered.RemoveListener(OnDisplayClicked);
    }

    void OnDisplayClicked(SelectEnterEventArgs args)
    {
        if (switchToggle != null && switchToggle.IsOn)
        {
            value += step;
            if (value > maxValue) value = minValue;
            if (displayText != null)
                displayText.text = value.ToString("D3");
        }
    }

    void Update()
    {
        if (switchToggle != null && switchToggle.IsOn)
        {
            if (displayText != null)
                displayText.text = value.ToString("D3");
        }
        else
        {
            if (displayText != null)
                displayText.text = "";
        }
    }
}
