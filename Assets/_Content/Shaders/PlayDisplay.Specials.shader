Shader "PalyDisplay/Specials"
{
	Properties
	{
        _Atlas ("_Atlas", 2D) = "white" {}
		_Index ("_Index", Range(0.0, 35.0)) = 0.0
	}
	
	Category
	{
		Tags
		{
			"RenderType" = "Opaque"
			"LightMode" = "PrePassFinal"
		}

		Fog
		{
			Color(0.0, 0.0, 0.0, 0.0)
		}

		LOD 200
		ColorMask RGB
		Cull Off
		Lighting Off
		ZWrite On

		SubShader
		{
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#pragma target 2.0

				#include "UnityCG.cginc"

				float _Index;
		
				sampler2D _Atlas;
				float4 _Atlas_ST;

				struct v2f
				{
					float4 pos : POSITION;
					float2 uv : TEXCOORD0;
				};

				v2f vert (appdata_base v)
				{
					v2f o;
					o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
					float id = floor(_Index);
					float vid = floor(_Index / 6.0);
					float uid = _Index - vid * 6.0;
					o.uv = TRANSFORM_TEX(v.texcoord * float4(1.0 / 6.0, 1.0 / 6.0, 0.0, 0.0), _Atlas) + float2(uid / 6.0, vid / 6.0);
					return o;
				}

				half4 frag(v2f i) : COLOR
				{
					return tex2D (_Atlas, i.uv);
				}

				ENDCG
			}
		}
	}

	FallBack "Unlit/Texture"
}
