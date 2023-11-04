#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;
float4 Center;
float Radius;
float LightAmount;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
	float2 RelWorldPos : TEXCOORD0;
	float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.RelWorldPos = input.Position.xy - Center.xy;
	output.Color = input.Color;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 relPosSquared = input.RelWorldPos * input.RelWorldPos;
    float dist = sqrt(relPosSquared.x + relPosSquared.y);
    float factor = 1 / (1 + max(0, dist - Radius) / (LightAmount * 10000));
    return input.Color * factor;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};