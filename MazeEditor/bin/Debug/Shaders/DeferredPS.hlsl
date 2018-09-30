#include "Utilities.hlsl"

Texture2D shaderTexture;
Texture2D normalMap;
Texture2D specularMap;

PS_GBUFFER_OUT PackGBuffer(float3 baseColor, float3 normal, float specIntensity, float specPower, float fogFactor)
{
	PS_GBUFFER_OUT Out;
	float specPowerNorm = (specPower - g_SpecPowerRange.x) / g_SpecPowerRange.y;
	
	Out.ColorSpecInt = float4(baseColor, specIntensity);
	Out.Normal = float4(normal * 0.5 + 0.5, 0.0);
	Out.SpecPow = float4(specPowerNorm, fogFactor, 0.0, 0.0);

	return Out;
}


////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
PS_GBUFFER_OUT LightPixelShader(DomainOut input)
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

	float4 specIntensity = specularMap.Sample(samLinear, input.tex);

	return PackGBuffer(textureColor, bumpedNormalW, specIntensity.x, material.Specular, input.fogFactor);
}