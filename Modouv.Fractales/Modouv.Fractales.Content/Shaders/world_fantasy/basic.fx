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
sampler tree = sampler_state {
   texture = (TreeTexture);
   MinFilter = Point; // Minification Filter
   MagFilter = Point; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Wrap; // Address Mode for U Coordinates
   AddressV = Wrap; // Address Mode for V Coordinates
};


VertexShaderOutput VertexShaderFunction(VertexShaderInputInstanced input)
{
	return ComputeVertexShaderInstancedInput(input);
}

/* -----------------------------------------------------------------
 * Pixel shader
 * Applique les effets d'éclairage et de brouillard ainsi que de texture.
 * ---------------------------------------------------------------*/
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Détermine la distance à la caméra et en fait une valeur l entre 0 et 1.
	float cameraDistance = distance(xCameraPos, input.Position3D);
	float l = ComputeFogDistance(cameraDistance);
	
	// Couleur du pixel de texture.
	float4 texColor = tex2D(tree, float2(input.TextureCoord.x/20, input.TextureCoord.y/20.0f));
	
	// Application de l'éclairage (pixel)
	texColor.rgb = ApplyPerPixelDiffuseLightning(texColor, 1, 0.5, input.Position3D, input.Normal);

	texColor.a = lerp(1, 0, saturate(l-0.6)*2.5);
	texColor.rgb = lerp(texColor, texColor/6, saturate(l-0.5)).rgb;
	return texColor;
}

technique Ambient
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}