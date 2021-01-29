Shader "Custom/IndexTexture"
{
	Properties
	{
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Id ("Id", int) = 1
	}
	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
			"RenderQueue" = "Transparent"
		}

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows //finalcolor:mycolor
		#pragma target 4.5


        sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
			float4 screenPos;
            float eyeDepth;
			half4 dest1 : SV_Target1;
		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			const float4 clipPos = UnityObjectToClipPos(v.vertex);
			o.screenPos = ComputeScreenPos(clipPos);
            COMPUTE_EYEDEPTH(o.eyeDepth);
		}


		#if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)
		uniform RWTexture2D<float> _IdTexture : register(u4);
		#endif
		float _Id;

		sampler2D_float _CameraDepthTexture;
        float4 _CameraDepthTexture_TexelSize;

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float2 coords = IN.screenPos.xy / IN.screenPos.w;
			
			#if defined(SHADER_API_D3D11) || defined(SHADER_API_METAL)
			uint idw, idh;
			_IdTexture.GetDimensions(idw, idh);
			const int2 pixel = coords * float2(idw, idh);
			float2 values = _IdTexture[pixel];
			float _DepthNear = 5;
			float _DepthFar = 10;
            float dist = IN.eyeDepth;// (clamp(IN.eyeDepth, _DepthNear, _DepthFar) - _DepthNear) / (_DepthFar - _DepthNear);
			// float rawZ = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
			// float sceneZ = 1 - LinearEyeDepth(rawZ) / 5;
			// if(dist > values.y)
			{
				values.x = _Id / 10.;
				values.y = dist;
				_IdTexture[pixel] = values;
			}
			#endif

			o.Emission.xy = coords*0.00000001;
			o.Albedo.xy = tex2D(_MainTex, IN.uv_MainTex) * _Id / 10.0;// _Id / 20.0;
		}
		ENDCG
	}
	FallBack "Diffuse"
}