using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;

public class WeldingGunInteraction : MonoBehaviour
{
    [Header("References")]
    public Transform rayOrigin;
    public float rayDistance = 3f;
    public LayerMask metalLayer;
    public Camera xrCamera;
    public Camera weldingZoomCam;
    public ParticleSystem weldSparks;
    public Transform metalSurface;
    public float weldSparkDistance = 0.08f;
    public GameObject weldBeadPrefab;
    public float weldMarkSpacing = 0.015f;

    [Header("Weld Seam")]
    public Transform seamStart;
    public Transform seamEnd;
    public float seamThreshold = 0.02f;

    [Header("Bead Size Settings")]
    public float minWeldDistance = 0.01f;
    public float maxWeldDistance = 0.06f;
    public float optimalWeldDistance = 0.03f;
    public float optimalBand = 0.01f;
    public float fatScale = 1.5f;
    public float goodScale = 1.0f;
    public float thinScale = 0.5f;

    [Header("Audio & Light")]
    public AudioSource weldingLoopAudio;
    public Light weldPointLight;

    [Header("Display/Score System")]
    public DisplayClickIncrement displayScript;
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

    // Internal State
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
            if (weldSparks && weldSparks.isEmitting) weldSparks.Stop();
            if (weldPointLight) weldPointLight.enabled = false;
            if (weldingLoopAudio && weldingLoopAudio.isPlaying) weldingLoopAudio.Stop();
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        HideAllScorePanels();

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

    private void OnTriggerReleased(DeactivateEventArgs args)
    {
        triggerHeld = false;
        if (weldSparks && weldSparks.isEmitting) weldSparks.Stop();
        if (weldPointLight) weldPointLight.enabled = false;
        if (weldingLoopAudio) weldingLoopAudio.Stop();
        hasPlacedMark = false;
    }

    private void EvaluateWeldingQuality()
    {
        weldDuration += Time.deltaTime;
        timeInOptimalZone += Time.deltaTime;
    }

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

                // -------- Robust STATE/Material switching --------
                int displayValue = displayScript ? displayScript.value : 0;
                Debug.Log("Display Value at bead: " + displayValue); // For diagnosis; can remove later

                Material selectedMat = moderateMoltenMaterial;
                bool colorOptimal = false;

                // State switching: make sure NO overlaps in ranges and all values are handled
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
                else // ≤ 80, catch explicitly
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

    private void HideAllScorePanels()
    {
        if (excellentUIPanel) excellentUIPanel.SetActive(false);
        if (goodUIPanel) goodUIPanel.SetActive(false);
        if (badUIPanel) badUIPanel.SetActive(false);
    }
}
