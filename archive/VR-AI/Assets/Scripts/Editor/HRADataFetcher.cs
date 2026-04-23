using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System.Text;
using UnityEngine.Networking;

public class HRADataFetcher : EditorWindow
{
    [MenuItem("VR-AI-ment/Fetch HRA Heart Data")]
    public static void ShowWindow()
    {
        GetWindow<HRADataFetcher>("HRA Fetcher");
    }

    private VisualizationData target;

    void OnGUI()
    {
        target = (VisualizationData)EditorGUILayout.ObjectField(
            "Target ScriptableObject", target, typeof(VisualizationData), false);

        if (target == null) { EditorGUILayout.HelpBox("Assign a VisualizationData asset.", MessageType.Info); return; }

        if (GUILayout.Button("Fetch & Populate from HRA API"))
        {
            FetchAndPopulate(target);
        }
    }

    static async void FetchAndPopulate(VisualizationData data)
    {
        // HRA ASCT+B API — heart table
        // organ param maps to the organ slug used by the reporter
        string url = "https://apps.humanatlas.io/asctb-api/v2/asctb-tables?organ=heart";
        
        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Accept", "application/json");
        var op = req.SendWebRequest();
        
        while (!op.isDone) await Task.Yield();
        
        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"HRA API error: {req.error}");
            return;
        }

        string json = req.downloadHandler.text;
        // Parse and format — see Step 3
        string prose = FormatAsProse(json, data.sex);
        
        data.contextualData = prose;
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
        Debug.Log("VisualizationData updated with HRA context.");
    }

    static string FormatAsProse(string json, string sex)
    {
        // Parse the ASCT+B JSON and build a prose description
        // The API returns rows with AS/CT/B columns
        var sb = new StringBuilder();
        sb.AppendLine($"This is the HRA 2.4 {sex} human heart model (UBERON:0000948), " +
                      $"part of the Human Reference Atlas built by the HuBMAP consortium.");
        sb.AppendLine();
        
        // You'll extend this with actual parsed fields from json
        // For now, a structured fallback 
        sb.AppendLine(ParseHRAJson(json));
        
        return sb.ToString();
    }

    static string ParseHRAJson(string json)
    {
        // Implement JSON parsing here based on actual API response shape
        // The ASCT+B API returns { data: [ { anatomical_structures, cell_types, biomarkers } ] }
        return "[Parsed HRA data goes here — implement after inspecting API response]";
    }
}