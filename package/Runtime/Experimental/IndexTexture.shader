Shader "Custom/IndexTexture"
{
	Properties
	{
	}
	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
		}

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 5.0
		

		#if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)
		uniform RWTexture2D<float4> _MyTex : register(u4); 
		#endif


		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			#if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)
			for (int x = 0; x < 5; x++)
				_MyTex[int2(x,x)] = 1;// .5 + sin(_Time.y) * .5 + .5;
			#endif

			o.Albedo = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}