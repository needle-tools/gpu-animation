Shader "Custom/IndexTextureMRT"
{
    Properties
    {
        _MainTex("", 2D) = "white"{}
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D _SecondTex;
    sampler2D _ThirdTex;

    // MRT shader
    struct FragmentOutput
    {
        half4 dest0 : SV_Target0;
        float dest1 : SV_Target1;
    };

    FragmentOutput frag_mrt(v2f_img i) : SV_Target
    {
        FragmentOutput o;
        o.dest0 = frac(float4(i.uv, 0, 0));
        o.dest1 = 100;
        return o;
    }
    ENDCG

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_mrt
            ENDCG
        }
    }
}
