#include "DeferredUtilities.hlsl"

static const float3 HemiDir[2] = 
{
	float3(1.0, 1.0, 1.0),
	float3(-1.0, 1.0, -1.0)
}

struct HS_OUTPUT
{
	HemiDir HemiDir;
};

struct HS_CONSTANT_DATA_OUTPUT
{
	float Edges[4] : SV_TessFactor;
	float Inside[2] : SV_InsideTessFactor;
};

HS_CONSTANT_DATA_OUTPUT PointLightConstantHS()
{
	HS_CONSTANT_DATA_OUTPUT output;

	float tessFactor = 18.0;
	output.Edges[0] = output.Edges[1] = output.Edges[2] = output.Edges[3] = tessFactor;
	output.Inside[0] = output.Inside[1] = tessFactor;

	return output;
}


[domain("quad")]
[partitioning("integer")]
[outputtopology("triangle_ccw")]
[outputcontrolpoints(4)]
[patchconstantfunc("PointLightConstantHS")]
float3 PointLightHS(uint PatchId: SV_PrimitiveID): POSITION
{
	HS_OUTPUT output;

	output.HemiDir = HemiDir[PatchID];

	return output;
}