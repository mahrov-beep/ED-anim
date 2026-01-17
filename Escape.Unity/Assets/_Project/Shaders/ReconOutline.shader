Shader "Custom/ReconOutline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0,1,0,1)
        _OutlineWidth("Outline Width", Range(0.0,0.1)) = 0.008
        [HDR]_EmissionColor("Emission Color", Color) = (0,1,0,1)
    }
    SubShader
    {
        Tags { "Queue"="Overlay+1" "RenderType"="Transparent" }

        Pass
        {
            Name "ReconOutlineThroughWalls"
            Cull Front
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform float4 _OutlineColor;
            uniform float _OutlineWidth;
            uniform float4 _EmissionColor;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float4 screenPosOrig : TEXCOORD0;
                float rawDepth : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float4 clipPos = UnityObjectToClipPos(v.vertex);
                float4 offsetClipPos = clipPos;
                float3 normalView = normalize(mul((float3x3)UNITY_MATRIX_MV, v.normal));
                offsetClipPos.xy += normalView.xy * _OutlineWidth * offsetClipPos.w;
                offsetClipPos.z  += normalView.z  * _OutlineWidth * offsetClipPos.w;

                o.pos = offsetClipPos;
                o.screenPosOrig = ComputeScreenPos(clipPos);
                o.rawDepth = clipPos.z / clipPos.w;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float rawSceneDepth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPosOrig));
                float rawMyDepth = i.rawDepth;

                if (rawSceneDepth <= rawMyDepth + 0.0005f)
                {
                    discard;
                }

                fixed4 col = _OutlineColor + _EmissionColor;
                col.a = 0.7;
                return col;
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}
