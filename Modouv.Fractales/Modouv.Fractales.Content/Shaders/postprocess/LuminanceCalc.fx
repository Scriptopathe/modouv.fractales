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

texture PreviousMipLevel;
sampler useless : register(s0);
sampler previousMipLevelSampler = sampler_state 
{ 
   texture = <PreviousMipLevel>; 
   magfilter = Linear; 
   minfilter = Linear; 
   mipfilter = None; 
   AddressU = Wrap; 
   AddressV = Wrap; 
};

sampler HDRSampler = sampler_state 
{ 
   texture = <PreviousMipLevel>; 
   magfilter = Point; 
   minfilter = Point; 
   mipfilter = None; 
   AddressU = Wrap; 
   AddressV = Wrap; 
};

static const float3 LUM_CONVERT = float3(0.299f, 0.587f, 0.114f);
float4 LuminancePS (in float2 coords	: TEXCOORD0, uniform bool ComputeLuminance)	: COLOR0
{					
    
	if(ComputeLuminance)
	{
		float3 color = tex2D(HDRSampler, coords).rgb;
		float luminance = dot(color, LUM_CONVERT);
		float logLuminace = log(1e-5 + luminance); 
		return float4(luminance, luminance, luminance, 1.0f);//float4(luminance, luminance, luminance, 1.0f);
	}
	else
	{
		float3 color = tex2D(previousMipLevelSampler, coords).rgb;
		return float4(color.r, color.g, color.b, 1.0f);
	}
}

technique ComputeLuminance
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 LuminancePS(true);
    }
}

technique DownscaleBilinear
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 LuminancePS(false);
    }
}