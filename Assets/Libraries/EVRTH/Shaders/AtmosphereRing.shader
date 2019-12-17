Shader "Atmosphere Ring"
{
	Properties
	{
		_MainTex ("Atmosphere Gradient", 2D) = "white" {}
		_EdgeMax ("Edge Max", Float) = 1.0
		_EdgeMin("Edge Min", Float) = 0.0
		_MinBackGlow("Minimum Reverse Glow", Float) = 0.7
		_ReferenceValue("Stencil Reference Value", Int) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent-1" }
		Stencil
		{
			Ref[_ReferenceValue]
			Comp Equal
			Pass Keep
			Fail Keep
		}

		LOD 100

		Blend SrcAlpha OneMinusSrcAlpha
		Zwrite off


		Pass
		{
			Cull Back
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
	

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float2 offset : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _EdgeMax;
			float _EdgeMin;
			float _MinBackGlow;
			v2f vert (appdata v)
			{
				v2f o;
				float4 screenVertex = UnityObjectToClipPos(v.vertex);
				o.vertex = screenVertex;

				// This needs to change to take the camera angle into account
				float4 centerVertex = UnityObjectToClipPos(float3(0, 0, 0));

				float3 worldNormal = UnityObjectToWorldNormal(normalize(v.vertex));
				
				// We might want to do this step in the pixel shader but our simple globe mesh is high enough res that it tends not to be a problem
				o.offset = float2(dot(normalize(ObjSpaceViewDir(v.vertex)), normalize(v.vertex)), 1.0 - (dot(worldNormal, -_WorldSpaceLightPos0.xyz) + 1)*.5);


				#if UNITY_SINGLE_PASS_STEREO
					half4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
					half aspectRatio = (_ScreenParams.x*scaleOffset.x) / (_ScreenParams.y*scaleOffset.y);
				#else
					half aspectRatio = _ScreenParams.x / _ScreenParams.y;
				#endif

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float offset = saturate(saturate(1.0 - i.offset.x) - _EdgeMin) / (_EdgeMax - _EdgeMin);
			
				fixed4 col = tex2D(_MainTex, float2(offset, max(i.offset.y, step(1.0 - offset, .5)*_MinBackGlow)));
				//fixed4 col = fixed4(offset, offset, offset, 1.0);

				return col;
			}
			ENDCG
		}
	}
}
