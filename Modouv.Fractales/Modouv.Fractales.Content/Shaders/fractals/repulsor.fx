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
 
float complexinv(float2 complex)
{
	float denom = complexmodule(complex);
	return float2(complex.x/denom, -complex.y/denom);
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
 * Color maps
 * ---------------------------------------------------------------*/
// Couleurs fleuresques avec teintes de feu.
float3 c_fireflower(float val)
{
	float threshold = 0.05;
	if(val > 0.9)
	{
		return float3(0, 0, 0);
	}
	else if(val < threshold)
	{
		return HSVtoRGB(float3(0.4-val, 1, 0.4-val*2));
	}
	else
	{
		return HSVtoRGB(float3(0.0+val/4, 1, (val-threshold)*5));
	}
}
// zoli
float3 c_euhjesaispasmaiscestjoli(float val)
{
	if(val >= 0.9999) // les centres sont noirs
	{
		return float3(0, 0, 0);
	}
	else if(val > 0.5) // quand on s'éloigne un peu, on a du gris
	{
		return val;
	}
	else if (val > colorParam.y) // si on s'éloigne +, on a du rouge.
	{
		return HSVtoRGB(float3(val/4, 1, 1));
	}
	else if(val < colorParam.x) // si on est très très loin, on a ça
	{
		return HSVtoRGB(float3(0.4+val*2, 1, val*2));
	}
	else // entre [colorParam.x, colorParam.y] on a plein de zolies couleurs.
	{
		return HSVtoRGB(float3(abs(cos(val)), 1, val*2));
	}
}
// Couleurs fleuresques.
float3 c_flower(float val)
{
	if(val > 0.9)
	{
		return float3(0, 0, 0);
	}
	else
	{
		return HSVtoRGB(float3(0.4-val, 1, 0.4-val*2));
	}
}


/* -----------------------------------------------------------------
 * Pixel shader
 * ---------------------------------------------------------------*/
float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float2 c = colorParam;
	float depth = 0;
	float2 z = float2(scale * (coords.x + (origin.x / width)), 
					  scale * (coords.y + (origin.y / height)));

	bool cont = true;
	while(cont)
	{
		float2 zsqr = complexmult(z, z);
		float2 zp4 = complexmult(zsqr, zsqr);
		
		// Quadruple attracteur + quadruple inverseur.
		z = zp4 + 0.4*complexinv(zp4*50) + c;

		// Triple attracteur trop classe.
		// z = zp4 + sin(zsqr/10) - 0.4*complexinv(zp4*45) + complexmult(c, z);
		
		// Ceci envoie du pathé :
		// float2 zp3 = complexmult(z, zsqr);
		// zp4 + 0.2*zp3 + 0.4*zsqr - complexinv(100*zsqr) + 0.4*complexinv(10*z) - 0.1*complexinv(50*zp3) + c;

		depth++;
		cont = complexmodule(z) <= 4 && depth <= MAX_DEPTH;
	}

	// Permet l'obtention d'un nombre compris entre 0 et 1.
	// float val = depth/MAX_DEPTH;

	// Permet l'obtention d'un nombre compris entre 0 et 1 mais lissé.
	float val = (depth - (log(log(sqrt(z.x*z.x + z.y*z.y))) / 0.69)) / float(MAX_DEPTH);

	// Crée la couleur correspondant à la valeur de profondeur de la fractale.
	float4 color;
	color.rgb = c_euhjesaispasmaiscestjoli(val);
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