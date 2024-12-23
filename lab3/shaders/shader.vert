#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 Normal;
out vec3 FragPos;

out vec2 texCoord;

void main()
{
    FragPos = vec3(model * vec4(aPosition, 1.0));
    Normal = aNormal * mat3(transpose(inverse(model)));
    texCoord = aTexCoord;
    gl_Position = projection * view * model * vec4(aPosition, 1.0);
}