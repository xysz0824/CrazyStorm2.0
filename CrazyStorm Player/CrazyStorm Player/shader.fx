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
DECLARE_TEXTURE(DistortTexture, 2, Clamp)

#define MASK_COUNT 8

uniform float2 MaskSize[MASK_COUNT];
uniform float2 MaskPosition[MASK_COUNT];
uniform float MaskLayer[MASK_COUNT];
uniform float MaskShape[MASK_COUNT];
uniform float2 MaskRotateTrig[MASK_COUNT];
uniform float2 MaskEllipseInvSizeSq[MASK_COUNT];
uniform float2 MaskInvDiameter[MASK_COUNT];
uniform float4 MaskFrameRect[MASK_COUNT];
uniform float MaskDissolveStrength[MASK_COUNT];
uniform float MaskDissolveEdgeWidth[MASK_COUNT];
uniform float2 MaskAnimateOffset[MASK_COUNT];
uniform float MaskTextureEnabled[MASK_COUNT];
uniform int MaskCount;
uniform float2 RenderCenter;
uniform float4 ParticleMaskFrameRect;
uniform float4 ParticleSourceRect;
uniform float ParticleMaskDissolveStrength;
uniform float ParticleMaskDissolveEdgeWidth;
uniform float2 ParticleMaskAnimateOffset;
uniform float ParticleMaskTextureEnabled;
uniform float4 ParticleDistortFrameRect;
uniform float ParticleDistortStrength;
uniform float2 ParticleDistortAnimateOffset;
uniform float ParticleDistortTextureEnabled;

float WrapUnit(float value)
{
    return frac(frac(value) + 1);
}

float EvaluateMaskJudge(float2 pos, int index)
{
    float2 maskPosition = MaskPosition[index].xy;
    float2 rotateTrig = MaskRotateTrig[index];
    float2 maskSize = MaskSize[index];
    float maskShape = MaskShape[index];
    float2 ellipseInvSizeSq = MaskEllipseInvSizeSq[index];
    float2 maskInvDiameter = MaskInvDiameter[index];
    float4 frameRect = MaskFrameRect[index];
    float dissolveStrength = MaskDissolveStrength[index];
    float dissolveEdgeWidth = MaskDissolveEdgeWidth[index];
    float2 maskAnimateOffset = MaskAnimateOffset[index];
    float maskTextureEnabled = MaskTextureEnabled[index];

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
    if (shapeMask <= 0 || maskTextureEnabled == 0)
    {
        return shapeMask;
    }

    float2 localUv = maskD * maskInvDiameter + 0.5;
    if (localUv.x < 0 || localUv.x > 1 || localUv.y < 0 || localUv.y > 1)
    {
        return 0;
    }

    if (frameRect.z <= 0 || frameRect.w <= 0)
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
float EvaluateParticleMask(float2 texCoord)
{
    if (ParticleMaskTextureEnabled == 0 || ParticleSourceRect.z <= 0 || ParticleSourceRect.w <= 0)
    {
        return 1;
    }

    float2 localUv = (texCoord - ParticleSourceRect.xy) / ParticleSourceRect.zw;
    if (localUv.x < 0 || localUv.x > 1 || localUv.y < 0 || localUv.y > 1)
    {
        return 1;
    }

    if (ParticleMaskFrameRect.z <= 0 || ParticleMaskFrameRect.w <= 0)
    {
        return 1;
    }

    float2 animateUv = float2(
        WrapUnit(localUv.x + ParticleMaskAnimateOffset.x),
        WrapUnit(localUv.y + ParticleMaskAnimateOffset.y));
    float2 maskUv = ParticleMaskFrameRect.xy + animateUv * ParticleMaskFrameRect.zw;
    float2 maskOffset = SAMPLE_TEXTURE(MaskTexture, maskUv).rg * 2 - 1;
    float2 displacedLocalUv = localUv + maskOffset;
    if (displacedLocalUv.x < 0 || displacedLocalUv.x > 1 || displacedLocalUv.y < 0 || displacedLocalUv.y > 1)
    {
        return 0;
    }

    float2 displacedMaskUv = ParticleMaskFrameRect.xy + displacedLocalUv * ParticleMaskFrameRect.zw;
    float maskValue = SAMPLE_TEXTURE(MaskTexture, displacedMaskUv).r;
    if (ParticleMaskDissolveEdgeWidth <= 0)
    {
        return maskValue > ParticleMaskDissolveStrength ? 1 : 0;
    }

    float halfEdge = max(fwidth(maskValue) * ParticleMaskDissolveEdgeWidth * 0.5, 1e-5);
    return smoothstep(ParticleMaskDissolveStrength - halfEdge,
                      ParticleMaskDissolveStrength + halfEdge,
                      maskValue);
}
float2 ApplyParticleDistort(float2 texCoord)
{
    if (ParticleDistortTextureEnabled == 0 || ParticleSourceRect.z <= 0 || ParticleSourceRect.w <= 0)
    {
        return texCoord;
    }

    float2 localUv = (texCoord - ParticleSourceRect.xy) / ParticleSourceRect.zw;
    if (localUv.x < 0 || localUv.x > 1 || localUv.y < 0 || localUv.y > 1)
    {
        return texCoord;
    }

    if (ParticleDistortFrameRect.z <= 0 || ParticleDistortFrameRect.w <= 0 || ParticleDistortStrength == 0)
    {
        return texCoord;
    }

    float2 distortLocalUv = float2(
        WrapUnit(localUv.x + ParticleDistortAnimateOffset.x),
        WrapUnit(localUv.y + ParticleDistortAnimateOffset.y));
    float2 distortUv = ParticleDistortFrameRect.xy + distortLocalUv * ParticleDistortFrameRect.zw;
    float2 distortSample = SAMPLE_TEXTURE(DistortTexture, distortUv).rg * 2 - 1;
    float2 distortedLocalUv = localUv + distortSample * ParticleDistortStrength;
    return ParticleSourceRect.xy + distortedLocalUv * ParticleSourceRect.zw;
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
    float4 Color = SAMPLE_TEXTURE(Texture, ApplyParticleDistort(texCoord)) * color;
    float result = lerp(1, lerp(0, 1, MaskLayer[0] - 1), min(1, MaskLayer[0]));
    float2 pos = position.xy - RenderCenter;
    for (int i = 0; i < MaskCount; ++i)
    {
        float judge = EvaluateMaskJudge(pos, i);
        result = lerp(result, lerp(result + judge, result * (1 - judge), MaskLayer[i] - 1), min(1, MaskLayer[i]));
    }
    Color.a *= saturate(result * EvaluateParticleMask(texCoord));
    return Color;
}
