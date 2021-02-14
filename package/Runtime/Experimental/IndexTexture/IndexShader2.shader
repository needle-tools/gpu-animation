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
	struct v2f
	{
		half4 dest0 : SV_Target0;
		float dest1 : SV_Target1;
	};

	void frag(
		v2f i,
		out float GRT0:SV_Target0,
		out float GRT1:SV_Target1,
		out float GRTDepth:SV_Depth)
	{
		// float4 col = tex2D(_MainTex, i.uv); 
		GRT0 = 0;
		GRT1 = 1;
		GRTDepth = 1;
	}
	ENDCG

	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			ENDCG
		}
	}
}