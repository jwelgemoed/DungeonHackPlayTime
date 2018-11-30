#define NUM_DIRECTIONAL_LIGHTS 1
#define NUM_POINT_LIGHTS 2
#define NUM_SPOT_LIGHTS 1

struct AmbientLight {
	float3 AmbientDown;
	float3 AmbientUp;
};

struct DirectionalLight {
	float4 Color;
	float3 Direction;
	float pad;
};

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
	float4 Ambient;
	float4 Diffuse;
	float4 Specular;
	float4 Reflect;
};

cbuffer cbPerObject : register(b0)
{
	matrix worldMatrix;
	matrix viewMatrix;
	matrix viewProjectionMatrix;
	Material material;
};

cbuffer cbPerFrame : register(b1)
{
	DirectionalLight gDirLight[NUM_DIRECTIONAL_LIGHTS];
	SpotLight gSpotLight[NUM_SPOT_LIGHTS];
	PointLight gPointLight[NUM_POINT_LIGHTS];
	float3 cameraPosition;
	float fogStart;
	float fogEnd;
	bool useNormalMap;
	float gMaxTessDistance;
	float gMinTessDistance;
	float gMinTessFactor;
	float gMaxTessFactor;
	float2 pad;
	AmbientLight gAmbientLight;
};

SamplerState SampleType;

SamplerState samLinear
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

struct VertexInputType
{
	float4 position : POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
};

struct VertexOutputType
{
	float4 position : SV_POSITION;
	float4 worldPosition :  POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float3 viewDirection : TEXCOORD1;
	float fogFactor : FOG;
	float tessFactor : TESS;
};

struct PatchTess
{
	float EdgeTess[3] : SV_TessFactor;
	float InsideTess : SV_InsideTessFactor;
};

struct HullOut
{
	float4 position : SV_POSITION;
	float4 worldPosition :  POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float3 viewDirection : TEXCOORD1;
	float fogFactor : FOG;
};

struct DomainOut
{
	float4 position : SV_POSITION;
	float4 worldPosition :  POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float3 viewDirection : TEXCOORD1;
	float fogFactor : FOG;
};

struct PS_GBUFFER_OUT
{
	float4 ColorSpecInt: SV_TARGET0;
	float4 Normal: SV_TARGET1;
	float4 SpecPow: SV_TARGET2;
};

static const float2 g_SpecPowerRange = { 0.1, 250.0 };

//---------------------------------------------------------------------------------------
// Computes the ambient, diffuse, and specular terms in the lighting equation
// from a directional light.  We need to output the terms separately because
// later we will modify the individual terms.
//---------------------------------------------------------------------------------------
//void ComputeDirectionalLight(Material mat, DirectionalLight L, float3 normal, float3 toEye,
//	out float4 ambient, out float4 diffuse, out float4 spec)
//{
//	// Initialize outputs.
//	ambient = L.Ambient;
//	diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
//	spec = float4(0.0f, 0.0f, 0.0f, 0.0f);
//
//	// The light vector aims opposite the direction the light rays travel.
//	float3 lightVec = -L.Direction;
//
//	// Add ambient term.
//	ambient = mat.Ambient * L.Ambient;
//
//	// Add diffuse and specular term, provided the surface is in 
//	// the line of site of the light.
//
//	float diffuseFactor = dot(lightVec, normal);
//
//	// Flatten to avoid dynamic branching.
//	[flatten]
//	if (diffuseFactor > 0.0f)
//	{
//		float3 v = reflect(-lightVec, normal);
//		float specFactor = pow(max(dot(v, toEye), 0.0f), mat.Specular.w);
//
//		diffuse = diffuseFactor * mat.Diffuse * L.Diffuse;
//		spec = specFactor * mat.Specular * L.Specular;
//	}
//}
//
////---------------------------------------------------------------------------------------
//// Computes the ambient, diffuse, and specular terms in the lighting equation
//// from a point light.  We need to output the terms separately because
//// later we will modify the individual terms.
////---------------------------------------------------------------------------------------
//void ComputePointLight(float4 specIntensity, Material mat, PointLight L, float3 pos, float3 normal, float3 toEye,
//	out float4 ambient, out float4 diffuse, out float4 spec)
//{
//	// Initialize outputs.
//	ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
//	diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
//	spec = float4(0.0f, 0.0f, 0.0f, 0.0f);
//
//	// The vector from the surface to the light.
//	// Something's wrong I can feel it!
//	float3 lightVec = L.Position - pos;//pos - L.Position;//L.Position - pos;//pos - L.Position;//L.Position - pos;
//
//	// The distance from surface to light.
//	float d = length(lightVec);
//
//	// Range test.
//	if (d > L.Range)
//		return;
//
//	// Normalize the light vector.
//	lightVec /= d;
//
//	// Ambient term.
//	ambient = mat.Ambient * L.Ambient;
//
//	// Add diffuse and specular term, provided the surface is in 
//	// the line of site of the light.
//	float diffuseFactor = dot(lightVec, normal);
//
//	// Flatten to avoid dynamic branching.
//	[flatten]
//	if (diffuseFactor > 0.0f)
//	{
//		float3 v = reflect(-lightVec, normal);
//
//		// Determine the amount of specular light based on the reflection vector, viewing direction, and specular power.
//		//specular = pow(saturate(dot(reflection, input.viewDirection)), specularPower);
//		//float specFactor = pow(max(dot(v, toEye), 0.0f), mat.Specular.w);
//		float specFactor = pow(saturate(dot(v, toEye)), L.Specular);
//
//		diffuse = diffuseFactor * mat.Diffuse * L.Diffuse;
//		//spec = specFactor * mat.Specular * L.Specular;
//		spec = specFactor * mat.Specular * specIntensity;
//
//	}
//
//	// Attenuate
//	float att = 1.0f / dot(L.Attentuation, float3(1.0f, d, d*d));
//
//	diffuse *= att;
//	spec *= att;
//}
//
////---------------------------------------------------------------------------------------
//// Computes the ambient, diffuse, and specular terms in the lighting equation
//// from a spotlight.  We need to output the terms separately because
//// later we will modify the individual terms.
////---------------------------------------------------------------------------------------
//void ComputeSpotLight(float4 specIntensity, Material mat, SpotLight L, float3 pos, float3 normal, float3 toEye,
//	out float4 ambient, out float4 diffuse, out float4 spec)
//{
//	// Initialize outputs.
//	ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
//	diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
//	spec = float4(0.0f, 0.0f, 0.0f, 0.0f);
//
//	// The vector from the surface to the light.
//	float3 lightVec = L.Position - pos;
//
//	// The distance from surface to light.
//	float d = length(lightVec);
//
//	// Range test.
//	if (d > L.Range)
//		return;
//
//	// Normalize the light vector.
//	lightVec /= d;
//
//	// Ambient term.
//	ambient = mat.Ambient * L.Ambient;
//
//	// Add diffuse and specular term, provided the surface is in 
//	// the line of site of the light.
//
//	float diffuseFactor = dot(lightVec, normal);
//
//	// Flatten to avoid dynamic branching.
//	[flatten]
//	if (diffuseFactor > 0.0f)
//	{
//		float3 v = reflect(-lightVec, normal);
//		//float specFactor = pow(max(dot(v, toEye), 0.0f), mat.Specular.w);
//		float specFactor = pow(saturate(dot(v, toEye)), L.Specular);
//
//		diffuse = diffuseFactor * mat.Diffuse * L.Diffuse;
//		//spec = specFactor * mat.Specular * L.Specular;
//		spec = specFactor * mat.Specular * specIntensity;
//	}
//
//	// Scale by spotlight factor and attenuate.
//	float spot = pow(max(dot(-lightVec, L.Direction), 0.0f), L.Spot);
//
//	// Scale by spotlight factor and attenuate.
//	float att = spot / dot(L.Attentuation, float3(1.0f, d, d*d));
//
//	ambient *= spot;
//	diffuse *= att;
//	spec *= att;
//}

//--------------------------------------------------------------------- 
// Transforms a normal map sample to world space. 
//--------------------------------------------------------------------- 
float3 NormalSampleToWorldSpace(float3 normalMapSample, float3 unitNormalW, float3 tangentW)
{
	// Uncompress each component from [0,1] to [-1,1]. 
	float3 normalT = 2.0f* normalMapSample - 1.0f;
	// Build orthonormal basis. 
	float3 N = unitNormalW;
	float3 T = normalize(tangentW - dot(tangentW, N)* N);
	float3 B = cross(N, T);
	float3x3 TBN = float3x3(T, B, N);
	// Transform from tangent space to world space. 
	float3 bumpedNormalW = mul(normalT, TBN);
	return bumpedNormalW;
}
