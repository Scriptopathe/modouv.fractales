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

float WindTime;
texture TreeTexture;
float4x4 xProjection;
float AlphaTestThreshold = 0.95;
float AlphaTestDirection = 1;
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

VertexShaderOutput VertexShaderFunction(VertexShaderInputInstanced input, float4 additional : TEXCOORD15)
{
	// On calcule l'output
	// input.Normal = additional.xyz;
	VertexShaderOutput output = ComputeVertexShaderInstancedInput(input);

	// On modifie la normale : ça sera nos données additionnelles.
	output.Normal = additional.xyz;


	/* BILLBOARD */
	// Work out what direction we are viewing the billboard from.
    float3 viewDirection = xView._m02_m12_m22;

    float3 rightVector = normalize(cross(viewDirection, output.Normal));

	// Calculate the position of this billboard vertex.
    float3 position = output.Position3D;

    // Offset to the left or right.
    position += rightVector * (input.TextureCoord.x - 0.5) * (1+additional.w/50);

	// Offset upward if we are one of the top two vertices.
	float3 normal = output.Normal;
    position += normal * ((input.TextureCoord.y-0.7) * (1+additional.w/50) + sin(additional.w/8 + WindTime/4) );
	
	// Apply the camera transform.
    float4 viewPosition = mul(float4(position, 1), xView);
    output.Position2D = mul(viewPosition, xProjection);
	output.OutPosition2D = output.Position2D;
	output.TextureCoord = input.TextureCoord;

	// Calcul du facteur d'éclairage
	output.Normal.xyz = 1; // on place le facteur d'éclairage dans la normale.

	return output;
}

/* -----------------------------------------------------------------
 * Pixel shader
 * Applique les effets d'éclairage et de brouillard ainsi que de texture.
 * ---------------------------------------------------------------*/
ColorAndDepth PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 texColor = tex2D(tree, float2(input.TextureCoord.x, input.TextureCoord.y));
	clip(texColor.a <= 0.25 ? -1 : 1);
	//clip((texColor.a - AlphaTestThreshold) * AlphaTestDirection);
	texColor *= input.Normal.x; // éclairage stocké dans les composantes de la normale.
	

	// Détermine la distance à la caméra et en fait une valeur l entre 0 et 1.
	float cameraDistance = distance(xCameraPos, input.Position3D);
	float l = ComputeFogDistance(cameraDistance);

	// Couleur du pixel de texture.
	texColor.a = lerp(0.25, 0, saturate(l-0.6)*2.5);
	texColor.rgb = lerp(texColor, texColor/6, saturate(l-0.5)).rgb;
	
	// Calcul de la profondeur.
	float dist = distance(xCameraPos, input.Position3D);
	float depth = saturate((dist-xFogNear)/(xMaxRenderDistance-xFogNear));
	ColorAndDepth colorAndDepth;
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