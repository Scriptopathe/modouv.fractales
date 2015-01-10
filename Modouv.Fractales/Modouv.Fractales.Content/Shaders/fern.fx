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

float4x4 xWorldViewProjection;
float4x4 xWorld;
float4 xCameraPos;
float xFogNear;
float4 xLightPosition;
float3 xLightDirection;
float3 xCameraDirection;
float xFogFar;;
float4 xFogColor = float4(1, 1, 1, 1);
texture Tex;
sampler tex = sampler_state {
   texture = (Tex);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Wrap; // Address Mode for U Coordinates
   AddressV = Wrap; // Address Mode for V Coordinates
};
// Diffuse
float3 DiffuseDirection = float3(0.75, 0.45, 0.75);
struct VertexShaderInput
{
    float4 Position		: POSITION0;		// Passée par notre Vertex
	float3 Normal		: NORMAL0;			// Passée par notre Vertex
	float4x4 InstanceTransform : TEXCOORD10;// Matrice World (contenant les transformations) de l'instance en cours de dessin.
	float2 TextureOffset : TEXCOORD14;
	float3 TextureCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position2D		: POSITION0;		// Position finale de dessin du vertex
	float3 Normal			: TEXCOORD1;			// Normale finale du vertex.
	float3 TextureCoord		: TEXCOORD2;
	float4 Position3D		: TEXCOORD3;		// Position en 3D du vertex.
};


VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	float4x4 world = transpose(input.InstanceTransform);
	output.Position3D = mul(input.Position, world);
    output.Position2D = mul(mul(input.Position, world), xWorldViewProjection);
	output.TextureCoord = input.TextureCoord;
	output.Normal = normalize(mul(input.Normal, (float3x3)xWorld));
    return output;
}

/* -----------------------------------------------------------------
 * Pixel shader
 * Applique les effets d'éclairage et de brouillard ainsi que de texture.
 * ---------------------------------------------------------------*/
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Détermine la distance à la caméra et en fait une valeur l entre 0 et 1.
	float dst = distance(xCameraPos, input.Position3D);
	float l = saturate((dst-xFogNear)/(xFogFar-xFogNear));
	
	// Couleur du pixel de texture.
	float4 texColor = tex2D(tex, float2(input.TextureCoord.x, input.TextureCoord.y));

	// Application de l'éclairage
    float4 norm = float4(normalize(input.Normal), 1.0);
    float4 diffuse = saturate(dot(DiffuseDirection, norm))*1.5;
	texColor.rgb = ((diffuse.r + 0.5)*texColor).rgb;
	texColor.rgb = lerp(texColor, texColor*3, saturate(l-0.5)).rgb;
	// Interpolation linéaire pour le brouillard.
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