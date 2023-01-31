Shader "Simulation/ParticleRenderer"
{
	CGINCLUDE
	#include "UnityCG.cginc"
	#include "ParticleData.cginc"

	struct v2g
	{
		float3 position : TEXCOORD0;
		float4 color : TEXCOORD1;
		float  size : TEXCOORD2;
	};
	struct g2f
	{
		float4 position : POSITION;
		float2 texcoord : TEXCOORD0;
		float4 color : TEXCOORD1;
	};

	struct gin
	{
		float3 position;
		float size;
		float4 color;
	};

	#define VertexIn gin
	#define VertexOut g2f
	#include "GeometryQuad.hlsl"

	StructuredBuffer<Particle> _ParticleBuffer;
	sampler2D _ParticleTex;
	float4 _Color;
	float _Size;

	// --------------------------------------------------------------------
	// Vertex Shader
	// --------------------------------------------------------------------
	v2g vert(uint id : SV_VertexID)
	{
		v2g o = (v2g)0;
		Particle p = _ParticleBuffer[id];

		o.position = float3(p.position, 0);
		o.color = _Color;
		o.size = _Size;
		return o;
	}

	// --------------------------------------------------------------------
	// Geometry Shader
	// --------------------------------------------------------------------
	[maxvertexcount(QuadVertex)]
	void geom(point v2g p[1], inout TriangleStream<g2f> outStream)
	{
		float size = p[0].size;
		if (size > 0)
		{
			float3 position = p[0].position;
			float4 color = p[0].color;
			VertexIn vin;
			vin.position = position;
			vin.color = color;
			vin.size = size;
			AddQuad(vin, outStream);
		}
	}

	// --------------------------------------------------------------------
	// Fragment Shader
	// --------------------------------------------------------------------
	float4 frag(g2f i) : SV_Target
	{
		return tex2D(_ParticleTex, i.texcoord.xy) * i.color;
	}

	ENDCG

	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		ZWrite Off
		Blend SrcAlpha One

		Pass
		{
			CGPROGRAM
			#pragma target   5.0
			#pragma vertex   vert
			#pragma geometry geom
			#pragma fragment frag
			ENDCG
		}
	}
}