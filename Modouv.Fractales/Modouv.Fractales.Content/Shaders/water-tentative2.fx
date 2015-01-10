float4x4 xWorldViewProjection;
float4x4 xWorld;
float4 xCameraPos;
float xFogNear;
float xFogFar;
float4 xFogColor = float4(1, 1, 1, 0);
float3 CameraDirection;
float3 LightDirection;
float4 LightPosition;
// Diffuse
float WaterFrame;
float AmplitudeX = 0.5;
float AmplitudeY = 0.4;
float PeriodX = 1;
float PeriodY = 1.7;
float PeriodY2 = 50;
float AmplitudeY2 = 4;

// Vagues
const int WaveXCount = 4;
float waveXAmplitudes[4] = {2.29, 1.32,  1.12, 0.75}; // {2.29, 0.32,  0.36, 0.15};
float waveXPeriods[4] =	 {50  , 32, 73, 124}; // {1  , 1.7, 50, 12}

const int WaveYCount = 4; 
float waveYAmplitudes[4] = {1.24, 2.15,  1.24, 0.5}; // {0.24, 1.15,  0.24, 0.5};
float waveYPeriods[4] =	 {93 , 214, 84, 126}; // {0.4  , 1.1, 43, 5};

// Texturing
float TextureFactor = 20;
float VertexSpeed = 1000;
float Scale = 20;

// Mapping
float3 wStartPos;
float wSize;
float2 wWindVector = float2(0.3, 0.7);
// Reflection
float4x4 wWorldReflectionViewProjection;
float4x4 wView;

texture WaterTexture;
texture Normals;
texture ReflectionTexture;

sampler reflection = sampler_state {
   texture = (ReflectionTexture);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Mirror; // Address Mode for U Coordinates
   AddressV = Mirror; // Address Mode for V Coordinates
};
sampler water = sampler_state {
   texture = (WaterTexture);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Wrap; // Address Mode for U Coordinates
   AddressV = Wrap; // Address Mode for V Coordinates
};
sampler normalMap = sampler_state {
   texture = (Normals);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Wrap; // Address Mode for U Coordinates
   AddressV = Wrap; // Address Mode for V Coordinates
};
struct VertexShaderInput
{
    float4 Position : POSITION0;		// Passée par notre Vertex
	float3 TextureCoord : TEXCOORD0;	// Passée par notre Vertex
	float3 Normal		: NORMAL0;		// Passée par notre Vertex
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;		// Position finale de dessin du vertex
	float3 Normal : TEXCOORD1;			// Normale finale du vertex.
	float4 Position3D : TEXCOORD3;	// Position du vertex dans la heightmap.
	float3 TextureCoord : TEXCOORD0;	// Coordonée texture (0 à 1 sur chaque composante)
	float4 ReflectionMapPos : TEXCOORD4; // Position sur la reflection map.
};

float DotProduct(float3 lightPos, float3 pos3D, float3 normal)
{
    float3 lightDir = normalize(pos3D - lightPos);
    return dot(-lightDir, normal);    
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
	float4 position = input.Position;
	float vertexFrame = VertexSpeed * WaterFrame;
	position.xy = ((position.xy + wWindVector * vertexFrame) % wSize) * Scale;
	output.Position3D = mul(position, xWorld);
    output.Position = mul(position, xWorldViewProjection);
	output.Normal.xy = (input.Position.xy - ((wWindVector * vertexFrame)%wSize)/Scale) / wSize; // stocke les coordonnées de la normale à utiliser (pas la normale direct).
	output.TextureCoord = input.TextureCoord;
	output.ReflectionMapPos = mul(position, wWorldReflectionViewProjection);
    return output;
}
/* -----------------------------------------------------------------
 * Pixel shader
 * Applique les effets d'éclairage et de brouillard ainsi que de texture.
 * ---------------------------------------------------------------*/
float4 BasicDraw(VertexShaderOutput input) : COLOR0
{
	float4 waterColor = float4(0, 0.4, 1, 0);
	return waterColor;
}
/* -----------------------------------------------------------------
 * Pixel shader
 * Applique les effets d'éclairage et de brouillard ainsi que de texture.
 * ---------------------------------------------------------------*/
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 texCoords = (input.TextureCoord);
	float4 waterColor = float4(0, 0.4, 1, 1);//+tex2D(water, texCoords)*0.3;
	float4 normal = normalize(tex2D(normalMap, input.Normal.xy));

	float fresnelTerm = dot(normalize(input.Position3D-xCameraPos), float3(0, 0, 1));

	// Couleur de la réflection
    float2 ProjectedTexCoords;
    ProjectedTexCoords.x = input.ReflectionMapPos.x/input.ReflectionMapPos.w/2.0f + 0.5f;
    ProjectedTexCoords.y = -input.ReflectionMapPos.y/input.ReflectionMapPos.w/2.0f + 0.5f;    
    float4 reflectionColor = tex2D(reflection, ProjectedTexCoords);
	
	// Détermine la distance à la caméra et en fait une valeur l entre 0 et 1.
	// Cela permet de donner un effet de brouillard quand on est loin.
	float dst = distance(xCameraPos, input.Position3D);
	float l = saturate((dst-xFogNear)/(xFogFar-xFogNear));

	// Specular
	float3 reflectionVector = -reflect(LightDirection, normal);
	float specular = dot(normalize(reflectionVector), normalize(CameraDirection));
	specular = pow(abs(specular), 16);

	// Distance au centre de la lumière
	float dstCenter = 1-saturate(dot(normalize(LightPosition-input.Position3D), normalize(LightPosition-xCameraPos)));//saturate(dot(normalize(input.Position3D-xCameraPos), normalize(-LightDirection)));
	dstCenter *= 20000;
	dstCenter = saturate(1 - dstCenter);
	specular *= (dstCenter+0.1);
	waterColor.rgb += specular;

	// Effet de réflection.
	//reflectionColor = lerp(reflectionColor, waterColor, distance(input.ReflectionMapPos.xy, float2(0.5, 0.5))/120);
	waterColor = lerp(reflectionColor, waterColor, saturate(fresnelTerm*1.25));

	// Application de l'éclairage (pixel)
	float factor = 1-saturate(-DotProduct(LightPosition, input.Position3D, normal));
	factor *= 1;
	waterColor.rgb = waterColor.rgb + float3(1, 1, 1) * (factor);
	// Effet de pseudo-réfraction
	float alpha = 0.5+waterColor.g/3.0f;

	// Brouillard
	float4 texColor = waterColor;
	texColor.rgb = lerp(texColor, xFogColor, l).rgb;
	texColor.a = lerp(alpha, 0, saturate(l-0.8)*5);

	return texColor;
}

technique ReflectedWater
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

technique BasicWater
{
	pass Pass1
	{
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 BasicDraw();
	}
}