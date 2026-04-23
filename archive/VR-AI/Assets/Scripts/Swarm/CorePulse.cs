using UnityEngine;

public class CorePulse : MonoBehaviour
{
    public Material coreMaterial;
    public float minIntensity = 1f;
    public float maxIntensity = 3f;
    public float pulseSpeed = 1.5f;
    public Color baseColor = new Color(0f, 0.87f, 1f);

    void Update()
    {
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, 
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        
        coreMaterial.SetColor("_EmissionColor", 
           baseColor * (intensity * 2f));
    }
}