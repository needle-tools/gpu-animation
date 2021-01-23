#if !NO_INTERNALS_ACCESS

using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Internal;

namespace UnityEditor.ShaderGraph
{
    [Title("Input", "Custom", "Generate Vertex ID")]
    internal class GenerateVertexIDNode : AbstractMaterialNode, IMayRequireVertexID, IGeneratesBodyCode
    {
        const string kOutputSlotName = "Out";
        public const int OutputSlotId = 0;
        
        public GenerateVertexIDNode()
        {
            name = "Generate Vertex ID";
            UpdateNodeAfterDeserialization();
        }
        
        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Vector1MaterialSlot(OutputSlotId, kOutputSlotName, kOutputSlotName, SlotType.Output, 0, ShaderStageCapability.Vertex));
            RemoveSlotsNameNotMatching(new[] { OutputSlotId });
        }
        
        public bool RequiresVertexID(ShaderStageCapability stageCapability = ShaderStageCapability.All) => true;
        
        public void GenerateNodeCode(ShaderStringBuilder sb, GenerationMode generationMode)
        {
            sb.AppendLine(string.Format("$precision {0} = {1};", GetVariableNameForSlot(OutputSlotId), "IN.VertexID"));
        }
    }
}

#endif