Shader "PalyDisplay/Marker"
{
	Properties
	{
		_Color ("_Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Mode ("_Mode", Range(0.0, 1.0)) = 0.0
		_Marker ("_Marker", 2D) = "white" { }
	}

	Category
	{
		Tags
		{
			"RenderType" = "Opaque"
			"LightMode" = "PrePassFinal"
		}

		LOD 200
		ZWrite Off

		Fog
		{
			Color(0.0, 0.0, 0.0, 0.0)
		}

		SubShader
		{
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#pragma target 2.0

				#include "UnityCG.cginc"

				float4 _Color;
				float _Mode;

				sampler2D _Marker;
				float4 _Marker_ST;

				struct v2f
				{
					float4 pos : POSITION;
					float2 uv : TEXCOORD0;
				};


				v2f vert(appdata_base v)
				{
					v2f o;
					o.pos = mul(UNITY_MATRIX_MVP, v.vertex + float4(0.0, 0.0, 0.0, -0.1));
					o.uv = TRANSFORM_TEX(v.texcoord * float4(1.1, 1.1, 0.0, 0.0), _Marker) - float2(0.05, 0.05);
					return o;
				}

				fixed4 frag(v2f i) : COLOR
				{
					float check = saturate(i.uv).x - i.uv.x + saturate(i.uv).y - i.uv.y;
					if(check)
						return fixed4(_Color.r, _Color.g, _Color.b, 1.0);
					if(_Mode + check)
						discard;
					return tex2D(_Marker, i.uv) * 0.5;
				}

				ENDCG
			}
		}

	}
	
	FallBack "Unlit/Texture"
}
