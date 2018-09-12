#include "Utilities.hlsl"

////////////////////////////////////////////////////////////////////////////////
// Hull Shader
////////////////////////////////////////////////////////////////////////////////
PatchTess PatchHullShader(InputPatch<VertexOutputType, 3> patch,
	uint patchID : SV_PrimitiveID)
{
	PatchTess pt;

	pt.EdgeTess[0] = 0.5f*(patch[1].tessFactor + patch[2].tessFactor);
	pt.EdgeTess[1] = 0.5f*(patch[2].tessFactor + patch[0].tessFactor);
	pt.EdgeTess[2] = 0.5f*(patch[0].tessFactor + patch[1].tessFactor);

	pt.InsideTess = pt.EdgeTess[0];

	return pt;
}

[domain("tri")] 
[partitioning("fractional_odd")] 
[outputtopology("triangle_cw")] 
[outputcontrolpoints(3)] 
[patchconstantfunc("PatchHullShader")] 
HullOut HS(InputPatch<VertexOutputType,3> p,
	uint i : SV_OutputControlPointID,
	uint patchId : SV_PrimitiveID)
{
	HullOut hout;

	// Pass through shader. 
	hout.position = p[i].position;
	hout.worldPosition = p[i].worldPosition;
	hout.tex = p[i].tex;
	hout.normal = p[i].normal;
	hout.tangent = p[i].tangent;
	hout.viewDirection = p[i].viewDirection;
	hout.fogFactor = p[i].fogFactor;

	return hout;
}