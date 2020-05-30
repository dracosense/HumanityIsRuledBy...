shader_type canvas_item;

uniform vec4 base_color : hint_color;
uniform vec2 time_c = vec2(0, 0);
uniform float noise_const : hint_range(0, 10);
uniform float scale = 10.0f;
uniform float add : hint_range(-1, 1);
uniform bool add_texture = false;
uniform bool alpha_func = true;
uniform bool get_texture_alpha;

float random(vec2 v)
{
    return fract(sin(dot(v.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

float noise(vec2 v)
{
    vec2 i = floor(v);
    vec2 f = fract(v);
    float va = random(i);
    float vb = random(i + vec2(1, 0));
    float vc = random(i + vec2(0, 1));
    float vd = random(i + vec2(1, 1));
    vec2 func = smoothstep(0.0f, 1.0f, f);
    return mix(mix(va, vb, func.x), mix(vc, vd, func.x), func.y);
}

void fragment()
{
	float n = noise((UV + TIME * time_c) * scale);
    vec3 v = noise_const * n * base_color.rgb + add * vec3(1.0f, 1.0f, 1.0f);
    vec4 c = vec4(v, alpha_func?(1.0f - n):1.0f);
	vec4 t = texture(TEXTURE, UV);
	if (add_texture)
	{
		c += t;
	}
	if (get_texture_alpha)
	{
		c.a = t.a;	
	}
	COLOR = c;
}