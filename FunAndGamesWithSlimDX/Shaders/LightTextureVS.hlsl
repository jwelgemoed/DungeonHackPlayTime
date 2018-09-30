#include "DeferredUtilities.hlsl"

////////////////////////////////////////////////////////////////////////////////
// Vertex Shader
////////////////////////////////////////////////////////////////////////////////
VS_OUTPUT LightVertexShader(uint VertexID : SV_VertexID)
{
	VS_OUTPUT output;

	output.Position = float4(arrBasePos[VertexID].xy, 0.0, 1.0);
	output.cpPosition = output.Position.xy;

	return output;
}

