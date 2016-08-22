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
    

    public int width { get; private set; }
    public int height { get; private set; }
    //public bool refreshMesh { get; private set; }

    private short[] _DepthData;
    private float[] _RedColorData;
    private float[] _GreenColorData;
    private float[] _BlueColorData;

    private int depthIndex;

    private bool generalReceived = false;

    public enum State
    {
        WaitingForGeneral,
        WaitingForDepth1,
        WaitingForDepth2,
        WaitingForRed,
        WaitingForGreen,
        WaitingForBlue
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

    State currentState = State.WaitingForGeneral;

    public bool InitDone()
    {
        return (currentState != State.WaitingForGeneral);
    }

    public float[] GetRedColorData()
    {
        return _RedColorData;
    }

    public float[] GetGreenColorData()
    {
        return _GreenColorData;
    }

    public float[] GetBlueColorData()
    {
        return _BlueColorData;
    }

    public short[] GetDepthData()
    {
        return _DepthData;
    }
    
    void Start() {

        Debug.Log("Data Receiver Working!");
        CustomMessagesPointCloud.Instance.MessageHandlers[CustomMessagesPointCloud.TestMessageID.StartID] = this.ReceiveData;

    }

    // Called when reading in Kinect data
    void ReceiveData(NetworkInMessage msg) {

        // 1) Read message ID type
        byte msgID = msg.ReadByte();
        currentState = (DataReceiver.State)msgID;

        Debug.Log("Current msg id: " + msgID);

        float t0 = DateTime.Now.Millisecond;
        float t1;

        int length = 0;

        switch (currentState)
        {
            case State.WaitingForGeneral:

                /*if (msgID != (byte)MsgID.GENERAL)
                {
                    Debug.Log("ERROR. Initial message not received.");
                    currentState = State.WaitingForGeneral;
                }
                else
                {*/
                    if (!generalReceived)
                    {

                        // 2) Read message length
                        length = msg.ReadInt32();
                        Debug.Log("Message length: " + length);

                        // 3) Read the data 
                        width = msg.ReadInt32();
                        height = msg.ReadInt32();

                        int vertices = width * height;

                        _DepthData = new short[vertices];
                        _RedColorData = new float[vertices];
                        _GreenColorData = new float[vertices];
                        _BlueColorData = new float[vertices];


                        currentState = State.WaitingForDepth1;

                        Debug.Log("Width: " + width);
                        Debug.Log("Height: " + height);

                        Debug.Log("Success! Initial msg processed.");

                        t1 = DateTime.Now.Millisecond - t0;
                        Debug.Log("Time for general:" + t1);

                        generalReceived = true;

                    //}

                }

                break;

            case State.WaitingForDepth1:

               /*if (msgID != (byte)MsgID.DEPTH1)
                {

                    Debug.Log("ERROR. Depth1 data not received.");
                    currentState = State.WaitingForGeneral;
                    return;

                } else
                {*/

                    // 2) Read msg length
                    length = msg.ReadInt32();

                    Debug.Log("Message length: " + length);

                    depthIndex = 0;    
                // 3) Read the data
                    for (int i = 0; i < length; i++)
                    {
                        _DepthData[i] = msg.ReadInt16();
                        depthIndex++;
                    }

                    Debug.Log("index: " + depthIndex);

                    currentState = State.WaitingForDepth2;
                    //currentState = State.WaitingForGeneral;
                    
                    Debug.Log("Success! Depth1 msg processed.");
                    Debug.Log("First value: " + _DepthData[140]);

                    t1 = DateTime.Now.Millisecond - t0;
                    Debug.Log("Time for depth1:" + t1);

                //}

                break;

            case State.WaitingForDepth2:

               /* if (msgID != (byte)MsgID.DEPTH2)
                {
                    Debug.Log("ERROR. Depth2 data not received.");
                    currentState = State.WaitingForGeneral;

                }
                else
                {*/

                    length = msg.ReadInt32();
                    for (int i = depthIndex; i < length; i++)
                    {
                        _DepthData[i] = msg.ReadInt16();
                    }

                    Debug.Log("last depth data 2: " + _DepthData[length]);
                    Debug.Log("Success! Depth2 msg processed.");
                    currentState = State.WaitingForGeneral;

               // }

                break;

                /*
            case State.WaitingForRed:

                if (msgID != (byte)MsgID.RED)
                {

                    currentState = State.WaitingForGeneral;

                }
                else
                {

                    int length = msg.ReadInt32();
                    for (int i = 0; i < length; i++)
                    {
                        _RedColorData[i] = ( (float)(uint)msg.ReadByte() ) / 255f;
                    }

                    currentState = State.WaitingForGreen;

                }

                break;

            case State.WaitingForGreen:

                if (msgID != (byte)MsgID.GREEN)
                {

                    currentState = State.WaitingForGeneral;

                }
                else
                {

                    int length = msg.ReadInt32();
                    for (int i = 0; i < length; i++)
                    {
                        _GreenColorData[i] = ((float)(uint)msg.ReadByte()) / 255f;
                    }

                    currentState = State.WaitingForBlue;

                }

                break;

            case State.WaitingForBlue:

                if (msgID == (byte)MsgID.BLUE){

                    int length = msg.ReadInt32();
                    for (int i = 0; i < length; i++)
                    {
                        _BlueColorData[i] = ((float)(uint)msg.ReadByte()) / 255f;
                    }

                }

                currentState = State.WaitingForGeneral;

                break;
                */

        }
    

    }

}