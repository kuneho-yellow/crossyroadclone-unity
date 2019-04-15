/******************************************************************************
*  @file       Screenshot.shader
*  @brief      Shader for displaying only part of the screenshot taken (for
*				sharing on Twitter) on the screenshot UI
*  @author     Ron
*  @date       October 11, 2015
*
*  @par [explanation]
*		> Basically the built-in shader Sprites/Default with additional
*			parameters for center, width, and height, specifying the sprite
*			area that should be displayed
*		> Center, width, and height are in normalized local texture coordinates
*		> The default values, center at 0.5, 0.5, width and height = 1 mean
*			that the entire sprite is shown
******************************************************************************/

Shader "Sprites/Screenshot"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_Center("Center", Vector) = (0.5, 0.5, 0, 0)
		_Width("Width", float) = 1
		_Height("Height", float) = 1
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest LEqual
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color	: COLOR;
				half2 texcoord  : TEXCOORD0;
			};

			fixed4 _Color;
			float4 _Center;
			float _Width;
			float _Height;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;

			fixed4 SampleSpriteTexture(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);
				if (_AlphaSplitEnabled)
					color.a = tex2D(_AlphaTex, uv).r;

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				// Hide pixels outside the specified rectangle
				if ((IN.texcoord.x > _Center.x + _Width * 0.5) ||
					(IN.texcoord.x < _Center.x - _Width * 0.5) ||
					(IN.texcoord.y > _Center.y + _Height * 0.5) ||
					(IN.texcoord.y < _Center.y - _Height * 0.5))
				{
					IN.color.a = 0;
				}

				fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}