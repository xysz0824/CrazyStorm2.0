#define PS_SHADERMODEL ps_4_0

#define DECLARE_TEXTURE(Name, index, address) \
    Texture2D Name : register(t##index); \
    SamplerState Name##Sampler : register(s##index) { \
    AddressU = address; \
    AddressV = address; \
    AddressW = address; };

#define SAMPLE_TEXTURE(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)

DECLARE_TEXTURE(Texture, 0, Wrap)
DECLARE_TEXTURE(MaskTexture, 1, Clamp)
DECLARE_TEXTURE(DistortTexture, 2, Clamp)

#define MASK_COUNT 8

uniform float4 MaskRect[MASK_COUNT];
uniform float4 MaskInfo[MASK_COUNT];
uniform float4 MaskInv[MASK_COUNT];
uniform float4 MaskFrameRect[MASK_COUNT];
uniform float4 MaskDissolve[MASK_COUNT];
uniform int MaskCount;
uniform float2 RenderCenter;
uniform float2 ViewportSize;
uniform float4 CurveMaskFrameRect;
uniform float4 CurveSourceRect;
uniform float4 CurveMaskDissolve;
uniform float4 CurveTypeInfo;
uniform float4 CurveDistortFrameRect;
uniform float4 CurveDistort;

float WrapUnit(float value)
{
    return frac(frac(value) + 1);
}

float EvaluateMaskJudge(float2 pos, int index)
{
    float4 maskRect = MaskRect[index];
    float4 maskInfo = MaskInfo[index];
    float4 maskInv = MaskInv[index];
    float2 maskPosition = maskRect.xy;
    float2 rotateTrig = maskInfo.zw;
    float2 maskSize = maskRect.zw;
    float maskShape = maskInfo.y;
    float2 ellipseInvSizeSq = maskInv.xy;
    float2 maskInvDiameter = maskInv.zw;
    float4 frameRect = MaskFrameRect[index];
    float4 maskDissolve = MaskDissolve[index];
    float dissolveStrength = maskDissolve.x;
    float dissolveEdgeWidth = maskDissolve.y;
    float2 maskAnimateOffset = maskDissolve.zw;

    float2 maskD = pos - maskPosition;
    maskD = float2(maskD.x * rotateTrig.x + maskD.y * rotateTrig.y,
                   -maskD.x * rotateTrig.y + maskD.y * rotateTrig.x);
    float shapeJudge = 0;
    if (maskShape == 0)
    {
        shapeJudge = min(1, smoothstep(maskSize.x - 1, maskSize.x + 1, maskD.x) +
                            smoothstep(maskD.x - 1, maskD.x + 1, -maskSize.x) +
                            smoothstep(maskSize.y - 1, maskSize.y + 1, maskD.y) +
                            smoothstep(maskD.y - 1, maskD.y + 1, -maskSize.y));
    }
    else if (maskShape == 1)
    {
        float2 maskDSq = maskD * maskD;
        float ellipseDistanceSq = maskDSq.x * ellipseInvSizeSq.x + maskDSq.y * ellipseInvSizeSq.y;
        shapeJudge = smoothstep((1 - 0.01) * (1 - 0.01), (1 + 0.01) * (1 + 0.01), ellipseDistanceSq);
    }
    float shapeMask = lerp(1, 0, shapeJudge);
    if (shapeMask <= 0)
    {
        return shapeMask;
    }

    if (frameRect.z <= 0 || frameRect.w <= 0)
    {
        return shapeMask;
    }

    float2 localUv = maskD * maskInvDiameter + 0.5;
    if (localUv.x < 0 || localUv.x > 1 || localUv.y < 0 || localUv.y > 1)
    {
        return 0;
    }

    float2 animateUv = float2(
        WrapUnit(localUv.x + maskAnimateOffset.x),
        WrapUnit(localUv.y + maskAnimateOffset.y));
    float2 maskUv = frameRect.xy + animateUv * frameRect.zw;
    float2 maskOffset = SAMPLE_TEXTURE(MaskTexture, maskUv).rg * 2 - 1;
    float2 displacedLocalUv = localUv + maskOffset;
    if (displacedLocalUv.x < 0 || displacedLocalUv.x > 1 || displacedLocalUv.y < 0 || displacedLocalUv.y > 1)
    {
        return 0;
    }

    float2 displacedMaskUv = frameRect.xy + displacedLocalUv * frameRect.zw;
    float maskValue = SAMPLE_TEXTURE(MaskTexture, displacedMaskUv).r;
    float dissolveJudge = 0;
    if (dissolveEdgeWidth <= 0)
    {
        dissolveJudge = maskValue > dissolveStrength ? 1 : 0;
    }
    else
    {
        float halfEdge = max(fwidth(maskValue) * dissolveEdgeWidth * 0.5, 1e-5);
        dissolveJudge = smoothstep(dissolveStrength - halfEdge,
                                   dissolveStrength + halfEdge,
                                   maskValue);
    }
    return shapeMask * dissolveJudge;
}
float EvaluateParticleMask(float2 texCoord, float4 sourceRect, float4 maskFrameRect,
    float dissolveStrength, float dissolveEdgeWidth, float2 maskAnimateOffset)
{
    if (sourceRect.z <= 0 || sourceRect.w <= 0)
    {
        return 1;
    }

    float2 localUv = (texCoord - sourceRect.xy) / sourceRect.zw;
    if (localUv.x < 0 || localUv.x > 1 || localUv.y < 0 || localUv.y > 1)
    {
        return 1;
    }

    float2 animateUv = float2(
        WrapUnit(localUv.x + maskAnimateOffset.x),
        WrapUnit(localUv.y + maskAnimateOffset.y));
    float2 maskUv = maskFrameRect.xy + animateUv * maskFrameRect.zw;
    float2 maskOffset = SAMPLE_TEXTURE(MaskTexture, maskUv).rg * 2 - 1;
    float2 displacedLocalUv = localUv + maskOffset;
    if (displacedLocalUv.x < 0 || displacedLocalUv.x > 1 || displacedLocalUv.y < 0 || displacedLocalUv.y > 1)
    {
        return 0;
    }

    float2 displacedMaskUv = maskFrameRect.xy + displacedLocalUv * maskFrameRect.zw;
    float maskValue = SAMPLE_TEXTURE(MaskTexture, displacedMaskUv).r;
    if (dissolveEdgeWidth <= 0)
    {
        return maskValue > dissolveStrength ? 1 : 0;
    }

    float halfEdge = max(fwidth(maskValue) * dissolveEdgeWidth * 0.5, 1e-5);
    return smoothstep(dissolveStrength - halfEdge,
                      dissolveStrength + halfEdge,
                      maskValue);
}
float2 ApplyParticleDistort(float2 texCoord, float4 sourceRect, float4 distortFrameRect,
    float distortStrength, float2 distortAnimateOffset)
{
    if (sourceRect.z <= 0 || sourceRect.w <= 0)
    {
        return texCoord;
    }

    float2 localUv = (texCoord - sourceRect.xy) / sourceRect.zw;
    if (localUv.x < 0 || localUv.x > 1 || localUv.y < 0 || localUv.y > 1)
    {
        return texCoord;
    }

    float2 distortLocalUv = float2(
        WrapUnit(localUv.x + distortAnimateOffset.x),
        WrapUnit(localUv.y + distortAnimateOffset.y));
    float2 distortUv = distortFrameRect.xy + distortLocalUv * distortFrameRect.zw;
    float2 distortSample = SAMPLE_TEXTURE(DistortTexture, distortUv).rg * 2 - 1;
    float2 distortedLocalUv = localUv + distortSample * distortStrength;
    return sourceRect.xy + distortedLocalUv * sourceRect.zw;
}

struct ParticleInstancedVSInput
{
    float2 QuadPosition : POSITION0;
    float2 QuadTexCoord : TEXCOORD0;
    float4 InstanceTransform0 : TEXCOORD1;
    float4 InstanceTransform1 : TEXCOORD2;
    float4 InstanceSourceRect : TEXCOORD3;
    float4 InstanceColor : TEXCOORD4;
    float4 InstanceRotationFlip : TEXCOORD5;
    float4 InstanceMaskFrameRect : TEXCOORD6;
    float4 InstanceDistortFrameRect : TEXCOORD7;
    float4 InstanceMaskAndDistort : TEXCOORD8;
    float4 InstanceAnimateOffsets : TEXCOORD9;
    float4 InstanceTypeInfo : TEXCOORD10;
};

struct ParticleInstancedPSInput
{
    float4 Position : SV_Position;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float4 SourceRect : TEXCOORD1;
    float4 MaskFrameRect : TEXCOORD2;
    float4 DistortFrameRect : TEXCOORD3;
    float4 MaskAndDistort : TEXCOORD4;
    float4 AnimateOffsets : TEXCOORD5;
    float4 TypeInfo : TEXCOORD6;
};

struct CurveBatchVSInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct CurveBatchPSInput
{
    float4 Position : SV_Position;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

CurveBatchPSInput CurveBatchVS(CurveBatchVSInput input)
{
    CurveBatchPSInput output;
    output.Position = float4(
        input.Position.x / ViewportSize.x * 2 - 1,
        1 - input.Position.y / ViewportSize.y * 2,
        input.Position.z,
        input.Position.w);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

ParticleInstancedPSInput ParticleInstancedVS(ParticleInstancedVSInput input)
{
    ParticleInstancedPSInput output;
    float2 instancePosition = input.InstanceTransform0.xy;
    float2 instanceOrigin = input.InstanceTransform0.zw;
    float2 instanceSize = input.InstanceTransform1.xy;
    float2 instanceScale = input.InstanceTransform1.zw;
    float2 localPos = input.QuadPosition * instanceSize - instanceOrigin;
    localPos *= instanceScale;
    float2 rotatedPos = float2(
        localPos.x * input.InstanceRotationFlip.x - localPos.y * input.InstanceRotationFlip.y,
        localPos.x * input.InstanceRotationFlip.y + localPos.y * input.InstanceRotationFlip.x);
    float2 worldPos = instancePosition + rotatedPos;
    output.Position = float4(
        worldPos.x / ViewportSize.x * 2 - 1,
        1 - worldPos.y / ViewportSize.y * 2,
        0,
        1);
    float2 flippedTexCoord = float2(
        lerp(input.QuadTexCoord.x, 1 - input.QuadTexCoord.x, input.InstanceRotationFlip.z),
        lerp(input.QuadTexCoord.y, 1 - input.QuadTexCoord.y, input.InstanceRotationFlip.w));
    output.TexCoord = input.InstanceSourceRect.xy + flippedTexCoord * input.InstanceSourceRect.zw;
    output.Color = input.InstanceColor;
    output.SourceRect = input.InstanceSourceRect;
    output.MaskFrameRect = input.InstanceMaskFrameRect;
    output.DistortFrameRect = input.InstanceDistortFrameRect;
    output.MaskAndDistort = input.InstanceMaskAndDistort;
    output.AnimateOffsets = input.InstanceAnimateOffsets;
    output.TypeInfo = input.InstanceTypeInfo;
    return output;
}

float Median3(float3 value)
{
    return max(min(value.r, value.g), min(max(value.r, value.g), value.b));
}

float ComputeTextAlpha(float4 sampleValue)
{
    float signedDistance = max(Median3(sampleValue.rgb), sampleValue.a) - 0.5;
    float width = max(fwidth(signedDistance) * 0.8, 1e-5);
    return saturate(signedDistance / width + 0.5);
}

float EvaluateLayerMaskAlpha(float2 position)
{
    float result = lerp(1, lerp(0, 1, MaskInfo[0].x - 1), min(1, MaskInfo[0].x));
    float2 pos = position - RenderCenter;
    for (int i = 0; i < MaskCount; ++i)
    {
        float judge = EvaluateMaskJudge(pos, i);
        result = lerp(result, lerp(result + judge, result * (1 - judge), MaskInfo[i].x - 1), min(1, MaskInfo[i].x));
    }
    return saturate(result);
}

float4 ApplyCurveColor(CurveBatchPSInput input, bool useMask, bool useDistort) : COLOR
{
    float2 texCoord = input.TexCoord;
    if (useDistort)
    {
        texCoord = ApplyParticleDistort(
            texCoord,
            CurveSourceRect,
            CurveDistortFrameRect,
            CurveDistort.x,
            CurveDistort.yz);
    }

    float4 sampled = SAMPLE_TEXTURE(Texture, texCoord);
    float4 Color = sampled * input.Color;
    if (CurveTypeInfo.x > 0.5)
    {
        Color = float4(input.Color.rgb, input.Color.a * ComputeTextAlpha(sampled));
    }
    float alpha = EvaluateLayerMaskAlpha(input.Position.xy);
    if (useMask)
    {
        alpha *= EvaluateParticleMask(
            input.TexCoord,
            CurveSourceRect,
            CurveMaskFrameRect,
            CurveMaskDissolve.x,
            CurveMaskDissolve.y,
            CurveMaskDissolve.zw);
    }

    Color.a *= alpha;
    return Color;
}

float4 CurveBatchTexturedPS(CurveBatchPSInput input) : COLOR
{
    return ApplyCurveColor(input, false, false);
}

float4 CurveBatchTexturedMaskPS(CurveBatchPSInput input) : COLOR
{
    return ApplyCurveColor(input, true, false);
}

float4 CurveBatchTexturedDistortPS(CurveBatchPSInput input) : COLOR
{
    return ApplyCurveColor(input, false, true);
}

float4 CurveBatchTexturedMaskDistortPS(CurveBatchPSInput input) : COLOR
{
    return ApplyCurveColor(input, true, true);
}

float4 ApplyParticleColor(ParticleInstancedPSInput input, bool useMask, bool useDistort) : COLOR
{
    float2 texCoord = input.TexCoord;
    if (useDistort)
    {
        texCoord = ApplyParticleDistort(
            texCoord,
            input.SourceRect,
            input.DistortFrameRect,
            input.MaskAndDistort.z,
            input.AnimateOffsets.zw);
    }

    float4 sampled = SAMPLE_TEXTURE(Texture, texCoord);
    float4 Color = sampled * input.Color;
    if (input.TypeInfo.x > 0.5)
    {
        Color = float4(input.Color.rgb, input.Color.a * ComputeTextAlpha(sampled));
    }
    float alpha = EvaluateLayerMaskAlpha(input.Position.xy);
    if (useMask)
    {
        alpha *= EvaluateParticleMask(
            input.TexCoord,
            input.SourceRect,
            input.MaskFrameRect,
            input.MaskAndDistort.x,
            input.MaskAndDistort.y,
            input.AnimateOffsets.xy);
    }

    Color.a *= alpha;
    return Color;
}

float4 ParticleInstancedTexturedPS(ParticleInstancedPSInput input) : COLOR
{
    return ApplyParticleColor(input, false, false);
}

float4 ParticleInstancedTexturedMaskPS(ParticleInstancedPSInput input) : COLOR
{
    return ApplyParticleColor(input, true, false);
}

float4 ParticleInstancedTexturedDistortPS(ParticleInstancedPSInput input) : COLOR
{
    return ApplyParticleColor(input, false, true);
}

float4 ParticleInstancedTexturedMaskDistortPS(ParticleInstancedPSInput input) : COLOR
{
    return ApplyParticleColor(input, true, true);
}

technique CurveTextured
{
    pass P0
    {
        VertexShader = compile vs_4_0 CurveBatchVS();
        PixelShader = compile PS_SHADERMODEL CurveBatchTexturedPS();
    }
}

technique CurveTexturedMask
{
    pass P0
    {
        VertexShader = compile vs_4_0 CurveBatchVS();
        PixelShader = compile PS_SHADERMODEL CurveBatchTexturedMaskPS();
    }
}

technique CurveTexturedDistort
{
    pass P0
    {
        VertexShader = compile vs_4_0 CurveBatchVS();
        PixelShader = compile PS_SHADERMODEL CurveBatchTexturedDistortPS();
    }
}

technique CurveTexturedMaskDistort
{
    pass P0
    {
        VertexShader = compile vs_4_0 CurveBatchVS();
        PixelShader = compile PS_SHADERMODEL CurveBatchTexturedMaskDistortPS();
    }
}

technique BatchTexturedInstanced
{
    pass P0
    {
        VertexShader = compile vs_4_0 ParticleInstancedVS();
        PixelShader = compile PS_SHADERMODEL ParticleInstancedTexturedPS();
    }
}

technique BatchTexturedMaskInstanced
{
    pass P0
    {
        VertexShader = compile vs_4_0 ParticleInstancedVS();
        PixelShader = compile PS_SHADERMODEL ParticleInstancedTexturedMaskPS();
    }
}

technique BatchTexturedDistortInstanced
{
    pass P0
    {
        VertexShader = compile vs_4_0 ParticleInstancedVS();
        PixelShader = compile PS_SHADERMODEL ParticleInstancedTexturedDistortPS();
    }
}

technique BatchTexturedMaskDistortInstanced
{
    pass P0
    {
        VertexShader = compile vs_4_0 ParticleInstancedVS();
        PixelShader = compile PS_SHADERMODEL ParticleInstancedTexturedMaskDistortPS();
    }
}
