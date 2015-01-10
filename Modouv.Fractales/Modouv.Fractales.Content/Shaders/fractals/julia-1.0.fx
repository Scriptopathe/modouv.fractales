sampler s0;
float frame;
float4x4 MatrixTransform;
const int MAX_DEPTH = 50;

int width;
int height;
float2 origin;
float scale;
float2 c;

void SpriteVertexShader(inout float4 color    : COLOR0,
                        inout float2 texCoord : TEXCOORD0,
                        inout float4 position : SV_Position)
{
    position = mul(position, MatrixTransform);
}
/* -----------------------------------------------------------------
 * Complex
 * ---------------------------------------------------------------*/
float2 complexadd(float2 a, float2 b)
{
	return float2(a.x+b.x, a.y+b.y);
}
float2 complexmult(float2 a, float2 b)
{
	return float2(a.x*b.x - a.y*b.y, a.x*b.y + a.y*b.x);
}

float complexmodule(float2 complex)
{
	return pow(complex.x, 2) + pow(complex.y, 2);
}

float3 Hue(float H)
{
    float R = abs(H * 6 - 3) - 1;
    float G = 2 - abs(H * 6 - 2);
    float B = 2 - abs(H * 6 - 4);
    return saturate(float3(R,G,B));
}

float3 HSVtoRGB(in float3 HSV)
{
    return ((Hue(HSV.x) - 1) * HSV.y + 1) * HSV.z;
}

/* -----------------------------------------------------------------
 * Pixel shader
 * ---------------------------------------------------------------*/
float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float2 z;
	float depth = 0; // sur une CG un float + rapide qu'un int...
	z.x = scale * (coords.x + (origin.x / width));
	z.y = scale * (coords.y + (origin.y / height));

	bool cont = true;
	while(cont)
	{
		z = complexadd(complexmult(z, z), c);
		depth++;
		cont = complexmodule(z) <= 4 && depth <= MAX_DEPTH;
	}

	float val = depth/MAX_DEPTH;// / (scale);
	float4 color;
	color.rgb = HSVtoRGB(float3(val/4, 1, val*2));
	color.a = 1;
	return color;
}  



technique Technique1  
{  
    pass Pass1  
    {  
		VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction();  
    }
}  