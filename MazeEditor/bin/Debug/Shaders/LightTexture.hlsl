//Frank Luna 
#define NUM_DIRECTIONAL_LIGHTS 1
#define NUM_POINT_LIGHTS 2
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
void ComputeDirectionalLight(Material mat, DirectionalLight L,
	float3 normal, float3 toEye,
	out float4 ambient,
	out float4 diffuse,
	out float4 spec)
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
void ComputePointLight(Material mat, PointLight L, float3 pos, float3 normal, float3 toEye,
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
		float specFactor = pow(max(dot(v, toEye), 0.0f), mat.Specular.w);

		diffuse = diffuseFactor * mat.Diffuse * L.Diffuse;
		spec = specFactor * mat.Specular * L.Specular;
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
void ComputeSpotLight(Material mat, SpotLight L, float3 pos, float3 normal, float3 toEye,
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
		float specFactor = pow(max(dot(v, toEye), 0.0f), mat.Specular.w);

		diffuse = diffuseFactor * mat.Diffuse * L.Diffuse;
		spec = specFactor * mat.Specular * L.Specular;
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
	float3 bumpedNormalW = mul( normalT, TBN); return bumpedNormalW; 
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
	float2 pad;
};

Texture2D shaderTexture;
Texture2D normalMap;

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

struct PixelInputType
{
	float4 position : SV_POSITION;
	float4 worldPosition :  POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float3 viewDirection : TEXCOORD1;
	float fogFactor : FOG;
};

////////////////////////////////////////////////////////////////////////////////
// Vertex Shader
////////////////////////////////////////////////////////////////////////////////
PixelInputType LightVertexShader(VertexInputType input)
{
	float4 worldPos;
	PixelInputType output;

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
		//cameraPosition.xyz - output.worldPosition.xyz;

	// Normalize the viewing direction vector.
	output.viewDirection = normalize(output.viewDirection);

	// Calculate linear fog.    
	worldPos = mul(worldPos, viewMatrix);
	output.fogFactor = saturate((fogEnd - worldPos.z) / (fogEnd - fogStart));

	return output;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 LightPixelShader(PixelInputType input) : SV_TARGET
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
		//gPointLight.Attentuation = float3(0.0f, 100.0f, 100.0f);
		ComputePointLight(material, gPointLight[i], input.worldPosition, bumpedNormalW, input.viewDirection, A, D, S);

		ambient += A;
		diffuse += D;
		specular += S;
	}

	for (uint i = 0; i < NUM_SPOT_LIGHTS; i++)
	{
		ComputeSpotLight(material, gSpotLight[i], input.worldPosition, bumpedNormalW, input.viewDirection, A, D, S);

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
	fogColor = float4(0.2f, 0.2f, 0.2f, 0.5f);
	//The fog color equation then does a linear interpolation between the texture color and the fog color based on the fog factor.

	// Calculate the final color using the fog effect equation.
	color = input.fogFactor * color + (1.0 - input.fogFactor) * fogColor;

	return color;
}