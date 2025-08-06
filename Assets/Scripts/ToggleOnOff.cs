using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SwitchToggle : MonoBehaviour
{
    public Animator animator;
    private bool isOn = false;
    public bool IsOn => isOn;

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

    void OnTogglePressed(SelectEnterEventArgs args)
    {
        isOn = !isOn;
        animator.SetBool("IsOn", isOn);
        Debug.Log("Switch is now " + (isOn ? "ON ✅" : "OFF ❌"));
    }
}
