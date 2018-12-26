#include "Utilities.hlsl"

Texture2D displacementMap;

[domain("tri")] 
DomainOut DS(PatchTess patchTess, 
	float3 bary : SV_DomainLocation, const OutputPatch<HullOut,3> tri) 
{
	DomainOut dout; 
	
	// Interpolate patch attributes to generated vertices. 
	dout.position = bary.x*tri[0].position + bary.y* tri[1].position + bary.z*tri[2].position; 
	dout.worldPosition = bary.x*tri[0].worldPosition + bary.y* tri[1].worldPosition + bary.z*tri[2].worldPosition;
	dout.normal = bary.x*tri[0].normal + bary.y* tri[1].normal + bary.z* tri[2].normal; 
	dout.tangent = bary.x*tri[0].tangent + bary.y*tri[1].tangent + bary.z*tri[2].tangent; 
	dout.tex = bary.x*tri[0].tex + bary.y*tri[1].tex + bary.z* tri[2].tex;
	dout.viewDirection = bary.x*tri[0].viewDirection + bary.y*tri[1].viewDirection + bary.z* tri[2].viewDirection;
	dout.fogFactor = bary.x*tri[0].fogFactor + bary.y*tri[1].fogFactor + bary.z* tri[2].fogFactor;
	
	// Interpolating normal can unnormalize it, so normalize it. 
	dout.normal = normalize(dout.normal); 
	
	// 
	// Displacement mapping. 
	// 
	// Choose the mipmap level based on distance to the eye; 
	// specifically, choose the next miplevel every MipInterval units, 
	// and clamp the miplevel in [0,6]. 
	const float MipInterval = 20.0f; 
	float mipLevel = clamp( (distance(dout.worldPosition, cameraPosition) - MipInterval) / MipInterval, 0.0f, 6.0f); 
	
	// Sample height map 
	float h = displacementMap.SampleLevel(samLinear, dout.tex, mipLevel).r; 
	
	// Offset vertex along normal. 
	float heightScale = 0.0;

	dout.worldPosition += float4((heightScale*(h-1))*dout.normal, 1.0f); 
	dout.worldPosition.w = 1.0f;
	
	// Project to homogeneous clip space. 
	dout.position = mul(dout.worldPosition, viewProjectionMatrix);

	return dout;
}