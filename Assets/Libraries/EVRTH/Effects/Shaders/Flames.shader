// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Flames" 
{
    Properties 
	{
        _Mask ("Ember Mask", 2D) = "black" {}
        _FlameMask ("Flame Mask", 2D) = "white" {}
        _FlamesDistortion ("FlamesDistortion", 2D) = "black" {}
    }
    SubShader 
	{
        Tags 
		{
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 200
        Pass 
		{
            Name "FORWARD"
            Tags 
			{
                "LightMode"="ForwardBase"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers xbox360 ps3 
            #pragma target 3.0
            uniform sampler2D _Mask; uniform float4 _Mask_ST;
            uniform sampler2D _FlameMask; uniform float4 _FlameMask_ST;
            uniform sampler2D _FlamesDistortion; uniform float4 _FlamesDistortion_ST;

            struct VertexInput 
			{
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput 
			{
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) 
			{
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR 
			{
				float distortionUVOffset = tex2D(_FlamesDistortion,TRANSFORM_TEX((((1.5*i.uv0) + i.vertexColor.g) + _Time.y*float2(0.67,-0.157)), _FlamesDistortion)).g*0.05;
                float2 flameMaskUVA = ((((0.5*i.uv0)+i.vertexColor.g)+ _Time.y*float2(0.475,0.18))+ distortionUVOffset);
                float flameValueA = tex2D(_FlameMask,TRANSFORM_TEX(flameMaskUVA, _FlameMask)).g;

				distortionUVOffset = tex2D(_FlamesDistortion,TRANSFORM_TEX(((i.vertexColor.g + i.uv0) + _Time.y*float2(0.38, 0.0741)), _FlamesDistortion)).g*0.05;
                float2 flameMaskUVB = (((i.vertexColor.g+(0.25*i.uv0))+ _Time.y*float2(0.127,-0.053))+ distortionUVOffset);
                float flameValueB = tex2D(_FlameMask,TRANSFORM_TEX(flameMaskUVB, _FlameMask)).g;

                float2 flameSwirls = lerp(0.3,1,float2(flameValueA,flameValueB)).rg;
                float emberMask = saturate(((tex2D(_Mask, TRANSFORM_TEX(i.uv0, _Mask)).g*saturate(lerp(-1,3,(flameValueA+flameValueB))))*i.vertexColor.a*2.0) - 0.5);
    			return fixed4((lerp(float3(1, 0.2339833, 0), float3(.25, .079, 0), (flameSwirls.r*flameSwirls.g))*emberMask), 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
