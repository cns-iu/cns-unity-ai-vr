using UnityEngine;

public class GazeDetector : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float rayDistance = 10f;
    [SerializeField] private float sphereRadius = 0.15f;
    [SerializeField] private Camera gazeCamera;

    [Header("Gaze Hold")]
    [SerializeField] private float gazeHoldTime = 0.5f;

    public VisualizationData CurrentTarget { get; private set; }

    private float gazeTimer = 0f;
    private VisualizationData pendingTarget;

    void Start()
    {
        if (gazeCamera == null)
            gazeCamera = Camera.main;
    }

    void Update()
    {
        Ray ray = new Ray(gazeCamera.transform.position, gazeCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);

        if (Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, rayDistance))
        {
            VizObject vizObject = hit.collider.GetComponentInParent<VizObject>();
            if (vizObject != null)
            {
                if (pendingTarget != vizObject.data)
                {
                    pendingTarget = vizObject.data;
                    gazeTimer = 0f;
                }

                gazeTimer += Time.deltaTime;
                if (gazeTimer >= gazeHoldTime && CurrentTarget != pendingTarget)
                {
                    CurrentTarget = pendingTarget;
                    Debug.Log($"Now looking at: {CurrentTarget.modelName}");
                }
            }
            else
            {
                CurrentTarget = null;
                pendingTarget = null;
                gazeTimer = 0f;
            }
        }
        else
        {
            CurrentTarget = null;
            pendingTarget = null;
            gazeTimer = 0f;
        }
    }
}