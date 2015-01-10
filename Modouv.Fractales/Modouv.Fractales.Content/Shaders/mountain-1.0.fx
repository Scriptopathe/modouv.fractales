float4x4 World;
float4x4 View;
float4x4 Projection;


// Diffuse
float3 DiffuseDirection = float3(0.75, 0.45, 0.75);
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseIntensity = 0.5;
float ColorIntensity = 0.7;

texture MountainTexture;
texture GrassTexture;
texture IceTexture;
sampler moutain = sampler_state {
   texture = (MountainTexture);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Wrap; // Address Mode for U Coordinates
   AddressV = Wrap; // Address Mode for V Coordinates
};
sampler ice = sampler_state {
   texture = (IceTexture);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Wrap; // Address Mode for U Coordinates
   AddressV = Wrap; // Address Mode for V Coordinates
};
sampler grass = sampler_state {
   texture = (GrassTexture);
   MinFilter = Linear; // Minification Filter
   MagFilter = Linear; // Magnification Filter
   MipFilter = Linear; // Mip-mapping
   AddressU = Wrap; // Address Mode for U Coordinates
   AddressV = Wrap; // Address Mode for V Coordinates
};

struct VertexShaderInput
{
    float4 Position : POSITION0;		// Passée par notre Vertex
	float4 TextureCoord : TEXCOORD0;	// Passée par notre Vertex
	float3 Normal		: NORMAL0;		// Passée par notre Vertex
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;		// Position finale de dessin du vertex
	float3 Normal : TEXCOORD1;			// Normale finale du vertex.
	float4 Outpos : TEXCOORD2;			// 
	float4 HeightmapPos : TEXCOORD3;	// Position du vertex dans la heightmap.
	float3 TextureCoord : TEXCOORD0;	// Coordonée texture (0 à 1 sur chaque composante)
};


VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	output.HeightmapPos = input.Position;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	float3 normal = normalize(mul(input.Normal, World));
    output.Outpos = normalize(input.Position);
	output.Normal = normal;
	output.TextureCoord = input.TextureCoord;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float texZoom = 10;
	float4 mountainColor = tex2D(moutain, input.TextureCoord*texZoom);
	float4 grassColor =   tex2D(grass, input.TextureCoord*texZoom);
	float4 iceColor = tex2D(ice, input.TextureCoord*texZoom);
	//float z = (sin(input.HeightmapPos.x/50000.0f) + sin(input.HeightmapPos.y/50000.0f))/40.0f + input.HeightmapPos.z/20000.0f ;
	float z = input.HeightmapPos.z/4000.0f ;
	// Obtient une couleur en fonction de l'altitude du point :
	// Si on est bas : prends la couleur de l'herbe, et en haut, la montagne.
	float4 texColor;
	if(z > 0.9)
	{
		texColor = iceColor;
	}
	else if(z > 0.8)
	{
		float pcIce = (0.9-z)*10;
		texColor = mountainColor*pcIce + iceColor*(1-pcIce);
	}
	else if(z > 0.6)
	{
		texColor = mountainColor;
	}
	else if(z > 0.5)
	{
		float pcGrass = (0.6-z)*10;
		texColor = grassColor*pcGrass + mountainColor*(1-pcGrass);
	}
	else
	{
		texColor = grassColor;
	}

    float4 norm = float4(normalize(input.Normal), 1.0);
    float4 diffuse = -0.5+saturate(dot(DiffuseDirection, norm));
	
	
	return DiffuseIntensity*DiffuseColor*diffuse+ColorIntensity*texColor;
}

technique Ambient
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}