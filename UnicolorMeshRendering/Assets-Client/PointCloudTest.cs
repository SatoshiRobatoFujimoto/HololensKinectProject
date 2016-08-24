using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PointCloudTest : MonoBehaviour
{

    // Mesh parameters
    private Mesh _MeshTest;
    public int length;

    private Vector3[] points;
    private int[] index;
    private Color[] colors;


    void Start()
    {

        points = new Vector3[length];
        index = new int[length];
        colors = new Color[length];

        _MeshTest = new Mesh();
        GetComponent<MeshFilter>().mesh = _MeshTest;
        CreateMeshTest();

    }

    void CreateMeshTest()
    {
        int s = 240;
        for (int i = 0; i < length; ++i)
        {
            points[i] = new Vector3(UnityEngine.Random.Range(-s, s),
                        UnityEngine.Random.Range(-s, s),
                        UnityEngine.Random.Range(-s, s));
            index[i] = i;
            colors[i] = new Color(0, 0, 1, 1);
            /*colors[i] = new Color(UnityEngine.Random.Range(0.0f, 1.0f),
                        UnityEngine.Random.Range(0.0f, 1.0f),
                        UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);*/
        }

        _MeshTest.vertices = points;
        _MeshTest.colors = colors;
        _MeshTest.SetIndices(index, MeshTopology.Points, 0);

    }

    void Update()
    {
        RefreshMesh();
        //RefreshBlackMesh();
    }


    void RefreshMesh()
    {

        for (int i = 0; i < length; ++i)
        {
            points[i] = new Vector3(UnityEngine.Random.Range(-10, 10),
                        UnityEngine.Random.Range(-10, 10),
                        UnityEngine.Random.Range(-10, 10));
            index[i] = i;
            /*colors[i] = new Color(UnityEngine.Random.Range(0.0f, 1.0f),
                        UnityEngine.Random.Range(0.0f, 1.0f),
                        UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);*/
            colors[i] = new Color(1, 0, 0, 1);
        }

        _MeshTest.vertices = points;
        _MeshTest.colors = colors;
        _MeshTest.SetIndices(index, MeshTopology.Points, 0);

    }

    void RefreshBlackMesh()
    {
        for (int i = 0; i < length; ++i)
        {
            points[i] = new Vector3(UnityEngine.Random.Range(-10, 10),
                        UnityEngine.Random.Range(-10, 10),
                        UnityEngine.Random.Range(-10, 10));
            index[i] = i;
            colors[i] = new Color(0,0,0,1);
        }

        _MeshTest.vertices = points;
        _MeshTest.colors = colors;
        _MeshTest.SetIndices(index, MeshTopology.Points, 0);

    }


}
