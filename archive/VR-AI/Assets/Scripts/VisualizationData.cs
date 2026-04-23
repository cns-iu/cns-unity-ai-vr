using UnityEngine;

[CreateAssetMenu(fileName = "VisualizationData", menuName = "VR-AI/Visualization Data")]
public class VisualizationData : ScriptableObject
{
    [Header("Basic Info")]
    public string modelName;
    public string organType;
    [TextArea(3, 6)]
    public string description;

    [Header("HRA Identifiers")]
    public string uberonId;       // e.g. "UBERON:0000948"
    public string sex;            // "female" or "male"

    [Header("LLM Context")]
    [TextArea(10, 30)]
    public string contextualData; // Rich prose baked in by Editor script
}