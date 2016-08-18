/*
 * BodyDataReceiver.cs
 *
 * Receives body data from the network
 * Requires Message.cs
 */

using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;

public class DepthDataReceiver : MonoBehaviour {
    private ushort[] DepthData; //what is the size???? 

    public ushort[] GetDepthData()
    {
        return DepthData;
    }

    // Use this for initialization
    void Start()
    {
        Message.Instance.MessageHandlers[Message.TestMessageID.DepthData] =
            this.UpdateDepthData;
    }

    // Update is called once per frame
    void UpdateDepthData (NetworkInMessage msg)
    {  
        DepthData = new ushort[(int) Message.Instance.ReadSize(msg)];     // CHECK !!!  -> This should be called once.
        for (int i = 0; i < DepthData.Length; i++)
        {
           DepthData[i] += (ushort) Message.Instance.ReadDepth(msg);
        }
    }
}
