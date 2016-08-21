using UnityEngine;
using System.Collections;
using System.IO;

public class DataManager : MonoBehaviour {

    public string depthFile;
    public string colorFile;
    public string frameFile;
    //public string tableXFile;
    //public string tableYFile;

    //private Vector3[] _CameraData;
    private ushort[] _DepthData;
    private float[] _ColorData;
    private Vector2[] _TableData;

    public int frameWidth { get; private set; }
    public int frameHeight { get; private set; }
    public bool initialized { get; private set; }

    /*
    public Vector3[] GetCameraData()
    {
        return _CameraData;
    }
    */

    public ushort[] GetDepthData()
    {
        return _DepthData;
    }

    public float[] GetColorData()
    {
        return _ColorData;
    }

    /*
    public Vector2[] GetTableData()
    {
        return _TableData;
    }
    */

    // Use this for initialization
    void Start () {

        initialized = false;

        /*frameWidth = 130; // tableXReader.ReadInt32();
        frameHeight = 424; // tableXReader.ReadInt32();*/

        using (BinaryReader frameReader = new BinaryReader(File.Open(frameFile, FileMode.Open)))
        {
            frameWidth = frameReader.ReadInt32();
            frameHeight = frameReader.ReadInt32();
        }

        initialized = true;

        /*
        // read table x values
        using (BinaryReader tableXReader = new BinaryReader(File.Open(tableXFile, FileMode.Open)))
        {
            frameWidth = 130; // tableXReader.ReadInt32();
            frameHeight = 424; // tableXReader.ReadInt32();
            int sizeArray = tableXReader.ReadInt32();
            Debug.Log("Frame height: " + frameHeight);
            Debug.Log(sizeArray);
            _TableData = new Vector2[sizeArray];
            for (int i = 0; i < _TableData.Length; i++)
            {
                _TableData[i] = new Vector2(tableXReader.ReadInt32(), 0);
            }
            //tableXReader.Close();
        }

        // read table y values
        using (BinaryReader tableYReader = new BinaryReader(File.Open(tableYFile, FileMode.Open)))
        {
            for (int i = 0; i < _TableData.Length; i++)
            {
                _TableData[i].y = (float)tableYReader.ReadInt32();
            }
            //tableYReader.Close();
        }
        */

        Debug.Log("Frame width: " + frameWidth);
        // read depth text file
        using (BinaryReader depthReader = new BinaryReader(File.Open(depthFile, FileMode.Open)))
        {

            int sizeArray = depthReader.ReadInt32();
            Debug.Log(sizeArray);
            _DepthData = new ushort[sizeArray];

            for (int i = 0; i < _DepthData.Length; i++)
            {
                _DepthData[i] = (ushort) depthReader.ReadUInt16();
            }
            depthReader.Close();
        }

        // read color values
        using (BinaryReader colorReader = new BinaryReader(File.Open(colorFile, FileMode.Open)))
        {
            int sizeArray = colorReader.ReadInt32();
            _ColorData = new float[sizeArray];
            Debug.Log(sizeArray);
            for (int i = 0; i < _ColorData.Length; i++)
            {
                _ColorData[i] = (float)colorReader.ReadByte() / 255f;
                //float test = (float)colorReader.ReadByte();
            }
            //colorReader.Close();
        }
        
        /*
        _CameraData = new Vector3[frameHeight * frameWidth];
        // Reconstruct XYZ camera space table
        for (int x = 0; x < frameWidth; x++)
        {
            for (int y = 0; y < frameHeight; y++)
            {
                int index = (y * frameWidth) + x;
                Vector2 lutValue = _TableData[index];
                    ushort depth = _DepthData[index];
                    _CameraData[index] = new Vector3(
                        lutValue.x * depth * 0.001f,
                        lutValue.y * depth * 0.001f,
                        depth * 0.001f);

            }
       }
        */

    } // end start

    // Update is called once per frame
    void Update () {
	
	}

}
