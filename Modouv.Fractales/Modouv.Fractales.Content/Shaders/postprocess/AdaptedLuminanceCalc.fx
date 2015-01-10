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

float xDT = 0.1f;				// Période de temps écoulée depuis la dernière frame.
// Constance de temps qui détermine la vitesse à la quelle l'adaptation se fait.
const float Tau = 0.5f;


texture LastAdaptedLuminanceTexture;
texture CurrentLuminanceTexture;
sampler useless : register(s0);
sampler lastAdaptedLuminance = sampler_state 
{ 
   texture = <LastAdaptedLuminanceTexture>; 
   magfilter = LINEAR; 
   minfilter = LINEAR; 
   mipfilter = LINEAR; 
   AddressU = Wrap; 
   AddressV = Wrap; 
};

sampler currentLuminance = sampler_state 
{ 
   texture = <CurrentLuminanceTexture>; 
   magfilter = LINEAR; 
   minfilter = LINEAR; 
   mipfilter = LINEAR; 
   AddressU = Wrap; 
   AddressV = Wrap; 
};


// Calcule la luminance adaptée
// A partir de la luminance de cette frame, et de celle à atteindre.
float4 CalcAdaptedLumPS (in float2 coords : TEXCOORD0)    : COLOR0 
{
	float lastLum = tex2D(lastAdaptedLuminance, float2(0.5f, 0.5f)).r; 
	float currentLum = tex2D(currentLuminance, float2(0.5f, 0.5f)).r; 
	float adaptedLum = lastLum + (currentLum - lastLum) * (1 - exp(-xDT * Tau));
	
	return float4(adaptedLum, adaptedLum, adaptedLum, 1.0f); 
} 

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 CalcAdaptedLumPS();
    }
}

