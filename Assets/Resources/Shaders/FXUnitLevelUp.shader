Shader "Custom/FX/Unit Level Up"
{
    Properties
    {
        _BottomColor ("Bottom Color", Color) = (1, 1, 1, 1)
        _TopColor ("Top Color", Color) = (1, 1, 1, 1)
        _CurrentTime ("Current time", Range(0, 1)) = 0.0
        _Loops ("Loops", Float) = 2.0
        _LoopCompression ("Loop Compression", Float) = 4
    }
    SubShader
    {
        Tags {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Cull Off
            ZWrite Off
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _BottomColor;
            float4 _TopColor;
            float _CurrentTime;
            float _Loops;
            float _LoopCompression;

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD1;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float InverseLerp( float a, float b, float v )
            {
                return ( v - a ) / ( b - a );
            }

            float4 frag (Interpolators i) : SV_Target
            {
                // get spiral mask

                float lineMask = 0;
                float timeOffset = _Time.y;
                for(int n=0; n<_Loops; n++)
                {
                    float compression = _LoopCompression;
                    lineMask += saturate( 1 - compression * abs ( i.uv.x - ( i.uv.y * _Loops - n ) ) );
                }

                // remap vertical range to [0, 0.85] to get fade earlier

                float tColor = InverseLerp( 0, 0.9, i.uv.y );

                // lerp according to (remapped) vertical position

                float4 lineColor = lerp ( _BottomColor, _TopColor, tColor );
                float4 lineFinal = lerp( float4(0, 0, 0, 0), lineColor, lineMask );

                // apply reveal over time
                float reveal = saturate( 1 - abs ( 1 - i.uv.y * _Loops + _Loops * ( 2.0 * _CurrentTime - 1) ) );

                return lineFinal * reveal;
            }
            ENDCG
        }
    }
}
