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

// Receives the body data messages
public class DataReceiver : Singleton<DataReceiver> {
    
    private int _ClipWidth;
    private int _ClipHeight;
    private ushort[] _DepthData;

    private float[] _RedColorData;
    private float[] _GreenColorData;
    private float[] _BlueColorData;

    private int depthIndex = 0;

    enum State
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

    public bool ReceivedAll { get; private set; }
    
    public int GetClipWidth()
    {
        return _ClipWidth;
    }

    public int GetClipHeight()
    {
        return _ClipHeight;
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

    public ushort[] GetDepthData()
    {
        return _DepthData;
    }
    
    void Start() {
        
        CustomMessagesPointCloud.Instance.MessageHandlers[CustomMessagesPointCloud.TestMessageID.StartID] = this.ReceiveData;

    }

    // Called when reading in Kinect data
    void ReceiveData(NetworkInMessage msg) {

        byte msgID = msg.ReadByte();

        switch (currentState)
        {
            case State.WaitingForGeneral:

                if (msgID != (byte)MsgID.GENERAL)
                {
                    Debug.Log("ERROR. Initial message not received.");
                }

                _ClipWidth = msg.ReadInt32();
                _ClipHeight = msg.ReadInt32();

                int vertices = _ClipWidth * _ClipHeight;
                _DepthData = new ushort[vertices];
                _RedColorData = new float[vertices];
                _GreenColorData = new float[vertices];
                _BlueColorData = new float[vertices];
                
                currentState = State.WaitingForRed;

                break;

            case State.WaitingForDepth1:

                if (msgID != (byte)MsgID.DEPTH1)
                {

                    currentState = State.WaitingForDepth1;

                } else
                {

                    int length = msg.ReadInt32();

                    for (int i = 0; i < length; i++)
                    {
                        _DepthData[i] = (ushort)msg.ReadInt16();
                        depthIndex++;
                    }

                    currentState = State.WaitingForDepth2;

                }

                break;

            case State.WaitingForDepth2:

                if (msgID != (byte)MsgID.DEPTH2)
                {

                    currentState = State.WaitingForDepth1;

                }
                else
                {

                    int length = msg.ReadInt32();
                    for (int i = depthIndex; i < length; i++)
                    {
                        _DepthData[i] = (ushort)msg.ReadInt16();
                    }

                    currentState = State.WaitingForRed;

                }

                break;

            case State.WaitingForRed:

                if (msgID != (byte)MsgID.RED)
                {

                    currentState = State.WaitingForDepth1;

                }
                else
                {

                    int length = msg.ReadInt32();
                    for (int i = 0; i < length; i++)
                    {
                        _RedColorData[i] = (float)msg.ReadByte() / 255f;
                    }

                    currentState = State.WaitingForGreen;

                }

                break;

            case State.WaitingForGreen:

                if (msgID != (byte)MsgID.GREEN)
                {

                    currentState = State.WaitingForDepth1;

                }
                else
                {

                    int length = msg.ReadInt32();
                    for (int i = 0; i < length; i++)
                    {
                        _GreenColorData[i] = (float)msg.ReadByte() / 255f;
                    }

                    currentState = State.WaitingForBlue;

                }

                break;

            case State.WaitingForBlue:

                if (msgID == (byte)MsgID.BLUE){

                    int length = msg.ReadInt32();
                    for (int i = 0; i < length; i++)
                    {
                        _BlueColorData[i] = (float)msg.ReadByte() / 255f;
                    }

                }

                currentState = State.WaitingForDepth1;

                break;

        }
    

    }

}