using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class SpeechToText : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string groqApiKey = "";
    
    private const string API_URL = "https://api.groq.com/openai/v1/audio/transcriptions";

    public async Task<string> ConvertSpeech(AudioClip audioClip)
    {
        byte[] audioData = ConvertAudioClipToWav(audioClip);
        
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");
        form.AddField("model", "whisper-large-v3-turbo");
        form.AddField("language", "en");

        using (UnityWebRequest request = UnityWebRequest.Post(API_URL, form))
        {
            request.SetRequestHeader("Authorization", $"Bearer {groqApiKey}");

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Whisper API Error: {request.error}");
                return "";
            }

            return ParseTranscriptionResponse(request.downloadHandler.text);
        }
    }

    private string ParseTranscriptionResponse(string jsonResponse)
    {
        int textIndex = jsonResponse.IndexOf("\"text\":\"");
        if (textIndex == -1) return "";
        int start = textIndex + 8;
        int end = jsonResponse.IndexOf("\"", start);
        return jsonResponse.Substring(start, end - start);
    }

    private byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        byte[] wav = new byte[44 + samples.Length * 2];

        // WAV header
        System.Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, wav, 0, 4);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(wav.Length - 8), 0, wav, 4, 4);
        System.Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, wav, 8, 4);
        System.Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, wav, 12, 4);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(16), 0, wav, 16, 4);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes((short)1), 0, wav, 20, 2);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes((short)clip.channels), 0, wav, 22, 2);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(clip.frequency), 0, wav, 24, 4);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(clip.frequency * clip.channels * 2), 0, wav, 28, 4);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes((short)(clip.channels * 2)), 0, wav, 32, 2);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes((short)16), 0, wav, 34, 2);
        System.Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("data"), 0, wav, 36, 4);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(samples.Length * 2), 0, wav, 40, 4);

        // Audio data
        int offset = 44;
        foreach (float sample in samples)
        {
            short s = (short)(sample * 32767);
            wav[offset++] = (byte)(s & 0xff);
            wav[offset++] = (byte)((s >> 8) & 0xff);
        }

        return wav;
    }
}