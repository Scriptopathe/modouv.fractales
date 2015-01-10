float4x4 World;
float4x4 View;
float4x4 Projection;

float distors = 0.5;
float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.2;

// Diffuse
float3 DiffuseDirection = float3(0, 1, 0);
float4 DiffuseColor = float4(1, 0, 0, 1);
float DiffuseIntensity = 0.7;

sampler tex : register(s10);

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, float3 Normal : NORMAL)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	float3 normal = normalize(mul(Normal, World));
    output.Normal = normal;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 norm = float4(input.Normal, 1.0);
    float4 diffuse = saturate(dot(-DiffuseDirection,norm));
	float4 additional = float4(cos(input.Normal.y)*0.6, input.Normal.x*0.4, 0, 1);
	float4 rand = float4(input.Normal.z*input.Normal.x-sin(distors), input.Normal.x*(input.Normal.z+cos(distors/5)), input.Normal.y, 1);
    
	return tex2D(tex, float2(input.Normal.x, input.Normal.y));

	//return tex2D(tex, float2(input.Normal.x, input.Normal.y));
	//return AmbientColor*AmbientIntensity+DiffuseIntensity*DiffuseColor*diffuse+additional*0.2+tex2D(tex, float2(input.Normal.x, input.Normal.y))*0.4 + rand*0.3;
	//return AmbientColor*0.1+0.1*DiffuseColor*diffuse+additional*0.1+tex2D(tex, float2(input.Normal.x, input.Normal.y))*0.8 + rand*0.1;
	//return float4(1, 0, 0, 1);
}

technique Ambient
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}