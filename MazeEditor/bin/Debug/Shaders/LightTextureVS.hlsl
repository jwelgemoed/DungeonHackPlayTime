#include "Utilities.hlsl"

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

