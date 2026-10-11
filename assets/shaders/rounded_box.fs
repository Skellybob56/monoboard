#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;
in vec3 fragNormal;

// Output fragment color
out vec4 finalColor;

// NOTE: Add your custom variables here

void main()
{
	// make this a uniform
	#define pixelsPerUnit 56.

	float radius = fragNormal.z;

	vec2 solidBoxCorner = fragNormal.xy;
	float partialSdf = distance(max(abs(fragTexCoord), solidBoxCorner), solidBoxCorner) - radius;

	float roundedBox = clamp(partialSdf * (-pixelsPerUnit) + 0.5, 0., 1.);

	// Calculate final fragment color
	finalColor = vec4(fragColor.rgb, fragColor.a * roundedBox);
}
