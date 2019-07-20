#include "DeferredUtilities.hlsl"

#define EyePosition ViewInv[3].xyz;

Texture2D DepthTexture : register(t0);
Texture2D ColorSpecIntTexture : register(t1);
Texture2D NormalTexture : register(t2);
Texture2D SpecPowerTexture : register(t3);

cbuffer cbPointLight : register(b1)
{
	PointLight gPointLight;
}

SURFACE_DATA UnpackGBuffer(int2 location)
{
	SURFACE_DATA output;

	int3 location3 = int3(location, 0);

	float depth = DepthTexture.Load(location3).x;
	output.LinearDepth = ConvertDepthToLinear(depth);

	float4 baseColorSpecInt = ColorSpecIntTexture.Load(location3);
	output.Color = baseColorSpecInt.xyz;
	output.SpecInt = baseColorSpecInt.w;

	output.Normal = NormalTexture.Load(location3).xyz;

	float specPowerNorm = SpecPowerTexture.Load(location3).x;
	output.SpecPow = specPowerNorm.x + specPowerNorm * g_SpecPowerRange.y;

	return output;
}

float3 CalcWorldPosition(float2 csPos, float linearDepth)
{
	float4 position;

	position.xy = csPos.xy * PerspectiveValues.xy * linearDepth;
	position.z = linearDepth;
	position.w = 1.0;

	position = mul(position, ViewInv);

	return position.xyz;
}

float3 CalcPoint(float3 position, Material material)
{
	float3 toLight = gPointLight.Position - position;
	float3 toEye = EyePosition - position;
	float distToLight = length(toLight);

	//Phong diffuse
	toLight /= distToLight;
	float NDotL = saturate(dot(toLight, material.normal));
	float3 finalColor = gPointLight.Color.rgb * NDotL;

	//Blinn specular
	toEye = normalize(toEye);
	float3 halfway = normalize(toEye + toLight);
	float NDotH = saturate(dot(halfway, material.normal));
	//finalColor += gPointLight.Color.rgb * pow(NDotH, material.specPower)*material.specIntensity;
	finalColor += pow(NDotH, material.specPower)*material.specIntensity;

	//Attentuation	
	float distToLightNorm = 1.0 - saturate(distToLight * gPointLight.Range);
	float attn = distToLightNorm * distToLightNorm;
	finalColor *= gPointLight.Color.rgb * attn;
	//finalColor *= material.diffuseColor.rgb * attn;

	return finalColor;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 PointLightPS(VS_OUTPUT input) : SV_TARGET
{
	SURFACE_DATA gbd = UnpackGBuffer(input.Position.xy);

	Material mat;
	mat.normal = gbd.Normal;
	mat.diffuseColor.xyz = gbd.Color;
	mat.diffuseColor.w = 1.0;
	mat.specPower = g_SpecPowerRange.x + g_SpecPowerRange.y * gbd.SpecPow;
	mat.specIntensity = gbd.SpecInt;

	float3 position = CalcWorldPosition(input.cpPosition, gbd.LinearDepth);

	float3 finalColor = finalColor.xyz = CalcPoint(position, mat);

	return float4(finalColor, 1.0);
}