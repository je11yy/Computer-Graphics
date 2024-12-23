#version 330 core

struct Light {
    vec3  position;
    vec3  color;
    vec3  direction;
    float cutOff;
    float outerCutOff;

    vec3 ambient;
    vec3 diffuse;
    float constant;
    float linear;
    float quadratic;
};

uniform Light light;
uniform vec3 viewPos;

out vec4 FragColor;

in vec3 Normal;
in vec3 FragPos;
uniform vec3 inputColor;

void main() {
    vec3 ambient = light.ambient * inputColor;

    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * inputColor;

    float distance    = length(light.position - FragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance +
    light.quadratic * (distance * distance));

    float theta     = dot(lightDir, normalize(-light.direction));
    float epsilon   = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
     
    ambient  *= attenuation;
    diffuse  *= attenuation * intensity;

    vec3 result = ambient + diffuse;
    FragColor = vec4(result, 1.0);
    
}