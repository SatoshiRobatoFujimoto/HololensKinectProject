using UnityEngine;
using HoloToolkit.Unity;
using System.Collections;
using System;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PointCloudRender : Singleton<PointCloudRender> 
{
    
    // Mesh parameters
    private Mesh _Mesh;
    private Vector3[] _Vertices;
    private Color[] _Colors;
    private int[] _Index;

    // Kinect parsed data
    //private DataReceiver _DataReceiver;
    private short[] _DepthData;
    private byte[] _RedColorData;
    private byte[] _GreenColorData;
    private byte[] _BlueColorData;

    private int _Width; 
    private int _Height; 

    private const int BYTES_PER_PIXEL = 4;

   void Start()
    {
        Debug.Log("Renderer started.");
    }

    public void CreateMesh(int width, int height)
    {
        
        _Width = width;
        _Height = height;

        // create a fixed size mesh
        // mesh vertex position/color will be updated every frame
        _Mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _Mesh;

        _Vertices = new Vector3[_Width * _Height];
        _Index = new int[_Width * _Height];
        _Colors = new Color[_Width * _Height];

        
        for (int x = 0; x < _Width; x++)
        {
            for (int y = 0; y < _Height; y++)
            {
                int i = (y * _Width) + x;
                _Index[i] = i;

                _Vertices[i] = new Vector3(x, -y, -60000.0f);

                _Colors[i] = new Color(0, 0, 0, 0);
            }
        }
       
        _Mesh.vertices = _Vertices;
        _Mesh.colors = _Colors;
        _Mesh.SetIndices(_Index, MeshTopology.Points, 0);

    }

    public void RefreshMesh()
    {

        for (int x = 0; x < _Width; x++)
        {
            for (int y = 0; y < _Height; y++)
            {

                int i = (y * _Width) + x;
                _Index[i] = i;

                if (_DepthData[i] >= 1000 || (_DepthData[i] <= 100))
                {
                    _Colors[i] = new Color(0.0f, 0.0f, 0.0f, 0.0f); // don't render
                    _Vertices[i].z = -60000.0f;
                }
                else
                {
                    _Vertices[i].z = (float) _DepthData[i];
                    _Colors[i] = new Color(UnityEngine.Random.Range(0.0f, 1.0f),
                                       UnityEngine.Random.Range(0.0f, 1.0f),
                                       UnityEngine.Random.Range(0.0f, 1.0f),
                                       1.0f);
                }

            }
        }

        _Mesh.vertices = _Vertices;
        _Mesh.colors = _Colors;
        _Mesh.SetIndices(_Index, MeshTopology.Points, 0);

    }

}

/*
_Colors[i] = new Color(
    _RedColorData[i],
    _GreenColorData[i],
    _BlueColorData[i],
    1f);
*/

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


/*
int x = 0;
int y = 0;
int totalPoints = width * height;

for (int i = 0; i < totalPoints; i++)
{
x = i * (int)Math.Ceiling( (double) (1 - width) / (1 - width * height) );
y = i * (int)Math.Ceiling((double)(1 - height) / (1 - width * height));


_Index[i] = i;
_Vertices[i] = new Vector3(x, -y, 0);
_Colors[i] = new Color(0, 1, 0, 1);
}
*/
