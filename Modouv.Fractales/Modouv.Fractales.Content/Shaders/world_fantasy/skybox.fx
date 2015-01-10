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

float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float xGlobalIllumination = 0.6;
float3 xCameraPosition;
float xFogEnabled = true;
float3 xFogColor = float3(1, 1, 1);

Texture SkyBoxTextureDay; 
Texture SkyBoxTextureNight; 
samplerCUBE SkyBoxSamplerDay = sampler_state 
{ 
   texture = <SkyBoxTextureDay>; 
   magfilter = LINEAR; 
   minfilter = LINEAR; 
   mipfilter = LINEAR; 
   AddressU = Mirror; 
   AddressV = Mirror; 
};
samplerCUBE SkyBoxSamplerNight = sampler_state 
{ 
   texture = <SkyBoxTextureNight>; 
   magfilter = LINEAR; 
   minfilter = LINEAR; 
   mipfilter = LINEAR; 
   AddressU = Mirror; 
   AddressV = Mirror; 
};
struct VertexShaderInput
{
    float4 Position : POSITION0;
};
 
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 TextureCoordinate : TEXCOORD0;
};

// Permet de retourner deux couleur pour rendu sur deux render targets.
struct DoubleColor
{
	float4 color1 : COLOR0;
	float4 color2 : COLOR1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
 
    float4 worldPosition = mul(input.Position, xWorld);
    float4 viewPosition = mul(worldPosition, xView);
    output.Position = mul(viewPosition, xProjection).xyww;
 
    float4 VertexPosition = mul(input.Position, xWorld);
    output.TextureCoordinate = (VertexPosition - xCameraPosition).zyx;
	output.TextureCoordinate.x = -output.TextureCoordinate.x;
    return output;
}

DoubleColor PixelShaderFunctionDouble(VertexShaderOutput input)
{
	float4 outColor = float4(0, 0, 0, 1);
	if(xFogEnabled)
		outColor.rgb =  xFogColor*xGlobalIllumination;
	else
	{
		float4 tex = texCUBE(SkyBoxSamplerDay, normalize(input.TextureCoordinate));
		float4 tex2 = texCUBE(SkyBoxSamplerNight, normalize(input.TextureCoordinate));
		outColor.rgb = lerp(tex2.rgb, tex.rgb*2, saturate(xGlobalIllumination*0.70-0.3))*(xGlobalIllumination);
	}

	DoubleColor color;
	color.color1 = outColor;
	color.color2 = outColor;
	return color;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 outColor = float4(0, 0, 0, 1);
	if(xFogEnabled)
		outColor.rgb =  xFogColor*xGlobalIllumination;
	else
	{
		float4 tex = texCUBE(SkyBoxSamplerDay, normalize(input.TextureCoordinate));
		float4 tex2 = texCUBE(SkyBoxSamplerNight, normalize(input.TextureCoordinate));
		outColor.rgb = lerp(tex2.rgb, tex.rgb*2, saturate(xGlobalIllumination*0.70-0.3))*(xGlobalIllumination);
	}

	return outColor;
}

technique Skybox
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
technique SkyboxDouble
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunctionDouble();
    }
}