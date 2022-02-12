#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

struct VertexShaderOutput
{
    float4 ScreenPos : SV_POSITION; // is eaten by GPU, so inaccessible in the pixel shader
    float2 RelToCenterPos : TEXCOORD0;
    float RadiusSquared : TEXCOORD1;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in float2 screenPos : POSITION0, in float2 relToCenterPos : TEXCOORD0, in float radiusSquared : TEXCOORD1, in float4 color : COLOR0)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    output.ScreenPos.xy = screenPos;
    output.ScreenPos.w = 1;
    output.RelToCenterPos = relToCenterPos;
    output.RadiusSquared = radiusSquared;
    output.Color = color;

	return output;
}

float4 MainPS(in float2 relToCenterPos : TEXCOORD0, in float radiusSquared : TEXCOORD1, in float4 color : COLOR0) : COLOR
{
    float4 finalColor;
    float2 squared = relToCenterPos * relToCenterPos;
    if (squared.x + squared.y <= radiusSquared)
        finalColor = color;
    else
        finalColor = 0; //1 - color;
    //finalColor.a = .25;
    return finalColor;
}

technique DiskDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};