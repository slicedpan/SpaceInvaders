texture tex;
sampler texSampler = sampler_state
{
	Texture = (tex);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VSinfo
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};


VSinfo VertexShaderFunction(VSinfo input)
{
	return input;
}

float4 PixelShaderFunction(VSinfo input) : COLOR0
{
    return tex2D(texSampler, input.TexCoord);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
