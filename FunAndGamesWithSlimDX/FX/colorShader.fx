﻿cbuffer MatrixBuffer
{
	float4x4 gWorldViewProj;
};

struct VertexInputType
{
	float4 position : POSITION;
	float4 color : COLOR;
};

struct PixelInputType
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

////////////////////////////////////////////////////////////////////////////////
// Vertex Shader
////////////////////////////////////////////////////////////////////////////////
PixelInputType ColorVertexShader(VertexInputType input)
{
	PixelInputType output;

	// Change the position vector to be 4 units for proper matrix calculations.
	input.position.w = 1.0f;

	// Calculate the position of the vertex against the world, view, and projection matrices.
	output.position = mul(input.position, gWorldViewProj);

	// Store the input color for the pixel shader to use.
	output.color = input.color;

	return output;
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 ColorPixelShader(PixelInputType input) : SV_TARGET
{
	return input.color;
}

technique11 ColorTech
{
	pass P0
	{
		SetGeometryShader(NULL);
		SetVertexShader(CompileShader(vs_5_0, ColorVertexShader()));
		SetPixelShader(CompileShader(ps_5_0, ColorPixelShader()));
	}
}