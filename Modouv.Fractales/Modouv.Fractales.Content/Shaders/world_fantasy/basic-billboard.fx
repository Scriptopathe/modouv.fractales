#include "common.fx"

float3 WindDirection;
texture TreeTexture;
float4x4 xProjection;
float3 SnowNormal;
float SnowPow = 8;
float SnowThreshold;
float AlphaTestThreshold = 0.95;
float AlphaTestDirection = 1;
float4 LightColor =  float4(1, 0.95, 0.6, 1);
float SamplerLod = 0; // level of detail du sampler.
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
	
	// On applique un effet de vent
	input.Position.xy += WindDirection.xy * (1-input.TextureCoord.y);
						/*sin( WindTime * (0.5+additional.w/75) + 
							 additional.w/50.0)  
						
						* (1-input.TextureCoord.y) * 3.8;*/

	// On calcule l'output
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
    position += rightVector * (input.TextureCoord.x - 0.5) * (2+additional.w/200);
	// Offset upward if we are one of the top two vertices.
	float3 normal = output.Normal;
    position += normal * (input.TextureCoord.y-1) * (1.7+additional.w/150) ;
	
	// Apply the camera transform.
    float4 viewPosition = mul(float4(position, 1), xView);
    output.Position2D = mul(viewPosition, xProjection);
	output.OutPosition2D = output.Position2D;
	output.TextureCoord.xy = (input.TextureCoord.xy)/2 + input.TextureOffset;

	// Calcul du facteur d'éclairage
	output.Normal.xyz = GetPerPixelDefaultDiffuseLightningFactor(output.Position3D, output.Normal)*xGlobalIllumination; // on place le facteur d'éclairage dans la normale.
	return output;
}

/* -----------------------------------------------------------------
 * Pixel shader
 * Applique les effets d'éclairage et de brouillard ainsi que de texture.
 * ---------------------------------------------------------------*/
ColorAndDepth PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 texColor = tex2Dlod(tree, float4(input.TextureCoord.x, input.TextureCoord.y, 0, SamplerLod));
	clip((texColor.a - AlphaTestThreshold) * AlphaTestDirection);

	// Application de l'éclairage.
	float factor = input.Normal.x;
	texColor.rgb = texColor.rgb * (factor);

	// Détermine la distance à la caméra et en fait une valeur l entre 0 et 1.
	float dist = distance(xCameraPos, input.Position3D);
	float l = saturate((dist-xFogNear)/(xFogFar-xFogNear));

	// Couleur du pixel de texture.
	texColor.a = ApplyDefaultFadeOut(texColor.a, l);
	// Détermine la valeur permettant d'interpoler le fog
	l = saturate((dist-xFogNear)/(xMaxRenderDistance-xFogNear));
	texColor.rgb = ApplyDefaultFog(texColor.rgb, input.Position2D, l);

	// Calcul de la profondeur.
	dist = distance(xCameraPos, input.Position3D);
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