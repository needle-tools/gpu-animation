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
		#pragma target 4.5

		struct Input
		{
			float2 uv_MainTex;
			float4 screenPos;
		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			float4 clipPos = UnityObjectToClipPos(v.vertex);
			o.screenPos = ComputeScreenPos(clipPos);
		}


		#if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)
		uniform RWTexture2D<float> _MyTex : register(u4);
		#endif


		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float2 coords = IN.screenPos.xy / IN.screenPos.w;

			
			#if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)
			int2 px = coords * 100; // 100 is texture size
			_MyTex[px] = 1 + _Time.y;
			#endif


			o.Albedo.xy = coords;
		}
		ENDCG
	}
	FallBack "Diffuse"
}