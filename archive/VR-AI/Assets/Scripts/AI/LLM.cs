using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text;

public class LLM : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string groqApiKey = "";
    [SerializeField] private string model = "meta-llama/llama-4-scout-17b-16e-instruct";

    [Header("Agent Identity")]
    [SerializeField] private string agentName = "Glitch";q
    [SerializeField] private string agentDescription = "an AI assistant robot";

    private const string API_URL = "https://api.groq.com/openai/v1/chat/completions";

    public async Task<string> GetResponse(string userMessage, VisualizationData context = null)
    {
        string systemPrompt = BuildSystemPrompt(context);

        string jsonBody = $@"{{
            ""model"": ""{model}"",
            ""messages"": [
                {{""role"": ""system"", ""content"": ""{EscapeJson(systemPrompt)}""}},
                {{""role"": ""user"", ""content"": ""{EscapeJson(userMessage)}""}}
            ],
            ""max_tokens"": 150
        }}";

        using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {groqApiKey}");

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"LLM Error: {request.error}");
                return "Sorry, I encountered an error.";
            }

            string json = request.downloadHandler.text;
            // This breaks on ANY special character in the response
int contentIndex = json.IndexOf("\"content\":\"") + 11;
int contentEnd = json.IndexOf("\"", contentIndex); // ← finds wrong quote
            return json.Substring(contentIndex, contentEnd - contentIndex);
        }
    }

    private string BuildSystemPrompt(VisualizationData context)
    {
        string basePrompt = $"You are {agentName}, {agentDescription} helping users explore 3D anatomical visualizations from the Human Reference Atlas. Your name is {agentName}. If asked your name, respond that you are {agentName}. Give concise, informative answers under 50 words.";

        if (context == null)
        {
            Debug.Log("[SYSTEM PROMPT] No context — user not looking at a model.");
            return basePrompt + " The user is not currently looking at any specific model.";
        }

        string fullPrompt = basePrompt + $@" The user is currently looking at: {context.modelName}. 
Organ type: {context.organType}. 
Description: {context.description}. 
Contextual Data: {context.contextualData}.";

        Debug.Log("[SYSTEM PROMPT] " + fullPrompt);
        return fullPrompt;
    }
    //GENERIC 

    // private string BuildSystemPrompt(VisualizationData context)
    // {
    //     string basePrompt = "You are an AI assistant helping users explore 3D anatomical visualizations from the Human Reference Atlas. Give concise, informative answers under 50 words.";

    //     if (context == null)
    //         return basePrompt + " The user is not currently looking at any specific model.";

    //     return basePrompt + $@" The user is currently looking at: {context.modelName}. 
    // Organ type: {context.organType}. 
    // Description: {context.description}. 
    // Key facts: {context.keyFacts}. 
    // Cell type data: {context.cellTypeData}.";
    // }

    private string EscapeJson(string text)
    {
        return text.Replace("\\", "\\\\")
                   .Replace("\"", "\\\"")
                   .Replace("\n", "\\n")
                   .Replace("\r", "\\r");
    }
}