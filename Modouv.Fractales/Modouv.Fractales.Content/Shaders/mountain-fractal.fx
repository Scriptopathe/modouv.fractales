#include "world_fantasy/common.fx"
float2 mC1 = float2(-0.821, 0.2345);
float2 mC2 = float2(-0.821, 0.2345);
float MaxAltitude = 50.0f;
float ClipPlaneNear = 0;
float ClipPlaneFar = 0;

/* --------------------------------------------------------------
 * Paramètres de texturing / saison 
 * ------------------------------------------------------------*/
// Détermine la saison : hiver ou été.
bool Winter = false;
float4 LightColor =  float4(1, 0.95, 0.6, 1);//float4(0.6, 0.95, 1, 1);

// Normale à la propagation de la neige.
float3 SnowNormal = float3(0.3, 0.4, -1);
float SnowThreshold = 0.7;
float SnowPow = 0.12;
float2 Offset;
float LandscapeSampleSize;
// Facteur multiplicatif de la taille des textures
float TextureFactor = 0.01; // 25
float lod = -2;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                

texture HeightmapTexture;
sampler heightmap = sampler_state {
   texture = (HeightmapTexture);
   MinFilter = Point; // Minification Filter
   MagFilter = Point; // Magnification Filter
   MipFilter = None; // Mip-mapping
   AddressU = Clamp; // Address Mode for U Coordinates
   AddressV = Clamp; // Address Mode for V Coordinates
};

texture MountainTexture;
sampler mountain = sampler_state {
   texture = (MountainTexture);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = None; // Mip-mapping
   AddressU = Wrap; // Address Mode for U Coordinates
   AddressV = Wrap; // Address Mode for V Coordinates
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
	// La hauteur est dans la composante x
	// La normale est dans les composantes y, z, w.
	float2 offset = Offset - Offset % LandscapeSampleSize;
	float2 position = input.Position.xy - input.Position.xy % LandscapeSampleSize;
	float2 ComputedPosition = input.TextureCoord.xy; // Position à laquelle faire les calculs.
	float4 heightAndNormal = tex2Dlod(heightmap, float4(ComputedPosition.x, ComputedPosition.y, 0, 0));
	input.Normal = normalize(heightAndNormal.yzw);
	input.Normal.z = - input.Normal.z;
	input.Position.z = heightAndNormal.x;

	// On applique un offset sur les coordonnées de texture pour qu'un point donné de 
	// l'espace ait toujours la même coordonnée même si ce n'est pas le même point du mesh.
	input.TextureCoord.xy = abs((position+Offset));
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
 * Accès aux textures.
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
float4 tex_1(float x, float y)
{
	return tex2D(mountain, float2(x, y))*0.85;
}
// x et y entre 0 et 0.5
float4 tex_2(float x, float y)
{
	return tex2D(mountain, float2(x+0.5, y));
}
// x et y entre 0 et 0.5
float4 tex_4(float x, float y)
{
	return tex2D(mountain, float2(x, y+0.5));
}
// x et y entre 0 et 0.5
float4 tex_3(float x, float y)
{
	return tex2D(mountain, float2(x+0.5, y+0.5));
}
/* -----------------------------------------------------------------
 * Texturing des parties planes.
 * ---------------------------------------------------------------*/
float4 GetGrassSummer(VertexShaderOutput input, float x, float y, float z, float2 rand, float fractalValue1, float fractalValue2)
{
	float4 color;
	float fractalValue = (fractalValue1 + fractalValue2 + saturate(1-abs(z))*0.72)/3;
	//float fractalValue = (fractalValue1 +  fractalValue2);

	// Texturing 
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
 * Texturing de la partie montagneuse.
 * ---------------------------------------------------------------*/
float4 GetMountainSummer(VertexShaderOutput input, float x, float y, float z, float2 rand, float fractalValue1, float fractalValue2)
{
	float4 colorBase = tex_mountain(x, y);
	float4 color = colorBase;
	float fractalValue = (fractalValue1 + fractalValue2);

	if(fractalValue > 1) // neige
		color = lerp(color, tex_snow(x, y), saturate((fractalValue-1)/0.4));
	
	// Applique les textures des surfaces planes.
	color = lerp(color, GetGrassSummer(input, x, y, z, rand, fractalValue1, fractalValue2), saturate((abs(input.Normal.z)-0.700)*75));

	return color;
}

/* -----------------------------------------------------------------
 * Génère une texture 100% fractale.
 * ---------------------------------------------------------------*/
float4 GetColorSummer(VertexShaderOutput input) : COLOR0
{
	const float thresh1 = 0.30;
	const float thresh2 = 0.45; // 0.40


	// Coordonnées
	float x = input.TextureCoord.x;
	float y = input.TextureCoord.y;
	float z = input.Position3D.z / MaxAltitude;
	float realZ = -z;
	float absZ = realZ;// + sin(input.TextureCoord.x*14+input.TextureCoord.y*6+fractalValue2+fractalValue1/3)/4;//realZ+fractalValue2/4-fractalValue1/3 + sin(input.TextureCoord.x*14+input.TextureCoord.y*6+fractalValue2+fractalValue1/3)/4;
	bool inWater = realZ < -0.05;

	float2 xy = ((float2(x, y)*TextureFactor) % 0.48) + 0.01;
	float4 mountainColor = GetMountainSummer(input, xy.x, xy.y, realZ, 0, 1, 1);


	// Color
	float4 color = mountainColor;

	if(inWater)
	{
		color = lerp(saturate(color), float4(0, 0, 0, 1), saturate(abs(realZ+0.05)*5));
	}

	return color;
}
/* -----------------------------------------------------------------
 * Texturing des parties planes.
 * ---------------------------------------------------------------*/
float4 GetGrassWinter(VertexShaderOutput input, float x, float y, float z, float2 rand, float fractalValue1, float fractalValue2)
{
	float4 color;
	float fractalValue = (fractalValue1 + fractalValue2 + saturate(1-abs(z))*0.72)/3;
	//float fractalValue = (fractalValue1 +  fractalValue2);

	// Texturing 
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
 * Texturing de la partie montagneuse.
 * ---------------------------------------------------------------*/
float4 GetMountainWinter(VertexShaderOutput input, float x, float y, float z, float2 rand, float fractalValue1, float fractalValue2)
{
	// Couleur de base : montagne.
	float4 color;
	float4 snowColor;
	float fractalValue = (fractalValue1 + fractalValue2);
	// Applique les textures des surfaces planes.
	float snowAmount = saturate(saturate(dot(input.Normal.xyz, SnowNormal))-SnowThreshold)/(1-SnowThreshold);// (saturate( (distance(input.Normal.xyz, SnowNormal)-SnowThreshold)*75 ) * 2+fractalValue)/3;
	snowAmount = saturate((1.8-fractalValue)*snowAmount);//(snowAmount + (fractalValue-0.5)*2)/3;
	snowAmount = pow(snowAmount, SnowPow);
	float horizontalAmount = saturate((abs(input.Normal.z)-0.500)*75);

	// Texture de base
	color = tex_3(x, y);//lerp(tex_4(x, y), tex_3(x, y), horizontalAmount);
	snowColor = tex_1(x, y);//lerp(tex_1(x, y), tex_2(x, y), horizontalAmount);
	
	// Ajout de la neige
	color = lerp(color, snowColor, snowAmount);


	return color;
}
/* -----------------------------------------------------------------
 * Génère une texture 100% fractale.
 * ---------------------------------------------------------------*/
float4 GetColorWinter(VertexShaderOutput input) : COLOR0
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
	float4 mountainColor = GetMountainWinter(input, xy.x, xy.y, realZ, 0, fractalValue1, fractalValue2);


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
float4 ApplyEffects(VertexShaderOutput input, float4 texColor)
{
	// Détermine la distance à la caméra et en fait une valeur l entre 0 et 1.
	float dist = distance(xCameraPos, input.Position3D);
	float l = saturate((dist-xFogNear)/(xFogFar-xFogNear));

	// Si on est trop près (dans le cas de la 2e passe de dessin en général).
	if(l < ClipPlaneNear || l > ClipPlaneFar)
		clip(-1);

	
	// Application de l'éclairage + ombres
	input.Normal.z = -input.Normal.z;
	float ambiant = -0.5;
	
	
	// Ombres
	/*float2 projected;
	projected.x = input.Pos2DFromLight.x/input.Pos2DFromLight.w/2+0.5;
	projected.y = -input.Pos2DFromLight.y/input.Pos2DFromLight.w/2+0.5;
    float depthStoredInShadowMap = tex2D(shadow, projected).r;
	//return float4(depthStoredInShadowMap, depthStoredInShadowMap, depthStoredInShadowMap, 1);
    float realDistance = input.Pos2DFromLight.z/input.Pos2DFromLight.w;
	
	// Si la zone a dessiner est éclairée :
    if ((realDistance - 1/100.0f) > depthStoredInShadowMap)
    {
		ambiant -= 1;
    }
    // */

	// Specular
    /*float3 reflectionVector = reflect(xLightDirection, specNormal);
	float specular = abs(dot(normalize(reflectionVector), normalize(xCameraDirection)));
	specular = pow(abs(specular), 16);*/

	float factor = DotProduct(xLightPosition, input.Position3D, input.Normal);
	factor = factor+ambiant;
	if(factor > 0)
		// Application de l'éclairage avec la lumière de couleur.
		texColor = texColor + LightColor * (factor);
	else
		// Assombrissement utilisant du gris.
		texColor = texColor + float4(0.4, 0.4, 0.4, 1) * (factor);

	// Tentative d'atmospheric scatering
	float z = -input.Position3D.z / MaxAltitude;
	float4 diff = float4(0.72, 0.82, 1, 1)*xGlobalIllumination/2;

	// Brouillard
	texColor.rgb = lerp(texColor, diff, saturate(l-0.2)*1.6).rgb;
	texColor.a = lerp(1, 0, saturate(l-0.8)/0.2);
	return texColor;
}

float4 LandscapePixelShaderSummer(VertexShaderOutput input) : COLOR0
{
	// Couleur du pixel de texture.
	float4 texColor = GetColorSummer(input);
	return ApplyEffects(input, texColor);
}

float4 LandscapePixelShaderWinter(VertexShaderOutput input) : COLOR0
{
	// Couleur du pixel de texture.
	float4 texColor = GetColorWinter(input);
	return ApplyEffects(input, texColor);
}

/* -----------------------------------------------------------------
 * Pixel shader pour le dessin de la réflection de l'eau.
 * Supprime les pixels en dessous du niveau de l'eau.
 * ---------------------------------------------------------------*/
float4 LandscapeReflectionPixelShaderSummer(VertexShaderOutput input) : COLOR0
{
	// Si il est en dessous de la position de l'eau, on coupe le pixel.
	if(input.Position3D.z > 0.005)
		clip(-1);

	return LandscapePixelShaderSummer(input);
}
float4 LandscapeReflectionPixelShaderWinter(VertexShaderOutput input) : COLOR0
{
	// Si il est en dessous de la position de l'eau, on coupe le pixel.
	if(input.Position3D.z > 0.005)
		clip(-1);

	return LandscapePixelShaderWinter(input);
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
        PixelShader = compile ps_3_0 LandscapePixelShaderWinter();
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


technique Reflection
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 LandscapeReflectionPixelShaderWinter();
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
