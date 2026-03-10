#define PS_SHADERMODEL ps_4_0

#define DECLARE_TEXTURE(Name, index, address) \
    texture2D Name; \
    sampler Name##Sampler : register(s##index) = sampler_state { Texture = (Name); \
    AddressU = address; \
    AddressV = address; \
    AddressW = address; };

#define SAMPLE_TEXTURE(Name, texCoord)  tex2D(Name##Sampler, texCoord)

DECLARE_TEXTURE(Texture, 0, Wrap)
DECLARE_TEXTURE(MaskTexture, 1, Clamp)

#define MASK_COUNT 8

uniform float2 MaskSize[MASK_COUNT];
uniform float2 MaskPosition[MASK_COUNT];
uniform float MaskLayer[MASK_COUNT];
uniform float MaskShape[MASK_COUNT];
uniform float2 MaskRotateTrig[MASK_COUNT];
uniform float2 MaskEllipseInvSizeSq[MASK_COUNT];
uniform float4 MaskFrameRect[MASK_COUNT];
uniform float MaskDissolveStrength[MASK_COUNT];
uniform float MaskDissolveEdgeWidth[MASK_COUNT];
uniform float MaskTextureEnabled[MASK_COUNT];
uniform int MaskCount;
uniform float2 RenderCenter;
float EvaluateMaskJudge(float2 pos, int index)
{
    float2 maskD = pos - MaskPosition[index].xy;
    float2 rotateTrig = MaskRotateTrig[index];
    maskD = float2(maskD.x * rotateTrig.x + maskD.y * rotateTrig.y,
                   -maskD.x * rotateTrig.y + maskD.y * rotateTrig.x);
    float shapeJudge = 0;
    if (MaskShape[index] == 0)
    {
        shapeJudge = min(1, smoothstep(MaskSize[index].x - 1, MaskSize[index].x + 1, maskD.x) +
                            smoothstep(maskD.x - 1, maskD.x + 1, -MaskSize[index].x) +
                            smoothstep(MaskSize[index].y - 1, MaskSize[index].y + 1, maskD.y) +
                            smoothstep(maskD.y - 1, maskD.y + 1, -MaskSize[index].y));
    }
    else if (MaskShape[index] == 1)
    {
        float2 maskDSq = maskD * maskD;
        float ellipseDistance = sqrt(maskDSq.x * MaskEllipseInvSizeSq[index].x + maskDSq.y * MaskEllipseInvSizeSq[index].y);
        shapeJudge = smoothstep(1 - 0.01, 1 + 0.01, ellipseDistance);
    }
    float shapeMask = lerp(1, 0, shapeJudge);
    if (shapeMask <= 0 || MaskTextureEnabled[index] == 0)
    {
        return shapeMask;
    }

    float2 localUv = maskD / (MaskSize[index] * 2) + 0.5;
    if (localUv.x < 0 || localUv.x > 1 || localUv.y < 0 || localUv.y > 1)
    {
        return 0;
    }

    float4 frameRect = MaskFrameRect[index];
    float2 maskUv = frameRect.xy + localUv * frameRect.zw;
    float maskValue = SAMPLE_TEXTURE(MaskTexture, maskUv).r;
    float dissolveJudge = 0;
    if (MaskDissolveEdgeWidth[index] <= 0)
    {
        dissolveJudge = maskValue > MaskDissolveStrength[index] ? 1 : 0;
    }
    else
    {
        float halfEdge = max(fwidth(maskValue) * MaskDissolveEdgeWidth[index] * 0.5, 1e-5);
        dissolveJudge = smoothstep(MaskDissolveStrength[index] - halfEdge,
                                   MaskDissolveStrength[index] + halfEdge,
                                   maskValue);
    }
    return shapeMask * dissolveJudge;
}

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
    float result = lerp(1, lerp(0, 1, MaskLayer[0] - 1), min(1, MaskLayer[0]));
    float2 pos = position.xy - RenderCenter;
    for (int i = 0; i < MaskCount; ++i)
    {
        float judge = EvaluateMaskJudge(pos, i);
        result = lerp(result, lerp(result + judge, result * (1 - judge), MaskLayer[i] - 1), min(1, MaskLayer[i]));
    }
    Color.a *= saturate(result);
    return Color;
}
