using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text;


public class TextToSpeech : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string elevenLabsApiKey = "sk_c4fe0a87504049908eb3272b634071d92551b29621d3c450";
    [SerializeField] private string voiceId = "XrExE9yKIg1WjnnlVkGX"; // Matilda - Knowledgeable, Professional (educational)

    private const string API_URL = "https://api.elevenlabs.io/v1/text-to-speech/";

    public async Task<AudioClip> GenerateSpeech(string text)
    {
        // Escape quotes in text to avoid breaking JSON
        string safeText = text.Replace("\"", "\\\"");
        
        string jsonBody = $@"{{
            ""text"": ""{safeText}"",
            ""model_id"": ""eleven_flash_v2_5"",
            ""output_format"": ""pcm_22050""
        }}";

        string url = API_URL + voiceId + "?output_format=pcm_22050";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("xi-api-key", elevenLabsApiKey);

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"TTS API Error: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
                Debug.LogError($"Status Code: {request.responseCode}");
                return null;
            }

            Debug.Log($"TTS Success! Received {request.downloadHandler.data.Length} bytes");
            return ConvertPcmToAudioClip(request.downloadHandler.data, 22050);
        }
    }

    private AudioClip ConvertPcmToAudioClip(byte[] pcmData, int sampleRate)
    {
    if (pcmData == null || pcmData.Length == 0)
    {
        Debug.LogError("Invalid PCM data!");
        return null;
    }

    int sampleCount = pcmData.Length / 2;
    float[] samples = new float[sampleCount];

    for (int i = 0; i < sampleCount; i++)
    {
        short s = System.BitConverter.ToInt16(pcmData, i * 2);
        samples[i] = s / 32768f;
    }

    // Normalize to prevent clipping
    float maxVal = 0f;
    foreach (float s in samples)
        if (Mathf.Abs(s) > maxVal) maxVal = Mathf.Abs(s);
    
    if (maxVal > 1f)
        for (int i = 0; i < samples.Length; i++)
            samples[i] /= maxVal;

    AudioClip clip = AudioClip.Create("tts_output", sampleCount, 1, sampleRate, false);
    clip.SetData(samples, 0);

    Debug.Log($"AudioClip created: {clip.length:F2} seconds");
    return clip;
}
}



// CODE FOR ORPHEUS
// public class TextToSpeech : MonoBehaviour
// {
//     [Header("API Configuration")]
//     [SerializeField] private string groqApiKey = "";
//     [SerializeField] private string voice = "hannah";

//     private const string API_URL = "https://api.groq.com/openai/v1/audio/speech";

//     public async Task<AudioClip> GenerateSpeech(string text)
// {
//     string jsonBody = $@"{{
//         ""model"": ""canopylabs/orpheus-v1-english"",
//         ""input"": ""{text}"",
//         ""voice"": ""{voice}"",
//         ""response_format"": ""wav""
//     }}";

//     using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
//     {
//         byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
//         request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//         request.downloadHandler = new DownloadHandlerBuffer();
//         request.SetRequestHeader("Content-Type", "application/json");
//         request.SetRequestHeader("Authorization", $"Bearer {groqApiKey}");

//         var operation = request.SendWebRequest();
//         while (!operation.isDone)
//             await Task.Yield();

//         if (request.result != UnityWebRequest.Result.Success)
//         {
//             Debug.LogError($"TTS API Error: {request.error}");
//             Debug.LogError($"Response: {request.downloadHandler.text}");
//             Debug.LogError($"Status Code: {request.responseCode}");
//             Debug.LogError($"Request body: {jsonBody}");
//             return null;
//         }

//         Debug.Log($"TTS Success! Received {request.downloadHandler.data.Length} bytes of audio data");
//         return ConvertWavToAudioClip(request.downloadHandler.data);
//     }
// }

//     private AudioClip ConvertWavToAudioClip(byte[] wavData)
// {
//     if (wavData == null || wavData.Length < 44)
//     {
//         Debug.LogError($"Invalid WAV data! Length: {wavData?.Length ?? 0}");
//         return null;
//     }

//     // Read essential header info
//     int channels = System.BitConverter.ToInt16(wavData, 22);
//     int sampleRate = System.BitConverter.ToInt32(wavData, 24);
    
//     // Read all data after standard 44-byte header
//     int dataSize = wavData.Length - 44;
//     int sampleCount = dataSize / 2; // 16-bit samples = 2 bytes each
    
//     Debug.Log($"WAV: {channels} channels, {sampleRate}Hz, {dataSize} bytes, {sampleCount} samples");

//     if (sampleCount <= 0)
//     {
//         Debug.LogError("No audio samples found!");
//         return null;
//     }

//     float[] samples = new float[sampleCount];
//     int offset = 44; // Standard WAV header size

//     for (int i = 0; i < sampleCount; i++)
//     {
//         if (offset + 1 < wavData.Length)
//         {
//             short s = System.BitConverter.ToInt16(wavData, offset);
//             samples[i] = s / 32768f;
//             offset += 2;
//         }
//     }

//     AudioClip clip = AudioClip.Create("tts_output", sampleCount / channels, channels, sampleRate, false);
//     clip.SetData(samples, 0);
    
//     Debug.Log($"AudioClip created: {clip.length:F2} seconds");
//     return clip;
// }
// }