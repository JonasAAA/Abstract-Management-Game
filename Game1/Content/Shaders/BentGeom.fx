#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define SHELL_RADIUS 20
#define MAX_POLYS 8187
#define TWO_PI 6.283185

matrix WorldViewProjection;
float4 Colors[MAX_POLYS];

struct VertexShaderInput
{
    float4 Position : POSITION0;
    uint ShellNum : TEXCOORD1;
    uint Index : TEXCOORD2;
    //// in [-pi, pi]
    float AngleA : TEXCOORD3;
    float AngleAB : TEXCOORD4;
    float AngleAC : TEXCOORD5;
    float AngleAD : TEXCOORD6;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 WorldPos : TEXCOORD0;
    uint ShellNum : TEXCOORD1;
    float4 Color : TEXCOORD2;
    float AngleA : TEXCOORD3;
    float AngleAB : TEXCOORD4;
    float AngleAC : TEXCOORD5;
    float AngleAD : TEXCOORD6;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.WorldPos = input.Position.xy;
    output.ShellNum = input.ShellNum;
    output.AngleA = input.AngleA;
    output.AngleAB = input.AngleAB;
    output.AngleAC = input.AngleAC;
    output.AngleAD = input.AngleAD;
    output.Color = Colors[input.Index];

	return output;
}

float Length(float2 vector2)
{
    float2 diffSquared = vector2 * vector2;
    return sqrt(diffSquared.x + diffSquared.y);
}

bool IsBetween(float value, float endPoint1, float endPoint2)
{
    return (endPoint1 <= value) == (value <= endPoint2);
}

float4 MainPS(in VertexShaderOutput input) : COLOR
{
    float shellProp = Length(input.WorldPos) / SHELL_RADIUS - input.ShellNum;
    
    if (shellProp < 0 || shellProp > 1)
        return 0;
    
    float minRelAngle = input.AngleAB * shellProp,
        maxRelAngle = input.AngleAD * (1 - shellProp) + input.AngleAC * shellProp,
        relAngle = atan2(input.WorldPos.y, input.WorldPos.x) - input.AngleA;
    if (IsBetween(relAngle - TWO_PI, minRelAngle, maxRelAngle)
        || IsBetween(relAngle, minRelAngle, maxRelAngle)
        || IsBetween(relAngle + TWO_PI, minRelAngle, maxRelAngle))
        return input.Color;
    return 0;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};