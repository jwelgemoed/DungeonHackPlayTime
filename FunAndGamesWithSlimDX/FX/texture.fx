cbuffer MatrixBuffer
{
	float4x4 gWorldViewProj;
};

Texture2D shaderTexture;
SamplerState SampleType;

struct VertexInputType
{
    float4 position : POSITION;
    float2 tex : TEXCOORD0;
};

struct PixelInputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
};

////////////////////////////////////////////////////////////////////////////////
// Vertex Shader
////////////////////////////////////////////////////////////////////////////////
PixelInputType TextureHightVertexShader(VertexInputType input)
{
    PixelInputType output;
		float2 height = shaderTexture.SampleLevel(SampleType, input.tex, 0).rg;
			input.position.y = (height.x * height.y) * 100;
    // Change the position vector to be 4 units for proper matrix calculations.
    //input.position.w = 1.0f;

    // Calculate the position of the vertex against the world, view, and projection matrices.
	  output.position = mul(input.position, gWorldViewProj);

		// Store the input color for the pixel shader to use.
    output.tex = input.tex;
    
    return output;
};

PixelInputType TextureVertexShader(VertexInputType input)
{
	PixelInputType output;
	
	output.position = mul(input.position, gWorldViewProj);
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

technique11 ColorTech
{
    pass P0
    {
		SetGeometryShader( NULL );
        SetVertexShader( CompileShader( vs_5_0, TextureVertexShader() ) );
        SetPixelShader( CompileShader( ps_5_0, TexturePixelShader() ) );
    }
}