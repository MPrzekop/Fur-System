// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "HairShader"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Clipping("Clipping", Range( 0 , 1)) = 0
		[Toggle(_USEHAIRPATTERNTEXTURE_ON)] _Usehairpatterntexture("Use hair pattern texture", Float) = 0
		_HairPattern("HairPattern", 2D) = "white" {}
		_EndColor("End Color", Color) = (1,1,1,0)
		_Color("Color", Color) = (0,0,0,0)
		[Toggle(_SHADEROFFSET_ON)] _Shaderoffset("Shader offset", Float) = 0
		[Toggle(_WORLDOFFSET_ON)] _WorldOffset("WorldOffset", Float) = 0
		[Toggle(_ADDWIND_ON)] _AddWind("Add Wind", Float) = 0
		_Windstrength("Wind strength", Float) = 0
		[Toggle(_ADDGRAVITY_ON)] _AddGravity("Add Gravity", Float) = 0
		_GravityStrength("GravityStrength", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 4.0
		#pragma shader_feature_local _ADDWIND_ON
		#pragma shader_feature_local _ADDGRAVITY_ON
		#pragma shader_feature_local _SHADEROFFSET_ON
		#pragma shader_feature_local _WORLDOFFSET_ON
		#pragma shader_feature_local _USEHAIRPATTERNTEXTURE_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows nodynlightmap vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform half _GravityStrength;
		uniform half _Windstrength;
		uniform sampler2D _MainTex;
		uniform half4 _MainTex_ST;
		uniform half4 _Color;
		uniform half4 _EndColor;
		uniform sampler2D _HairPattern;
		uniform half4 _HairPattern_ST;
		uniform half _Clipping;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			half3 temp_output_5_0 = (v.color).rgb;
			half3 appendResult4 = (half3(temp_output_5_0));
			half4 transform3 = mul(unity_WorldToObject,half4( appendResult4 , 0.0 ));
			#ifdef _WORLDOFFSET_ON
				half4 staticSwitch2 = transform3;
			#else
				half4 staticSwitch2 = half4( temp_output_5_0 , 0.0 );
			#endif
			#ifdef _SHADEROFFSET_ON
				half4 staticSwitch43 = staticSwitch2;
			#else
				half4 staticSwitch43 = float4( 0,0,0,0 );
			#endif
			half temp_output_20_0 = length( temp_output_5_0 );
			half temp_output_21_0 = pow( temp_output_20_0 , 2.0 );
			half4 transform24 = mul(unity_WorldToObject,half4( ( temp_output_21_0 * half3(0,-10,0) * _GravityStrength ) , 0.0 ));
			half4 temp_output_26_0 = ( staticSwitch43 + transform24 );
			#ifdef _ADDGRAVITY_ON
				half4 staticSwitch19 = temp_output_26_0;
			#else
				half4 staticSwitch19 = staticSwitch43;
			#endif
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			half simplePerlin2D29 = snoise( ( _Time.y + ase_worldPos ).xy );
			#ifdef _ADDWIND_ON
				half4 staticSwitch27 = ( temp_output_26_0 + ( simplePerlin2D29 * temp_output_21_0 * _Windstrength ) );
			#else
				half4 staticSwitch27 = staticSwitch19;
			#endif
			half4 vertexToFrag39 = staticSwitch27;
			v.vertex.xyz += vertexToFrag39.xyz;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			half3 temp_output_5_0 = (i.vertexColor).rgb;
			half temp_output_20_0 = length( temp_output_5_0 );
			half4 lerpResult36 = lerp( _Color , _EndColor , saturate( ( temp_output_20_0 / 0.02 ) ));
			half simplePerlin2D15 = snoise( i.uv_texcoord*100.0 );
			simplePerlin2D15 = simplePerlin2D15*0.5 + 0.5;
			float2 uv_HairPattern = i.uv_texcoord * _HairPattern_ST.xy + _HairPattern_ST.zw;
			#ifdef _USEHAIRPATTERNTEXTURE_ON
				half staticSwitch17 = tex2D( _HairPattern, uv_HairPattern ).a;
			#else
				half staticSwitch17 = simplePerlin2D15;
			#endif
			clip( ( staticSwitch17 * saturate( i.vertexColor.a ) ) - _Clipping);
			o.Albedo = ( tex2D( _MainTex, uv_MainTex ) * lerpResult36 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
229;73;1730;869;2142.854;623.3113;1.087959;True;False
Node;AmplifyShaderEditor.VertexColorNode;1;-1364.041,340.5917;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;5;-1176.681,382.5741;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LengthOpNode;20;-997.1249,472.0585;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;4;-829.5919,376.8315;Inherit;False;FLOAT3;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleTimeNode;32;-902.9005,976.6159;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-1104,752;Inherit;False;Property;_GravityStrength;GravityStrength;11;0;Create;True;0;0;0;False;0;False;0;0.667;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;21;-848,512;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;30;-768,768;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldToObjectTransfNode;3;-668.4897,335.1905;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;22;-976,608;Inherit;False;Constant;_Vector0;Vector 0;6;0;Create;True;0;0;0;False;0;False;0,-10,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TexCoordVertexDataNode;16;-1648.089,-2.794403;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;2;-571.0173,266.3324;Inherit;False;Property;_WorldOffset;WorldOffset;7;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-640,512;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;31;-705.6462,967.4174;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;13;-1344,-160;Inherit;True;Property;_HairPattern;HairPattern;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;38;-791.3669,-155.0127;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.02;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldToObjectTransfNode;24;-560,608;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;29;-534.965,810.0229;Inherit;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;43;-378.1135,356.8727;Inherit;False;Property;_Shaderoffset;Shader offset;6;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;15;-1299.089,62.2056;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-500.0925,1069.044;Inherit;False;Property;_Windstrength;Wind strength;9;0;Create;True;0;0;0;False;0;False;0;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-245.5059,455.6684;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;17;-960,-32;Inherit;False;Property;_Usehairpatterntexture;Use hair pattern texture;2;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;44;-654.4957,-102.1013;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;9;-672,80;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;37;-1043.516,-267.6752;Inherit;False;Property;_EndColor;End Color;4;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.2592303,0.3207547,0.2345141,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;10;-1024,-489.5632;Inherit;False;Property;_Color;Color;5;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-321.3584,774.2512;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;19;-123.2173,386.5869;Inherit;False;Property;_AddGravity;Add Gravity;10;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-304.1998,-10.3;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-564.6819,159.9322;Inherit;False;Property;_Clipping;Clipping;1;0;Create;True;0;0;0;False;0;False;0;0.806;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-256,608;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;36;-609.376,-277.9074;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;41;-1069.182,-694.7759;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;27;-119.4232,611.9661;Inherit;False;Property;_AddWind;Add Wind;8;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ClipNode;40;-52.73433,-27.75036;Inherit;False;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexToFragmentNode;39;175.2916,531.8511;Inherit;False;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;151.6184,-158.7758;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;11;400,16;Half;False;True;-1;4;ASEMaterialInspector;0;0;Standard;HairShader;False;False;False;False;False;False;False;True;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;1;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;1;0
WireConnection;20;0;5;0
WireConnection;4;0;5;0
WireConnection;21;0;20;0
WireConnection;3;0;4;0
WireConnection;2;1;5;0
WireConnection;2;0;3;0
WireConnection;23;0;21;0
WireConnection;23;1;22;0
WireConnection;23;2;25;0
WireConnection;31;0;32;0
WireConnection;31;1;30;0
WireConnection;38;0;20;0
WireConnection;24;0;23;0
WireConnection;29;0;31;0
WireConnection;43;0;2;0
WireConnection;15;0;16;0
WireConnection;26;0;43;0
WireConnection;26;1;24;0
WireConnection;17;1;15;0
WireConnection;17;0;13;4
WireConnection;44;0;38;0
WireConnection;9;0;1;4
WireConnection;33;0;29;0
WireConnection;33;1;21;0
WireConnection;33;2;34;0
WireConnection;19;1;43;0
WireConnection;19;0;26;0
WireConnection;14;0;17;0
WireConnection;14;1;9;0
WireConnection;28;0;26;0
WireConnection;28;1;33;0
WireConnection;36;0;10;0
WireConnection;36;1;37;0
WireConnection;36;2;44;0
WireConnection;27;1;19;0
WireConnection;27;0;28;0
WireConnection;40;0;36;0
WireConnection;40;1;14;0
WireConnection;40;2;12;0
WireConnection;39;0;27;0
WireConnection;42;0;41;0
WireConnection;42;1;40;0
WireConnection;11;0;42;0
WireConnection;11;11;39;0
ASEEND*/
//CHKSM=89DF342C8BE6CA64EB1E7CCA1A131B42D2896DBE