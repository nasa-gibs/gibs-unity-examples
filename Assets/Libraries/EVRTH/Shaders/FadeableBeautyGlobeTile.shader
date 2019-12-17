Shader "Custom/FadeableBeautyGlobeTile"
{
	Properties
	{
		_NewTex("New Texture", 2D) = ""
		_Overlay1("Overlay 1", 2D) = "black"
		_Overlay2("Overlay 2", 2D) = "black"
	}

	CGINCLUDE
		float GetCloudValue(float cloudLayer, float baseLayer, float planetNormal)
		{
			// First we pull the clouds out of viirs
			// There are sun-streaks in the viirs data around the middle of the map
			// So we use the globe normal to calculate a value of how much we are in this zone
			// In that zone, we will make our filters more strict to pull the streaks out.
			float streakStrength = 1.0 - saturate((abs(planetNormal) - 0.2) * 2);
			float capPlug = saturate(abs(planetNormal) - .991) * 128;

			// The clouds are separated in two passes
			// First we get the intense clouds with a strict filter on the blue channel of viirs data
			const float cloudHighPass = 0.7;
			float cloudValue = max(saturate((cloudLayer - cloudHighPass) / (1.0 - cloudHighPass)), capPlug);

			// Then we apply a more forgiving filter for 'haze'
			// This is what we need to gate with the streak filter or we pick those up as haze as well
			float cloudLowPass = lerp(0.0, 0.3, streakStrength);
			float hazeValue = saturate(((cloudLayer - baseLayer) - cloudLowPass));
			return max(cloudValue, hazeValue);
		}
	ENDCG

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		// Beauty mode
		// We assume viirs is on layer 0
		// Blue marble is layer 1
		// Citylights is layer 2

		Pass
		{

			ZWrite on
			Blend One OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 data : TEXCOORD1;
			};
			sampler2D _NewTex;
			float4 _NewTex_ST;

			sampler2D _Overlay1;
			float4 _Overlay1_ST;

			sampler2D _Overlay2;
			float4 _Overlay2_ST;

			uniform float3 _SunDirection;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				// We get away with doing this in the vertex step purely because we are dealing with a high-res nearly spherical object
				o.data = float4(dot(worldNormal, _SunDirection), v.normal.z, sin(v.normal.x), 0);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				// Load the cloud layer from a few mips up to get slightly blurred cloud shadows
				// Pull the nighttime layer and bump it up
				// Blend in nighttime layer from light angle or shadow strength, whatever is greater
				float cloudValue = GetCloudValue(tex2Dbias(_NewTex, float4(TRANSFORM_TEX(i.uv, _NewTex).xy, 0, 1)).b, tex2Dbias(_Overlay1, float4(TRANSFORM_TEX(i.uv, _Overlay1).xy, 0, 1)).b, i.data.y);
				float4 dayLight = tex2D(_Overlay1, TRANSFORM_TEX(i.uv, _Overlay1));

				float4 nightLight = tex2D(_Overlay2, TRANSFORM_TEX(i.uv, _Overlay2).xy);
				nightLight *= nightLight;

				float lightBlend = 1.0 - saturate(i.data.x * 2);
				float4 finalColor = lerp(nightLight*((1.0 - cloudValue)*(.8 + .2)), dayLight*(1.0 - cloudValue), lightBlend);
				finalColor.a = 1.0;

				return finalColor;
				
				// Pull the water out of blue marble as well
				//float water = saturate(((newLayer1.b / max(newLayer1.g, 0.004)) - 1.0)*8.0);
			}
			ENDCG
		}
		Pass
		{

			ZWrite off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 data : TEXCOORD1;
			};
			sampler2D _NewTex;
			float4 _NewTex_ST;

			sampler2D _Overlay1;
			float4 _Overlay1_ST;

			sampler2D _Overlay2;
			float4 _Overlay2_ST;

			uniform float3 _SunDirection;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex*1.01);
				o.uv = v.uv;
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				// We get away with doing this in the vertex step purely because we are dealing with a high-res nearly spherical object
				o.data = float4(dot(worldNormal, _SunDirection), v.normal.z, sin(v.normal.x), v.color.r);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				// 1. Pull the cloud data out of viirs + blue marble
				// 2. Pull the bright lights out of the nighttime layer
				// We overlay this on top of the regular globe

				float cloudValue = GetCloudValue(tex2D(_NewTex, TRANSFORM_TEX(i.uv, _NewTex)).b, tex2D(_Overlay1, TRANSFORM_TEX(i.uv, _Overlay1)).b, i.data.y);
//				float4 nightLight = tex2Dbias(_Overlay2, float4(TRANSFORM_TEX(i.uv, _Overlay2).xy, 0, cloudValue));
				// We square the light layers to up their contrast
	//			nightLight *= nightLight;

				float lightBlend = 1.0 - saturate(i.data.x * 2);
				float4 finalColor = lerp(float4(0, 0, .021, 1), float4(1, 1, 1, 1), lightBlend);
				finalColor.a = i.data.w*cloudValue*(lightBlend*.5 + .5);

				return finalColor;
			}
			ENDCG
		}

	}
}
