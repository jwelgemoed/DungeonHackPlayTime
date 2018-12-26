#include "DeferredUtilities.hlsl"

//////////////////////////////////////////////////////////////////////////////
// Vertex Shader
//////////////////////////////////////////////////////////////////////////////
VS_OUTPUT AmbientLightVS(uint VertexID : SV_VertexID)
{
	VS_OUTPUT output;

	output.UV = float2((VertexID << 1) & 2, VertexID & 2);
	output.Position = float4(output.UV * float2(2, -2) + float2(-1, 1), 0, 1);

	//output.Position = float4(arrBasePos[VertexID].xy, 0.0, 1.0);
	output.cpPosition = output.Position.xy;

	return output;
}