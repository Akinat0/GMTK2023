Shader "PlayQuantum/PostProcess/Blit_CRT"
{
	Properties
	{
		_MainTex("MainTex", Color) = (1,1,1,1)
		_BordersCS("Effect borders: x = top. y = bottom, ClipSpace", Vector) = (1, 0.6, 0, 0)
		_FrameColor("TV frame color", Color) = (0,0,0,0)
		_FrameCorners("Frame corners rounding radius", Range(0, 0.5)) = 0.1
		_SpherizeStrength("Spherize", Range(0, 5)) = 0.1
		_ScanlineDensity("Scanline lines density", float) = 300
		_ScanlineStrength("Scanline strength", Range(0, 1)) = 1
		_SubpixelStripes("Subpixel Stripes count", float) = 60
		_SubpixelStrength("Subpixel strength", Range(0, 1)) = 1
		_Brightness("Brightness", Range(0, 5)) = 1
	}
        SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "ColorBlitPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionHCS   : POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                float2  uv          : TEXCOORD0;
            };
            
            sampler2D _MainTex;
            half2 _BordersCS;
            half3 _FrameColor;
            half _FrameCorners;
            half _SpherizeStrength;
            half _ScanlineDensity;
            half _ScanlineStrength;
            half _SubpixelStripes;
            half _SubpixelStrength;
            half _Brightness;

            float2 Spherize(float2 UV, float2 Offset, float2 Center, float Strength)
            {
            	float2 delta = Offset - Center;
            	float delta2 = dot(delta.xy, delta.xy);
            	float delta4 = delta2 * delta2;
            	float2 delta_offset = delta4 * Strength;
            	return  UV + delta * delta_offset;
            }

            float RemapTo01(float value, float from1, float to1)
            {
            	return (value - from1) / (to1 - from1);
            }
            
            float RoundedRectangle(float2 UV, float Radius)
            {
            	Radius = max(min(min(abs(Radius * 2), 1), 1), 1e-5);
            	float2 uv = abs(UV * 2 - 1) - float2(1, 1) + Radius;
            	float d = length(max(0, uv)) / Radius;
            	return  saturate((1 - d) / fwidth(d));
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
            	
                output.positionCS = TransformObjectToHClip(input.positionHCS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
            	half spherizeUVY = RemapTo01(input.uv.y, _BordersCS.y, _BordersCS.x);
            	half2 croppedUV = float2(input.uv.x, spherizeUVY);
            	half2 center = float2(0.5, 0.5);
            	half2 targetUV = Spherize(input.uv, croppedUV, center, _SpherizeStrength);

            	half2 edgesUV = float2(targetUV.x, RemapTo01(targetUV.y, _BordersCS.y, _BordersCS.x));
            	half edges = RoundedRectangle(edgesUV, _FrameCorners);

            	half lines = _ScanlineDensity * (_BordersCS.x - _BordersCS.y);
            	half scanline = saturate(abs(frac(edgesUV.y * lines) - 0.5) + 1 - _ScanlineStrength);

            	half subpixelMod = fmod(_SubpixelStripes * edgesUV.x, 3);
            	half subR = step(subpixelMod, 2);
            	half subB = step(subpixelMod, 1);
            	half subG = subR - subB;
            	half3 subpixelStripes = float3(1 - subR, subG, subB) * _SubpixelStrength + (_Brightness - 1);

            	half3 frame = _FrameColor * (1 - edges);
            	
            	float4 color = float4(tex2D(_MainTex, targetUV) * subpixelStripes * edges * scanline + frame, 1);
            	
                return color;
            }
            ENDHLSL
        }
    }
}