using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("Audio")]   
    [SerializeField] private AudioSource audioSource;

    [Header("Look At")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform botBody; 
    [SerializeField] private float rotationSpeed = 3f;

    private bool isTalking = false;

    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
{
    if (isTalking && player != null && botBody != null)
    {
        Vector3 direction = player.position - botBody.position;
        direction.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        botBody.rotation = Quaternion.Slerp(botBody.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        Debug.Log($"Rotating bot towards player, direction: {direction}");
    }
}

   public void SetTalking(bool talking)
{
    isTalking = talking;
    Debug.Log($"SetTalking called: {talking}, botBody: {botBody}, player: {player}");
}



    public void Speak(AudioClip audioClip)
    {
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
            Debug.Log($"Bot is speaking... isPlaying: {audioSource.isPlaying}, length: {audioClip.length}");
        }
    }

    public void StopSpeaking()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
    }
}