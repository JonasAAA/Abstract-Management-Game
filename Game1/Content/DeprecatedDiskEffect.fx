#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;
float2 Center;
float Radius;
float4 Color;

struct VertexShaderOutput
{
	// is eaten by GPU, so need a copy of it if want to do computations on it.
	float4 Position : SV_POSITION;
	float2 WorldPosition : TEXCOORD0;
};

VertexShaderOutput MainVS(in float4 worldPosition : POSITION0)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(worldPosition, WorldViewProjection);
    output.WorldPosition = worldPosition;

	return output;
}

float4 MainPS(in float2 worldPosition : TEXCOORD0) : COLOR
{
    float2 squared = (worldPosition - Center) * (worldPosition - Center);
	if (squared.x + squared.y < Radius * Radius)
		return Color;
	else
		return 0;
}

technique DiskDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
