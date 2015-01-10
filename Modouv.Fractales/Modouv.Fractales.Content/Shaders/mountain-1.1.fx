float4x4 xWorldViewProjection;
float4x4 xWorld;
float4 xCameraPos;
float3 xCameraDirection;
float xFogNear;
float xFogFar;
float4 xFogColor = float4(1, 1, 1, 1);
float2 mC1 = float2(-0.821, 0.2345);
float2 mC2 = float2(-0.821, 0.2345);
float MaxAltitude = 50.0f;
// Diffuse
float4 xLightPosition;
float3 xLightDirection;
float4x4 xLightWorldViewProjection;
texture xShadowMapTexture;

texture MountainTexture;
sampler mountain = sampler_state {
   texture = (MountainTexture);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Wrap; // Address Mode for U Coordinates
   AddressV = Wrap; // Address Mode for V Coordinates
};

sampler shadow = sampler_state {
   texture = (xShadowMapTexture);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Wrap; // Address Mode for U Coordinates
   AddressV = Wrap; // Address Mode for V Coordinates
};
struct VertexShaderInput
{
    float4 Position			: POSITION0;		// Passée par notre Vertex
	float4 TextureCoord		: TEXCOORD0;	// Passée par notre Vertex
	float3 Normal			: NORMAL0;		// Passée par notre Vertex
};

struct VertexShaderOutput
{
    float4 OutPosition2D	: POSITION0;		// Position finale de dessin du vertex
	float4 Position2D		: TEXCOORD2;		// Position 2D telle que vue par la caméra.
	float4 Position2DLight  : TEXCOORD5;		// Position 2D telle que vue par la lumière.
	float3 Normal			: TEXCOORD1;		// Normale finale du vertex.
	float4 Position3D		: TEXCOORD3;		// Position en 3D du vertex.
	float3 TextureCoord		: TEXCOORD0;		// Coordonée texture (0 à 1 sur chaque composante)
};

VertexShaderOutput VertexShaderShadowMapFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
	output.OutPosition2D = mul(input.Position, xLightWorldViewProjection);
	output.Position2D = output.OutPosition2D;
    return output;
}

VertexShaderOutput VertexShaderShadowedFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position3D = mul(input.Position, xWorld);
    output.OutPosition2D = mul(input.Position, xWorldViewProjection);
	output.Position2DLight = mul(output.Position3D, xLightWorldViewProjection);
	output.Normal = normalize(input.Normal);
	output.TextureCoord = input.TextureCoord;
    return output;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position3D = mul(input.Position, xWorld);
    output.OutPosition2D = mul(input.Position, xWorldViewProjection);
	output.Normal = normalize(input.Normal);
	output.TextureCoord = input.TextureCoord;
    return output;
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


float DotProduct(float3 lightPos, float3 pos3D, float3 normal)
{
    float3 lightDir = normalize(pos3D - lightPos);
    return dot(lightDir, normal);    
}


/* -----------------------------------------------------------------
 * Version optimisée de la fractale de julia.
 * ---------------------------------------------------------------*/
 float3 Hue(float H)
{
    float R = abs(H * 6 - 3) - 1;
    float G = 2 - abs(H * 6 - 2);
    float B = 2 - abs(H * 6 - 4);
    return saturate(float3(R,G,B));
}
// Un truc au hasard. HYPER CLASSE !!
float3 c_theClasse(float val)
{
	float3 color = float3(sin(val*3), sin(val*4), sin(val*6));
	if(val < 0.5)
		color *= val;
	return color;
}

float3 HSVtoRGB(in float3 HSV)
{
    return ((Hue(HSV.x) - 1) * HSV.y + 1) * HSV.z;
}

float3 c_textured(float val)
{
	return tex2D(mountain, float2(val, val));
}
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
/* -----------------------------------------------------------------
 * Perlin noise implementation
 * ---------------------------------------------------------------*/
float Noise(float2 xy)
{
    float2 noise = (frac(sin(dot(xy ,float2(12.9898,78.233)*2.0)) * 43758.5453));
    return abs(noise.x + noise.y) * 0.5;
}

float SmoothNoise( float integer_x, float integer_y ) {
   float corners = ( Noise( float2(integer_x - 1, integer_y - 1) ) + Noise( float2(integer_x + 1, integer_y + 1 )) + Noise( float2(integer_x + 1, integer_y - 1 )) + Noise( float2(integer_x - 1, integer_y + 1 )) ) / 16.0f;
   float sides = ( Noise( float2(integer_x, integer_y - 1 )) + Noise( float2(integer_x, integer_y + 1 )) + Noise( float2(integer_x + 1, integer_y )) + Noise( float2(integer_x - 1, integer_y )) ) / 8.0f;
   float center = Noise( float2(integer_x, integer_y )) / 4.0f;

   return corners + sides + center;
}

float2 rand_2_0004(float2 uv)
{
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233)      )) * 43758.5453));
    float noiseY = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43718.5453));
    return float2(noiseX, noiseY) * 0.4;
}

const int MAX_DEPTH = 25;
/* -----------------------------------------------------------------
 * Implémentation de la fractale de julia.
 * Range de rendu : 
 * [origin-scale; origin+scale] en X et Y
 * ---------------------------------------------------------------*/
float GetFractalValue(VertexShaderOutput input, float scale=0.5f, float2 origin={0.0, 0.0}, float2 c ={-0.68, -0.44}) : COLOR0
{
	float2 coords = (input.TextureCoord-0.5);
	float2 z;
	float depth = 0;

	// On positionne la condition initiale aux coordonnées indiquées par les options.
	z.x = origin.x + scale * (coords.x);
	z.y = origin.y + scale * (coords.y);

	bool cont = true;
	float zxSqr, zySqr;
	while(cont)
	{
		// On précalcule le carré de chaque composante.
		zxSqr = z.x * z.x;
		zySqr = z.y * z.y; 
		z = float2(zxSqr - zySqr + c.x, 2*z.x*z.y + c.y);
		depth++;

		cont = zxSqr + zySqr <= 4 && depth <= MAX_DEPTH;
	}
	// Après 14000 essais hazardeux une fonction qui pue pas trop pour adoucir les couleurs.
	// Le log c'est LAVI.
	float val = depth / float(MAX_DEPTH);
	//float val = depth / MAX_DEPTH;
	return val;
}  

/* -----------------------------------------------------------------
 * Implémentation d'une fractale Julia de polynôme :
 * 0.000001/X + 0.92*z^3 + 0.42*z^2 + c
 * Range de rendu : 
 * [origin-scale; origin+scale] en X et Y
 * ---------------------------------------------------------------*/
float GetFractalValue2(VertexShaderOutput input, float scale=0.5f, float2 origin={0.0, 0.0}, float2 c ={-0.68, -0.44}) : COLOR0
{
	float2 coords = (input.Normal-0.5);
	float2 z;
	float depth = 0;
	// On va le représenter en coordonnées polaires.
	z.x = origin.x + scale * (coords.x);
	z.y = origin.y + scale * (coords.y);

	bool cont = true;
	float zxSqr, zySqr;
	while(cont)
	{
		// On précalcule le carré de chaque composante.
		zxSqr = z.x * z.x;
		zySqr = z.y * z.y; 

		float2 z2 = cmult(z, z);
		float2 z3 = cmult(z2, z);
			
		z = cinv(z)*0.000001 + z3*0.92 + z2*0.42 + c;
		depth++;

		cont = zxSqr + zySqr <= 4 && depth <= MAX_DEPTH;
	}
	float val = (depth - (log(log(sqrt(zxSqr + zySqr))) / 0.69)) / float(MAX_DEPTH);
	return val;
}
/* -----------------------------------------------------------------
 * Génère une texture de montagne.
 * ---------------------------------------------------------------*/
float4 GetMountain(VertexShaderOutput input, float x, float y, float2 rand, float fractalValue1, float fractalValue2)
{
	float4 colorBase = float4(0.32, 0.32, 0.42, 1);
	float4 color = colorBase;
	float fractalValue = (fractalValue1 + fractalValue2)/2;
	if(fractalValue > 0.8) // neige
		color = float4(0.72, 0.86+abs(sin(rand.x+fractalValue))/25, 0.95, 1);
	else if (fractalValue > 0.62) // effet cool
		color = float4(0.80, 0.80+rand.x/2, 1, 1);
	
	// Si on veut que la neige ne soit que sur des surfaces planes :
	// color = lerp(colorBase, color, saturate(input.Normal.z-0.80)*6);

	return color;
}
/* -----------------------------------------------------------------
 * Génère une texture d'herbe.
 * ---------------------------------------------------------------*/
float4 GetGrass(VertexShaderOutput input, float x, float y, float z, float2 rand, float fractalValue1, float fractalValue2)
{
	float4 color;
	float fractalValue = fractalValue1/3 + + fractalValue2/3 + saturate(1-abs(z))/3;


	//float4 mountainColor = float4(0, 0.6+rand.y*cos(rand.x), 0.09, 1);
	float4 mountainColor = float4(0.80+rand.y/6, 0.80+rand.x/2, 1, 1);
	float4 sandColor = mountainColor*1.2;
	float4 sand2Color = mountainColor*0.6;


	if(fractalValue > 0.7) // sable
		color = sandColor;
	else if (fractalValue > 0.6) // effet cool interpolé
	{
		color = lerp(sand2Color, sandColor, (fractalValue-0.6)/0.10);
	}
	else if(fractalValue > 0.40)
	{
		color = lerp(mountainColor, sand2Color, (fractalValue-0.40)/0.20);
	}
	else
		color =  mountainColor;

	return color;
}

/* -----------------------------------------------------------------
 * Génère une texture 100% fractale.
 * ---------------------------------------------------------------*/
float4 GetColor(VertexShaderOutput input) : COLOR0
{
	const float thresh1 = 0.35;
	const float thresh2 = 0.65; // 0.40
	// Précalcul des valeurs des fractales.
	float fractalValue1 = GetFractalValue(input, 0.5, float2(0, 0), mC1); // float2(-0.847 0.242)
	float fractalValue2 = GetFractalValue2(input, 0.5, float2(0, 0), mC2);//float2(-0.824, 0.231));

	// Coordonnées
	float x = input.TextureCoord.x + input.TextureCoord.z * 0.12;
	float y = input.TextureCoord.y - input.TextureCoord.z * 0.06;
	float z = input.Position3D.z / MaxAltitude;
	float absZ = -z+fractalValue2/2-fractalValue1/3 + sin(input.TextureCoord.x*14+input.TextureCoord.y*6+fractalValue2+fractalValue1/3)/4;
	bool inWater = absZ < -0.1;

	// Noise
	float sx = x*5000;
	float sy = y*5000;
	x = sx - frac(sx);
	y = sy - frac(sy);
	float2 rand = rand_2_0004(float2(x, y));



	// Pour réduire le nombre d'instruction slots :
	float4 mountainColor;
	if(absZ >= thresh1)
		mountainColor = GetMountain(input, x, y, rand, fractalValue1, fractalValue2);

	float4 grassColor;
	if(absZ <= thresh2)
		grassColor = GetGrass(input, x, y, abs(absZ), rand, fractalValue1, fractalValue2);


	// Color
	float4 color;
	if(absZ > thresh2)
	{
		color = mountainColor;
	}
	else if(absZ > thresh1)
	{
		color = lerp(grassColor, mountainColor, (absZ-thresh1)/(thresh2-thresh1));
	}
	else
	{
		color = grassColor;
	}

	if(inWater)
	{
		color = lerp(saturate(color), float4(0, 0, 0, 1), saturate(abs(absZ+0.1)*5));
	}
	color.gb += rand.yx/20.0f;

	return color;
}


/* -----------------------------------------------------------------
 * Effectue le rendu de la shadowmap.
 * ---------------------------------------------------------------*/
float4 LandscapeShadowMapPixelShader(VertexShaderOutput input) : COLOR0
{
	float4 color;
	color.rgb = input.Position2D.z/input.Position2D.w;
	color.a = 1;
	return color;
}
/* -----------------------------------------------------------------
 * Pixel shader
 * Applique les effets d'éclairage et de brouillard ainsi que de texture.
 * ---------------------------------------------------------------*/
float4 LandscapeShadowedPixelShader(VertexShaderOutput input) : COLOR0
{

    float2 ProjectedTexCoords;
    ProjectedTexCoords.x = input.Position2DLight.x/input.Position2DLight.w/2.0f +0.5f;
    ProjectedTexCoords.y = -input.Position2DLight.y/input.Position2DLight.w/2.0f +0.5f;

    float4 color = tex2D(shadow, ProjectedTexCoords);

	return tex2D(shadow, color);
}
/* -----------------------------------------------------------------
 * Pixel shader
 * Applique les effets d'éclairage et de brouillard ainsi que de texture.
 * ---------------------------------------------------------------*/
float4 LandscapePixelShader(VertexShaderOutput input) : COLOR0
{
	
	// Détermine la distance à la caméra et en fait une valeur l entre 0 et 1.
	float dist = distance(xCameraPos, input.Position3D);
	float l = saturate((dist-xFogNear)/(xFogFar-xFogNear));

	// Couleur du pixel de texture.
	float4 texColor = GetColor(input);//GetMountainColor(input); //GetFractalColor(input, l)/4+GetMountainColor(input)*3/4;//GetMountainColor(input);

	// Application de l'éclairage (pixel)
	float factor = DotProduct(xLightPosition, input.Position3D, input.Normal);
	factor *= 0.7;
	texColor = texColor * (factor+0.61);

	// Specular (au dessus du niveau de l'eau uniquement)
	
	float z = -input.Position3D.z / MaxAltitude;

	float3 reflectionVector = reflect(xLightDirection, input.Normal);
	float specular = saturate(dot(normalize(reflectionVector), normalize(xCameraDirection)));
	specular = pow(abs(specular), 16);
	/*if(z > 0.00)
	{
		texColor += specular/4;
	}
	else if (z > -0.1) // degradé
	{
		texColor = lerp(texColor, texColor + specular/4, saturate((z+0.05)/0.05))/2;
	}*/
	
	// Tentative d'atmospheric scatering
	float radius = 120;
	float size = 2000;
	float pointPlanetZ = xCameraPos.z + (cos(input.Position3D.x/size) + sin(input.Position3D.y/size))*radius;
	float cameraPlanetZ = input.Position3D.z + (cos(xCameraPos.x/size) + sin(xCameraPos.y/size))*radius;
	float diff1 = clamp((-saturate((cameraPlanetZ - pointPlanetZ)/radius)+l), 0, 0.85);
	/*
	radius = 80;
	size = 1800;
	pointPlanetZ = xCameraPos.z + (cos(input.Position3D.x/size) + sin(input.Position3D.y/size))*radius;
	cameraPlanetZ = input.Position3D.z + (cos(xCameraPos.x/size) + sin(xCameraPos.y/size))*radius;
	float diff2 = clamp((-saturate((cameraPlanetZ - pointPlanetZ)/radius)+l), 0, 0.85);

	radius = 150;
	size = 2100;	
	pointPlanetZ = xCameraPos.z + (cos(input.Position3D.x/size) + sin(input.Position3D.y/size))*radius;
	cameraPlanetZ = input.Position3D.z + (cos(xCameraPos.x/size) + sin(xCameraPos.y/size))*radius;
	float diff3 = clamp((-saturate((cameraPlanetZ - pointPlanetZ)/radius)+l), 0, 0.85);*/

	float4 diff = float4(diff1*0.87, diff1*0.92, diff1*1.12, 1);
	//return float4(diff.x, diff.y, diff.z, 1);
	// Brouillard
	texColor.rgb = lerp(texColor, texColor+diff, saturate(l-0.8)*5).rgb;
	//texColor.rgb = lerp(texColor, xFogColor, diff);//).rgb;
	texColor.a = lerp(1, 0, saturate(l-0.8)*5);
	return texColor;
}
/* -----------------------------------------------------------------
 * Pixel shader pour le dessin de la réflection de l'eau.
 * Supprime les pixels en dessous du niveau de l'eau.
 * ---------------------------------------------------------------*/
float4 LandscapeReflectionPixelShader(VertexShaderOutput input) : COLOR0
{
	// Si il est en dessous de la position de l'eau, on coupe le pixel.
	if(input.Position3D.z > 0.05)
		clip(-1);

	return LandscapePixelShader(input);
}

// Technique de dessin permettant le rendu visual "normal" de l'objet.
technique Landscape
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 LandscapePixelShader();
    }
}

// Technique de dessin permettant le rendu visuel de la ReflectionMap
// utilisée pour le dessin de l'eau.
technique Reflection
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 LandscapeReflectionPixelShader();
    }
}
// Technique de dessin permettant de générer la shadow map.
technique ShadowMap
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderShadowMapFunction();
		PixelShader = compile ps_3_0 LandscapeShadowMapPixelShader();
	}
}
// Technique de dessin permettant de dessiner en utilisant la shadow map.
technique Shadowed
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 VertexShaderShadowedFunction();//VertexShaderFunction();//VertexShaderShadowedFunction();
		PixelShader = compile ps_3_0 LandscapeShadowedPixelShader();//LandscapePixelShader();//LandscapeShadowedPixelShader();
	}
}
