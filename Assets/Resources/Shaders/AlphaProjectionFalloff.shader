Shader "Custom/Alpha Projection Falloff"
{
	Properties
	{
		_FullTexture ("Full Texture", 2D) = "white" {}
		_SemiTexture ("Semi Texture", 2D) = "white" {}
		_SemiOpacity ("Semi Opacity", float) = 0.5
		_FalloffTexture ("Falloff Texture", 2D) = "white" {}
		_Color ("Color", Color) = (0, 0, 0, 0)
	}
	SubShader
	{
		Tags { "Queue"="Transparent+100" } // to cover other transparent non-z-write things
 
		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Equal
 
			CGPROGRAM
			// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
			#pragma exclude_renderers d3d11 gles
			#pragma vertex vert
			#pragma fragment frag
 
			#include "UnityCG.cginc"
 
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 uvShadow : TEXCOORD0;
				float4 uvFalloff : TEXCOORD1;
			};
 
 
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;

			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(vertex);
				o.uvShadow = mul(unity_Projector, vertex);
				o.uvFalloff = mul(unity_ProjectorClip, vertex);
				return o;
			}

			fixed4 _Color;
			sampler2D _FullTexture;
			sampler2D _SemiTexture;
			sampler2D _FalloffTexture;
			float _SemiOpacity;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 texFull = tex2Dproj (_FullTexture, UNITY_PROJ_COORD(i.uvShadow));
				fixed4 texSemi = tex2Dproj (_SemiTexture, UNITY_PROJ_COORD(i.uvShadow));
				texFull.rgb *= _Color.rgb;
				texFull.a = 1.0 - max(texFull.a, _SemiOpacity * texSemi.a);
	
				fixed4 texF = tex2Dproj (_FalloffTexture, UNITY_PROJ_COORD(i.uvFalloff));
				fixed4 res = texFull * texF.a;

				return res;


				//float4 UV = i.uv;

				//float aFull = tex2Dproj(_FullTexture, UV).a;
				//float aSemi = tex2Dproj(_SemiTexture, UV).a;

				//float a = aFull + _SemiOpacity * aSemi;

				// weird things happen to minimap if alpha value gets negative
				//_Color.a = max(0, _Color.a - a);
				//return _Color;
			}
			ENDCG
		}
	}
}
