Shader "Custom/MobileOutline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Width", Range(0.0,0.1)) = 0.005
        [HDR]_EmissionColor("Emission Color", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Opaque" }
        Pass
        {
            Cull Front
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform float4 _OutlineColor;
            uniform float _OutlineWidth;
            uniform float4 _EmissionColor;

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 pos : SV_POSITION; };

            v2f vert(appdata v)
            {
                float4 clipPos = UnityObjectToClipPos(v.vertex);
                float3 normalView = normalize(mul((float3x3)UNITY_MATRIX_MV, v.normal));
                clipPos.xy += normalView.xy * _OutlineWidth * clipPos.w;
                v2f o; o.pos = clipPos; return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor + _EmissionColor;
            }
            ENDCG
        }
    }
}
