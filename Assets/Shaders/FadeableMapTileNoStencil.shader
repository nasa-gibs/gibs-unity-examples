Shader "Custom/FadeableMapTileNoStencil"
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

        _SpyPos("SpyPos", Vector) = (0, 0, 0, 0)
        _Radius("Radius", Float) = 0.05
        _Toggle("Toggle", int) = 1
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
            float3 worldPos;
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

    float3 _SpyPos;
    float _Radius;
    int _Toggle;

    void surf(Input IN, inout SurfaceOutput o)
	{
        float d = distance(_SpyPos, IN.worldPos);
		float3 t1 = tex2D(_OldTex, IN.uv_NewTex).rgb;        
        float3 t2 = tex2D(_NewTex, IN.uv_NewTex).rgb;
        float4 ot1 = tex2D(_OldOverlay1, IN.uv2_Overlay1).rgba;
        float4 ot2 = tex2D(_Overlay1, IN.uv2_Overlay1).rgba;
        float4 ot3 = tex2D(_OldOverlay2, IN.uv3_Overlay2).rgba;
        float4 ot4 = tex2D(_Overlay2, IN.uv3_Overlay2).rgba;
        float4 overlay1;
        float4 overlay2;

        if(d < _Radius && _Toggle == 1){
            o.Albedo = t1;
            overlay1 = ot1;
            overlay2 = ot3;
        }
        else{
            o.Albedo = lerp(0, t2, _Blend);
            overlay1 = lerp(0, ot2, _Overlay1Blend);
            overlay2 = lerp(0, ot4, _Overlay2Blend);
        }

        // Use the overlay alpha to select between the overlay texture and the base texture
        o.Albedo = lerp(o.Albedo, overlay1, overlay1.a);
        o.Albedo = lerp(o.Albedo, overlay2, overlay2.a);
	}
	ENDCG
	}
		//Fallback "Diffuse"
}
