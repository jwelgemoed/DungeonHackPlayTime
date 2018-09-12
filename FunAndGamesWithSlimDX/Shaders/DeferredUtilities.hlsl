#define NUM_DIRECTIONAL_LIGHTS 1
#define NUM_POINT_LIGHTS 2
#define NUM_SPOT_LIGHTS 1

static const float2 arrBasePos[4] = 
{
	float2(-1.0, 1.0),
	float2(1.0, 1.0),
	float2(-1.0, -1.0),
	float2(1.0, -1.0),
}

struct Material {
	float3 normal;
	float4 diffuseColor;
	float specPower;
	float specIntensity;
};

struct SURFACE_DATA
{
	float LinearDepth;
	float3 Color;
	float3 Normal;
	float SpecInt;
	float SpecPow;
	float FogFactor;
};

struct VS_OUTPUT
{
	float4 Position : SV_Position;
	float2 UV : TEXCOORD0;
	float2 cpPosition
};

cbuffer cbGBufferUnpack: register(b0)
{
	float4 PerspectiveValues : packoffset(c0);
	float4 ViewInv : packoffset(c1);
};

cbuffer cbPerObject: register(b0)
{
	float3 DirToLight : packoffset(c0);
	float4 DirLightColor : packoffset(c1);
}

float ConvertDepthToLinear(float depth)
{
	float linearDepth = PerspectiveValues.z / (depth + PerspectiveValues.w);

	return linearDepth;
}