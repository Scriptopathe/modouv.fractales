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


/* -------------------------------------------------------------------------
 * Paramètres de l'effet
 * -----------------------------------------------------------------------*/
float4x4 xWorldViewProjection;	// Matrice WorldViewProjection précalculée
float4x4 xView;					// Matrice View.
float4x4 xWorld;				// Matrice World contenant la transformation du monde SEULE. (Identité usuellement)
float xFogNear;					// Distance à partir de laquelle le brouillard commence
float xFogFar;					// Distance à partir de laquelle on ne voit plus rien.
float xMaxRenderDistance;		// Render distance max de la scène.
float3 xFogColor = float3(0.7, 0.7, 0.7);	// Couleur du brouillard
float xFogStart;				// Début du brouillard
float xFogAmount;				// Quantité de brouillard.
float3 xCameraDirection;		// Direction de la caméra
float4 xLightPosition;			// Position de la lumière, non transformée.
float3 xLightDirection;			// Direction de la lumière.
float4 xCameraPos;				// Position de la caméra dans l'espace 3D (non transformée).
float4 xGlobalIllumination;		// Illumination globale de la scène.
float4x4 xLightWorldViewProjection; // Matrice WVP de la lumière
float xAmbiant = 0.3;			// Couleur ambiante
bool xFogEnabled = true;		// Vaut true si le brouillard est activé.
texture xShadowMapTexture;		// Texture de la shadow map
texture xBackgroundTexture;
sampler shadow = sampler_state { texture = <xShadowMapTexture> ; magfilter = Point; minfilter = Point; mipfilter=None; AddressU = CLAMP; AddressV = CLAMP;};
sampler background = sampler_state { texture = <xBackgroundTexture> ; magfilter = Point; minfilter = Point; mipfilter=None; AddressU = CLAMP; AddressV = CLAMP;};
// Structure contenant tous les éléments passés au Vertex shader.
struct VertexShaderInput
{
    float4 Position			: POSITION0;		// Position absolue (non transformée) du vertex dans l'espace 3D.
	float4 TextureCoord		: TEXCOORD0;		// Coordonée de texture du Vertex.
	float3 Normal			: NORMAL0;			// Normale du vertex.
};

// Structure contenant tous les éléments passés au Vertex shader.
// Cette version prends en charge l'instancing.
struct VertexShaderInputInstanced
{
	float4x4 InstanceTransform	: TEXCOORD10;		// Matrice World (contenant les transformations) de l'instance en cours de dessin.
	float2 TextureOffset		: TEXCOORD14;		// Offset de la teXture.
	float4 TextureCoord			: TEXCOORD0;		// Coordonée de texture du Vertex.
    float4 Position				: POSITION0;		// Position absolue (non transformée) du vertex dans l'espace 3D.
	float3 Normal				: NORMAL0;			// Normale du vertex.
};

// Structure contenant les éléments en sortie du Vertex Shader et à l'entrée du pixel Shader.
struct VertexShaderOutput
{
    float4 OutPosition2D	: POSITION0;			// Position finale de dessin du vertex
	float4 TextureCoord		: TEXCOORD0;			// Coordonée texture (0 à 1 sur chaque composante).
	float4 Position2D		: TEXCOORD1;			// Même que OutPosition2D mais pouvant être passé au PS.
	float3 Normal			: TEXCOORD2;			// Normale interpolée du vertex (non transformée).
	float4 Position3D		: TEXCOORD3;			// Position interpolée du pixel (non transformée).
	float4 Pos2DFromLight	: TEXCOORD4;			// Position 2D du pixel tel que vu par la lumière.
};


// Effectue les opérations pour préparer une instance de VertexShaderOutput pour l'envoyer au pixel shader.
VertexShaderOutput ComputeVertexShaderInput(VertexShaderInput input)
{
    VertexShaderOutput output;
	float4 position = float4(input.Position.x, input.Position.y, input.Position.z, 1);
	output.Position3D = mul(position, xWorld);
    output.Position2D = mul(position, xWorldViewProjection);
	output.OutPosition2D = output.Position2D;
	output.Normal = normalize(mul(input.Normal, (float3x3)xWorld));
	output.TextureCoord = input.TextureCoord;
	output.Pos2DFromLight = mul(output.Position3D, xLightWorldViewProjection);
    return output;
}

// Effectue les opérations pour préparer une instance de VertexShaderOutput pour l'envoyer au pixel shader.
// Version prenant en charge l'instancing.
VertexShaderOutput ComputeVertexShaderInstancedInput(VertexShaderInputInstanced input)
{
    VertexShaderOutput output;
	float4x4 world = input.InstanceTransform;//transpose(input.InstanceTransform); // matrice world de transformation de l'instance.
	output.Position3D = mul(input.Position, world);
    output.Position2D = mul(output.Position3D, xWorldViewProjection);
	output.OutPosition2D = output.Position2D;
	output.Normal = normalize(mul(input.Normal, (float3x3)xWorld));
	output.TextureCoord = input.TextureCoord;
	output.Pos2DFromLight = mul(output.Position3D, xLightWorldViewProjection);
    return output;
}
/* -----------------------------------------------------------------
 * Opérations sur les complexes
 * ---------------------------------------------------------------*/
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
/* -----------------------------------------------------------------
 * Opérations vectorielles
 * ---------------------------------------------------------------*/
float DotProduct(float3 lightPos, float3 pos3D, float3 normal)
{
    float3 lightDir = normalize(pos3D - lightPos);
    return dot(lightDir, normal);    
}

// Retourne une valeur RGB à partir d'une valeur de teine.
float3 Hue(float H)
{
    float R = abs(H * 6 - 3) - 1;
    float G = 2 - abs(H * 6 - 2);
    float B = 2 - abs(H * 6 - 4);
    return saturate(float3(R,G,B));
}
// Convertit une couleur au format HSV vers RGB.
float3 HSVtoRGB(in float3 HSV)
{
    return ((Hue(HSV.x) - 1) * HSV.y + 1) * HSV.z;
}

/* -----------------------------------------------------------------
 * Effets
 * ---------------------------------------------------------------*/

// Applique l'effet d'éclairage par piXel. 
// currentColorRgb	: couleur actuelle du piXel.
// lightPower		: facteur multiplicatif de l'éclairage diffus. Valeur + élevée = plus de contraste.
// ambiant			: composante constante de luminosité : il s'agit de l'éclairage minimal du piXel.
// position3D		: position non transformée (sauf par world) du piXel.
// normal			: normale interpolée au piXel.
float3 ApplyPerPixelDiffuseLightning(float3 currentColorRgb, float lightPower, float ambient, float4 position3D, float3 normal)
{
	// Application de l'éclairage (pixel)
	float factor = saturate(DotProduct(xLightPosition, position3D, normal));
	factor *= lightPower;
	return currentColorRgb * (factor+ambient);
}
// Applique l'effet d'éclairage par piXel. 
// currentColorRgb	: couleur actuelle du piXel.
// lightPower		: facteur multiplicatif de l'éclairage diffus. Valeur + élevée = plus de contraste.
// ambiant			: composante constante de luminosité : il s'agit de l'éclairage minimal du piXel.
// position3D		: position non transformée (sauf par world) du piXel.
// normal			: normale interpolée au piXel.
float GetPerPixelDefaultDiffuseLightningFactor(float4 position3D, float3 normal)
{
	// Application de l'éclairage (pixel)
	float factor = DotProduct(xLightPosition, position3D, normal);
	if(factor < 0)
		factor /= 6.0f;
	return (factor+xAmbiant);
}

// Applique l'effet d'éclairage par piXel par défaut. 
// currentColorRgb	: couleur actuelle du piXel.
// position3D		: position non transformée (sauf par world) du piXel.
// normal			: normale interpolée au piXel.
float3 ApplyPerPixelDefaultDiffuseLightning(float3 currentColorRgb, float4 position3D, float3 normal)
{
	// Application de l'éclairage (pixel)
	float factor = GetPerPixelDefaultDiffuseLightningFactor(position3D, normal);
	return currentColorRgb * factor;
}

// Applique un brouillard sur la couleur donnée.
// currentColorRgb	: couleur actuelle du piXel.
// fogColor			: couleur du brouillard
// dist				: distance (entre 0 et 1) entre la caméra et le piXel. Utiliser ComputeFogDistance pour l'obtenir.
float4 ApplyFog(float3 currentColorRgb, float3 fogColor, float dist)
{
	float4 texColor;
	texColor.rgb = lerp(currentColorRgb, fogColor, dist).rgb;
	texColor.a = lerp(1, 0, saturate(dist-0.6)*2);
	return texColor;
}
// Applique le brouillard par défaut sur la couleur donnée.
// currentColorRgb	: couleur actuelle du piXel.
// fogColor			: couleur du brouillard
// dist				: distance (entre 0 et 1) entre la caméra et le piXel. Utiliser ComputeFogDistance pour l'obtenir.
float3 ApplyDefaultFog(float3 currentColorRgb, float4 position2D, float dist)
{
	if(!xFogEnabled)
	{
		float3 fogColor = tex2D(background,
						float2(position2D.x/position2D.w/2+0.5, -position2D.y/position2D.w/2+0.5));
		return lerp(currentColorRgb, fogColor, saturate((dist-xFogStart)*xFogAmount)).rgb;;
	}

	float3 texColor;
	texColor = lerp(currentColorRgb, xFogColor*xGlobalIllumination, saturate((dist-xFogStart)*xFogAmount)).rgb;
	return texColor;
}
// Retourne une valeur d'alpha permettant de simuler un fade out s'il n'y a pas d'alpha.
// currentAlpha		: alpha actuelle du piXel.
// dist				: distance (entre 0 et 1) entre la caméra et le piXel. Utiliser ComputeFogDistance pour l'obtenir.
float ApplyDefaultFadeOut(float currentAlpha, float dist)
{
	return lerp(currentAlpha, 0, saturate(dist-0.95)/0.05);
}
// Calcule une valeur entre 0 et 1 correspondant à la distance du piXel à position3D interpolée par rapport
// au near et far planes. (0 : near plane, 1 : far plane).
// cameraDistance	: distance(xCameraPos, input.Position3D).
float ComputeFogDistance(float cameraDistance)
{
	return saturate((cameraDistance-xFogNear)/(xFogFar-xFogNear));
}

/* ----------------------------------------------------------------------------
 * Dessin de la shadow map pour les objets non instancés.
 * --------------------------------------------------------------------------*/
void ShadowMapVertexShader(inout float4 position : POSITION0, out float4 pos3D : TEXCOORD3)
{
	pos3D = mul(position, xWorld);
	position = mul(pos3D, xWorldViewProjection);
}

float4 ShadowMapPixelShader(float4 position : TEXCOORD3, float4 pos2D : TEXCOORD1) : COLOR0
{
	// Détermine la distance à la caméra et en fait une valeur l entre 0 et 1.
	float dist = distance(xLightPosition, position);
	float l = saturate((dist-xFogNear)/(xMaxRenderDistance-xFogNear));
	return float4(l, l, l, 1);
}


/* ----------------------------------------------------------------------------
 * Dessin de la shadow map pour les objets instancés.
 * --------------------------------------------------------------------------*/
void ShadowMapVertexShaderInstanced(float4x4 transform	: TEXCOORD10, inout float4 position : POSITION0, out float4 pos3D : TEXCOORD3)
{
	pos3D = mul(position, transform);
	position = mul(pos3D, xWorldViewProjection);
}