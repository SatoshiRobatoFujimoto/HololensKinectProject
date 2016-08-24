using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PointCloudRender : MonoBehaviour
{

    public GameObject DataManager;

    // Mesh parameters
    private Mesh _Mesh;
    private Vector3[] _Vertices;
    private Color[] _Colors;
    private int[] _Index;

    // Kinect parsed data
    private ServerDataManager _DataManager;
    private ushort[] _DepthData;
    private byte[] _ColorData;
    private int width;  // frame width
    private int height; // frame height
    private int _DownsampleSize;

    private const int BYTES_PER_PIXEL = 4;

    void Start()
    {
        _DataManager = GetComponent<ServerDataManager>();
        if (_DataManager == null)
        {
            return;
        }

        if (_DataManager.GetDownsampleSize() == 1)
        {
            return;
        }

        width = _DataManager.DSPWidth;
        height = _DataManager.DSPHeight;

        // create a fixed size mesh
        // mesh vertex position/color will be updated every frame
        _Mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _Mesh;
        CreateMesh(width, height);

    }

    void CreateMesh(int width, int height)
    {
        _Vertices = new Vector3[width * height];
        _Index = new int[width * height];
        _Colors = new Color[width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int i = (y * width) + x;
                _Index[i] = i;

                _Vertices[i] = new Vector3(x, -y, 0);

                _Colors[i] = new Color(0, 0, 0, 0);
            }
        }

        //Debug.Log("Num of vertices: " + _Vertices.Length);
        
        _Mesh.vertices = _Vertices;
        _Mesh.colors = _Colors;
        _Mesh.SetIndices(_Index, MeshTopology.Points, 0);

    }

    void Update()
    {

        _DataManager = DataManager.GetComponent<ServerDataManager>();
        if (_DataManager == null)
        {
            return;
        }

        if (_DataManager.GetDownsampleSize() == 1)
        {
            return;
        }

        if (_Mesh == null)
        {
            width = _DataManager.DSPWidth;
            height = _DataManager.DSPHeight;
            _Mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _Mesh;
            CreateMesh(width, height);
        }

        _DepthData = _DataManager.GetChosenDepthData();
        if (_DepthData == null)
        {
            return;
        }
       
        _ColorData = _DataManager.GetChosenColorData();
        if (_ColorData == null)
        {
            return;
        }

        RefreshMesh(width, height);

    }

    void RefreshMesh(int width, int height)
    {
       
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int i = (y * width) + x;

                _Index[i] = i;
                
                if (_DepthData[i] >= 1000 || (_DepthData[i] <= 10) )
                {
                    _Colors[i] = new Color(0, 0, 0, 0);
                    _Vertices[i].z = 60000f;
                } else
                {
                    _Colors[i] = new Color(0.0f, 1.0f, 0.0f, 1.0f);
                    _Vertices[i].z = _DepthData[i];
                }

            }
        }

        _Mesh.vertices = _Vertices;
        _Mesh.colors = _Colors;
        _Mesh.SetIndices(_Index, MeshTopology.Points, 0);

    }

}

/*for (int i = 0; i < points.Length; ++i)
{
    points[i] = new Vector3(UnityEngine.Random.Range(-10, 10), 
    UnityEngine.Random.Range(-10, 10), 
    UnityEngine.Random.Range(-10, 10));
    index[i] = i;
    colors[i] = new Color(UnityEngine.Random.Range(0.0f, 1.0f), 
    UnityEngine.Random.Range(0.0f, 1.0f), 
    UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);
}*/

/*
_Colors[i] = new Color(UnityEngine.Random.Range(0.0f, 1.0f),
                   UnityEngine.Random.Range(0.0f, 1.0f),
                   UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);
                   */
