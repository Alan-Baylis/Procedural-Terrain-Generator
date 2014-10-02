Shader "Custom/ExtrudeShader"
{
	Properties
	{
		_Color ("MainColor", Color) = (1, 1, 1, 1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Extrusion ("Extrusion", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }
		LOD 200
		
		Pass
		{
			ZWrite On
			ZTest On
		
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			uniform sampler2D _MainTex;
			uniform float _Extrusion;
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			
			void vert(	in float4 iPos : POSITION,
						in float2 iUv : TEXCOORD0,
						in float3 iNormal : NORMAL,
						
						out float4 oPosition : SV_POSITION,
						out float2 oUv : TEXCOORD0,
						out float3 oNormal : TEXCOORD1)
			{
				float4 worldPosition = mul(_Object2World, iPos);
				float3 viewVector = normalize(_WorldSpaceCameraPos - worldPosition.xyz);
				worldPosition = worldPosition + float4(_Extrusion * viewVector, 0);
				oPosition = mul(UNITY_MATRIX_VP, worldPosition);
				
				oUv = iUv;
				oNormal = mul((float3x3)_Object2World, iNormal);
			}
			
			half4 frag(	in float2 uv : TEXCOORD0,
						in float3 normal : TEXCOORD1) : COLOR
			{
				float diffuse = dot(normal, _WorldSpaceLightPos0.xyz);
				return tex2D(_MainTex, uv) * (UNITY_LIGHTMODEL_AMBIENT * 2 + _LightColor0 * diffuse * 2);
			}
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
