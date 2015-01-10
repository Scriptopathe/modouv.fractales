sampler s0;
float frame;
float4x4 MatrixTransform;
const int MAX_DEPTH = 80;

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
float2 cmult(float2 a, float2 b)
{
	return float2(a.x*b.x - a.y*b.y, a.x*b.y + a.y*b.x);
}

float cmodule(float2 complex)
{
	return pow(complex.x, 2) + pow(complex.y, 2);
}
 
float cinv(float2 complex)
{
	float denom = cmodule(complex);
	return float2(complex.x/denom, -complex.y/denom);
}
float cdiv(float2 a, float2 b)
{
	return cmult(a, cinv(b));
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
 * Color models
 * ---------------------------------------------------------------*/
 // Un bleu super classe (bleu / noir)
 float3 c_blue(float val)
 {
	return float3(val*3, val*4, val*6);
 }
 // Couleurs paradis. (bleu / blanc)
 float3 c_heaven(float val)
 {
	return float3(sin(0.4+val*3), sin(exp(val)), cos(val/10));
 }
 // Un rouge flamme
 float3 c_redhot(float val)
 {
	return HSVtoRGB(float3(val/4, 1, val*2));
 }
 // Un truc au hasard. HYPER CLASSE !!
 float3 c_theClasse(float val)
 {
	return float3(sin(val*3), sin(val*4), sin(val*6));
 }
 // Un truc au hasard.
 float3 c_randomClasseSansPlus(float val)
 {
	return float3(tan(val*3), sin(val*val), sin(val*6));
 }
 // Un truc qui inspire les couleurs de la jamaique...
 float3 c_jamaique(float val)
 {
	return float3(sin(val*3), sin(exp(val)), sin(log(val)));
 }
 // Orange/vert
 float3 c_random(float val)
 {
	if(val > 0.9)
		return float3(0, 0, 0);
	else
		return float3(sin(val*6.28), sin(val*3.14)*0.7, sin(val/4));
 }
 
 // Calcule la valeur de la fractale aux coordonnées données.
 float ComputeFractalValue(float2 coords)
 {	
	// Profondeur 
	float iterations = 0;

	// Application d'une échelle et de l'origine aux coordonnées des points
	// à calculer.
	float2 z;
	z.x = scale * (coords.x + (origin.x / width));
	z.y = scale * (coords.y + (origin.y / height));

	// Variable valant true tant qu'il reste des itérations
	// à effectuer.
	bool cont = true;
	float zxSqr, zySqr;
	while(cont)
	{
		// On précalcule le carré de chaque composante.
		zxSqr = z.x * z.x;
		zySqr = z.y * z.y; 

		// On calcule le z de la prochaine itération.
		// z = z² + c
		z = float2(zxSqr - zySqr + c.x, 2*z.x*z.y + c.y);


		iterations++;

		// On s'arrête si le carré du module est supérieur à 4
		// ou si le nombre d'itérations dépasse le nombre maximum d'itérations.
		cont = zxSqr + zySqr <= 4 && iterations <= MAX_DEPTH;
	}
	// Permet de lisser les valeurs et de les mettre à une échelle comprise entre 0 et 1.
	float val = (iterations - (log(0.5*log(zxSqr + zySqr)) / 0.69)) / float(MAX_DEPTH);
	return val;
 }
/* -----------------------------------------------------------------
 * Pixel shader
 * Fractale de julia colorée.
 * ---------------------------------------------------------------*/
float4 ColoredJulia(float2 coords: TEXCOORD0) : COLOR0
{
	
	float val = ComputeFractalValue(coords);
	float4 color;
	color.rgb = c_theClasse(val);
	color.a = 1;
	return color;
}
/* -----------------------------------------------------------------
 * Pixel shader
 * Fractale de julia sous forme de heightmap.
 * ---------------------------------------------------------------*/
float4 HeightmapJulia(float2 coords: TEXCOORD0) : COLOR0
{

	float val = ComputeFractalValue(coords);
	float4 color;
	color.rgb = val;
	color.a = 1;
	return color;
}  


technique Colored  
{  
    pass Pass1  
    {  
		VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 ColoredJulia();  
    }
}
technique Heightmap  
{  
    pass Pass1  
    {  
		VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 HeightmapJulia();  
    }
}  