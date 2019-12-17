// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/VertexColorAlpha" {
	Properties {
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_AlphaIntensity ("Alpha Intensity", Range(0,40)) = 1.0
		_ViewAngleIntensity ("View Angle Intensity", Range(0,1)) = 1.0
		_LightReactivity ("Light Reactivity", Range(0,1)) = 0.75
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 200
		
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert fullforwardshadows alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
			float3 viewDir;
			float3 worldNormal;
			float viewAngleStrength;
			float4 color : COLOR;
		};

		half _Glossiness;
		half _Metallic;
		half _AlphaIntensity;
		half _ViewAngleIntensity;
		half _LightReactivity;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_full v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.viewAngleStrength = dot(normalize(ObjSpaceViewDir(float4(0,0,0,0))), normalize(v.vertex));
			o.viewAngleStrength = saturate(o.viewAngleStrength*o.viewAngleStrength*o.viewAngleStrength*o.viewAngleStrength*8);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			half viewAngleStrength = lerp(1, IN.viewAngleStrength, _ViewAngleIntensity);
			half alphaStrength = saturate(IN.color.a*_AlphaIntensity)*viewAngleStrength;

			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = IN.color.rgb*_LightReactivity;
			o.Emission = IN.color.rgb*(1 - _LightReactivity)*alphaStrength;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = alphaStrength;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
