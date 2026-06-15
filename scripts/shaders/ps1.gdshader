shader_type spatial;
render_mode blend_premul_alpha,
	depth_prepass_alpha,
	shadows_disabled;

uniform bool affine_mapping = false;
uniform bool should_reflect = false;
uniform sampler2D albedo : source_color, filter_nearest;
uniform sampler2D sphere_map : source_color;
uniform float texture_scale : hint_range(1.0, 1000.0) = 1.0;
uniform float alpha_scissor : hint_range(0, 1) = 0.5;
uniform float jitter: hint_range(0, 1) = 0.25;
uniform float reflect_strength : hint_range(0.0, 1.0) = 0.0;
uniform float metalness : hint_range(0.0, 1.0) = 0.0;
uniform float specular : hint_range(0.0, 1.0) = 0.0;
uniform float roughness : hint_range(0.0, 1.0) = 0.0;
uniform ivec2 resolution = ivec2(320, 240);

vec4 snap_to_position(vec4 base_position)
{
	vec4 snapped_position = base_position;
	snapped_position.xyz = base_position.xyz / base_position.w;

	vec2 snap_resulotion = floor(vec2(resolution) * (1.0 - jitter));
	snapped_position.x = floor(snap_resulotion.x * snapped_position.x) / snap_resulotion.x;
	snapped_position.y = floor(snap_resulotion.y * snapped_position.y) / snap_resulotion.y;

	snapped_position.xyz *= base_position.w;
	return snapped_position;
}

void vertex()
{
	vec4 snapped_position = snap_to_position(PROJECTION_MATRIX * MODELVIEW_MATRIX * vec4(VERTEX, 1.0));
	if (affine_mapping)
	{
		POSITION = snapped_position;
		POSITION /= abs(POSITION.w);
	}
	else
	{
		POSITION = snapped_position;
	}
}

void fragment()
{
	vec4 color_base = COLOR;
	vec4 texture_color = texture(albedo, UV * texture_scale);

	ALBEDO = (color_base * texture_color).rgb;
	ALPHA = texture_color.a * color_base.a;
	ALPHA_SCISSOR_THRESHOLD = alpha_scissor;
	METALLIC = metalness;
	ROUGHNESS = roughness;
	SPECULAR = specular;

	if (should_reflect){
		// Normal in view-space (Godot gives this already)
		vec3 n = NORMAL;

    	// View direction (toward camera)
    	vec3 v = normalize(-VIEW);

    	// Reflection vector
    	vec3 r = reflect(v, n);

    	// Convert to sphere-map UVs
    	float m = 2.0 * sqrt(
        	r.x * r.x +
        	r.y * r.y +
        	(r.z + 1.0) * (r.z + 1.0)
    	);

    	vec2 uv = vec2(
        	r.x / m + 0.5,
        	r.y / m + 0.5
    	);

    	vec4 sphere_col = texture(sphere_map, uv);

		ALBEDO = mix(ALBEDO, sphere_col.rgb, metalness * reflect_strength);
	}
}