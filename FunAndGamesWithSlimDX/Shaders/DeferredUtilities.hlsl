#define NUM_DIRECTIONAL_LIGHTS 1
#define NUM_POINT_LIGHTS 2
#define NUM_SPOT_LIGHTS 1

static const float2 g_SpecPowerRange = { 0.1, 250.0 };

static const float2 arrBasePos[4] =
{
	float2(-1.0, 1.0),
	float2(1.0, 1.0),
	float2(-1.0, -1.0),
	float2(1.0, -1.0),
};

struct AmbientLight {
	float3 AmbientDown;
	float3 AmbientUp;
};

//struct DirectionalLight {
//	float4 Color;
//	float3 Direction;
//	float pad;
//};

struct PointLight {
	float4 Color;
	float3 Position;
	float Range;
	float3 Attentuation;
	float pad;
	matrix LightCalculations;
};

struct SpotLight {
	float4 Ambient;
	float4 Diffuse;
	float4 Specular;
	float3 Position;
	float Range;
	float3 Direction;
	float Spot;
	float3 Attentuation;
	float pad;
};

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
	float2 cpPosition : POSITION;
};

struct HS_CONSTANT_DATA_OUTPUT
{
	float Edges[4] : SV_TessFactor;
	float Inside[2] : SV_InsideTessFactor;
};

static const float3 HemiDir[2] =
{
	float3(1.0, 1.0, 1.0),
	float3(-1.0, 1.0, -1.0)
};


struct HS_OUTPUT
{
	float3 HemiDir : HEMIDIR;
};

cbuffer cbGBufferUnpack: register(b0)
{
	float4 PerspectiveValues : packoffset(c0);
	matrix ViewInv : packoffset(c1);
};

//cbuffer cbPerFrame : register(b1)
//{
//	DirectionalLight gDirLight[NUM_DIRECTIONAL_LIGHTS];
//	SpotLight gSpotLight[NUM_SPOT_LIGHTS];
//	PointLight gPointLight[NUM_POINT_LIGHTS];
//	float3 cameraPosition;
//	float fogStart;
//	float fogEnd;
//	bool useNormalMap;
//	float gMaxTessDistance;
//	float gMinTessDistance;
//	float gMinTessFactor;
//	float gMaxTessFactor;
//	float2 pad;
//	AmbientLight gAmbientLight;
//};

float ConvertDepthToLinear(float depth)
{
	float linearDepth = PerspectiveValues.z / (depth + PerspectiveValues.w);

	return linearDepth;
};
