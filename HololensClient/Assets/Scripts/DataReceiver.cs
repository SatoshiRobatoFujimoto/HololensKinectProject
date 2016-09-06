/*
 * DataReceiver.cs
 *
 * Receives depth and color data from the network
 * Requires CustomMessagesPointCloud.cs
 */

using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;
using System;

// Receives the body data messages
public class DataReceiver : Singleton<DataReceiver> {

    GameObject Sphere;

    // Kinect frame width and height
    public int _Width;
    public int _Height;

    public int MAX_PACKET_SIZE;

    // Arrays to store received data
    private short[] _DepthData;
    private byte[] _RedColorData;
    private byte[] _GreenColorData;
    private byte[] _BlueColorData;

    // Index for handling two array of depth data received
    private int depthIndex = 0;

    // Indicates whether general info has been received
    private bool generalReceived = false;

    // Mesh parameters
    private Mesh _Mesh;
    private Vector3[] _Vertices;
    private Color[] _Colors;
    private int[] _Index;
    
    private const int BYTES_PER_PIXEL = 4;

    // Current state of the rendering loop
    private enum State
    {
        WaitingForGeneral,
        CreateMesh,
        WaitingForDepth1,
        WaitingForDepth2,
        WaitingForRed,
        WaitingForGreen,
        WaitingForBlue,
        RefreshMesh
    }

    // Broadcasted message must have an identifying ID
    private enum MsgID : byte
    {
        GENERAL,  // Frame width/height
        DEPTH1,   // Depth 
        DEPTH2,
        RED,      // Red color channel
        GREEN,    // Green color channel
        BLUE      // Blue color channel
    }

    // Start waiting for general message
    //State currentState = State.WaitingForGeneral;
    State currentState = State.CreateMesh; // TODO

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public short[] GetDepthData()
    {
        return _DepthData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public byte[] GetRedColorData()
    {
        return _RedColorData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public byte[] GetGreenColorData()
    {
        return _GreenColorData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public byte[] GetBlueColorData()
    {
        return _BlueColorData;
    }
    
    /// <summary>
    /// 
    /// </summary>
    void Start() {

        int vertices = _Width * _Height;
        
        _DepthData = new short[vertices];
        _RedColorData = new byte[vertices];
        _GreenColorData = new byte[vertices];
        _BlueColorData = new byte[vertices];

        CustomMessagesPointCloud.Instance.MessageHandlers[CustomMessagesPointCloud.TestMessageID.StartID] = this.ReceiveData;

    }

    // Called when reading in Kinect data
    void ReceiveData(NetworkInMessage msg) {

        // 1) Read message ID type
        byte msgID = msg.ReadByte();
        //Debug.Log("Current msg id: " + msgID);

        int length = 0; // store message length

        switch (currentState)
        {
            case State.WaitingForGeneral:

                if (msgID != (byte)MsgID.GENERAL)
                {
                    //Debug.Log("ERROR. Initial message not received.");
                    currentState = State.WaitingForGeneral;
                    return;
                }

                // A mesh will only be created once. 
                // After the first pass, this state will be skipped 
                // to Waiting for Depth1
                if (!generalReceived)
                {
                    // 2) Read message length
                    length = msg.ReadInt32();

                    // 3) Read the data 
                    _Width = msg.ReadInt32();
                    _Height = msg.ReadInt32();

                    int vertices = _Width * _Height;

                    _DepthData = new short[vertices];
                    _RedColorData = new byte[vertices];
                    _GreenColorData = new byte[vertices];
                    _BlueColorData = new byte[vertices];

                    //t1 = DateTime.Now.Millisecond - t0;
                    //Debug.Log("Time for general:" + t1);
                    
                    generalReceived = true;
                    currentState = State.CreateMesh;

                    Debug.Log("Success! Initial msg processed.");

                    return;

                }

                currentState = State.WaitingForDepth1;

                break;

            case State.CreateMesh:

                Debug.Log("Creating mesh");
                CreateMesh();  // Create an empty mesh
                currentState = State.WaitingForDepth1;

                break;

            case State.WaitingForDepth1:

                if (msgID != (byte)MsgID.DEPTH1)
                {

                    //Debug.Log("ERROR. Depth1 data not received.");
                    currentState = State.WaitingForDepth1;
                    return;

                }

                // 2) Read msg length
                length = msg.ReadInt32();

                //depthIndex = 0;

                // 3) Read the data
                for (int i = 0; i < length; i++)
                {
                    _DepthData[i] = msg.ReadInt16();
                    //depthIndex++;
                }
                
                /*if ( (2 * length) > MAX_PACKET_SIZE ) // will need to read two messages
                {
                    currentState = State.WaitingForDepth2;
                } else
                {
                    currentState = State.RefreshMesh;
                }*/

                currentState = State.RefreshMesh;

                Debug.Log("Success! Depth1 msg processed.");

                break;

            case State.WaitingForDepth2:

                if (msgID != (byte)MsgID.DEPTH2)
                {
                    //Debug.Log("ERROR. Depth2 data not received.");
                    currentState = State.WaitingForDepth1;
                    return;

                }

                length = msg.ReadInt32();
                for (int i = depthIndex; i < length; i++)
                {
                    _DepthData[i] = msg.ReadInt16();
                }

                currentState = State.RefreshMesh;

                //Debug.Log("Success! Depth2 msg processed.");

                break;

            case State.RefreshMesh:

                RefreshMesh();
                currentState = State.WaitingForDepth1;

                Debug.Log("Refreshing mesh");
                
                break;

        }


    }


    void CreateMesh()
    {

        _Mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _Mesh;

        _Vertices = new Vector3[_Width * _Height];
        _Index = new int[_Width * _Height];
        _Colors = new Color[_Width * _Height];

        for (int x = -_Width/2; x < _Width/2; x++)
        {
            for (int y = -_Height/2; y < _Height/2; y++)
            {
                int i = ((y + _Height / 2) * _Width) + (x + _Width / 2);
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

    void RefreshMesh()
    {

        for (int x = -_Width / 2; x < _Width / 2; x++)
        {
            for (int y = -_Height / 2; y < _Height / 2; y++)
            {
                int i = ((y + _Height/2) * _Width) + (x + _Width/2);

                _Index[i] = i;

                if (_DepthData[i] >= 1000 || (_DepthData[i] <= 5))
                {
                    _Colors[i] = new Color(0, 0, 0, 0);
                    _Vertices[i].z = 60000f;
                }
                else
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