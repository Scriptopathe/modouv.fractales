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

texture TreeTexture;
float time;
float3 WindDirection;

float3 SnowNormal;
float SnowPow = 8;
float SnowThreshold;
// Permet d'ajuster le taux de froid de l'arbre, afin d'ajouter de la neige sur les parties
// hautes.
float Coldness = 1;

sampler tree = sampler_state {
   texture = (TreeTexture);
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

VertexShaderOutput VertexShaderFunction(VertexShaderInputInstanced input, float4 color : COLOR0)
{
	input.Position.xy += WindDirection.xy * (1-color.r);
	if(input.TextureCoord.z > 0) // si c'est une feuille
	{
		input.Position.xy += WindDirection.xy * input.TextureCoord.z / 4;
		
		//input.TextureCoord.xy = clamp(float2(0.1, 0.1), float2(0.4, 0.9), input.TextureCoord.xy);
	}
	return ComputeVertexShaderInstancedInput(input);
}

/* -----------------------------------------------------------------
 * Pixel shader
 * Applique les effets d'éclairage et de brouillard ainsi que de texture.
 * ---------------------------------------------------------------*/
ColorAndDepth PixelShaderFunction(VertexShaderOutput input)
{
	// Détermine la distance à la caméra et en fait une valeur l entre 0 et 1.
	ColorAndDepth colorAndDepth;
	float dist = distance(xCameraPos, input.Position3D);
	float l = saturate((dist-xFogNear)/(xFogFar-xFogNear));
	
	// Couleur du pixel de texture.
	float4 texColor;
	texColor = tex2Dbias(tree, float4(input.TextureCoord.x, input.TextureCoord.y, 0, -2));
	clip(texColor.a <= 0.40 ? -1 : 1);

	float2 snowCoords = float2(input.TextureCoord.x%0.25, 0.5+input.TextureCoord.y%0.50);
	float4 snowColor = tex2D(tree, snowCoords);
	

	// Application de la neige
	float3 normal = float3(-input.Normal.x, -input.Normal.y, -input.Normal.z);
	float thresh = 0.4f;
	float snowAmount = saturate(saturate(dot(normal, SnowNormal))-thresh)/(1-thresh);// (saturate( (distance(input.Normal.xyz, SnowNormal)-SnowThreshold)*75 ) * 2+fractalValue)/3;
	snowAmount = pow(snowAmount, max(0.0001f, SnowPow/4))*Coldness;
	texColor.rgb = lerp(texColor.rgb, snowColor, saturate(snowAmount));

	// Application de l'éclairage (pixel)
	if(input.TextureCoord.z > 0)
	{
		// Feuilles
		texColor.rgb *= 1.1*xGlobalIllumination;
	}
	else
	{
		texColor.rgb = ApplyPerPixelDefaultDiffuseLightning(texColor, input.Position3D, input.Normal);
		texColor.rgb *= 1.1*xGlobalIllumination;
	}
	// Brouillard.
	texColor.a = ApplyDefaultFadeOut(1, l);

	// Détermine la valeur permettant d'interpoler le fog
	l = saturate((dist-xFogNear)/(xMaxRenderDistance-xFogNear));
	texColor.rgb = ApplyDefaultFog(texColor.rgb, input.Position2D, l);

	// Calcul de la profondeur.
	dist = distance(xCameraPos, input.Position3D);
	float depth = saturate((dist-xFogNear)/(xMaxRenderDistance-xFogNear));
	
	colorAndDepth.color = texColor;
	colorAndDepth.depth = float4(depth, depth, depth, 1);
	return colorAndDepth;
}

technique Ambient
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

technique ShadowMapInstanced
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 ShadowMapVertexShaderInstanced();
		PixelShader = compile ps_3_0 ShadowMapPixelShader();
	}
}