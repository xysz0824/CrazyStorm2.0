#define PS_SHADERMODEL ps_4_0

#define DECLARE_TEXTURE(Name, index, address) \
    texture2D Name; \
    sampler Name##Sampler : register(s##index) = sampler_state { Texture = (Name); \
    AddressU = address; \
    AddressV = address; \
    AddressW = address; };

#define SAMPLE_TEXTURE(Name, texCoord)  tex2D(Name##Sampler, texCoord)

DECLARE_TEXTURE(Texture, 0, Wrap)

#define MASK_COUNT 8

uniform float2 MaskSize[MASK_COUNT];
uniform float2 MaskPosition[MASK_COUNT];
uniform float MaskType[MASK_COUNT];
uniform float MaskShape[MASK_COUNT];
uniform float MaskRotate[MASK_COUNT];
uniform int MaskCount;
uniform float2 RenderCenter;

technique PostProcess
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL mainPS();
    }
}

float4 mainPS(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR
{
    float4 Color = SAMPLE_TEXTURE(Texture, texCoord) * color;
    float result = lerp(1, lerp(0, 1, MaskType[0] - 1), min(1, MaskType[0]));
    float2 pos = position.xy - RenderCenter;
    for (int i = 0; i < MaskCount; ++i)
    {
        float2 maskD = (pos - MaskPosition[i].xy);
        maskD = float2(maskD.x * cos(MaskRotate[i]) + maskD.y * sin(MaskRotate[i]),
                       -maskD.x * sin(MaskRotate[i]) + maskD.y * cos(MaskRotate[i]));
        float rectJudge = min(1, smoothstep(MaskSize[i].x - 1, MaskSize[i].x + 1, maskD.x) + 
                                 smoothstep(maskD.x - 1, maskD.x + 1, -MaskSize[i].x) + 
                                 smoothstep(MaskSize[i].y - 1, MaskSize[i].y + 1, maskD.y) + 
                                 smoothstep(maskD.y - 1, maskD.y + 1, -MaskSize[i].y));
        float circleJudge = smoothstep(1 - 0.01, 1 + 0.01, sqrt(maskD.x * maskD.x / (MaskSize[i].x * MaskSize[i].x) + maskD.y * maskD.y / (MaskSize[i].y * MaskSize[i].y)));
        float judge = lerp(rectJudge, circleJudge, MaskShape[i]);
        judge = lerp(1, 0, judge);
        result = lerp(result, lerp(result + judge, result * (1 - judge), MaskType[i] - 1), min(1, MaskType[i]));
    }
    Color.a *= saturate(result);
    return Color;
}