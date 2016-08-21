using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PointCloudRender : MonoBehaviour
{

    public GameObject DataManager;

    // Mesh parameters
    private const int _DownsampleSize = 4; // to render less than 65k points
    private Mesh _Mesh;
    private Vector3[] _Vertices;
    private Color[] _Colors;
    private int[] _Index;

    // Kinect parsed data
    private DataManager _DataManager;
    private Vector3[] _CameraData;
    private float[] _ColorData;
    private int width;  // frame width
    private int height; // frame height

    private const int BYTES_PER_PIXEL = 4;

    bool refresh = true;

    void Start()    
    {
        _DataManager = GetComponent<DataManager>();
        if (_DataManager != null)
        {
            if (_DataManager.initialized)
            {
                width = _DataManager.frameWidth;
                height = _DataManager.frameHeight;

                // create a fixed size mesh
                // mesh vertex position/color will be updated every frame
                _Mesh = new Mesh();
                GetComponent<MeshFilter>().mesh = _Mesh;
                CreateMesh(width, height);
            }
        }
    }

    void CreateMesh(int width, int height)
    {
        _Vertices = new Vector3[width * height];
        _Index = new int[width * height];
        _Colors = new Color[width * height];

        for (int x = 0; x < width; x++ )
        {
            for (int y = 0; y < height; y++ )
            {
                int i = (y * width) + x;
                _Index[i] = i;

                _Vertices[i] = new Vector3(x, -y, 0);

                _Colors[i] = new Color(0,0,0,1);
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

        _Mesh.vertices = _Vertices;
        _Mesh.colors = _Colors;
        _Mesh.SetIndices(_Index, MeshTopology.Points, 0);

    }

    void Update()
    {
        
        _DataManager = DataManager.GetComponent<DataManager>();
        if (_DataManager == null)
        {
            return;
        }
        
        if (!_DataManager.initialized)
        {
            return;
        }

        if (_Mesh == null)
        {
            width = _DataManager.frameWidth;
            height = _DataManager.frameHeight;
            _Mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _Mesh;
            CreateMesh(width, height);
        }

        _CameraData = _DataManager.GetCameraData();
        if (_CameraData == null)
        {
            return;
        }

        _ColorData = _DataManager.GetColorData();
        if (_ColorData == null )
        {
            return;
        }

        RefreshMesh(width, height);

    }

    void RefreshMesh(int width, int height)
    {

        int colorCount = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int i = (y * width) + x;
                _Index[i] = i;
                _Vertices[i].z = _CameraData[i].z;

                /*
                _Colors[i] = new Color(UnityEngine.Random.Range(0.0f, 1.0f),
                                       UnityEngine.Random.Range(0.0f, 1.0f),
                                       UnityEngine.Random.Range(0.0f, 1.0f), 1.0f);
                                       */
               
                if (colorCount <= ( (width*height) - BYTES_PER_PIXEL) )
                {

                    _Colors[i] = new Color( _ColorData[i + 0],
                                            _ColorData[i + 1],
                                            _ColorData[i + 2],
                                            1.0f);

                    colorCount += BYTES_PER_PIXEL;
                }
                

            }
        }

        _Mesh.vertices = _Vertices;
        _Mesh.colors = _Colors;
        _Mesh.SetIndices(_Index, MeshTopology.Points, 0);

    }

}
