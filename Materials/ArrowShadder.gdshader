shader_type canvas_item;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
    // Suaviza a transição para a ponta da seta
    float distance = clamp(1.0 - UV.x, 0.0, 1.0);
    COLOR = vec4(vec3(distance), 1.0); // Gradiente para suavizar a ponta
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
