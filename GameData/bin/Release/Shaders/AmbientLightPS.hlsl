#include "DeferredUtilities.hlsl"

#define EyePosition ViewInv[3].xyz;

struct DirectionalLight {
	float4 Color;
	float3 Direction;
	float pad;
};

Texture2D DepthTexture : register(t0);
Texture2D ColorSpecIntTexture : register(t1);
Texture2D NormalTexture : register(t2);
Texture2D SpecPowerTexture : register(t3);

cbuffer cbAmbientLight : register(b1)
{
	AmbientLight gAmbientLight;
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

float3 CalcAmbient(float3 normal, float3 color)
{
	float up = normal.y * 0.5 + 0.5;
	float3 ambient = gAmbientLight.AmbientDown + up * gAmbientLight.AmbientUp;

	return ambient * color;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 AmbientLightPS(VS_OUTPUT input) : SV_TARGET
{
	SURFACE_DATA gbd = UnpackGBuffer(input.Position.xy);

	Material mat;
	mat.normal = gbd.Normal;
	mat.diffuseColor.xyz = gbd.Color;
	mat.diffuseColor.w = 1.0;
	mat.specPower = g_SpecPowerRange.x + g_SpecPowerRange.y * gbd.SpecPow;
	mat.specIntensity = gbd.SpecInt;

	float3 position = CalcWorldPosition(input.cpPosition, gbd.LinearDepth);

	float4 finalColor;
	finalColor.xyz = CalcAmbient(mat.normal, mat.diffuseColor.xyz);
	finalColor.w = 1.0;

	return finalColor;
}