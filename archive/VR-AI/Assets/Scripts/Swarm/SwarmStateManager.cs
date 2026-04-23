using UnityEngine;
using UnityEngine.VFX;

public class SwarmStateManager : MonoBehaviour
{
    public enum SwarmState { Idle, Listening, Speaking }

    [Header("References")]
    public VisualEffect vfx;
    public CorePulse corePulse;

    [Header("Colors")]
    public Color idleColor = new Color(0f, 3f, 4f, 1f);
    public Color listeningColor = new Color(0.1f, 0.3f, 1f, 0f);
    public Color speakingColor = new Color(0.2f, 4f, 0.5f, 0f);

    [Header("Attractor Force")]
    public float idleAttractorForce = 0f;
    public float listeningAttractorForce = 1f;

    public float speakingAttractorForce = 0.2f; 


    [Header("Transition Speed")]
    public float colorLerpSpeed = 2f;
    public float attractorLerpSpeed = 3f;

    private SwarmState currentState = SwarmState.Idle;
    private Color targetColor;
    private float targetAttractorForce;
    private float currentAttractorForce;
    private Color currentColor;

    void Start()
    {
        SetState(SwarmState.Idle);
        currentColor = idleColor;
        currentAttractorForce = 0f;
    }

    void Update()
    {
        currentColor = Color.Lerp(currentColor, targetColor,
            Time.deltaTime * colorLerpSpeed);
        currentAttractorForce = Mathf.Lerp(currentAttractorForce,
            targetAttractorForce, Time.deltaTime * attractorLerpSpeed);

        vfx.SetVector4("ParticleColor",
            new Vector4(currentColor.r, currentColor.g, currentColor.b, currentColor.a));
        vfx.SetFloat("AttractorForce", currentAttractorForce);

        corePulse.baseColor = new Color(currentColor.r, currentColor.g, currentColor.b);

        corePulse.coreMaterial.SetColor("_BaseColor", currentColor);

        //temporary - to test on laptop

        //sDebug.Log($"CurrentColor: {currentColor} | State: {currentState}");

        if (Input.GetKeyDown(KeyCode.Alpha1)) SetState(SwarmState.Idle);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetState(SwarmState.Listening);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetState(SwarmState.Speaking);
    }

    public void SetState(SwarmState newState)
    {
        currentState = newState;
        switch (newState)
        {
            case SwarmState.Idle:
                targetColor = idleColor;
                targetAttractorForce = idleAttractorForce;
                corePulse.pulseSpeed = 1.5f;
                break;
            case SwarmState.Listening:
                targetColor = listeningColor;
                targetAttractorForce = listeningAttractorForce;
                corePulse.pulseSpeed = 0f;
                break;
            case SwarmState.Speaking:
                targetColor = speakingColor;
                targetAttractorForce = speakingAttractorForce;
                corePulse.pulseSpeed = 4f;
                break;
        }
    }
}