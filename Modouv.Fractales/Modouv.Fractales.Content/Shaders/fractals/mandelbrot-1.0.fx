sampler s0;
float frame;
float4x4 MatrixTransform;
const int MAX_DEPTH = 100;

int width;
int height;
float2 origin;
float scale;
float2 colorParam;

void SpriteVertexShader(inout float4 color    : COLOR0,
                        inout float2 texCoord : TEXCOORD0,
                        inout float4 position : SV_Position)
{
    position = mul(position, MatrixTransform);
}
/* -----------------------------------------------------------------
 * Complex
 * ---------------------------------------------------------------*/
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
	float2 z = float2(0, 0);
	float depth = 0;
	float2 c = float2(scale * (coords.x + (origin.x / width)), 
					  scale * (coords.y + (origin.y / height)));

	bool cont = true;
	while(cont)
	{
		//z = complexmult(z, z) + c;
		float2 zsqr = complexmult(z, z);
		z = 0.11*complexmult(z, zsqr) + 
			0.80 * zsqr + 
			-0.22*z +
			c;
		depth++;
		cont = complexmodule(z) <= 4 && depth <= MAX_DEPTH;
	}

	// Permet l'obtention d'un nombre compris entre 0 et 1.
	float val = depth/MAX_DEPTH;

	// Crée la couleur correspondant à la valeur de profondeur de la fractale.
	float4 color;
	if(val >= 0.9999) // les centres sont noirs
	{
		color.rgb = float3(0, 0, 0);
	}
	else if(val > 0.5) // quand on s'éloigne un peu, on a du gris
	{
		color.rgb = val;
	}
	else if (val > colorParam.y) // si on s'éloigne +, on a du rouge.
	{
		color.rgb = HSVtoRGB(float3(val/4, 1, 1));
	}
	else if(val < colorParam.x) // si on est très très loin, on a ça
	{
		color.rgb = HSVtoRGB(float3(0.4+val*2, 1, val*2));
	}
	else // entre [colorParam.x, colorParam.y] on a plein de zolies couleurs.
	{
		color.rgb = HSVtoRGB(float3(abs(cos(val)), 1, val*2));
	}
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