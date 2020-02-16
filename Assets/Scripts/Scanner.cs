// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;

public class Scanner : MonoBehaviour
{   


    public void setMesh(Mesh mesh, Vector3[] points)
    {
        Vector2[] points2D = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            points2D[i] = points[i];
        }
        
        mesh.vertices = points;
        int[] triangles = new int[(points.Length - 2) * 3];

        for (int i = 0; i < points.Length - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }
        mesh.triangles = triangles;

        Vector2[] uvs = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < points.Length; i++)
        {
            if ((i % 2) == 0)
            {
                uvs[i] = new Vector2(0, 0);
            }
            else
            {
                uvs[i] = new Vector2(1, 1);
            }
        }

        mesh.uv = uvs;

        mesh.RecalculateNormals();

        MeshFilter filter = GetComponent<MeshFilter>();
        filter.mesh = mesh;
    }
}
