using UnityEngine;
using TMPro;

public class KnobDisplay : MonoBehaviour
{
    public TextMeshPro digitalDisplay;   // Assign your TextMeshPro object here
    public int minValue = 0;
    public int maxValue = 200;

    // This method can be called from XR Knob's On Value Changed event
    public void UpdateDisplay(float normalizedValue)
    {
        int displayValue = Mathf.RoundToInt(Mathf.Lerp(minValue, maxValue, normalizedValue));
        if (digitalDisplay != null)
            digitalDisplay.text = displayValue.ToString("D3");
    }
}
