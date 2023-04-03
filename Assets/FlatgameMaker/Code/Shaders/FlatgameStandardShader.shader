Shader "Flatgame/StandardShader"
{
    Properties
    {
        //Feature Toggles
        [Toggle(HSV_Shift)]
        _HSVShift("HSV shift enabled", Float) = 0
        [Toggle(Distortion)]
        _Distortion("Distortion enabled", Float) = 0
        [Toggle(Remap)]
        _Remap("Remap colours", Float) = 0
        [Toggle(UVOffset)]
        _Offset("Animated UV Offset", Float) = 0
		[Toggle(Clipping)]
		_Clipping("Transparency Cutoff", Float) = 0
        [Toggle(WorldSpaceMain)]
        _WorldSpaceMain("Tile in World Space", Float) = 0
		[Toggle(WorldSpaceDist)]
        _WorldSpaceDist("Tile Distortion in World Space", Float) = 0

        //Basic Properties
        _Color("Colour tint", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
		_AlphaCutoff("Transparency Cutoff Value", Range(0, 1.0)) = 0.5
        //HSV Values
        [HideIfDisabled(HSV_Shift)]
        _Hue("Hue", Range(-0.5, 0.5)) = 0
        [HideIfDisabled(HSV_Shift)]
        _Sat("Saturation", Range(-0.5, 0.5)) = 0
        [HideIfDisabled(HSV_Shift)]
        _Val("Value", Range(-0.5, 0.5)) = 0

        //Distortion Values
        [HideIfDisabled(Distortion)]
        _DistortionTex ("Distortion Normal Map", 2D) = "white" {}
        [HideIfDisabled(Distortion)]
        _DistortionVals("Distrortion (speed x, y, amount x, y)", Vector) = (0,0,0,0)
        [HideIfDisabled(Distortion)]
        _DistortionFPS("Distortion FPS",Float) = 0

        //Remap Values
        [HideIfDisabled(Remap)]
        _BlackColor("Black Colour Remap", Color) = (0,0,0,1)
        [HideIfDisabled(Remap)]
        _WhiteColor("White Colour Remap", Color) = (1,1,1,1)
        [HideIfDisabled(Remap)]
        _BlVal("Black Value", Range(0, 1.0)) = 0
        [HideIfDisabled(Remap)]
        _WhVal("White Value", Range(0, 1.0)) = 1

        //UV Offset Values
        [HideIfDisabled(UVOffset)]
        _OffsetSpeedX("Horizontal UV Offset Speed",Float) = 1
        [HideIfDisabled(UVOffset)]
        _OffsetSpeedY("Vertical UV Offset Speed",Float) = 1
        [HideIfDisabled(UVOffset)]
        _OffsetFPS("UV Offset FPS",Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma shader_feature HSV_Shift
            #pragma shader_feature Distortion
            #pragma shader_feature Remap
            #pragma shader_feature UVOffset
			#pragma shader_feature Clipping
            #pragma shader_feature WorldSpaceMain
			#pragma shader_feature WorldSpaceDist
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvDist : TEXCOORD1;
                float2 pos : TEXCOORD2;
            };

            sampler2D _MainTex, _DistortionTex;
            float4 _MainTex_ST, _DistortionTex_ST;
            float4 _Color, _BlackColor, _WhiteColor;
            float4 _DistortionVals;
            float _Hue, _Sat, _Val, _BlVal, _WhVal, _OffsetSpeedX, _OffsetSpeedY, _OffsetFPS, _DistortionFPS, _AlphaCutoff;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.pos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvDist = TRANSFORM_TEX(v.uv, _DistortionTex);
                return o;
            }

            float3 rgb2hsv(float3 c) {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            float3 hsv2rgb(float3 c) {
                c = float3(c.x, clamp(c.yz, 0.0, 1.0));
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed2 newuv = i.uv;

				//World space tiling for main texture
                #if defined(WorldSpaceMain)
                newuv = _MainTex_ST.zw + ((1/_MainTex_ST.xy)*i.pos);
                #endif

				//Texture panning for main texture
                #if defined(UVOffset)
                float fpsVal = _Time.y;
                if (_OffsetFPS > 0) {
                    fpsVal = round(_Time.y * _OffsetFPS) / _OffsetFPS;
                }
                newuv += float2(_OffsetSpeedX != 0 ? fmod(fpsVal, 1 / _OffsetSpeedX) * _OffsetSpeedX:0 , _OffsetSpeedY != 0 ? fmod(fpsVal, 1 / _OffsetSpeedY) * _OffsetSpeedY:0);
                #endif

				//Handles Distortion
                #if defined(Distortion)
				fixed2 newuvDist = i.uvDist;

				//World Space tiling for Distortion Texture
				#if defined(WorldSpaceDist)
				newuvDist = _DistortionTex_ST.zw + ((1/_DistortionTex_ST.xy)*i.pos);
				#endif
                float dfpsVal = _Time.x;
                if (_DistortionFPS > 0) {
                    dfpsVal = round(_Time.y * _DistortionFPS) / _DistortionFPS;
                }
                fixed2 normOffset = fmod(_DistortionVals.xy * dfpsVal, 1/ max((0.001,0.001),_DistortionVals.xy));
                fixed2 norm = (tex2D(_DistortionTex, newuvDist + normOffset).gb * 2) - 1;
                newuv += (norm * _DistortionVals.zw);
                #endif

                fixed4 col = tex2D(_MainTex, newuv);

				//Black/White colour remapping
                #if defined(Remap)
                float greyscale = dot(col.rgb, float3(0.3, 0.59, 0.11));
                greyscale = (greyscale - _BlVal) / (_WhVal - _BlVal);
                fixed4 coloured = lerp(_BlackColor, _WhiteColor, greyscale);
                col.rgb = coloured.rgb;
                col.a *= min(max(0,coloured.a),1);
                #endif

				//Hue Saturation Value shifting
                #if defined(HSV_Shift)
                float3 hsv = rgb2hsv(col.rgb);
                col.rgb = hsv2rgb(hsv + float3(_Hue, _Sat, _Val)).rgb;
                #endif
                col *= _Color;

				//Halpha cutout
				#if defined(Clipping)
				clip(col.a - _AlphaCutoff - 0.01);
				col.a = 1;
				#endif

                return col;
            }
            ENDCG
        }
    }
}
