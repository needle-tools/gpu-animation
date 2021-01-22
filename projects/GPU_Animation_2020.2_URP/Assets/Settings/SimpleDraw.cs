using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SimpleDraw : MonoBehaviour
{
    public Mesh Mesh;
    public Material Mat;

    private void Update()
    {
        Graphics.DrawMesh(Mesh, transform.localToWorldMatrix, Mat, 0);
    }
}
