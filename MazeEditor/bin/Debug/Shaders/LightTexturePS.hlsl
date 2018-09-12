#include "Utilities.hlsl"


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