using UnityEngine;

public class EntityFollower : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Offset")]
    [SerializeField] private Vector3 localOffset = new Vector3(1.2f, -0.3f, 1.0f);

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 2.5f;
    [SerializeField] private float rotationSpeed = 2f;

    [Header("Trail")]
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private float moveThreshold = 0.02f; // min speed to show trail

    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
        if (trail != null)
            trail.emitting = false;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 targetPosition = player.position
            + player.right * localOffset.x
            + player.up * localOffset.y
            + player.forward * localOffset.z;

        transform.position = Vector3.Lerp(transform.position, targetPosition,
            followSpeed * Time.deltaTime);

        Vector3 directionToPlayer = player.position - transform.position;
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation,
                rotationSpeed * Time.deltaTime);
        }

        // Only emit trail when moving fast enough
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        if (trail != null)
            trail.emitting = speed > moveThreshold;

        lastPosition = transform.position;
    }
}