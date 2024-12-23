#version 430

in vec3 glPosition;
out vec4 FragColor;

const float BIG = 1e6;
const float EPSILON = 1e-3;

const int DIFFUSE = 1;
const int REFLECTION = 2;
const int REFRACTION = 3;

const int DIFFUSE_REFLECTION = 1;
const int MIRROR_REFLECTION = 2;
const int CASE_REFRACTION = 3;

const vec3 Unit = vec3(1.0, 1.0, 1.0);

struct Sphere {
    vec3 Center;
    float Radius;
    int MaterialIdx;
};

struct Cube {
    vec3 Center;
    float Size;
    int MaterialIdx;
};

struct Triangle {
    vec3 v1, v2, v3;
    int MaterialIdx;
};

struct Camera {
    vec3 Position, View, Up, Side;
    vec2 Scale;
};

struct Ray {
    vec3 Origin, Direction;
};

struct Intersection {
    float Time;
    vec3 Point, Normal, Color;
    vec4 LightCoeffs;
    float ReflectionCoef, RefractionCoef;
    int MaterialType;
};

struct Light {
    vec3 Position;
    float Radius;
};

struct Material {
    vec3 Color;
    vec4 LightCoeffs;
    float ReflectionCoef, RefractionCoef;
    int MaterialType;
};

struct TracingRay {
    Ray ray;
    float contribution;
    int depth;
};

uniform Camera uCamera;

const int lightCount = 5;
Light lights[lightCount];
const int sphereCount = 8;
Sphere spheres[sphereCount];
const int triangleCount = 26;
Triangle triangles[triangleCount];
Material materials[6];

void addCube(vec3 center, float size, int materialIdx, inout int triangleIndex) {
    vec3 halfSize = vec3(size / 2.0);
    vec3 vertices[8] = vec3[](
        center + vec3(-halfSize.x, -halfSize.y, halfSize.z),
        center + vec3(halfSize.x, -halfSize.y, halfSize.z),
        center + vec3(halfSize.x, halfSize.y, halfSize.z),
        center + vec3(-halfSize.x, halfSize.y, halfSize.z),
        center + vec3(-halfSize.x, -halfSize.y, -halfSize.z),
        center + vec3(halfSize.x, -halfSize.y, -halfSize.z),
        center + vec3(halfSize.x, halfSize.y, -halfSize.z),
        center + vec3(-halfSize.x, halfSize.y, -halfSize.z)
    );

    int indices[36] = int[](
        // Front face
		0, 1, 2,
		2, 3, 0,

		// Back face
		4, 5, 6,
		6, 7, 4,

		// Left face
		4, 0, 3,
		3, 7, 4,

		// Right face
		1, 5, 6,
		6, 2, 1,

		// Top face
		3, 2, 6,
		6, 7, 3,

		// Bottom face
		0, 1, 5,
		5, 4, 0
    );

    for (int i = 0; i < 36; i += 3) {
        triangles[triangleIndex++] = Triangle(
            vertices[indices[i]],
            vertices[indices[i + 1]],
            vertices[indices[i + 2]],
            materialIdx
        );
    }
}

void initializeDefaultScene() {
    float x = -4, y = 2, z = 4;
    // Mirror spheres
    float radius = 0.7;
    for (int i = 0; i < 4; ++i) {
		int materialIdx = i % 2 == 0 ? 0 : 4;
        spheres[i] = Sphere(vec3(x, y, z), radius, materialIdx);
        x += 1.5;
    }
    // Refraction spheres
    for (int i = 4; i < 8; ++i) {
        spheres[i] = Sphere(vec3(x, y, z), radius, 1);
        x += 1.5;
    }

    // Mirror cubes
    x = -2, y = -2;
    float size = 1.7;
	int triangleIndex = 2;
	addCube(vec3(x, y, z), size, 4, triangleIndex);
	addCube(vec3(x + 3, y, z), size, 5, triangleIndex);

    // WALL
    triangles[0] = Triangle(vec3(-15.0, -15.0, 5.0), vec3(15.0, -15.0, 5.0),
                            vec3(-15.0, 15.0, 5.0), 2);
    triangles[1] = Triangle(vec3(15.0, 15.0, 5.0), vec3(-15.0, 15.0, 5.0),
                            vec3(15.0, -15.0, 5.0), 2);

    // Lights
    lights[0] = Light(vec3(-6, -3, 2.0), 20.0);
    lights[1] = Light(vec3(6, 3, 2.0), 20.0);
    lights[2] = Light(vec3(-6, 3, 2.0), 20.0);
    lights[3] = Light(vec3(6, -3, 2.0), 20.0);
	lights[4] = Light(vec3(0, 0, -3.0), 20.0);
}

void initializeDefaultLightMaterials() {
    vec4 lightCoefs = vec4(0.4, 0.9, 0.0, 1000.0);

    vec3 mutedColor1 = vec3(0.3, 0.5, 0.7);
    vec3 mutedColor2 = vec3(0.6, 0.4, 0.2);
    vec3 mutedColor3 = vec3(0.4, 0.6, 0.4);
    vec3 mutedColor4 = vec3(0.7, 0.5, 0.6);
    vec3 mutedColor5 = vec3(0.4, 0.4, 0.4);

    materials[0] = Material(vec3(0.0, 1.0, 0.0), lightCoefs, 1.5, 1.0,
                            REFLECTION); // Mirror
    materials[1] = Material(vec3(1.0, 1.0, 1.0), lightCoefs, 0.0, 1.5,
                            REFRACTION); // Transparent
    materials[2] = Material(vec3(0.1, 0.14, 0.15), lightCoefs, 0.0, 1.0,
                            DIFFUSE); // Diffuse
    materials[3] = Material(mutedColor2, lightCoefs, 1.0, 1.0,
                            DIFFUSE); // Diffuse
    materials[4] = Material(mutedColor4, lightCoefs, 0.7, 1.0,
                            REFLECTION); // Mirror
    materials[5] = Material(mutedColor3, lightCoefs, 0.9, 1.0,
                            REFLECTION); // Mirror
}

Ray GenerateRay() {
    vec2 coords = glPosition.xy * uCamera.Scale;
    return Ray(uCamera.Position,
               normalize(uCamera.View + uCamera.Side * coords.x +
                         uCamera.Up * coords.y));
}

bool IntersectSphere(Sphere sphere, Ray ray, out float time) {
    ray.Origin -= sphere.Center;
    float A = dot(ray.Direction, ray.Direction);
    float B = dot(ray.Direction, ray.Origin);
    float C = dot(ray.Origin, ray.Origin) - sphere.Radius * sphere.Radius;
    float D = B * B - A * C;
    if (D > 0.0) {
        D = sqrt(D);
        float t1 = (-B - D) / A;
        float t2 = (-B + D) / A;
        if (t1 < 0 && t2 < 0)
            return false;
        if (min(t1, t2) < 0) {
            time = max(t1, t2);
            return true;
        }
        time = min(t1, t2);
        return true;
    }
    return false;
}

bool IntersectTriangle(Ray ray, Triangle triangle, out float time) {
    time = -1;
    vec3 A = triangle.v2 - triangle.v1;
    vec3 B = triangle.v3 - triangle.v1;
    vec3 N = cross(A, B);

    float NdotRayDirection = dot(N, ray.Direction);
    if (abs(NdotRayDirection) < EPSILON)
        return false;

    float d = dot(N, triangle.v1);
    float t = -(dot(N, ray.Origin) - d) / NdotRayDirection;
    if (t < 0)
        return false;

    vec3 P = ray.Origin + t * ray.Direction;
    if (dot(N, cross(triangle.v2 - triangle.v1, P - triangle.v1)) < 0)
        return false;
    if (dot(N, cross(triangle.v1 - triangle.v3, P - triangle.v3)) < 0)
        return false;
    if (dot(N, cross(triangle.v3 - triangle.v2, P - triangle.v2)) < 0)
        return false;

    time = t;
    return true;
}

bool Raytrace(Ray ray, float start, float final, inout Intersection intersect) {
    bool result = false;
    float test = start;
    intersect.Time = final;

    // Calculate intersect with spheres
    for (int i = 0; i < sphereCount; i++) {
        Sphere sphere = spheres[i];
        if (IntersectSphere(sphere, ray, test) && test < intersect.Time) {
            intersect.Time = test;
            intersect.Point = ray.Origin + ray.Direction * test;
            intersect.Normal = normalize(intersect.Point - spheres[i].Center);
            intersect.Color = materials[sphere.MaterialIdx].Color;
            intersect.LightCoeffs = materials[sphere.MaterialIdx].LightCoeffs;
            intersect.ReflectionCoef =
                materials[sphere.MaterialIdx].ReflectionCoef;
            intersect.RefractionCoef =
                materials[sphere.MaterialIdx].RefractionCoef;
            intersect.MaterialType = materials[sphere.MaterialIdx].MaterialType;
            result = true;
        }
    }

    for (int i = 0; i < triangleCount; i++) {
        Triangle triangle = triangles[i];
        if (IntersectTriangle(ray, triangle, test) && test < intersect.Time) {
            intersect.Time = test;
            intersect.Point = ray.Origin + ray.Direction * test;
            intersect.Normal = normalize(
                cross(triangle.v1 - triangle.v2, triangle.v3 - triangle.v2));
			if (dot(intersect.Normal, ray.Direction) > 0) {
				intersect.Normal = -intersect.Normal;
			}
            intersect.Color = materials[triangle.MaterialIdx].Color;
            intersect.LightCoeffs = materials[triangle.MaterialIdx].LightCoeffs;
            intersect.ReflectionCoef =
                materials[triangle.MaterialIdx].ReflectionCoef;
            intersect.RefractionCoef =
                materials[triangle.MaterialIdx].RefractionCoef;
            intersect.MaterialType =
                materials[triangle.MaterialIdx].MaterialType;
            result = true;
        }
    }
    return result;
}

vec3 Phong(Intersection intersect) {
    vec3 resultColor = vec3(0.0);
    for (int i = 0; i < lightCount; i++) {
        vec3 lightDirection = normalize(lights[i].Position - intersect.Point);
        float distanceToLight = distance(lights[i].Position, intersect.Point);
        float attenuation = 1.0 / (distanceToLight * distanceToLight + 0.1);
        float diffuse = max(dot(lightDirection, intersect.Normal), 0.0) * 2.0;

        vec3 view = normalize(uCamera.Position - intersect.Point);
        vec3 reflected = reflect(-view, intersect.Normal);
        float specular = pow(max(dot(reflected, lightDirection), 0.0),
                             intersect.LightCoeffs.w) *
                         2.0;
        resultColor +=
            intersect.LightCoeffs.x * intersect.Color * attenuation +
            intersect.LightCoeffs.y * diffuse * intersect.Color * attenuation +
            intersect.LightCoeffs.z * specular * Unit * attenuation;
    }
    return 10 * resultColor;
}

const int MAX_STACK_SIZE = 40;
uniform int MAX_TRACE_DEPTH;
TracingRay stack[MAX_STACK_SIZE];
int stackSize = 0;

bool pushRay(TracingRay secondaryRay) {
    if (stackSize < MAX_STACK_SIZE && secondaryRay.depth < MAX_TRACE_DEPTH) {
        stack[stackSize] = secondaryRay;
        stackSize++;
        return true;
    }
    return false;
}

bool isEmpty() { return (stackSize < 0); }

TracingRay popRay() { return stack[--stackSize]; }

void main(void) {
    vec3 resultColor = vec3(0, 0, 0);

    float start = 0;
    float final = 1e6;

    initializeDefaultScene();
    initializeDefaultLightMaterials();

    Ray ray = GenerateRay();
    TracingRay trRay = TracingRay(ray, 1, 0);
    pushRay(trRay);

    while (stackSize > 0) {
        trRay = popRay();
        ray = trRay.ray;

        Intersection intersect;
        intersect.Time = BIG;

        if (Raytrace(ray, start, final, intersect)) {
            switch (intersect.MaterialType) {
            case DIFFUSE_REFLECTION: {
                resultColor += trRay.contribution * Phong(intersect);
                break;
            }

            case MIRROR_REFLECTION: {
                if (intersect.ReflectionCoef < 1) {
                    resultColor += trRay.contribution *
                                   (1 - intersect.ReflectionCoef) *
                                   Phong(intersect);
                }
                vec3 reflectDirection =
                    reflect(ray.Direction, intersect.Normal);
                vec3 viewDir = normalize(uCamera.Position - intersect.Point);

                // float angle = dot(reflectDirection, viewDir);
                // float distortionFactor = 1.0 + (1.0 - angle) * 2.0;
                // reflectDirection = reflectDirection * distortionFactor;

                float contribution =
                    trRay.contribution * intersect.ReflectionCoef;
                TracingRay reflectRay =
                    TracingRay(Ray(intersect.Point + reflectDirection * EPSILON,
                                   reflectDirection),
                               contribution, trRay.depth + 1);
                pushRay(reflectRay);
                break;
            }

            case CASE_REFRACTION: {
                float ior =
                    dot(ray.Direction, intersect.Normal) < 0 ? 0.1 : 0.7;
                vec3 refractDirection =
                    dot(ray.Direction, intersect.Normal) < 0
                        ? refract(ray.Direction, intersect.Normal, 1 / 1.3)
                        : -refract(ray.Direction, intersect.Normal, 1.3);

                TracingRay refractRay =
                    TracingRay(Ray(intersect.Point + EPSILON * refractDirection,
                                   refractDirection),
                               ior, trRay.depth + 1);
                pushRay(refractRay);
                break;
            }
            }
        }
    }
    FragColor = vec4(resultColor, 1.0);
}