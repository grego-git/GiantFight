shader_type spatial;
render_mode unshaded;

uniform vec3 light_direction = vec3(0.0, -1.0, -0.5); // Light *direction*, not position
uniform vec4 color: source_color;
uniform vec4 highlight_color : source_color;

varying float vertex_light;

void vertex() {
    // Transform normal to world space
    vec3 world_normal = normalize((MODEL_MATRIX * vec4(NORMAL, 0.0)).xyz);

    // Normalize light direction and invert it to point *towards* surface
    vec3 light_dir = normalize(-light_direction);

    // Lambertian reflectance
    vertex_light = max(dot(world_normal, light_dir), 0.0);
}

void fragment() {
    // Multiply interpolated vertex lighting with base color
	ALBEDO = mix(color.rgb, highlight_color.rgb, pow(vertex_light, 2.0)) * mix(0.1, 1.0, vertex_light);
}
