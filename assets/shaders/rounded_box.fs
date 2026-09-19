#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform vec4 colDiffuse;

// Output fragment color
out vec4 finalColor;

// NOTE: Add your custom variables here

void main()
{
	#define radius 0.3125
	#define boxRadius .5
	#define pixelsPerUnit 56.

	vec4 color = colDiffuse*fragColor;

	vec2 uv = fragTexCoord - vec2(0.5, 0.5);

	vec2 solidBoxCorner = vec2(boxRadius - radius);
	float partialSdf = distance(max(abs(uv), solidBoxCorner), solidBoxCorner) - radius;

	float roundedBox = clamp(partialSdf * (-pixelsPerUnit), 0., 1.);

	// Calculate final fragment color
	finalColor = vec4(color.rgb, color.a * roundedBox);
}
