// Copyright (C) 2013, 2014 Alvarez Josué
//
// This code is free software; you can redistribute it and/or modify it
// under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2.1 of the License, or (at
// your option) any later version.
//
// This code is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public
// License (LICENSE.txt) for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this library; if not, write to the Free Software Foundation,
// Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// The developer's email is jUNDERSCOREalvareATetudDOOOTinsa-toulouseDOOOTfr (for valid email, replace 
// capital letters by the corresponding character)

#include "common.fx"
float2 mC1 = float2(-0.821, 0.2345);
float2 mC2 = float2(-0.821, 0.2345);
float MaxAltitude = 50.0f;
float ClipPlaneNear = 0;
float ClipPlaneFar = 0;
float WaterPlaneZ = 0.05;
sampler useless : register(s0);
/* --------------------------------------------------------------
 * Paramètres de texturing / saison 
 * ------------------------------------------------------------*/
// Détermine la saison : hiver ou été.
bool Winter = false;
float4 LightColor =  float4(1, 0.95, 0.6, 1);//float4(0.6, 0.95, 1, 1);

// Normale à la propagation de la neige.
float3 SnowNormal = normalize(float3(0.3, 0.4, -1));
float SnowThreshold = 0.7;
float SnowPow = 16;

// Facteur multiplicatif de la taille des textures
float TextureFactor = 60; // 25
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

texture NormalMapTexture;
sampler normals = sampler_state {
   texture = (NormalMapTexture);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Clamp; // Address Mode for U Coordinates
   AddressV = Clamp; // Address Mode for V Coordinates
};

struct SurfaceData
{
	float4 color;
	float4 normal;
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

const int MAX_DEPTH = 28;//28;

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
// x et y entre 0 et 0.5
float4 n_grass(float x, float y)
{
	return tex2Dbias(normals, float4(x, y, 0, lod))*0.85;
}
// x et y entre 0 et 0.5
float4 n_mountain(float x, float y)
{
	return tex2Dbias(normals, float4(x+0.5, y, 0, lod));
}
// x et y entre 0 et 0.5
float4 n_snow(float x, float y)
{
	return tex2Dbias(normals, float4(x, y+0.5, 0, lod));
}
// x et y entre 0 et 0.5
float4 n_sand(float x, float y)
{
	return tex2Dbias(normals, float4(x+0.5, y+0.5, 0, lod));
}

float4 tex_1(float x, float y)
{
	return tex2Dbias(mountain, float4(x, y, 0, lod))*0.85;
}
// x et y entre 0 et 0.5
float4 tex_2(float x, float y)
{
	return tex2Dbias(mountain, float4(x+0.5, y, 0, lod));
}
// x et y entre 0 et 0.5
float4 tex_4(float x, float y)
{
	return tex2Dbias(mountain, float4(x, y+0.5, 0, lod));
}
// x et y entre 0 et 0.5
float4 tex_3(float x, float y)
{
	return tex2Dbias(mountain, float4(x+0.5, y+0.5, 0, lod));
}

float4 n_1(float x, float y)
{
	return tex2Dbias(normals, float4(x, y, 0, lod))*0.85;
}
// x et y entre 0 et 0.5
float4 n_2(float x, float y)
{
	return tex2Dbias(normals, float4(x+0.5, y, 0, lod));
}
// x et y entre 0 et 0.5
float4 n_4(float x, float y)
{
	return tex2Dbias(normals, float4(x, y+0.5, 0, lod));
}
// x et y entre 0 et 0.5
float4 n_3(float x, float y)
{
	return tex2Dbias(normals, float4(x+0.5, y+0.5, 0, lod));
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
	// Couleur de base : montagne.
	float4 color;
	float4 normal;
	float4 snowColor;
	float fractalValue = (fractalValue1 + fractalValue2);
	// Applique les textures des surfaces planes.
	float snowAmount = saturate(saturate(dot(input.Normal.xyz, SnowNormal))-SnowThreshold)/(1-SnowThreshold);// (saturate( (distance(input.Normal.xyz, SnowNormal)-SnowThreshold)*75 ) * 2+fractalValue)/3;
	snowAmount = saturate((1.8-fractalValue)*snowAmount);//(snowAmount + (fractalValue-0.5)*2)/3;
	snowAmount = pow(snowAmount, SnowPow);
	float horizontalAmount = saturate((-input.Normal.z-0.800)*12);

	// Texture de base
	color = lerp(tex_2(x, y), tex_1(x, y), horizontalAmount);
	snowColor = lerp(tex_4(x, y), tex_3(x, y), horizontalAmount);
	
	// Ajout de la neige
	color = lerp(color, snowColor, snowAmount);

	return color;
}

/* -----------------------------------------------------------------
 * Génère une texture 100% fractale.
 * ---------------------------------------------------------------*/
float4 GetColorSummer(VertexShaderOutput input) : COLOR0
{
	const float thresh1 = 0.30;
	const float thresh2 = 0.45; // 0.40

	// Précalcul des valeurs des fractales.
	float fractalValue1 = GetFractalValue(input, 0.5, float2(0, 0), mC1);	// float2(-0.847 0.242)
	float fractalValue2 = GetFractalValue2(input, 0.5, float2(0, 0), mC2);	// float2(-0.824, 0.231));

	// Coordonnées
	float x = input.TextureCoord.x;
	float y = input.TextureCoord.y;
	float z = input.Position3D.z / MaxAltitude;
	float realZ = -z;
	float absZ = realZ-fractalValue2/6+fractalValue1/6;// + sin(input.TextureCoord.x*14+input.TextureCoord.y*6+fractalValue2+fractalValue1/3)/4;//realZ+fractalValue2/4-fractalValue1/3 + sin(input.TextureCoord.x*14+input.TextureCoord.y*6+fractalValue2+fractalValue1/3)/4;
	bool inWater = realZ < -0.05;

	float2 xy = ((float2(x, y)*TextureFactor) % 0.48) + 0.01;
	float4 mountainColor = GetMountainSummer(input, xy.x, xy.y, realZ, 0, fractalValue1, fractalValue2);


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
SurfaceData GetGrassWinter(VertexShaderOutput input, float x, float y, float z, float2 rand, float fractalValue1, float fractalValue2)
{
	float4 color;
	float4 normal;
	float fractalValue = (fractalValue1 + fractalValue2 + saturate(1-abs(z))*0.72)/3;
	//float fractalValue = (fractalValue1 +  fractalValue2);

	// Texturing 
	if(fractalValue > 0.90) // sable
	{
		color = tex_sand(x, y);
		normal = n_sand(x, y);
	}
	else if (fractalValue > 0.40) // effet cool interpolé
	{
		float interpValue =  abs((fractalValue-0.3))/0.5;
		color = lerp(tex_grass(x, y), tex_sand(x, y), interpValue);
		normal = lerp(n_grass(x, y), n_sand(x, y), interpValue);
	}
	else
	{
		color = tex_grass(x, y);
		normal = n_grass(x, y);
	}

	// Crée et retourne les informations de surface.
	SurfaceData surfaceData;
	surfaceData.color = color;
	surfaceData.normal = normal;
	return surfaceData;
}
/* -----------------------------------------------------------------
 * Texturing de la partie montagneuse.
 * ---------------------------------------------------------------*/
SurfaceData GetMountainWinter(VertexShaderOutput input, float x, float y, float z, float2 rand, float fractalValue1, float fractalValue2)
{
	// Couleur de base : montagne.
	float4 color;
	float4 normal;
	float4 snowColor;
	float fractalValue = (fractalValue1 + fractalValue2);
	// Applique les textures des surfaces planes.
	float snowAmount = saturate(saturate(dot(input.Normal.xyz, SnowNormal))-SnowThreshold)/(1-SnowThreshold);// (saturate( (distance(input.Normal.xyz, SnowNormal)-SnowThreshold)*75 ) * 2+fractalValue)/3;
	snowAmount = saturate((1.8-fractalValue)*snowAmount);//(snowAmount + (fractalValue-0.5)*2)/3;
	snowAmount = pow(snowAmount, SnowPow);
	float horizontalAmount = saturate((-input.Normal.z-0.800)*12);

	// Texture de base
	color = lerp(tex_4(x, y), tex_3(x, y), saturate(horizontalAmount-z*5));
	snowColor = lerp(tex_1(x, y), tex_2(x, y), horizontalAmount);
	
	// Ajout de la neige
	color = lerp(color, snowColor, snowAmount);
	normal = lerp(n_3(x, y), n_2(x, y), snowAmount);

	// Crée et retourne les informations de surface.
	SurfaceData surfaceData;
	surfaceData.color = color;
	surfaceData.normal = normal;
	return surfaceData;
}
/* -----------------------------------------------------------------
 * Génère une texture 100% fractale.
 * ---------------------------------------------------------------*/
SurfaceData GetColorWinter(VertexShaderOutput input) : COLOR0
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
	float absZ = realZ-fractalValue2/4+fractalValue1/4;// + sin(input.TextureCoord.x*14+input.TextureCoord.y*6+fractalValue2+fractalValue1/3)/4;//realZ+fractalValue2/4-fractalValue1/3 + sin(input.TextureCoord.x*14+input.TextureCoord.y*6+fractalValue2+fractalValue1/3)/4;
	bool inWater = realZ < -0.05;

	float2 xy = ((float2(x, y)*TextureFactor) % 0.48) + 0.01;
	SurfaceData mountainColor = GetMountainWinter(input, xy.x, xy.y, realZ, 0, fractalValue1, fractalValue2);


	// Color
	
	if(inWater)
	{
		mountainColor.color = lerp(saturate(mountainColor.color), float4(0, 0, 0, 1), saturate(abs(realZ+0.05)*5));
	}

	return mountainColor;
}

/* -----------------------------------------------------------------
 * Pixel shader
 * Applique les effets d'éclairage et de brouillard ainsi que de texture.
 * ---------------------------------------------------------------*/
float4 ApplyEffects(VertexShaderOutput input, float4 texColor, float4 bumpValue)
{
	// Détermine la distance à la caméra et en fait une valeur l entre 0 et 1.
	float dist = distance(xCameraPos, input.Position3D);
	float l = saturate((dist-xFogNear)/(xFogFar-xFogNear));

	// Si on est trop près (dans le cas de la 2e passe de dessin en général).
	if(l < ClipPlaneNear || l > ClipPlaneFar)
		clip(-1);

	
	// Application de l'éclairage + ombres
	input.Normal.z = -input.Normal.z; 
	
	
	// Specular
    /*float3 reflectionVector = reflect(xLightDirection, specNormal);
	float specular = abs(dot(normalize(reflectionVector), normalize(xCameraDirection)));
	specular = pow(abs(specular), 16);*/

	// Calcul de la normale avec bump mapping appliqué.

	// Bump map
	/*float3 tangent = float3(1, 0, 0);
	float3 binormal = float3(0, 1, 0);
	float3 bump = 0.8 * (bumpValue.xyz - (0.5, 0.5, 0.5));
    float3 bumpNormal = input.Normal + (bump.x * tangent + bump.y * binormal);
	bumpNormal = normalize(bumpNormal);
	float factor = GetPerPixelDefaultDiffuseLightningFactor(input.Position3D, bumpNormal)*xGlobalIllumination;	
	
	// Calculate le specular
    float3 light = xLightDirection;
    float3 r = normalize(2 * dot(light, bumpNormal) * bumpNormal - light);
    float3 v = normalize(mul(normalize(xView), xWorld));
	float dotProduct = dot(r, v);
	float shininess = 8;
    float4 specular = float4(0.7, 0.7, 0.7, 0.7) * max(pow(dotProduct, shininess), 0)*xGlobalIllumination;*/

	//return input.Position2D.x/input.Position2D.w/2;
	// if(input.Position2D.x/input.Position2D.w < 0.25)
	float factor = GetPerPixelDefaultDiffuseLightningFactor(input.Position3D, input.Normal)*xGlobalIllumination;	
	texColor.rgb = texColor*factor; // + specular*xGlobalIllumination;
	// Ombres
	
	/*
	float2 projected;
	projected.x = input.Pos2DFromLight.x/input.Pos2DFromLight.w/2+0.5;
	projected.y = -input.Pos2DFromLight.y/input.Pos2DFromLight.w/2+0.5;
    float depthStoredInShadowMap = tex2D(shadow, projected).r;
	//return float4(depthStoredInShadowMap, depthStoredInShadowMap, depthStoredInShadowMap, 1);
	float ldist = distance(xLightPosition, input.Position3D);
    float realDistance = saturate((ldist-xFogNear)/(xFogFar-xFogNear));//input.Pos2DFromLight.z/input.Pos2DFromLight.w;
	
	return float4(depthStoredInShadowMap, depthStoredInShadowMap, depthStoredInShadowMap, 1);
	// Si la zone a dessiner est éclairée :
    if ((realDistance - 1/100.0f) < depthStoredInShadowMap)
    {
		texColor.rgb *= 0.2;
    }*/

	// Brouillard
	texColor.rgb = ApplyDefaultFog(texColor.rgb, input.Position2D, l);
	// Si le brouillard est activé pas besoin de fade out.
	texColor.a = 1;

	return texColor;
}

struct ColorAndDepth
{
	float4 Color : COLOR0;
	float4 Depth : COLOR1;
};

ColorAndDepth LandscapePixelShaderSummer(VertexShaderOutput input) : COLOR0
{
	if(input.Position3D.z > WaterPlaneZ)
		clip(-1);
	// Calcul de la profondeur.
	float dist = distance(xCameraPos, input.Position3D);
	float depth = saturate((dist-xFogNear)/(xMaxRenderDistance-xFogNear));
	// Couleur du pixel de texture.
	float4 texColor = GetColorSummer(input);
	texColor = ApplyEffects(input, texColor, float4(0.5, 0.5, 0.5, 1));
	// Retour
	ColorAndDepth val;
	val.Color = texColor;
	val.Depth = depth;
	val.Depth.a = 1;
	return val;
}

ColorAndDepth LandscapePixelShaderWinter(VertexShaderOutput input) : COLOR0
{
	if(input.Position3D.z > WaterPlaneZ)
		clip(-1);

	// Calcul de la profondeur.
	float dist = distance(xCameraPos, input.Position3D);
	float depth = saturate((dist-xFogNear)/(xFogFar-xFogNear));

	// Couleur du pixel de texture.
	SurfaceData data = GetColorWinter(input);
	float4 texColor = ApplyEffects(input, data.color, data.normal);

	// Retour
	ColorAndDepth val;
	val.Color = texColor;
	val.Depth = depth;
	val.Depth.a = 1;
	return val;
}

/* -----------------------------------------------------------------
 * Pixel shader pour le dessin de la réflection de l'eau.
 * Supprime les pixels en dessous du niveau de l'eau.
 * ---------------------------------------------------------------*/
float4 LandscapeReflectionPixelShaderSummer(VertexShaderOutput input) : COLOR0
{
	// Si il est en dessous de la position de l'eau, on coupe le pixel.
	if(input.Position3D.z > WaterPlaneZ)
		clip(-1);

	// Couleur du pixel de texture.
	float4 texColor = GetColorSummer(input);
	texColor = ApplyEffects(input, texColor, float4(0.5, 0.5, 0.5, 1));
	return texColor;
}
float4 LandscapeReflectionPixelShaderWinter(VertexShaderOutput input) : COLOR0
{
	// Si il est en dessous de la position de l'eau, on coupe le pixel.
	if(input.Position3D.z > WaterPlaneZ)
		clip(-1);

	// Couleur du pixel de texture.
	float4 texColor = GetColorWinter(input).color;
	texColor = ApplyEffects(input, texColor, float4(0.5, 0.5, 0.5, 1));
	return texColor;
}

float4 LandscapeRefractionPixelShaderSummer(VertexShaderOutput input) : COLOR0
{
	// Si il est en dessous de la position de l'eau, on coupe le pixel.
	if(input.Position3D.z < WaterPlaneZ)
		clip(-1);

	// Couleur du pixel de texture.
	float4 texColor = GetColorSummer(input);
	texColor = ApplyEffects(input, texColor, float4(0.5, 0.5, 0.5, 1));
	return texColor;
}
float4 LandscapeRefractionPixelShaderWinter(VertexShaderOutput input) : COLOR0
{
	// Si il est en dessous de la position de l'eau, on coupe le pixel.
	if(input.Position3D.z < WaterPlaneZ)
		clip(-1);

	// Couleur du pixel de texture.
	float4 texColor = GetColorWinter(input).color;
	texColor = ApplyEffects(input, texColor, float4(0.5, 0.5, 0.5, 1));
	return texColor;
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
technique LandscapeSummer
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 LandscapePixelShaderSummer();
    }
}
// Technique de dessin permettant le rendu visual "normal" de l'objet.
technique LandscapeWinter
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

// Technique de dessin permettant le rendu visuel de la ReflectionMap
// utilisée pour le dessin de l'eau.
technique ReflectionSummer
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 LandscapeReflectionPixelShaderSummer();
    }
}

technique ReflectionWinter
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 LandscapeReflectionPixelShaderWinter();
    }
}
// Technique de dessin permettant le rendu visuel de la Refraction Map
// utilisée pour le dessin de l'eau.
technique RefractionSummer
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 LandscapeRefractionPixelShaderSummer();
    }
}

technique RefractionWinter
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 LandscapeRefractionPixelShaderWinter();
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
