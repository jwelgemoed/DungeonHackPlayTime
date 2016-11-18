//Frank Luna 
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
	ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
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


/*cbuffer MatrixBuffer : register(cb0)
{
	matrix worldMatrix;
	matrix viewMatrix;
	matrix projectionMatrix;
};

cbuffer LightBuffer : register(cb1)
{
	float4 ambientColor;
	float4 diffuseColor;
	float3 lightDirection;
	float specularPower;
	float4 specularColor;
}

cbuffer CameraBuffer : register(cb2)
{
	float3 cameraPosition;
	float padding;
}*/

cbuffer cbPerFrame
{
	DirectionalLight gDirLight;
	PointLight gPointLight;
	SpotLight gSpotLight;
	float3 cameraPosition;
};

cbuffer cbPerObject
{
	matrix worldMatrix;
	matrix viewMatrix;
	matrix projectionMatrix;
	Material material;
};

Texture2D shaderTexture;
SamplerState SampleType;

struct VertexInputType
{
	float4 position : POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
};

struct PixelInputType
{
	float4 position : SV_POSITION;
	float4 worldPosition :  POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float3 viewDirection : TEXCOORD1;
};

////////////////////////////////////////////////////////////////////////////////
// Vertex Shader
////////////////////////////////////////////////////////////////////////////////
PixelInputType LightVertexShader(VertexInputType input)
{
	PixelInputType output;

	// Change the position vector to be 4 units for proper matrix calculations.
	input.position.w = 1.0f;

	// Calculate the position of the vertex against the world, view, and projection matrices.
	output.position = mul(input.position, worldMatrix);
	output.position = mul(output.position, viewMatrix);
	output.position = mul(output.position, projectionMatrix);

	// Store the input color for the pixel shader to use.
	output.tex = input.tex;

	output.normal = mul(input.normal, (float3x3) worldMatrix);
	output.normal = normalize(output.normal);

	// Calculate the position of the vertex in the world.
	output.worldPosition = mul(input.position, worldMatrix);

	// Determine the viewing direction based on the position of the camera and the position of the vertex in the world.
	output.viewDirection = cameraPosition.xyz - output.worldPosition.xyz;

	// Normalize the viewing direction vector.
	output.viewDirection = normalize(output.viewDirection);

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

	input.normal = normalize(input.normal);

	// Sample the pixel color from the texture using the sampler at this texture coordinate location.
	textureColor = shaderTexture.Sample(SampleType, input.tex);

	float4 ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 specular = float4(0.0f, 0.0f, 0.0f, 0.0f);

	float4 A, D, S;

	ComputeDirectionalLight(material, gDirLight, input.normal, input.viewDirection, A, D, S);
	
	ambient += A;
	diffuse += D;
	specular += S;

	/*ComputePointLight(material, gPointLight, input.position, input.normal, toEyeW, A, D, S);

	ambient += A;
	diffuse += D;
	specular += S;*/

	ComputeSpotLight(material, gSpotLight, input.worldPosition, input.normal, input.viewDirection, A, D, S); 

	ambient += A;
	diffuse += D;
	specular += S;

	float4 litColor = ambient + diffuse;

	litColor.a = material.Diffuse.a;

	// Multiply the texture pixel and the input color to get the textured result.
	color = litColor * textureColor;

	// Add the specular component last to the output color.
	color = saturate(color + specular);

	return color;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 LightPixelShaderNoSpotLight(PixelInputType input) : SV_TARGET
{
	float4 textureColor;
float3 lightDir;
float lightIntensity;
float4 color;
float3 reflection;

input.normal = normalize(input.normal);

// Sample the pixel color from the texture using the sampler at this texture coordinate location.
textureColor = shaderTexture.Sample(SampleType, input.tex);

float4 ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
float4 diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
float4 specular = float4(0.0f, 0.0f, 0.0f, 0.0f);

float4 A, D, S;

ComputeDirectionalLight(material, gDirLight, input.normal, input.viewDirection, A, D, S);

ambient += A;
diffuse += D;
specular += S;

/*ComputePointLight(material, gPointLight, input.position, input.normal, toEyeW, A, D, S);

ambient += A;
diffuse += D;
specular += S;*/

float4 litColor = ambient + diffuse;

litColor.a = material.Diffuse.a;

// Multiply the texture pixel and the input color to get the textured result.
color = litColor * textureColor;

// Add the specular component last to the output color.
color = saturate(color + specular);

return color;
}

////////////////////////////////////////////////////////////////////////////////
// Vertex Shader
////////////////////////////////////////////////////////////////////////////////
PixelInputType TextureVertexShader(VertexInputType input)
{
	PixelInputType output;

	// Change the position vector to be 4 units for proper matrix calculations.
	input.position.w = 1.0f;

	// Calculate the position of the vertex against the world, view, and projection matrices.
	output.position = mul(input.position, worldMatrix);
	output.position = mul(output.position, viewMatrix);
	output.position = mul(output.position, projectionMatrix);

	// Store the input color for the pixel shader to use.
	output.tex = input.tex;

	return output;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 TexturePixelShader(PixelInputType input) : SV_TARGET
{
	float4 textureColor;
// Sample the pixel color from the texture using the sampler at this texture coordinate location.
textureColor = shaderTexture.Sample(SampleType, input.tex);

return textureColor;
}

technique11 LightTech
{
	pass P0
	{
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_5_0, LightVertexShader()));
		SetPixelShader(CompileShader(ps_5_0, LightPixelShader()));
	}
}

technique11 NoSpotSpotLightTech
{
	pass P0
	{
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_5_0, LightVertexShader()));
		SetPixelShader(CompileShader(ps_5_0, LightPixelShaderNoSpotLight()));
	}
}

technique11 TextureTech
{
	pass P0
	{
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_5_0, TextureVertexShader()));
		SetPixelShader(CompileShader(ps_5_0, TexturePixelShader()));
	}
}