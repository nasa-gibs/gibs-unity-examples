// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:32719,y:32712,varname:node_3138,prsc:2|emission-8035-OUT,alpha-9912-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:32140,y:32766,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_7241,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.07843138,c2:0.3921569,c3:0.7843137,c4:1;n:type:ShaderForge.SFN_ViewPosition,id:3745,x:31528,y:33066,varname:node_3745,prsc:2;n:type:ShaderForge.SFN_FragmentPosition,id:651,x:31541,y:32860,varname:node_651,prsc:2;n:type:ShaderForge.SFN_Append,id:381,x:31732,y:32860,varname:node_381,prsc:2|A-651-X,B-651-Z;n:type:ShaderForge.SFN_Append,id:2086,x:31717,y:33112,varname:node_2086,prsc:2|A-3745-X,B-3745-Z;n:type:ShaderForge.SFN_Subtract,id:5114,x:31910,y:33006,varname:node_5114,prsc:2|A-381-OUT,B-2086-OUT;n:type:ShaderForge.SFN_Length,id:3720,x:32061,y:33006,varname:node_3720,prsc:2|IN-5114-OUT;n:type:ShaderForge.SFN_Tex2d,id:1605,x:32181,y:32526,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_1605,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:8035,x:32401,y:32693,varname:node_8035,prsc:2|A-1605-RGB,B-7241-RGB;n:type:ShaderForge.SFN_OneMinus,id:1517,x:32207,y:33006,varname:node_1517,prsc:2|IN-3720-OUT;n:type:ShaderForge.SFN_Multiply,id:9912,x:32496,y:32973,varname:node_9912,prsc:2|A-1605-A,B-2840-OUT;n:type:ShaderForge.SFN_Clamp01,id:2840,x:32362,y:33065,varname:node_2840,prsc:2|IN-1517-OUT;proporder:7241-1605;pass:END;sub:END;*/

Shader "Shader Forge/HexFloor" {
    Properties {
        _Color ("Color", Color) = (0.07843138,0.3921569,0.7843137,1)
        _MainTex ("MainTex", 2D) = "white" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _Color;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 emissive = (_MainTex_var.rgb*_Color.rgb);
                float3 finalColor = emissive;
                return fixed4(finalColor,(_MainTex_var.a*saturate((1.0 - length((float2(i.posWorld.r,i.posWorld.b)-float2(_WorldSpaceCameraPos.r,_WorldSpaceCameraPos.b)))))));
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
