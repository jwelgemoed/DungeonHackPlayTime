//Frank Luna 
#define NUM_DIRECTIONAL_LIGHTS 1
#define NUM_POINT_LIGHTS 3
#define NUM_SPOT_LIGHTS 1

struct DirectionalLight {
	float4 Ambient;
	float4 Diffuse;
	float4 Specular;
	float3 Direction;
	float pad;
};

struct PointLight {
	float4 Ambient;
	float4 Diffuse;
	float4 Specular;
	float3 Position;
	float Range;
	float3 Attentuation;
	float pad;
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

//---------------------------------------------------------------------------------------
// Computes the ambient, diffuse, and specular terms in the lighting equation
// from a directional light.  We need to output the terms separately because
// later we will modify the individual terms.
//---------------------------------------------------------------------------------------
void ComputeDirectionalLight(Material mat, DirectionalLight L, float3 normal, float3 toEye,
	out float4 ambient, out float4 diffuse, out float4 spec)
{
	// Initialize outputs.
	ambient = L.Ambient;
	diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	spec = float4(0.0f, 0.0f, 0.0f, 0.0f);

	// The light vector aims opposite the direction the light rays travel.
	float3 lightVec = -L.Direction;

	// Add ambient term.
	ambient = mat.Ambient * L.Ambient;

	// Add diffuse and specular term, provided the surface is in 
	// the line of site of the light.

	float diffuseFactor = dot(lightVec, normal);

	// Flatten to avoid dynamic branching.
	[flatten]
	if (diffuseFactor > 0.0f)
	{
		float3 v = reflect(-lightVec, normal);
		float specFactor = pow(max(dot(v, toEye), 0.0f), mat.Specular.w);

		diffuse = diffuseFactor * mat.Diffuse * L.Diffuse;
		spec = specFactor * mat.Specular * L.Specular;
	}
}

//---------------------------------------------------------------------------------------
// Computes the ambient, diffuse, and specular terms in the lighting equation
// from a point light.  We need to output the terms separately because
// later we will modify the individual terms.
//---------------------------------------------------------------------------------------
void ComputePointLight(float4 specIntensity, Material mat, PointLight L, float3 pos, float3 normal, float3 toEye,
	out float4 ambient, out float4 diffuse, out float4 spec)
{
	// Initialize outputs.
	ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	spec = float4(0.0f, 0.0f, 0.0f, 0.0f);

	// The vector from the surface to the light.
	// Something's wrong I can feel it!
	float3 lightVec = L.Position - pos;//pos - L.Position;//L.Position - pos;//pos - L.Position;//L.Position - pos;

	// The distance from surface to light.
	float d = length(lightVec);

	// Range test.
	if (d > L.Range)
		return;

	// Normalize the light vector.
	lightVec /= d;

	// Ambient term.
	ambient = mat.Ambient * L.Ambient;

	// Add diffuse and specular term, provided the surface is in 
	// the line of site of the light.
	float diffuseFactor = dot(lightVec, normal);

	// Flatten to avoid dynamic branching.
	[flatten]
	if (diffuseFactor > 0.0f)
	{
		float3 v = reflect(-lightVec, normal);

		// Determine the amount of specular light based on the reflection vector, viewing direction, and specular power.
		//specular = pow(saturate(dot(reflection, input.viewDirection)), specularPower);
		//float specFactor = pow(max(dot(v, toEye), 0.0f), mat.Specular.w);
		float specFactor = pow(saturate(dot(v, toEye)), L.Specular);

		diffuse = diffuseFactor * mat.Diffuse * L.Diffuse;
		//spec = specFactor * mat.Specular * L.Specular;
		spec = specFactor * mat.Specular * specIntensity;

	}

	// Attenuate
	float att = 1.0f / dot(L.Attentuation, float3(1.0f, d, d*d));

	diffuse *= att;
	spec *= att;
}

//---------------------------------------------------------------------------------------
// Computes the ambient, diffuse, and specular terms in the lighting equation
// from a spotlight.  We need to output the terms separately because
// later we will modify the individual terms.
//---------------------------------------------------------------------------------------
void ComputeSpotLight(float4 specIntensity, Material mat, SpotLight L, float3 pos, float3 normal, float3 toEye,
	out float4 ambient, out float4 diffuse, out float4 spec)
{
	// Initialize outputs.
	ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	spec = float4(0.0f, 0.0f, 0.0f, 0.0f);

	// The vector from the surface to the light.
	float3 lightVec = L.Position - pos;

	// The distance from surface to light.
	float d = length(lightVec);

	// Range test.
	if (d > L.Range)
		return;

	// Normalize the light vector.
	lightVec /= d;

	// Ambient term.
	ambient = mat.Ambient * L.Ambient;

	// Add diffuse and specular term, provided the surface is in 
	// the line of site of the light.

	float diffuseFactor = dot(lightVec, normal);

	// Flatten to avoid dynamic branching.
	[flatten]
	if (diffuseFactor > 0.0f)
	{
		float3 v = reflect(-lightVec, normal);
		//float specFactor = pow(max(dot(v, toEye), 0.0f), mat.Specular.w);
		float specFactor = pow(saturate(dot(v, toEye)), L.Specular);

		diffuse = diffuseFactor * mat.Diffuse * L.Diffuse;
		//spec = specFactor * mat.Specular * L.Specular;
		spec = specFactor * mat.Specular * specIntensity;
	}

	// Scale by spotlight factor and attenuate.
	float spot = pow(max(dot(-lightVec, L.Direction), 0.0f), L.Spot);

	// Scale by spotlight factor and attenuate.
	float att = spot / dot(L.Attentuation, float3(1.0f, d, d*d));

	ambient *= spot;
	diffuse *= att;
	spec *= att;
}

//--------------------------------------------------------------------- 
// Transforms a normal map sample to world space. 
//--------------------------------------------------------------------- 
float3 NormalSampleToWorldSpace(float3 normalMapSample, float3 unitNormalW, float3 tangentW) 
{ 
	// Uncompress each component from [0,1] to [-1,1]. 
	float3 normalT = 2.0f* normalMapSample - 1.0f; 
	// Build orthonormal basis. 
	float3 N = unitNormalW; 
	float3 T = normalize( tangentW - dot( tangentW, N)* N); 
	float3 B = cross( N, T); 
	float3x3 TBN = float3x3( T, B, N); 
	// Transform from tangent space to world space. 
	float3 bumpedNormalW = mul( normalT, TBN);
	return bumpedNormalW; 
}

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
};

Texture2D shaderTexture;
Texture2D normalMap;
Texture2D specularMap;
Texture2D displacementMap;

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

////////////////////////////////////////////////////////////////////////////////
// Vertex Shader
////////////////////////////////////////////////////////////////////////////////
VertexOutputType LightVertexShader(VertexInputType input)
{
	float4 worldPos;
	VertexOutputType output;

	// Change the position vector to be 4 units for proper matrix calculations.
	input.position.w = 1.0f;

	// Calculate the position of the vertex against the world, view, and projection matrices.
	worldPos = mul(input.position, worldMatrix);

	output.worldPosition = worldPos;
	output.position = mul(worldPos, viewProjectionMatrix);

	// Store the input color for the pixel shader to use.
	output.tex = input.tex;

	output.normal = mul(input.normal, (float3x3) worldMatrix);
	output.normal = normalize(output.normal);

	output.tangent = mul(input.tangent, worldMatrix);

	// Calculate the position of the vertex in the world.
	//output.worldPosition = mul(input.position, worldMatrix);
	//output.worldPosition = input.position;

	// Determine the viewing direction based on the position of the camera and the position of the vertex in the world.
	output.viewDirection = cameraPosition.xyz - output.worldPosition.xyz;
	
	float d = distance(output.worldPosition, cameraPosition);
	float tess = saturate((gMinTessDistance - d) / (gMinTessDistance - gMaxTessDistance));

	output.tessFactor = gMinTessFactor + tess * (gMaxTessFactor - gMinTessFactor);
	
	//cameraPosition.xyz - output.worldPosition.xyz;

	// Normalize the viewing direction vector.
	output.viewDirection = normalize(output.viewDirection);

	// Calculate linear fog.    
	worldPos = mul(worldPos, viewMatrix);
	output.fogFactor = saturate((fogEnd - worldPos.z) / (fogEnd - fogStart));

	return output;
}

////////////////////////////////////////////////////////////////////////////////
// Hull Shader
////////////////////////////////////////////////////////////////////////////////
struct PatchTess
{
	float EdgeTess[3] : SV_TessFactor;
	float InsideTess : SV_InsideTessFactor;
};

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
	float heightScale = 1.0;

	dout.worldPosition += float4((heightScale*(h-1))*dout.normal, 1.0f); 
	dout.worldPosition.w = 1.0f;
	
	// Project to homogeneous clip space. 
	dout.position = mul(dout.worldPosition, viewProjectionMatrix);

	return dout;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 LightPixelShader(DomainOut input) : SV_TARGET
{
	float4 textureColor;
	float3 lightDir;
	float lightIntensity;
	float4 color;
	float3 reflection;
	float4 fogColor;

	input.normal = normalize(input.normal);

	// Sample the pixel color from the texture using the sampler at this texture coordinate location.
	textureColor = shaderTexture.Sample(SampleType, input.tex);
	textureColor = float4(textureColor.rgb * textureColor.rgb, textureColor.a);
	
	float3 bumpedNormalW = input.normal;

	if (useNormalMap)
	{
		float3 normalMapSample = normalMap.Sample(samLinear, input.tex).rgb;
		bumpedNormalW = NormalSampleToWorldSpace(normalMapSample, input.normal, input.tangent);
	}

	float4 ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 specular = float4(0.0f, 0.0f, 0.0f, 0.0f);

	float4 specIntensity = specularMap.Sample(samLinear, input.tex);

	float4 A, D, S;

	for (uint i = 0; i < NUM_DIRECTIONAL_LIGHTS; i++)
	{
		ComputeDirectionalLight(material, gDirLight[i], bumpedNormalW, input.viewDirection, A, D, S);

		ambient += A;
		diffuse += D;
		specular += S;
	}

	for (uint i = 0; i < NUM_POINT_LIGHTS; i++)
	{
		ComputePointLight(specIntensity, material, gPointLight[i], input.worldPosition, bumpedNormalW, input.viewDirection, A, D, S);

		ambient += A;
		diffuse += D;
		specular += S;
	}

	for (uint i = 0; i < NUM_SPOT_LIGHTS; i++)
	{
		ComputeSpotLight(specIntensity, material, gSpotLight[i], input.worldPosition, bumpedNormalW, input.viewDirection, A, D, S);

		ambient += A;
		diffuse += D;
		specular += S;
	}

	float4 litColor = ambient + diffuse;

	litColor.a = material.Diffuse.a;

	// Multiply the texture pixel and the input color to get the textured result.
	color = litColor * textureColor;

	// Add the specular component last to the output color.
	color = saturate(color + specular);

	// Set the color of the fog to grey.
	fogColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
	//The fog color equation then does a linear interpolation between the texture color and the fog color based on the fog factor.

	// Calculate the final color using the fog effect equation.
	color = input.fogFactor * color + (1.0 - input.fogFactor) * fogColor;
	
	return color;
}