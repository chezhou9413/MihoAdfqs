using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.Effects
{
    //护盾薄片与整组碎片只生成一次网格，所有人物共用。
    [StaticConstructorOnStartup]
    internal static class ShieldVfxMeshes
    {
        public const int ShardCount = 48;
        public static readonly Mesh Shell = CreateShell();
        public static readonly Mesh Shards = CreateShards();

        //按球面弧长排列薄片，边缘自然缩窄，每片保留曲面、窄倒角和侧壁。
        private static Mesh CreateShell()
        {
            const float radius = .061f;
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uv = new List<Vector2>();
            var cells = new List<Vector4>();
            var triangles = new List<int>();
            for (int row = -6; row <= 6; row++)
            for (int col = -6; col <= 6; col++)
            {
                var center = new Vector2((col + (row & 1) * .5f) * radius * 1.73205f, row * radius * 1.5f);
                if (center.sqrMagnitude > .56f * .56f) continue;
                Vector3 normal = DomeDirection(center);
                float seed = Mathf.Repeat((row + 7) * .618034f + (col + 7) * .381966f, 1);
                float relief = seed > .72f ? -.009f : .006f;
                Vector4 cell = new Vector4(normal.x, normal.z, seed, normal.y);
                int first = vertices.Count;
                vertices.Add(normal * (.5f + relief));
                normals.Add(normal); uv.Add(Vector2.zero); cells.Add(cell);
                for (int ring = 0; ring < 3; ring++)
                for (int corner = 0; corner < 6; corner++)
                {
                    float angle = (30 + corner * 60) * Mathf.Deg2Rad;
                    Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    float scale = ring == 0 ? .88f : .96f;
                    Vector2 delta = direction * (radius * scale);
                    Vector3 radial = DomeDirection(center + delta);
                    Vector3 outward = (radial - normal * Vector3.Dot(radial, normal)).normalized;
                    var point = radial * (.5f + relief - (ring == 0 ? 0 : ring == 1 ? .004f : .012f));
                    vertices.Add(point);
                    normals.Add(ring == 0 ? radial : (radial * (ring == 1 ? .65f : .15f) + outward).normalized);
                    uv.Add(direction * scale); cells.Add(cell);
                }
                for (int corner = 0; corner < 6; corner++)
                {
                    int next = (corner + 1) % 6;
                    triangles.Add(first); triangles.Add(first + 1 + next); triangles.Add(first + 1 + corner);
                    for (int ring = 0; ring < 2; ring++)
                    {
                        int a = first + 1 + ring * 6 + corner;
                        int b = first + 1 + ring * 6 + next;
                        int c = first + 7 + ring * 6 + corner;
                        int d = first + 7 + ring * 6 + next;
                        triangles.Add(a); triangles.Add(b); triangles.Add(c);
                        triangles.Add(b); triangles.Add(d); triangles.Add(c);
                    }
                }
            }
            var mesh = new Mesh { name = "MihoShieldEnergyPlates" };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uv); mesh.SetUVs(1, cells);
            mesh.SetTriangles(triangles, 0);
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 2f);
            mesh.UploadMeshData(true);
            return mesh;
        }

        //平面坐标表示沿球面的弧长，越靠轮廓投影越窄；背面余量交给Shader裁切。
        private static Vector3 DomeDirection(Vector2 point)
        {
            float distance = point.magnitude;
            if (distance < .00001f) return Vector3.up;
            float angle = distance * Mathf.PI;
            float scale = Mathf.Sin(angle) / distance;
            return new Vector3(point.x * scale, Mathf.Cos(angle), point.y * scale);
        }

        //一张网格装入整组碎片，飞散与翻转由顶点Shader计算。
        private static Mesh CreateShards()
        {
            var vertices = new Vector3[ShardCount * 4];
            var normals = new Vector3[vertices.Length];
            var uv = new Vector2[vertices.Length];
            var cells = new List<Vector4>(vertices.Length);
            var triangles = new int[ShardCount * 6];
            for (int shard = 0; shard < ShardCount; shard++)
            {
                float seed = Mathf.Repeat(shard * .618034f, 1);
                for (int corner = 0; corner < 4; corner++)
                {
                    int index = shard * 4 + corner;
                    var point = new Vector2(corner >= 2 ? 1 : -1, corner == 1 || corner == 2 ? 1 : -1);
                    vertices[index] = new Vector3(point.x, 0, point.y);
                    normals[index] = Vector3.up; uv[index] = point;
                    cells.Add(new Vector4(shard, 0, seed, 0));
                }
                int start = shard * 4, tri = shard * 6;
                triangles[tri] = start; triangles[tri + 1] = start + 1; triangles[tri + 2] = start + 2;
                triangles[tri + 3] = start; triangles[tri + 4] = start + 2; triangles[tri + 5] = start + 3;
            }
            var mesh = new Mesh { name = "MihoShieldShatterBatch", vertices = vertices, normals = normals, uv = uv };
            mesh.SetUVs(1, cells); mesh.SetTriangles(triangles, 0);
            //包围盒包含顶点动画的最大位移，防止屏幕边缘提前裁掉碎片。
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 3.6f);
            mesh.UploadMeshData(true);
            return mesh;
        }
    }
}
