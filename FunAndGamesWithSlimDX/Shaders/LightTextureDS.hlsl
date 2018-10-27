#include "DeferredUtilities.hlsl"

struct DS_OUTPUT 
{
	float4 Position : SV_POSITION;
	float4 cpPos : TEXCOORD0;
};

[domain("quad")]
DS_OUTPUT PointLightDS(HS_CONSTANT_DATA_OUTPUT input, float2 UV : SV_DomainLocation, constOutputPatch<HS_OUTPUT, 4> quad)
{
	// Transform the UV's into clip-space
	float2 posClipSpace = UV.xy * 2.0 - 1.0;

	// Find the absulate maximum distance from the center
	float2 posClipSpaceAbs = abs(posClipSpace.xy);
	float maxLen = max(posClipSpaceAbs.x, posClipSpaceAbs.y);

	// Generate the final position in clip-space
	float3 normDir = normalize(float3(posClipSpace.xy, maxLen - 1.0) * quad[0].HemiDir);
	float4 posLS = float4(normDir.xyz, 1.0);

	// Transform all the way to projected space and generate the UV coordinates
	DS_OUTPUT Output;
	Output.Position = mul(posLS, LightProjection);

	// Store the clip space position
	Output.cpPos = Output.Position.xy / Output.Position.w;

	return Output;
}