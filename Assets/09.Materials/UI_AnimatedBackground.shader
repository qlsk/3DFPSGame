// 한 장짜리 배경 그림에서 마스크로 지정한 영역만 움직인다.
// 마스크 R: 천(펄럭임), G: 하늘(구름이 한 방향으로 흐름)
// UI/Default를 바탕으로 해서 Image의 색, 스텐실, 클리핑을 그대로 지원한다.
Shader "UI/AnimatedBackground"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [NoScaleOffset] _MaskTex ("Mask (R: Cloth, G: Sky)", 2D) = "black" {}

        [Header(Cloth)]
        _ClothAmplitude ("Cloth Amplitude (UV)", Float) = 0.004
        _ClothSpeed ("Cloth Speed", Float) = 2.2
        _ClothFrequency ("Cloth Frequency", Float) = 40

        [Header(Clouds)]
        _CloudDirection ("Cloud Direction (XY)", Vector) = (1, 0.1, 0, 0)
        _CloudDistance ("Cloud Distance (UV)", Float) = 0.03
        _CloudCycle ("Cloud Cycle (sec)", Float) = 12

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _ClothAmplitude;
            float _ClothSpeed;
            float _ClothFrequency;
            float4 _CloudDirection;
            float _CloudDistance;
            float _CloudCycle;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float2 mask = tex2D(_MaskTex, uv).rg;
                float t = _Time.y;

                // 1. 천: 서로 다른 속도의 사인파 두 개를 섞어 불규칙하게 좌우로 흔든다.
                float wave = sin(t * _ClothSpeed + uv.y * _ClothFrequency)
                           + 0.5 * sin(t * _ClothSpeed * 1.7 + uv.y * _ClothFrequency * 2.3 + uv.x * 20.0);
                float2 clothOffset = float2(wave, wave * 0.3) * _ClothAmplitude * mask.r;

                // 2. 구름: 같은 그림을 반 주기 어긋나게 두 번 흘려 보내고 교차시켜 끊김 없이 흐르게 한다.
                float2 direction = normalize(_CloudDirection.xy);
                float phase0 = frac(t / _CloudCycle);
                float phase1 = frac(t / _CloudCycle + 0.5);
                float2 flow = direction * _CloudDistance * mask.g;
                fixed4 sample0 = tex2D(_MainTex, uv + clothOffset - flow * phase0);
                fixed4 sample1 = tex2D(_MainTex, uv + clothOffset - flow * phase1);
                float weight0 = 1.0 - abs(1.0 - 2.0 * phase0);

                half4 color = (lerp(sample1, sample0, weight0) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}
