float4x4 World;
float4x4 View;
float4x4 Projection;

float distors = 0.5;
float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.2;

// Diffuse
float3 DiffuseDirection = float3(0.75, 0.45, 0.75);
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseIntensity = 0.8;

float3 ParticlePosition;
float Range = 500;
texture TexParam;
sampler tex = sampler_state {
   texture = (TexParam);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Mirror; // Address Mode for U Coordinates
   AddressV = Mirror; // Address Mode for V Coordinates
};


struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float3 Outpos : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, float3 Normal : NORMAL)
{
    VertexShaderOutput output;
	float4 pos = input.Position;
	// Calcule la position.
	float sx = (pos.x-ParticlePosition.x);
	float sy = (pos.y-ParticlePosition.y);
	float zOffset = (Range-sqrt(sx*sx + sy*sy));
	if(zOffset > 0)
		pos.z += zOffset/Range*ParticlePosition.z;
	
    float4 worldPosition = mul(pos, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	float3 normal = normalize(mul(Normal, World));
	if(abs(pos.z) <= 0.001)
	{
		pos = float4(0, 0, 0, 0);
	}
    output.Outpos = normalize(pos);
	output.Normal = normal;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	if(input.Outpos.x == 0)
		return float4(0, 0, 0, 0);

	float4 texColor = tex2D(tex, float2(abs(input.Outpos.z), abs(input.Outpos.y)));
    float4 norm = float4(normalize(input.Normal + texColor), 1.0);
    float4 diffuse = saturate(dot(-DiffuseDirection,norm))/2;
	float4 diffuse2 = saturate(dot(DiffuseDirection, norm))/2;
	return AmbientColor*AmbientIntensity+DiffuseIntensity*DiffuseColor*(diffuse+diffuse2)+0.4*texColor;
}

technique Ambient
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}