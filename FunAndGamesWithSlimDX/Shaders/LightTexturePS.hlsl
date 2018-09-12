#include "DeferredUtilities.hlsl"

#define EyePosition ViewInv[3].xyz;

Texture2D DepthTexture : register(t0);
Texture2D ColorSpecIntTexture : register(t1);
Texture2D NormalTexture : register(t2);
Texture2D SpecPowerTexture : register(t3);

cbuffer HemiConstants : register(b0)
{
	float3 AmbientDown   : packoffset(c0);
	float3 AmbientRange  : packoffset(c1);
}

SURFACE_DATA UnpackGBuffer(int2 location)
{
	SURFACE_DATA out;

	int3 location = int3(location, 0);

	float depth = DepthTexture.Load(location3).x;
	out.LinearDepth = ConvertDepthToLinear(depth);

	float4 baseColorSpecInt = ColorSpecIntTexture.Load(location3);
	out.Color = baseColorSpecInt.xyz;
	out.SpecInt = baseColorSpecInt.w;

	out.Normal = NormalTexture.Load(location3);
	out.Normal = normalize(out.Normal * 2.0 - 1.0);

	float specPowerNorm = SpecPowerTexture.Load(location3).x;
	out.SpecPower = specPowerNorm.x + specPowerNorm * g_SpecPowerRange.y;

	return out;
}

float3 CalcWorldPosition(float2 csPos, float linearDepth)
{
	float4 position;

	position.xy = csPos.xy * PerspectiveValues.xy * linearDepth;
	position.z = linearDepth;
	position.w = 1.0;

	return mul(position, ViewInv).xyz;
}

float3 CalcAmbient(float3 normal, float3 color)
{
	float up = normal.y * 0.5 + 0.5;
	float3 ambient = AmbientDown + up * AmbientRange;

	return ambient * color;
}

float3 CalcDirectional(float3 position, Material material)
{
	// Phong diffuse
	float NDotL = dot(DirToLight, material.normal);
	float3 finalColor = DirLightColor.rgb * saturate(NDotL);

	// Blinn specular
	float3 ToEye = EyePosition.xyz - position;
	ToEye = normalize(ToEye);
	float3 HalfWay = normalize(ToEye + DirToLight);
	float NDotH = saturate(dot(HalfWay, material.normal));
	finalColor += DirLightColor.rgb * pow(NDotH, material.specExp) * material.specIntensity;

	return finalColor * material.diffuseColor.rgb;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 LightPixelShader(VS_OUTPUT input) 
{
	SURFACE_DATA gbd = UnpackGBuffer(input.Position.xy);
	
	Material mat;
	mat.normal = gbd.Normal;
	mat.diffuseColor.xyz = gbd.Color;
	mat.diffuseColor.w = 1.0;
	mat.specPower = g_SpecPowerRange.x + g_SpecPowerRange.y * gbd.SpecPower;
	mat.specIntensity = gbd.SpecInt;

	float3 position = CalcWorldPosition(In.cpPos, gbd.LinearDepth);

	float4 finalColor = CalcAmbient(mat.normal, mat.diffuseColor.xyz);
	finalColor += CalcDirectional(position, mat);
	finalColor.w = 1.0;

	return finalColor;
}