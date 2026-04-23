using UnityEngine;
using UnityEngine.XR;
using System.Collections;

public class AIManager : MonoBehaviour
{
    [Header("Services")]
    [SerializeField] private SpeechToText stt;
    [SerializeField] private LLM glitchLLM;
    [SerializeField] private LLM orbLLM;
    [SerializeField] private TextToSpeech glitchTTS;
    [SerializeField] private TextToSpeech entityTTS;

    [Header("Bot")]
    [SerializeField] private BotController botController;
    [SerializeField] private Animator animator;

    [Header("Swarm")]
    [SerializeField] private SwarmStateManager swarmStateManager;

    [Header("Recording Settings")]
    [SerializeField] private int recordingDuration = 5;
    [SerializeField] private int sampleRate = 16000;

    [Header("Gaze")]
    [SerializeField] private GazeDetector gazeDetector;

    [Header("Atmosphere")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private float entityLightIntensity = 0.2f;
    [SerializeField] private Color entityLightColor = new Color(0.1f, 0.1f, 0.3f);
    private float defaultLightIntensity;
    private Color defaultLightColor;

    private bool isProcessing = false;
    private bool isRecording = false;
    private AudioClip recordedClip;
    private bool gripWasPressed = false;

    void Start()
    {
        if (directionalLight != null)
        {
            defaultLightIntensity = directionalLight.intensity;
            defaultLightColor = directionalLight.color;
        }
    }

    void Update()
    {
        var rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        rightHand.TryGetFeatureValue(CommonUsages.gripButton, out bool gripNow);

        bool recordInput = Input.GetKeyDown(KeyCode.Space) || (gripNow && !gripWasPressed);
        bool releaseInput = Input.GetKeyUp(KeyCode.Space) || (!gripNow && gripWasPressed);

        gripWasPressed = gripNow;

        if (recordInput && !isProcessing) StartRecording();
        if (releaseInput && isRecording) StopRecordingAndProcess();
    }

    private void StartRecording()
    {
        isRecording = true;

        if (botController != null)
            botController.StopSpeaking();

        StopAllCoroutines();

        recordedClip = Microphone.Start(null, false, recordingDuration, sampleRate);

        if (swarmStateManager != null)
            swarmStateManager.SetState(SwarmStateManager.SwarmState.Listening);

        Debug.Log("Recording started...");
    }

    private bool IsAddressedTo(string speech, string name)
    {
        return speech.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private async void StopRecordingAndProcess()
    {
        isRecording = false;
        isProcessing = true;
        Microphone.End(null);

        string userSpeech = await stt.ConvertSpeech(recordedClip);
        if (string.IsNullOrEmpty(userSpeech))
        {
            Debug.LogWarning("No speech detected");
            isProcessing = false;
            if (swarmStateManager != null)
                swarmStateManager.SetState(SwarmStateManager.SwarmState.Idle);
            return;
        }
        Debug.Log($"User: {userSpeech}");

        // Determine which agent is being addressed — Entity is default
        bool addressedGlitch = IsAddressedTo(userSpeech, "glitch");
        bool addressedOrb = IsAddressedTo(userSpeech, "entity");

        if (!addressedGlitch && !addressedOrb)
            addressedOrb = true;

        LLM activeLLM = addressedGlitch ? glitchLLM : orbLLM;
        string aiResponse = await activeLLM.GetResponse(userSpeech, gazeDetector.CurrentTarget);
        Debug.Log($"AI: {aiResponse}");
        Debug.Log($"[CONTEXT] {gazeDetector.CurrentTarget?.modelName ?? "null"}");

        TextToSpeech activeTTS = addressedGlitch ? glitchTTS : entityTTS;
        AudioClip audioClip = await activeTTS.GenerateSpeech(aiResponse);

        if (addressedGlitch)
        {
            if (botController != null)
                botController.Speak(audioClip);
            if (animator != null)
                animator.SetBool("IsTalking", true);
            botController?.SetTalking(true);

            if (swarmStateManager != null)
                swarmStateManager.SetState(SwarmStateManager.SwarmState.Idle);

            if (audioClip != null)
                StartCoroutine(ReturnToIdleAfterSpeech(audioClip.length, isGlitch: true));
        }
        else
        {
            botController?.SetTalking(false);

            if (botController != null)
                botController.Speak(audioClip);

            if (swarmStateManager != null)
                swarmStateManager.SetState(SwarmStateManager.SwarmState.Speaking);

            StartCoroutine(DimLightForEntity());

            if (audioClip != null)
                StartCoroutine(ReturnToIdleAfterSpeech(audioClip.length, isGlitch: false));
        }

        isProcessing = false;
    }

    private IEnumerator ReturnToIdleAfterSpeech(float duration, bool isGlitch)
    {
        yield return new WaitForSeconds(duration);

        if (isGlitch)
        {
            if (animator != null)
                animator.SetBool("IsTalking", false);
            botController?.SetTalking(false);
        }
        else
        {
            StartCoroutine(RestoreLight());
        }

        if (swarmStateManager != null)
            swarmStateManager.SetState(SwarmStateManager.SwarmState.Idle);
    }

    private IEnumerator DimLightForEntity()
    {
        if (directionalLight == null) yield break;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            directionalLight.intensity = Mathf.Lerp(defaultLightIntensity, entityLightIntensity, t);
            directionalLight.color = Color.Lerp(defaultLightColor, entityLightColor, t);
            yield return null;
        }
    }

    private IEnumerator RestoreLight()
    {
        if (directionalLight == null) yield break;
        float t = 0f;
        float currentIntensity = directionalLight.intensity;
        Color currentColor = directionalLight.color;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            directionalLight.intensity = Mathf.Lerp(currentIntensity, defaultLightIntensity, t);
            directionalLight.color = Color.Lerp(currentColor, defaultLightColor, t);
            yield return null;
        }
    }
}