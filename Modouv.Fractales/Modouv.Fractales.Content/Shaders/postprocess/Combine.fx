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

texture bloomTexture;
texture mapTexture;
float MapPower = 1;

// Profondeur à laquelle se fait le focus
texture FocusDepth;
sampler focusDepthSampler = sampler_state 
{ 
   texture = <FocusDepth>; 
   magfilter = Point; 
   minfilter = Point; 
   mipfilter = None; 
   AddressU = Clamp; 
   AddressV = Clamp; 
};

float FocusPower = 1;

// Valeur déterminant si oui ou non l'HDR rendering est utilisé pour le rendu.
bool UseHDR = true;

// Illumination globale de la scène.
float GlobalIllumination = 0.7f;

// Luminosité max d'un pixel pour le tone mapping.
float MaxLuminance;

// Texture contenant le pixel dont la valeur représente
// la luminosité adaptée de la scène.
texture AdaptedLuminanceTexture;
sampler adaptedLuminance = sampler_state 
{ 
   texture = <AdaptedLuminanceTexture>; 
   magfilter = Point; 
   minfilter = Point; 
   mipfilter = Point; 
   AddressU = Wrap; 
   AddressV = Wrap; 
};

// Texture contenant la profondeur des pixels dans la scène.
texture DepthBuffer;
sampler depthBuffer = sampler_state 
{ 
   texture = <DepthBuffer>; 
   magfilter = Point; 
   minfilter = Point; 
   mipfilter = Point; 
   AddressU = Wrap; 
   AddressV = Wrap; 
};

// Texture contenant la scène sans post process
sampler map : register(s0);

// Texture contenant l'effet de bloom
float BloomPower = 0.9;
sampler bloom = sampler_state 
{ 
   texture = <bloomTexture>; 
   magfilter = LINEAR; 
   minfilter = LINEAR; 
   mipfilter = LINEAR; 
   AddressU = Wrap; 
   AddressV = Wrap; 
};

static const float3 LUM_CONVERT = float3(0.299f, 0.587f, 0.114f);
float3 ToneMap(float3 color)
{
	if(!UseHDR)
		return color;
	// Moyenne ca 
	float adaptedLum = tex2D(adaptedLuminance, float2(0.5f, 0.5f)).r;
	if(adaptedLum < 0.15)
		adaptedLum = 0.15;
	// Valeur médiane de la luminosité de la scène.
	float middleGrey = GlobalIllumination*0.72;
	// Luminosité du pixel :
	float lumPixel = dot(color, LUM_CONVERT);	
	
	// Apply the modified operator (Eq. 4)
	float lumScaled = (lumPixel * middleGrey) / adaptedLum;	
	float lumCompressed = (lumScaled * (1 + (lumScaled / (MaxLuminance)))) / (1 + lumScaled);
	return lumCompressed * color / (lumPixel); //
}

// Effectue la combinaison du bloom + tone mapping (si HDR activé)
float4 ApplyBloomAndToneMap(float2 coords: TEXCOORD0) : COLOR0  
{  
	float3 bloomC = tex2D(bloom, coords).rgb;
	float3 mapC = tex2D(map, coords).rgb;
	float4 color = float4(mapC*MapPower+bloomC*BloomPower, 1);
	color.rgb = ToneMap(color.rgb);
	return color;
}

// Effectue la combinaison du bloom en tenant compte du depth buffer
// et du focus.
float4 ApplyFieldOfView(float2 coords: TEXCOORD0) : COLOR0  
{  
	float3 bloomC = tex2D(bloom, coords).rgb;
	float3 mapC = tex2D(map, coords).rgb;
	float depthC = tex2D(depthBuffer, coords).r;
	float focusDepth = tex2D(focusDepthSampler, float2(0.5, 0.5)).r;

	float3 mapColor = mapC*MapPower;
	float bloomMult = BloomPower*clamp(abs(depthC-focusDepth)*FocusPower, 0, 1);
	float3 bloomColor = bloomC * bloomMult;
	float4 color = float4((mapColor+bloomColor)/((MapPower+bloomMult)), 1);
	return color;
}  

technique UseDepth  
{  
    pass Pass1  
    {  
        PixelShader = compile ps_2_0 ApplyFieldOfView();  
    }
}  

technique Basic  
{  
    pass Pass1  
    {  
        PixelShader = compile ps_2_0 ApplyBloomAndToneMap();  
    }
}  