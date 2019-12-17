Shader "Custom/FadeableGlobeTileNoStencil"
{
	Properties
	{
		_Blend("Blend", Range(0, 1)) = 0.5
        _Overlay1Blend("Overlay 1 Blend", Range(0, 1)) = 1
        _Overlay2Blend("Overlay 2 Blend", Range(0, 1)) = 1

        _OldTex("Old Texture", 2D) = ""
		_NewTex("New Texture", 2D) = ""
        
        _Overlay1("Overlay 1", 2D) = "black"
        _OldOverlay1("Old Overlay 1", 2D) = "black"
        
        _Overlay2("Overlay 2", 2D) = "black"
        _OldOverlay2("Old Overlay 2", 2D) = "black"
	}

        
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		CGPROGRAM
#pragma surface surf Lambert
		struct Input
	    {
		    float2 uv_NewTex;
			float2 uv2_Overlay1;
			float2 uv3_Overlay2;
	    };


	sampler2D _OldTex;
	sampler2D _NewTex;

    sampler2D _Overlay1;
    sampler2D _OldOverlay1;

    sampler2D _Overlay2;
    sampler2D _OldOverlay2;

	float _Blend;
    float _Overlay1Blend;
    float _Overlay2Blend;

    void surf(Input IN, inout SurfaceOutput o)
	{
		float3 t1 = tex2D(_OldTex, IN.uv_NewTex).rgb;        
        float3 t2 = tex2D(_NewTex, IN.uv_NewTex).rgb;
		o.Albedo = lerp(t1, t2, _Blend);

        float4 ot1 = tex2D(_OldOverlay1, IN.uv2_Overlay1).rgba;
        float4 ot2 = tex2D(_Overlay1, IN.uv2_Overlay1).rgba;
        float4 overlay1 = lerp(ot1, ot2, _Overlay1Blend);

        ot1 = tex2D(_OldOverlay2, IN.uv3_Overlay2).rgba;
        ot2 = tex2D(_Overlay2, IN.uv3_Overlay2).rgba;
        float4 overlay2 = lerp(ot1, ot2, _Overlay2Blend);

        // Use the overlay alpha to select between the overlay texture and the base texture
        o.Albedo = lerp(o.Albedo, overlay1, overlay1.a);
        o.Albedo = lerp(o.Albedo, overlay2, overlay2.a);
	}
	ENDCG
	}
		//Fallback "Diffuse"
}
