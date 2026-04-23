using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class LocomotionVignette : MonoBehaviour
{
    [Header("References")]
    public Volume globalVolume;
    
    [Header("Input Actions")]
    public InputActionReference leftHandMove;
    public InputActionReference rightHandMove;
    
    [Header("Vignette Settings")]
    public float maxIntensity = 0.35f;
    public float fadeSpeed = 8f;

    private Vignette vignette;

    void Start()
    {
        globalVolume.profile.TryGet(out vignette);
        if (vignette == null)
            Debug.LogError("No Vignette override found on Global Volume!");
    }

    void Update()
    {
        if (vignette == null) return;

        Vector2 leftInput = leftHandMove != null ? 
            leftHandMove.action.ReadValue<Vector2>() : Vector2.zero;
        Vector2 rightInput = rightHandMove != null ? 
            rightHandMove.action.ReadValue<Vector2>() : Vector2.zero;

        float inputMagnitude = Mathf.Max(leftInput.magnitude, rightInput.magnitude);
        bool isMoving = inputMagnitude > 0.1f;

        float target = isMoving ? maxIntensity : 0f;
        vignette.intensity.value = Mathf.Lerp(
            vignette.intensity.value, target, Time.deltaTime * fadeSpeed);
    }
}
