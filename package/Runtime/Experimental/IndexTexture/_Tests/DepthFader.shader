Shader "Custom/DepthFade"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _DepthNear ("Depth Near", Range(0, 1000)) = 10
        _DepthFar ("Depth Far", Range(0, 1000)) = 200
        _DepthPower ("Depth Power", Range(1, 10)) = 2
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha        

        GrabPass { "_GrabTexture" } //We declare a GrabPass to get the rendered image as it was right before rendering the water (for the refraction effect )

        CGPROGRAM
        #pragma surface surf Standard vertex:vert fullforwardshadows alpha:fade
        #pragma target 4.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float eyeDepth;
            float4 screenPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        sampler2D _GrabTexture;
        half _DepthNear;
        half _DepthFar;
        half _DepthPower;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            COMPUTE_EYEDEPTH(o.eyeDepth);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

            // Change alpha by depth
            float2 screenUV = IN.screenPos.xy/IN.screenPos.w;
            float dist = (clamp(IN.eyeDepth, _DepthNear, _DepthFar) - _DepthNear) / (_DepthFar - _DepthNear);
            dist = 1 - pow(1 - dist * 0.99999, _DepthPower);
           
            o.Alpha = clamp(1 - dist, 0, 1);
        }
        ENDCG
    }
    FallBack "Diffuse"
}