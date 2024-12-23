#version 330 core

uniform vec3 lightColor;
uniform vec3 lightPos;

in vec3 Normal;
in vec3 FragPos;

out vec4 FragColor;

in vec2 texCoord;
uniform sampler2D texture0;

void main() {
    float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * lightColor;

    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);

    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;

    FragColor = (vec4(ambient + diffuse, 1.0)) * texture(texture0, texCoord);
}
