using System.Collections;
using System.Collections.Generic;
using csDelaunay;
using UnityEngine;


 public class MeshHelper
 {
     static List<Vector3> vertices;
     static List<Vector3> normals;
     static List<Vector2> uv;
     static List<Vector2> uv2;


     static List<int> indices;
     static Dictionary<uint,int> newVectices;
 
     static int GetNewVertex(int i1, int i2)
     {
         int newIndex = vertices.Count;

         uint t1 = ((uint)i1 << 16) | (uint)i2;
         uint t2 = ((uint)i2 << 16) | (uint)i1;
         if (newVectices.ContainsKey(t2))
             return newVectices[t2];
         if (newVectices.ContainsKey(t1))
             return newVectices[t1];
 
         newVectices.Add(t1,newIndex);
 
         vertices.Add((vertices[i1] + vertices[i2]) * 0.5f);
         if (normals.Count>0)
             normals.Add((normals[i1] + normals[i2]).normalized);
         if (uv.Count>0)
             uv.Add((uv[i1] + uv[i2])*0.5f);
         if (uv2.Count>0)
             uv2.Add((uv2[i1] + uv2[i2])*0.5f);
 
         return newIndex;
     }

     static void InitArrays(Mesh mesh)
     {
         vertices = new List<Vector3>(mesh.vertices);
         normals = new List<Vector3>(mesh.normals);
         uv  = new List<Vector2>(mesh.uv);
         uv2 = new List<Vector2>(mesh.uv2);
         indices = new List<int>();
     }

     public static void Subdivide(Mesh mesh)
     {
         newVectices = new Dictionary<uint,int>();
 
        InitArrays(mesh);
 
        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i1 = triangles[i + 0];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];
 
            int a = GetNewVertex(i1, i2);
            int b = GetNewVertex(i2, i3);
            int c = GetNewVertex(i3, i1);
            indices.Add(i1);   indices.Add(a);   indices.Add(c);
            indices.Add(i2);   indices.Add(b);   indices.Add(a);
            indices.Add(i3);   indices.Add(c);   indices.Add(b);
            indices.Add(a );   indices.Add(b);   indices.Add(c); // center triangle
        }
        mesh.vertices = vertices.ToArray();
        if (normals.Count > 0)
            mesh.normals = normals.ToArray();
        if (uv.Count>0)
            mesh.uv = uv.ToArray();
        if (uv2.Count>0)
            mesh.uv2 = uv2.ToArray();
 
        mesh.triangles = indices.ToArray();
 
        CleanUp();
    }
 
   
     static void CleanUp()
     {
         vertices = null;
         normals = null;
         uv  = null;
         uv2 = null;
         indices = null;
     }
 }