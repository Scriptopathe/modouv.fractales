#include "common.fx"
float2 mC1 = float2(-0.821, 0.2345);
float2 mC2 = float2(-0.821, 0.2345);
float MaxAltitude = 50.0f;
float ClipPlaneNear = 0;
float ClipPlaneFar = 0;
// Facteur multiplicatif de la taille des textures
float TextureFactor = 25; // 75
float lod = -2;

texture MountainTexture;
sampler mountain = sampler_state {
   texture = (MountainTexture);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Clamp; // Address Mode for U Coordinates
   AddressV = Clamp; // Address Mode for V Coordinates
};





/* -----------------------------------------------------------------
 * Perlin noise implementation
 * ---------------------------------------------------------------*/
float2 rand_2_0004(float2 uv)
{
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233)      )) * 43758.5453));
    float noiseY = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43718.5453));
    return float2(noiseX, noiseY) * 0.4;
}

const int MAX_DEPTH = 28;

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	return ComputeVertexShaderInput(input);
}

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
	float val = depth / float(MAX_DEPTH);
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
	return depth /float(MAX_DEPTH);
}
/* -----------------------------------------------------------------
 * Génère une texture de montagne.
 * ---------------------------------------------------------------*/
float4 GetMountain(VertexShaderOutput input, float x, float y, float2 rand, float fractalValue1, float fractalValue2)
{
	float4 colorBase = float4(0.32, 0.32, 0.34, 1);
	float4 color = colorBase;
	float fractalValue = (fractalValue1 + fractalValue2)/2;
	if(fractalValue > 0.8) // neige
		color = float4(0.86, 0.86+abs(sin(0.32+fractalValue))/25, 0.95, 1);
	else if (fractalValue > 0.62) // effet cool
		color = float4(0.80, 0.89, 1, 1);
	
	// Si on veut que la neige ne soit que sur des surfaces planes :
	//color = lerp(colorBase, color, saturate(input.Normal.z-0.80)*6);

	return color;
}
/* -----------------------------------------------------------------
 * Génère une texture d'herbe.
 * ---------------------------------------------------------------*/
float4 GetGrass(VertexShaderOutput input, float x, float y, float z, float2 rand, float fractalValue1, float fractalValue2)
{
	float4 color;
	float fractalValue = (fractalValue1 + + fractalValue2 + saturate(1-abs(z)))/3;
	fractalValue = (fractalValue % 0.1)*10;

	//float4 mountainColor = float4(0, 0.6+rand.y*cos(rand.x), 0.09, 1);
	float green = 0.12+rand.y/6;
	float4 mountainColor = float4(0.12, green, 0.07, 1);
	float4 sandColor = mountainColor*(0.7+fractalValue2);//float4(0.86+rand.y*rand.x, 0.54+fractalValue/10, 0.12, 1);
	float4 sandColor2 = mountainColor*(0.9+fractalValue*2);
	
	if(fractalValue > 0.7) // sable
		color = sandColor;
	else if (fractalValue > 0.6) // effet cool interpolé
	{
		color = lerp(sandColor2, sandColor, (fractalValue-0.6)/0.10);
	}
	else if(fractalValue > 0.40)
	{
		color = lerp(mountainColor, sandColor2, (fractalValue-0.40)/0.20);
	}
	else
		color =  mountainColor;

	return color;
}
/* -----------------------------------------------------------------
 * Génère une texture de neige.
 * ---------------------------------------------------------------*/
float4 GetSnow(VertexShaderOutput input, float x, float y, float z, float2 rand, float fractalValue1, float fractalValue2)
{
	float4 color;
	float fractalValue = fractalValue1/3 + + fractalValue2/3 + saturate(1-abs(z))/3;


	//float4 mountainColor = float4(0, 0.6+rand.y*cos(rand.x), 0.09, 1);
	float4 mountainColor = float4(0.95, 0.95, 1, 1);
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
float4 GetColorIce(VertexShaderOutput input) : COLOR0
{
	const float thresh1 = 0.30;
	const float thresh2 = 0.45; // 0.40
	// Précalcul des valeurs des fractales.
	float fractalValue1 = GetFractalValue(input, 0.5, float2(0, 0), mC1); // float2(-0.847 0.242)
	float fractalValue2 = GetFractalValue2(input, 0.5, float2(0, 0), mC2);//float2(-0.824, 0.231));

	// Coordonnées
	float x = input.TextureCoord.x;
	float y = input.TextureCoord.y;
	float z = input.Position3D.z / MaxAltitude;
	float realZ = -z;
	float absZ = realZ-fractalValue2/6+fractalValue1/6 + sin(input.TextureCoord.x*14+input.TextureCoord.y*6+fractalValue2+fractalValue1/3)/4;//realZ+fractalValue2/4-fractalValue1/3 + sin(input.TextureCoord.x*14+input.TextureCoord.y*6+fractalValue2+fractalValue1/3)/4;
	bool inWater = realZ < -0.05;

	// Noise
	float sx = x*10000;
	float sy = y*10000;
	sx = sx - frac(sx);
	sy = sy - frac(sy);
	float2 rand = rand_2_0004(float2(sx, sy));



	float4 mountainColor;
	if(absZ >= thresh1)
		mountainColor = GetMountain(input, x, y, 0, fractalValue1, fractalValue2);

	float4 grassColor = mountainColor;
	if(absZ <= thresh2)
		grassColor = GetGrass(input, x, y, abs(absZ), 0, fractalValue1, fractalValue2); // 


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
		color = lerp(saturate(color), float4(0, 0, 0, 1), saturate(abs(realZ+0.05)*5));
	}
	color.rgb += rand.y/8.0f;

	return color;
}

/* -----------------------------------------------------------------
 * Génère une texture d'herbe.
 * ---------------------------------------------------------------*/
float4 GetGrass2(VertexShaderOutput input, float x, float y, float z, float2 rand, float fractalValue1, float fractalValue2)
{
	float4 color;
	float fractalValue = (fractalValue1 + + fractalValue2 + saturate(1-abs(z)))/3;
	fractalValue = (fractalValue % 0.1)*10;

	//float4 mountainColor = float4(0, 0.6+rand.y*cos(rand.x), 0.09, 1);
	float green = 0.12+rand.y/6;
	float4 mountainColor = float4(0.12, green, 0.07, 1);
	float4 sandColor = mountainColor*(0.7+fractalValue2);//float4(0.86+rand.y*rand.x, 0.54+fractalValue/10, 0.12, 1);
	float4 sandColor2 = mountainColor*(0.9+fractalValue);
	if(fractalValue > 0.7) // sable
		color = sandColor;
	else if (fractalValue > 0.6) // effet cool interpolé
	{
		color = sandColor2;
	}
	else
	{
		color = mountainColor;
	}

	return color;
}
/* -----------------------------------------------------------------
 * Génère une texture de montagne.
 * ---------------------------------------------------------------*/
float4 GetMountain2(VertexShaderOutput input, float x, float y, float2 rand, float fractalValue1, float fractalValue2)
{
	float4 colorBase = float4(0.32, 0.32, 0.34, 1);
	float4 color = colorBase;
	float fractalValue = (fractalValue1 + fractalValue2)/2;
	if(fractalValue > 0.6) // neige
		color = float4(0.86, 0.86, 0.95, 1);
	
	// Si on veut que la neige ne soit que sur des surfaces planes :
	color = lerp(colorBase, color, saturate(-input.Normal.z-0.80)*6);

	return color;
}
/* -----------------------------------------------------------------
 * Génère une texture d'herbe.
 * ---------------------------------------------------------------*/

// x et y entre 0 et 0.5
float4 tex_grass(float x, float y)
{
	return tex2Dbias(mountain, float4(x, y, 0, lod))*0.85;
}
// x et y entre 0 et 0.5
float4 tex_mountain(float x, float y)
{
	return tex2Dbias(mountain, float4(x+0.5, y, 0, lod));
}
// x et y entre 0 et 0.5
float4 tex_snow(float x, float y)
{
	return tex2Dbias(mountain, float4(x, y+0.5, 0, lod));
}
// x et y entre 0 et 0.5
float4 tex_sand(float x, float y)
{
	return tex2Dbias(mountain, float4(x+0.5, y+0.5, 0, lod));
}
float4 GetGrass3(VertexShaderOutput input, float x, float y, float z, float2 rand, float fractalValue1, float fractalValue2)
{
	float4 color;
	//float fractalValue = (fractalValue1 + fractalValue2 + saturate(1-abs(z)))/3;
	float fractalValue = (fractalValue1 +  fractalValue2);
	if(fractalValue > 0.90) // sable
		color = tex_sand(x, y);
	else if (fractalValue > 0.40) // effet cool interpolé
	{
		color = lerp(tex_grass(x, y), tex_sand(x, y), abs((fractalValue-0.3))/0.5);;
	}
	else
	{
		color = tex_grass(x, y);;
	}

	return color;
}
/* -----------------------------------------------------------------
 * Génère une texture de montagne.
 * ---------------------------------------------------------------*/
float4 GetMountain3(VertexShaderOutput input, float x, float y, float z, float2 rand, float fractalValue1, float fractalValue2)
{
	float4 colorBase = tex_mountain(x, y);
	float4 color = colorBase;
	float fractalValue = (fractalValue1 + fractalValue2);
	if(fractalValue > 0.5) // neige
		color = lerp(color, tex_snow(x, y), saturate((fractalValue-0.5)/0.4));
	
	// Si on veut que la neige ne soit que sur des surfaces planes :
	color = lerp(color, GetGrass3(input, x, y, z, rand, fractalValue1, fractalValue2), saturate((-input.Normal.z-0.973)*75));

	return color;
}

/* -----------------------------------------------------------------
 * Génère une texture 100% fractale.
 * ---------------------------------------------------------------*/
float4 GetColor(VertexShaderOutput input) : COLOR0
{
	const float thresh1 = 0.30;
	const float thresh2 = 0.45; // 0.40
	// Précalcul des valeurs des fractales.
	float fractalValue1 = GetFractalValue(input, 0.5, float2(0, 0), mC1); // float2(-0.847 0.242)
	float fractalValue2 = GetFractalValue2(input, 0.5, float2(0, 0), mC2);//float2(-0.824, 0.231));

	// Coordonnées
	float x = input.TextureCoord.x;
	float y = input.TextureCoord.y;
	float z = input.Position3D.z / MaxAltitude;
	float realZ = -z;
	float absZ = realZ-fractalValue2/6+fractalValue1/6;// + sin(input.TextureCoord.x*14+input.TextureCoord.y*6+fractalValue2+fractalValue1/3)/4;//realZ+fractalValue2/4-fractalValue1/3 + sin(input.TextureCoord.x*14+input.TextureCoord.y*6+fractalValue2+fractalValue1/3)/4;
	bool inWater = realZ < -0.05;

	float2 xy = ((float2(x, y)*TextureFactor) % 0.48) + 0.01;
	float4 mountainColor = GetMountain3(input, xy.x, xy.y, realZ, 0, fractalValue1, fractalValue2);


	// Color
	float4 color = mountainColor;

	if(inWater)
	{
		color = lerp(saturate(color), float4(0, 0, 0, 1), saturate(abs(realZ+0.05)*5));
	}

	return color;
}


/* -----------------------------------------------------------------
 * Pixel shader
 * Applique les effets d'éclairage et de brouillard ainsi que de texture.
 * ---------------------------------------------------------------*/
float4 LandscapePixelShader(VertexShaderOutput input) : COLOR0
{
	float2 projected;
	projected.x = input.Pos2DFromLight.x/input.Pos2DFromLight.w/2+0.5;
	projected.y = -input.Pos2DFromLight.y/input.Pos2DFromLight.w/2+0.5;
	return tex2D(shadow, projected);
	// Détermine la distance à la caméra et en fait une valeur l entre 0 et 1.
	float dist = distance(xCameraPos, input.Position3D);
	float l = saturate((dist-xFogNear)/(xFogFar-xFogNear));

	// Si on est trop près (dans le cas de la 2e passe de dessin en général).
	if(l < ClipPlaneNear || l > ClipPlaneFar)
		clip(-1);

	// Couleur du pixel de texture.
	float4 texColor = GetColor(input);	//GetMountainColor(input); //GetFractalColor(input, l)/4+GetMountainColor(input)*3/4;//GetMountainColor(input);

	// Application de l'éclairage (pixel)
	input.Normal.z = -input.Normal.z;
	float factor = DotProduct(xLightPosition, input.Position3D, input.Normal);
	factor = factor;//clamp(factor*1.25, -0.75, 0.33);
	texColor = texColor + float4(1, 0.95, 0.6, 1) * (factor-0.50);// (factor+0.3);

	// Specular (au dessus du niveau de l'eau uniquement)
	float z = -input.Position3D.z / MaxAltitude;

	/*float3 reflectionVector = reflect(xLightDirection, input.Normal);
	float specular = saturate(dot(normalize(reflectionVector), normalize(xCameraDirection)));
	specular = pow(abs(specular), 16);*/

	// Tentative d'atmospheric scatering
	float radius = 120;
	float size = 2000;
	float pointPlanetZ = xCameraPos.z + (cos(input.Position3D.x/size) + sin(input.Position3D.y/size))*radius;
	float cameraPlanetZ = input.Position3D.z + (cos(xCameraPos.x/size) + sin(xCameraPos.y/size))*radius;
	float diff1 = clamp((-saturate((cameraPlanetZ - pointPlanetZ)/radius)+l*1.7), 0, 0.85);
	
	radius = 250;
	size = 1200;
	pointPlanetZ = xCameraPos.z + (cos(input.Position3D.x/size) + sin(input.Position3D.y/size))*radius;
	cameraPlanetZ = input.Position3D.z + (cos(xCameraPos.x/size) + sin(xCameraPos.y/size))*radius;
	float diff2 = clamp((-saturate((cameraPlanetZ - pointPlanetZ)/radius)+exp(l+0.4f)), 0, 0.95);



	float4 diff = texColor/2 + float4(diff1*0.72, diff2*0.82, diff2, 1)*xGlobalIllumination/2;
	//return float4(diff.x, diff.y, diff.z, 1);
	// Brouillard
	texColor.rgb = lerp(texColor, diff, saturate(l-0.2)*1.6).rgb;
	//texColor.rgb = lerp(texColor, xFogColor, diff);//).rgb;
	texColor.a = lerp(1, 0, saturate(l-0.8)/0.2);
	return texColor;
}
/* -----------------------------------------------------------------
 * Pixel shader pour le dessin de la réflection de l'eau.
 * Supprime les pixels en dessous du niveau de l'eau.
 * ---------------------------------------------------------------*/
float4 LandscapeReflectionPixelShader(VertexShaderOutput input) : COLOR0
{
	// Si il est en dessous de la position de l'eau, on coupe le pixel.
	if(input.Position3D.z > 0.005)
		clip(-1);

	return LandscapePixelShader(input);
}
/* -----------------------------------------------------------------
 * Pixel shader permettant le dessin sans couleur du paysage.
 * Utilisé pour remplir le depth buffer uniquement.
 * ---------------------------------------------------------------*/
float4 LandscapePixelShaderNoColor(VertexShaderOutput input) : COLOR0
{
	// Détermine la distance à la caméra et en fait une valeur l entre 0 et 1.
	float dist = distance(xCameraPos, input.Position3D);
	float l = saturate((dist-xFogNear)/(xFogFar-xFogNear));
	// Si on est trop près (dans le cas de la 2e passe de dessin en général).
	if(l < ClipPlaneNear || l > ClipPlaneFar)
		clip(-1);
	return 0;
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
// Technique de dessin permettant de remplir uniquement le depth buffer et de mettre
// une couleur bidon sur le paysage.
technique NoColor
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 LandscapePixelShaderNoColor();
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

technique ShadowMap
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 ComputeVertexShaderInput();//ShadowMapVertexShader();
		PixelShader = compile ps_3_0 ShadowMapPixelShader();
	}
}
