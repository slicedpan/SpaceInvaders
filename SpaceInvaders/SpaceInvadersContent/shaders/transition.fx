float t;

texture initialTexture;
sampler initialSampler = sampler_state
{
	Texture = (initialTexture);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture finalTexture;
sampler finalSampler = sampler_state
{
	Texture = (finalTexture);
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
    float3 initColor = tex2D(initialSampler, input.TexCoord);
	float3 finalColor = tex2D(finalSampler, input.TexCoord);
	float radialDist = input.TexCoord.x - 0.5f;
	float3 sinusoidalCombination = sin(radialDist * 30.0f + t * 24) * initColor + cos(radialDist * 30.0f + t * 24) * finalColor;
	float3 total = (1 - 2 * t + t * t) * initColor + 2 * (t - t * t) * sinusoidalCombination + (t * t) * finalColor;
	return float4(total, 1.0f);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
