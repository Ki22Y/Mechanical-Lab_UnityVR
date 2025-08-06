using UnityEngine;
using TMPro;

public class KnobDisplay : MonoBehaviour
{
    public TextMeshPro digitalDisplay;   
    public int minValue = 0;
    public int maxValue = 200;

    public void UpdateDisplay(float normalizedValue)
    {
        int displayValue = Mathf.RoundToInt(Mathf.Lerp(minValue, maxValue, normalizedValue));
        if (digitalDisplay != null)
            digitalDisplay.text = displayValue.ToString("D3");
    }
}
