#include "DeferredUtilities.hlsl"

#define EyePosition ViewInv[3].xyz;

Texture2D DepthTexture : register(t0);
Texture2D ColorSpecIntTexture : register(t1);
Texture2D NormalTexture : register(t2);
Texture2D SpecPowerTexture : register(t3);

SURFACE_DATA UnpackGBuffer(int2 location)
{
	SURFACE_DATA output;

	int3 location3 = int3(location, 0);

	float depth = DepthTexture.Load(location3).x;
	output.LinearDepth = ConvertDepthToLinear(depth);

	float4 baseColorSpecInt = ColorSpecIntTexture.Load(location3);
	output.Color = baseColorSpecInt.xyz;
	output.SpecInt = baseColorSpecInt.w;

	output.Normal = NormalTexture.Load(location3);
	output.Normal = normalize(output.Normal * 2.0 - 1.0);

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

float3 CalcDirectional(float3 position, Material material)
{
	// Phong diffuse
	float NDotL = dot(gDirLight[0].Direction, material.normal);
	float3 finalColor = gDirLight[0].Color.rgb * saturate(NDotL);

	// Blinn specular
	float3 ToEye = EyePosition - position;
	ToEye = normalize(ToEye);
	float3 HalfWay = normalize(ToEye + gDirLight[0].Direction);
	float NDotH = saturate(dot(HalfWay, material.normal));
	finalColor += gDirLight[0].Color.rgb * pow(NDotH, material.specPower) * material.specIntensity;

	return finalColor * material.diffuseColor.rgb;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 LightPixelShader(VS_OUTPUT input) : SV_TARGET
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
	finalColor.xyz += CalcDirectional(position, mat);
	finalColor.w = 1.0;

	return finalColor;
}