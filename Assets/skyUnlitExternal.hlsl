#include "stereokit.hlsli"

//--diffuse : external = white

Texture2D    diffuse   : register(t0);
SamplerState diffuse_s : register(s0);

struct vsIn {
	float4 pos     : SV_Position;
	float3 norm    : NORMAL0;
	float2 uv      : TEXCOORD0;
};
struct psIn {
	float4 pos     : SV_Position;
	float2 uv      : TEXCOORD0;
	uint   view_id : SV_RenderTargetArrayIndex;
};

psIn vs(vsIn input, uint id : SV_InstanceID) {
	psIn o;
	o.view_id = id % sk_view_count;
	id        = id / sk_view_count;
	o.pos = float4(input.pos.xy, 1, 1);
	o.uv  = float2(input.uv.y, 1-input.uv.x);
	
	return o;
}

half4 ps(psIn input) : SV_TARGET {
	return pow(diffuse.Sample(diffuse_s, input.uv), 2.2);
}