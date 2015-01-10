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

sampler s0;
const float threshold = 0.95;
float pWidth;
float pHeight;
float lum(float2 coords)
{
	float4 color = tex2D(s0, coords);
	return (0.2126*color.r+0.7152*color.g+0.0722*color.b);
}

float4 GenerateNormals(float2 coords: TEXCOORD0) : COLOR0  
{  
	// Intensité des pixels entourant le pixel courant.
	float tl = lum(coords + float2(-pWidth, -pHeight));
	float t = lum(coords + float2(0, -pHeight));
	float tr = lum(coords + float2(pWidth ,- pHeight));
	float r = lum(coords + float2(pWidth, 0));
	float br = lum(coords + float2(pWidth, pHeight));
	float b = lum(coords + float2(0, pHeight));
	float bl = lum(coords + float2(-pWidth, pHeight));
	float l = lum(coords + float2(-pWidth, 0));

	// sobel filter
	float dX = (tr + 2.0 * r + br) - (tl + 2.0 * l + bl);
	float dY = (bl + 2.0 * b + br) - (tl + 2.0 * t + tr);
	float dZ = 1.0 / 2.0;

	return float4(dX, dY, dZ, 1);
}  

technique Technique1  
{  
    pass Pass1  
    {  
        PixelShader = compile ps_2_0 GenerateNormals();  
    }
}  