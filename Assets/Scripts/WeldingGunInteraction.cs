using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Handles main interaction logic for the welding gun: 
/// - Grabbing, releasing, and welding,
/// - Placing beads and evaluating the weld,
/// - Managing visuals, audio, and scoring UI.
/// </summary>
public class WeldingGunInteraction : MonoBehaviour
{
    // =======================
    // Inspector References
    // =======================
    [Header("References")]
    public Transform rayOrigin;                        // Where the welding "ray" starts (tip)
    public float rayDistance = 3f;                     // Max reach for raycast when welding
    public LayerMask metalLayer;                       // Collision layers for valid metal
    public Camera xrCamera;                            // Main VR camera
    public Camera weldingZoomCam;                      // Secondary zoom camera during welding
    public ParticleSystem weldSparks;                  // Sparks effect at weld tip
    public Transform metalSurface;                     // The metal for distance effect
    public float weldSparkDistance = 0.08f;            // Fire sparks when close enough
    public GameObject weldBeadPrefab;                  // Bead mark object
    public float weldMarkSpacing = 0.015f;             // Spacing between individual weld marks

    [Header("Weld Seam")]
    public Transform seamStart;                        // Start point of the welding seam
    public Transform seamEnd;                          // End point of the welding seam
    public float seamThreshold = 0.02f;                // Allowable deviation from seam

    [Header("Bead Size Settings")]
    public float minWeldDistance = 0.01f;              // Too close (fat bead)
    public float maxWeldDistance = 0.06f;              // Too far (thin bead)
    public float optimalWeldDistance = 0.03f;          // Ideal welding tip-to-surface distance
    public float optimalBand = 0.01f;                  // Tolerance for optimal
    public float fatScale = 1.5f;
    public float goodScale = 1.0f;
    public float thinScale = 0.5f;

    [Header("Audio & Light")]
    public AudioSource weldingLoopAudio;               // Looping arc sound
    public Light weldPointLight;                       // Point light at welding tip

    [Header("Display/Score System")]
    public DisplayClickIncrement displayScript;        // Script that provides the display value
    public GameObject excellentUIPanel;
    public GameObject goodUIPanel;
    public GameObject badUIPanel;
    public AudioSource excellentAudio;
    public AudioSource goodAudio;
    public AudioSource badAudio;

    [Header("Instructor/Lock")]
    public AudioSource humanAudioSource;
    public SwitchToggle displaySwitch;
    public GameObject lockUIPopup;

    [Header("Weld State Materials")]
    public Material moderateMoltenMaterial;
    public Material optimalMoltenMaterial;
    public Material redHotMaterial;

    // =======================
    // Internal State
    // =======================
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isWeldingMode = false;
    private bool triggerHeld = false;
    private float weldDuration = 0f;
    private float timeInOptimalZone = 0f;
    private Vector3 lastMarkPosition;
    private bool hasPlacedMark = false;
    private string lastScoreResult = "bad";
    private List<(bool stateOptimal, bool sizeOptimal)> beadResults = new List<(bool, bool)>();
    private List<Vector3> beadPositions = new List<Vector3>();

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (weldSparks) weldSparks.Stop();
        if (weldPointLight) weldPointLight.enabled = false;
        if (weldingLoopAudio) weldingLoopAudio.Stop();
        HideAllScorePanels();
    }

    void Start()
    {
        if (humanAudioSource && humanAudioSource.clip)
            humanAudioSource.Play();
    }

    void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
            grabInteractable.activated.AddListener(OnTriggerPressed);
            grabInteractable.deactivated.AddListener(OnTriggerReleased);
        }
    }

    void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
            grabInteractable.activated.RemoveListener(OnTriggerPressed);
            grabInteractable.deactivated.RemoveListener(OnTriggerReleased);
        }
    }

    void Update()
    {
        // If in welding mode while trigger is pressed
        if (isWeldingMode && triggerHeld)
        {
            EvaluateWeldingQuality();
            UpdateWeldSparks();
            TryPlaceWeldMark();

            if (weldPointLight) weldPointLight.enabled = true;
            if (weldingLoopAudio && !weldingLoopAudio.isPlaying) weldingLoopAudio.Play();
        }
        else
        {
            // Turn off all welding effects when not welding
            if (weldSparks && weldSparks.isEmitting) weldSparks.Stop();
            if (weldPointLight) weldPointLight.enabled = false;
            if (weldingLoopAudio && weldingLoopAudio.isPlaying) weldingLoopAudio.Stop();
        }
    }

    // =======================
    // Event: On Grab
    // =======================
    private void OnGrab(SelectEnterEventArgs args)
    {
        HideAllScorePanels();

        // Lock out if gas is off or display switch is off
        if (!GasTankToggle.isGasOn || (displaySwitch && !displaySwitch.IsOn))
        {
            grabInteractable?.interactionManager.SelectExit(args.interactorObject, grabInteractable);

            if (lockUIPopup)
            {
                lockUIPopup.SetActive(true);
                StartCoroutine(HideLockUIRoutine(2f));
            }
            return;
        }
        if (lockUIPopup) lockUIPopup.SetActive(false);
    }

    private IEnumerator HideLockUIRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (lockUIPopup) lockUIPopup.SetActive(false);
    }

    // =======================
    // Event: On Release
    // =======================
    private void OnRelease(SelectExitEventArgs args)
    {
        xrCamera.gameObject.SetActive(true);
        weldingZoomCam.gameObject.SetActive(false);
        if (weldSparks) weldSparks.Stop();
        if (weldPointLight) weldPointLight.enabled = false;
        if (weldingLoopAudio) weldingLoopAudio.Stop();
        isWeldingMode = false;
        CalculateFinalScore();
        hasPlacedMark = false;
        StartCoroutine(ShowFinalScorePanelRoutine());
    }

    // Shows correct score panel, then hides after 3 seconds
    private IEnumerator ShowFinalScorePanelRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        HideAllScorePanels();

        switch (lastScoreResult)
        {
            case "excellent":
                if (excellentUIPanel) excellentUIPanel.SetActive(true);
                if (excellentAudio) excellentAudio.Play();
                break;
            case "good":
                if (goodUIPanel) goodUIPanel.SetActive(true);
                if (goodAudio) goodAudio.Play();
                break;
            case "bad":
                if (badUIPanel) badUIPanel.SetActive(true);
                if (badAudio) badAudio.Play();
                break;
        }

        yield return new WaitForSeconds(3.0f);
        HideAllScorePanels();
    }

    // =======================
    // Event: On Trigger Down
    // =======================
    private void OnTriggerPressed(ActivateEventArgs args)
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, metalLayer))
        {
            xrCamera.gameObject.SetActive(false);
            weldingZoomCam.gameObject.SetActive(true);
            isWeldingMode = true;
            triggerHeld = true;
            weldDuration = 0f;
            timeInOptimalZone = 0f;
            lastMarkPosition = hit.point;
            hasPlacedMark = false;
            beadResults.Clear(); 
            beadPositions.Clear(); 
        }
    }

    // =======================
    // Event: On Trigger Up
    // =======================
    private void OnTriggerReleased(DeactivateEventArgs args)
    {
        triggerHeld = false;
        if (weldSparks && weldSparks.isEmitting) weldSparks.Stop();
        if (weldPointLight) weldPointLight.enabled = false;
        if (weldingLoopAudio) weldingLoopAudio.Stop();
        hasPlacedMark = false;
    }

    // =======================
    // Welding Quality Checks
    // =======================
    private void EvaluateWeldingQuality()
    {
        weldDuration += Time.deltaTime;
        timeInOptimalZone += Time.deltaTime;
    }

    // Checks that seam is covered by beads with no gaps above threshold
    private bool IsSeamCovered(float sampleSpacing = 0.018f, float beadTolerance = 0.025f)
    {
        if (seamStart == null || seamEnd == null || beadPositions.Count == 0)
            return false;

        float seamLength = Vector3.Distance(seamStart.position, seamEnd.position);
        int numSamples = Mathf.Max(2, Mathf.FloorToInt(seamLength / sampleSpacing));

        Vector3 direction = (seamEnd.position - seamStart.position).normalized;

        for (int i = 0; i < numSamples; i++)
        {
            Vector3 testPoint = seamStart.position + direction * (seamLength * i / (numSamples - 1));
            bool hitFound = false;
            foreach (var bead in beadPositions)
            {
                if (Vector3.Distance(testPoint, bead) < beadTolerance)
                {
                    hitFound = true;
                    break;
                }
            }
            if (!hitFound)
                return false;
        }
        return true;
    }

    // Score the weld based on coverage and consistency
    private void CalculateFinalScore()
    {
        if (beadResults.Count == 0) return;

        bool seamCovered = IsSeamCovered();
        if (!seamCovered)
        {
            lastScoreResult = "bad";
            return;
        }

        bool allExcellent = beadResults.All(res => res.stateOptimal && res.sizeOptimal);
        bool allBad = beadResults.All(res => !res.stateOptimal && !res.sizeOptimal);

        if (allExcellent)
            lastScoreResult = "excellent";
        else if (allBad)
            lastScoreResult = "bad";
        else
            lastScoreResult = "good";
    }

    // =======================
    // Sparks Effect Toggle
    // =======================
    private void UpdateWeldSparks()
    {
        if (!weldSparks || !rayOrigin || !metalSurface) return;
        float tipToMetal = Vector3.Distance(rayOrigin.position, metalSurface.position);
        if (tipToMetal < weldSparkDistance)
        {
            if (!weldSparks.isEmitting) weldSparks.Play();
        }
        else
        {
            if (weldSparks.isEmitting) weldSparks.Stop();
        }
    }

    // =======================
    // Weld Mark Placement (Main Placement Logic!)
    // =======================
    private void TryPlaceWeldMark()
    {
        if (!weldBeadPrefab) return;
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, rayDistance, metalLayer))
        {
            float dist = Vector3.Distance(rayOrigin.position, hit.point);
            if (dist > maxWeldDistance) return;
            if (!IsTipOnSeam(hit.point)) return;
            if (!hasPlacedMark || Vector3.Distance(lastMarkPosition, hit.point) > weldMarkSpacing)
            {
                float scaleFactor;
                if (dist < minWeldDistance) scaleFactor = fatScale;
                else if (Mathf.Abs(dist - optimalWeldDistance) <= optimalBand) scaleFactor = goodScale;
                else scaleFactor = thinScale;

                GameObject mark = Instantiate(
                    weldBeadPrefab,
                    hit.point,
                    weldBeadPrefab.transform.rotation,
                    hit.collider.transform
                );
                mark.transform.localScale = Vector3.one * scaleFactor;

                // -------- STATE/Material switching by display value --------
                int displayValue = displayScript ? displayScript.value : 0;
                Debug.Log("Display Value at bead: " + displayValue); // For debugging

                Material selectedMat = moderateMoltenMaterial;
                bool colorOptimal = false;

                if (displayValue > 80 && displayValue <= 100)
                {
                    selectedMat = optimalMoltenMaterial;
                    colorOptimal = true;
                }
                else if (displayValue > 100 && displayValue <= 140)
                {
                    selectedMat = redHotMaterial;
                    colorOptimal = false;
                }
                else // ≤ 80
                {
                    selectedMat = moderateMoltenMaterial;
                    colorOptimal = false;
                }

                var rend = mark.GetComponent<Renderer>();
                if (rend != null && selectedMat != null)
                    rend.material = selectedMat;

                bool sizeOptimal = (Mathf.Abs(dist - optimalWeldDistance) <= optimalBand);
                beadResults.Add((colorOptimal, sizeOptimal));
                beadPositions.Add(hit.point);

                lastMarkPosition = hit.point;
                hasPlacedMark = true;
            }
        }
    }

    // Checks if tip is close enough to the seam to allow marking
    private bool IsTipOnSeam(Vector3 tipPosition)
    {
        if (!seamStart || !seamEnd) return false;
        Vector3 seamVec = seamEnd.position - seamStart.position;
        Vector3 tipToStart = tipPosition - seamStart.position;
        float t = Vector3.Dot(tipToStart, seamVec.normalized);
        t = Mathf.Clamp(t, 0, seamVec.magnitude);
        Vector3 closest = seamStart.position + seamVec.normalized * t;
        float dist = Vector3.Distance(tipPosition, closest);
        return dist <= seamThreshold;
    }

    // Utility: Turn off all result UI panels
    private void HideAllScorePanels()
    {
        if (excellentUIPanel) excellentUIPanel.SetActive(false);
        if (goodUIPanel) goodUIPanel.SetActive(false);
        if (badUIPanel) badUIPanel.SetActive(false);
    }
}
