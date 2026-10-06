using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BoidsUnderwaterScene.Editor
{
    public static class BoidsSceneAssets
    {
        private const string Generated = BoidsUnderwaterSceneBuilder.Root + "/Generated";

        public static Texture2D Texture(string filename)
        {
            string name = "Public" + System.IO.Path.GetFileNameWithoutExtension(filename).Replace("_", "");
            string path = Generated + "/" + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;
            bool normal = filename.IndexOf("_N", StringComparison.OrdinalIgnoreCase) >= 0;
            bool water = filename.IndexOf("Water", StringComparison.OrdinalIgnoreCase) >= 0;
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true, normal)
            {
                name = name, wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Trilinear
            };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float u = x / (float)size, v = y / (float)size;
                if (normal)
                {
                    float dx = Height(u + 1f / size, v, water) - Height(u - 1f / size, v, water);
                    float dy = Height(u, v + 1f / size, water) - Height(u, v - 1f / size, water);
                    Vector3 n = new Vector3(-dx * 6f, -dy * 6f, 1f).normalized;
                    pixels[y * size + x] = new Color(n.x * .5f + .5f, n.y * .5f + .5f, n.z * .5f + .5f, 1);
                }
                else
                {
                    float value = .65f + Height(u, v, false) * .28f;
                    pixels[y * size + x] = new Color(value, value, value, 1);
                }
            }
            texture.SetPixels(pixels); texture.Apply(); AssetDatabase.CreateAsset(texture, path);
            return texture;
        }

        private static float Height(float u, float v, bool water)
        {
            u = Mathf.Repeat(u, 1); v = Mathf.Repeat(v, 1);
            float value = 0, amplitude = 1, frequency = water ? 4 : 8;
            for (int octave = 0; octave < 3; octave++)
            {
                float x = u * frequency + 17, y = v * frequency + 31;
                float lower = Mathf.Lerp(Mathf.PerlinNoise(x, y), Mathf.PerlinNoise(x - frequency, y), u);
                float upper = Mathf.Lerp(Mathf.PerlinNoise(x, y - frequency), Mathf.PerlinNoise(x - frequency, y - frequency), u);
                value += (Mathf.Lerp(lower, upper, v) - .5f) * amplitude;
                frequency *= 2; amplitude *= .5f;
            }
            return value * (water ? .3f : .8f);
        }

        public static GameObject Object(string name)
        {
            bool rock = name.StartsWith("SM_Rock", StringComparison.Ordinal);
            string asset = "Public" + name.Replace("_", "");
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(Generated + "/" + asset + ".asset");
            if (mesh == null)
            {
                mesh = rock ? Rock(name[name.Length - 1] - '0') : Coral(name);
                mesh.name = asset; AssetDatabase.CreateAsset(mesh, Generated + "/" + asset + ".asset");
            }
            var material = AssetDatabase.LoadAssetAtPath<Material>(Generated + "/PublicCoral.mat");
            if (material == null)
            {
                material = new Material(Shader.Find("BoidsUnderwaterScene/EnvironmentLit")) { name = "PublicCoral", enableInstancing = true };
                material.SetColor("_BaseColor", new Color(.7f, .72f, .5f)); material.SetFloat("_Algae", 0);
                AssetDatabase.CreateAsset(material, Generated + "/PublicCoral.mat");
            }
            var obj = new GameObject(asset);
            obj.AddComponent<MeshFilter>().sharedMesh = mesh;
            obj.AddComponent<MeshRenderer>().sharedMaterial = material;
            if (rock) { obj.layer = 6; obj.AddComponent<MeshCollider>().sharedMesh = mesh; }
            return obj;
        }

        public static Mesh Fish()
        {
            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            const int rings = 18, sides = 12;
            for (int ring = 0; ring <= rings; ring++)
            {
                float t = ring / (float)rings;
                float radius = .012f + Mathf.Pow(Mathf.Sin(t * Mathf.PI), .85f) * .14f;
                for (int side = 0; side < sides; side++)
                {
                    float angle = side * Mathf.PI * 2 / sides;
                    vertices.Add(new Vector3(Mathf.Cos(angle) * radius * .63f, Mathf.Sin(angle) * radius, Mathf.Lerp(-.34f, .5f, t)));
                    uv.Add(new Vector2(side / (float)sides, t));
                    if (ring == 0) continue;
                    int a = (ring - 1) * sides + side, b = (ring - 1) * sides + (side + 1) % sides;
                    int c = ring * sides + side, d = ring * sides + (side + 1) % sides;
                    triangles.AddRange(new[] { a, b, c, b, d, c });
                }
            }
            Fin(vertices, uv, triangles, new Vector3(0, 0, -.32f), new Vector3(0, .2f, -.5f), new Vector3(0, 0, -.44f));
            Fin(vertices, uv, triangles, new Vector3(0, 0, -.32f), new Vector3(0, 0, -.44f), new Vector3(0, -.2f, -.5f));
            Fin(vertices, uv, triangles, new Vector3(0, .1f, -.15f), new Vector3(0, .25f, -.2f), new Vector3(0, .13f, .2f));
            Fin(vertices, uv, triangles, new Vector3(.06f, -.02f, .12f), new Vector3(.22f, -.08f, -.12f), new Vector3(.05f, -.08f, -.1f));
            Fin(vertices, uv, triangles, new Vector3(-.06f, -.02f, .12f), new Vector3(-.05f, -.08f, -.1f), new Vector3(-.22f, -.08f, -.12f));
            var mesh = Finish(vertices, triangles); mesh.SetUVs(0, uv); mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 2f);
            return mesh;
        }

        private static void Fin(List<Vector3> vertices, List<Vector2> uv, List<int> triangles, Vector3 a, Vector3 b, Vector3 c)
        {
            int start = vertices.Count;
            vertices.AddRange(new[] { a, b, c, a, c, b });
            for (int i = 0; i < 6; i++) { uv.Add(Vector2.zero); triangles.Add(start + i); }
        }

        private static Mesh Rock(int seed)
        {
            const int rings = 16, sides = 24;
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int ring = 0; ring <= rings; ring++) for (int side = 0; side < sides; side++)
            {
                float latitude = ring / (float)rings * Mathf.PI, longitude = side * Mathf.PI * 2 / sides;
                Vector3 direction = new Vector3(Mathf.Sin(latitude) * Mathf.Cos(longitude), Mathf.Cos(latitude), Mathf.Sin(latitude) * Mathf.Sin(longitude));
                float noise = Mathf.PerlinNoise(direction.x * 4 + seed * 3, direction.z * 4 + direction.y * 3 + 12);
                float radius = .8f + noise * .3f + Mathf.Sin(direction.y * 27 + seed) * .025f;
                vertices.Add(Vector3.Scale(direction * radius, new Vector3(1, .68f, .82f)));
                if (ring == 0) continue;
                int a = (ring - 1) * sides + side, b = (ring - 1) * sides + (side + 1) % sides;
                int c = ring * sides + side, d = ring * sides + (side + 1) % sides;
                triangles.AddRange(new[] { a, b, c, b, d, c });
            }
            return Finish(vertices, triangles);
        }

        private static Mesh Coral(string species)
        {
            int seed = 0; foreach (char c in species) seed = seed * 31 + c;
            var random = new System.Random(seed);
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            Branch(vertices, triangles, random, Vector3.zero, Vector3.up, .7f, .08f, 3);
            return Finish(vertices, triangles);
        }

        private static void Branch(List<Vector3> vertices, List<int> triangles, System.Random random, Vector3 start, Vector3 direction, float length, float radius, int depth)
        {
            const int sides = 7;
            int index = vertices.Count;
            Vector3 end = start + direction * length;
            Quaternion basis = Quaternion.LookRotation(direction);
            for (int ring = 0; ring < 2; ring++) for (int side = 0; side < sides; side++)
            {
                float angle = side * Mathf.PI * 2 / sides;
                vertices.Add((ring == 0 ? start : end) + basis * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius * (ring == 0 ? 1f : .5f));
            }
            for (int side = 0; side < sides; side++)
            {
                int a = index + side, b = index + (side + 1) % sides, c = a + sides, d = b + sides;
                triangles.AddRange(new[] { a, b, c, b, d, c });
            }
            int cap = vertices.Count; vertices.Add(end);
            for (int side = 0; side < sides; side++) triangles.AddRange(new[] { cap, index + sides + side, index + sides + (side + 1) % sides });
            if (depth == 0) return;
            for (int child = 0; child < 3; child++)
            {
                float angle = child * Mathf.PI * 2 / 3 + (float)random.NextDouble();
                Vector3 next = (direction * .65f + new Vector3(Mathf.Cos(angle) * .6f, .7f, Mathf.Sin(angle) * .6f)).normalized;
                Branch(vertices, triangles, random, end, next, length * .65f, radius * .55f, depth - 1);
            }
        }

        private static Mesh Finish(List<Vector3> vertices, List<int> triangles)
        {
            var mesh = new Mesh(); mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
    }
}
