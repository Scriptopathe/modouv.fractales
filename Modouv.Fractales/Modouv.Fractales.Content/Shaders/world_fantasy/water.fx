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

// Diffuse
float WaterFrame;

// Vagues
/*
const int WaveXCount = 4;
float waveXAmplitudes[4] = {0.229, 0.132,  0.112, 0.175}; // {2.29, 0.32,  0.36, 0.15};
float waveXPeriods[4] =	 {5  , 21, 73, 0.67}; // {1  , 1.7, 50, 12}

const int WaveYCount = 4; 
float waveYAmplitudes[4] = {0.084, 0.215,  0.124, 0.15}; // {0.24, 1.15,  0.24, 0.5};
float waveYPeriods[4] =	 {14 , 15, 2, 5}; // {0.4  , 1.1, 43, 5};
*/
const int WaveXCount = 0;
float waveXAmplitudes[4] = {0.129, 0.045,  0.0112, 0.0275}; // {2.29, 0.32,  0.36, 0.15};
float waveXPeriods[4] =	 {3  , 7, 9, 16}; // {1  , 1.7, 50, 12}

const int WaveYCount = 0; 
float waveYAmplitudes[4] = {0.184, 0.0215,  0.0604, 0.025}; // {0.24, 1.15,  0.24, 0.5};
float waveYPeriods[4] =	 {2 , 4, 21, 11}; // {0.4  , 1.1, 43, 5};


// Texturing
float TextureFactor = 5;
float VertexSpeed = 1000;
float Scale = 6;

// Mapping
float3 wStartPos;
float wSize;

// Reflection
float4x4 wWorldReflectionViewProjection;
float4x4 wView;

texture WaterTexture;
texture Normals;
texture ReflectionTexture;
texture RefractionTexture;

sampler reflection = sampler_state {
   texture = (ReflectionTexture);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Clamp; // Address Mode for U Coordinates
   AddressV = Clamp; // Address Mode for V Coordinates
};
sampler refraction = sampler_state {
   texture = (RefractionTexture);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Clamp; // Address Mode for U Coordinates
   AddressV = Clamp; // Address Mode for V Coordinates
};
sampler water = sampler_state {
   texture = (WaterTexture);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Wrap; // Address Mode for U Coordinates
   AddressV = Wrap; // Address Mode for V Coordinates
};

sampler normalMap = sampler_state {
   texture = (Normals);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Wrap; // Address Mode for U Coordinates
   AddressV = Wrap; // Address Mode for V Coordinates
};


struct ColorAndDepth
{
	float4 color : COLOR0;
	float4 depth : COLOR1;
};

struct VSOutput
{
    float4 P : POSITION0;		// Position finale de dessin du vertex
	float4 Position : TEXCOORD5;
	float3 Normal : TEXCOORD1;			// Normale finale du vertex.
	float4 Position3D : TEXCOORD3;	// Position du vertex dans la heightmap.
	float3 TextureCoord : TEXCOORD0;	// Coordonée texture (0 à 1 sur chaque composante)
	float4 ReflectionMapPos : TEXCOORD4; // Position sur la reflection map.
};


VSOutput VertexShaderFunction(VertexShaderInput input)
{
    VSOutput output;
	
	float vertexFrame = VertexSpeed * WaterFrame;
	float4 position = input.Position;
	int i;
	for(i = 0; i < WaveXCount; i++)
	{
		position.z += cos((input.Position.x+0.21*input.Position.y)/waveXPeriods[i] + vertexFrame)*waveXAmplitudes[i];
	}
	for(i = 0; i < WaveYCount; i++)
	{
		position.z += cos((input.Position.y+0.14*input.Position.x)/waveYPeriods[i] + vertexFrame)*waveYAmplitudes[i];
	}
	output.Position3D = mul(position, xWorld);
    output.Position = mul(position, xWorldViewProjection);
	output.P = output.Position;
	output.Normal = 0;
	output.TextureCoord = input.TextureCoord*TextureFactor;
	output.ReflectionMapPos = mul(position, wWorldReflectionViewProjection);
    return output;
}
/* -----------------------------------------------------------------
 * Pixel shader
 * Applique les effets d'éclairage et de brouillard ainsi que de texture.
 * ---------------------------------------------------------------*/
float4 BasicDraw(VSOutput input) : COLOR0
{
	float4 waterColor = float4(0, 0, 1, 0);
	return waterColor;
}
/* -----------------------------------------------------------------
 * Pixel shader
 * Applique les effets d'éclairage et de brouillard ainsi que de texture.
 * ---------------------------------------------------------------*/
ColorAndDepth PixelShaderFunction(VSOutput input) 
{
	// Détermine la distance à la caméra et en fait une valeur l entre 0 et 1.
	// Cela permet de donner un effet de brouillard quand on est loin.
	float dst = distance(xCameraPos, input.Position3D);
	float l = saturate((dst-xFogNear)/(xFogFar-xFogNear));

	float sinePerturbation = sin(WaterFrame*100.0)/50.0;
	float offset = WaterFrame*5;
	float2 texCoords = (input.TextureCoord + float2(offset+sinePerturbation*3, offset+sinePerturbation*3));

	// Couleur de l'eau
	float4 waterColor = float4(0, 0.2, 0.7, 1);
	if(xFogEnabled)
		waterColor.rgb = xFogColor*xGlobalIllumination;

	float4 normal = tex2D(normalMap, texCoords);
	float fresnelTerm = dot(normalize(input.Position3D-xCameraPos), float3(0, 0, 1));

	// Perturbation de réflection
	float2 perturbation = sinePerturbation + normal/50.0;

	// Couleur de la réflection
    float2 ProjectedTexCoords;
    ProjectedTexCoords.x = input.ReflectionMapPos.x/input.ReflectionMapPos.w/2.0f + 0.5f;
    ProjectedTexCoords.y = -input.ReflectionMapPos.y/input.ReflectionMapPos.w/2.0f + 0.5f;    
    float4 reflectionColor = tex2D(reflection, ProjectedTexCoords+perturbation);

	// Couleur de la réfraction
	float2 refractionCoords;
	refractionCoords.x = input.Position.x/input.Position.w/2.0f + 0.5f;
    refractionCoords.y = -input.Position.y/input.Position.w/2.0f + 0.5f;  
	float4 refractionColor = tex2D(refraction, refractionCoords + perturbation);
	refractionColor = refractionColor*0.9 + waterColor*0.1;

	// Specular
	float3 reflectionVector = -reflect(xLightDirection, normal);
	reflectionVector.y += sin(WaterFrame*12.0);
	float specular = dot(normalize(reflectionVector), normalize(xCameraDirection));
	specular = 0;//pow(abs(specular), 16);

	// Effet de réflection.
	//reflectionColor = lerp(reflectionColor, waterColor, distance(input.ReflectionMapPos.xy, float2(0.5, 0.5))/120);
	reflectionColor = reflectionColor * 0.9 + waterColor * 0.1; // Remove.
	waterColor = lerp(reflectionColor, refractionColor, saturate(fresnelTerm));

	// Applique l'éclairage. float3 ApplyPerPixelDiffuseLightning(float3 currentColorRgb, float lightPower, float ambient, float4 position3D, float3 normal)
	/*float factor = DotProduct(xLightPosition, input.Position3D+normal*0.1, normal);
	factor *= 0.1;
	waterColor.rgb += float3(1, 1, 0.9) * (factor+0.1);*/

	// Distance au centre de la lumière
	float dstCenter = saturate(dot(normalize(xLightPosition-input.Position3D), normalize(xLightPosition-xCameraPos)));//saturate(dot(normalize(xLightPosition-input.Position3D), normalize(xLightPosition-xCameraPos)));//saturate(dot(normalize(input.Position3D-xCameraPos), normalize(-LightDirection)));
	dstCenter = saturate(dstCenter-0.9999)*2000;
	specular *= (dstCenter+0.2);
	waterColor.rgb += specular;


	// Brouillard
	float4 texColor = waterColor;
	texColor.rgb = ApplyDefaultFog(texColor.rgb, input.Position, l);
	texColor.a = 1;

	// Calcul de la profondeur.
	float dist = distance(xCameraPos, input.Position3D);
	float depth = saturate((dist-xFogNear)/(xMaxRenderDistance-xFogNear));
	ColorAndDepth colorAndDepth;
	colorAndDepth.color = texColor;
	colorAndDepth.depth = float4(depth, depth, depth, 1);
	return colorAndDepth;
}

technique ReflectedWater
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

technique BasicWater
{
	pass Pass1
	{
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 BasicDraw();
	}
}