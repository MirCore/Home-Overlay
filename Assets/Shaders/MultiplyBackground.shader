Shader "VisionUI/MultiplyBackground"
{
    Properties
    {
        [NoScaleOffset] _Background ("Background Cubemap", Cube) = "white" {}
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)

        _MultColor("Multiply color", Color) = (1,1,1,0.2980392)
        _BaseColorValue("Base Color Value", Float) = 1

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Stencil
            {
                Ref[_Stencil]
                Comp[_StencilComp]
                Pass[_StencilOp]
                ReadMask[_StencilReadMask]
                WriteMask[_StencilWriteMask]
            }

            Cull Off
            Lighting Off
            ZWrite Off
            ZTest [unity_GUIZTestMode]
            ColorMask RGBA

            Pass
            {
                BlendOp Add, Max
                Blend SrcAlpha OneMinusSrcAlpha, One One

                Name "MultiplyLayer"
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
                    fixed4 color : COLOR;
                    float2 texcoord  : TEXCOORD0;
                    float4 worldPosition : TEXCOORD1;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                samplerCUBE _Background;
                sampler2D _MainTex;
                fixed4 _Color;
                fixed4 _TextureSampleAdd;
                fixed4 _StaticColor;
                float4 _ClipRect;
                float4 _MainTex_ST;
                fixed4 _MultColor;
                float _BaseColorValue;
                uniform float _staticValue;

                v2f vert(appdata_t v)
                {
                    v2f OUT;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                    OUT.worldPosition = v.vertex;
                    OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                    OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
    
                    OUT.color = lerp(_MultColor * half4(1, 1, 1, v.color.a), v.color * _Color * _MultColor, _BaseColorValue);
                    return OUT;
                }

                fixed4 frag(v2f IN) : SV_Target
                {
                    half4 color = lerp(texCUBE(_Background, -WorldSpaceViewDir(IN.worldPosition)), _StaticColor, _staticValue) * IN.color;
                    half4 sprite = tex2D(_MainTex, IN.texcoord);
                    color = lerp(half4(1, 1, 1, 1), color, color.a);
                    color.a *= sprite.a * IN.color.a;
    
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