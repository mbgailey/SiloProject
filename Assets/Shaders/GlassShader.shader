Shader "MyShaders/Glass-Seethrough" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
        _Shininess ("Shininess", Range (0.03, 1)) = 0.3
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _DistAmt  ("Distortion", range (0,128)) = 10
    }
    SubShader {
        GrabPass { }
       
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Opaque" }
        LOD 200
       
        CGPROGRAM
        #pragma exclude_renderers gles
        #pragma vertex vert
        #pragma surface surf BlinnPhong alpha
        #include "UnityCG.cginc"
 
        float4 _Color;
        float _Shininess;
        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _GrabTexture;
        float _DistAmt;
        float4 _GrabTexture_TexelSize;
 
        struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float4 proj : TEXCOORD;
        };
       
        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
			float4 oPos = mul(UNITY_MATRIX_MVP, v.vertex);
            #if UNITY_UV_STARTS_AT_TOP
                float scale = -1.0;
            #else
                float scale = 1.0;
            #endif
            o.proj.xy = (float2(oPos.x, oPos.y*scale) + oPos.w) * 0.5;
            o.proj.zw = oPos.zw;
        }
 
        void surf (Input IN, inout SurfaceOutput o) {
            half3 nor = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
           
            float2 offset = nor * _DistAmt * _GrabTexture_TexelSize.xy;
            IN.proj.xy = offset * IN.proj.z + IN.proj.xy;
            half4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(IN.proj));
           
            half4 tex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = tex.rgb * _Color.rgb * col.rgb;
            o.Normal = nor.rgb;
            o.Specular = _Shininess;
            o.Gloss = col.a;
            o.Alpha = col.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}