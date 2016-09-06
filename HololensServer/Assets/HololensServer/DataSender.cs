// DataSender.cs
using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
///  Broadcasts depth data and color data over the network.
///  Requires CustomMessagesPointCloud.cs
/// </summary>
public class DataSender : Singleton<DataSender> {
    
    public GameObject DataManager;
    private ServerDataManager _DataManager;

    private ushort[] _DepthData;
    private byte[] _ColorData;

    // Broadcasted message must have an identifying ID
    public enum MsgID : byte
    {
        GENERAL,  // Frame width/height
        DEPTH1,   // Depth 
        DEPTH2,
        RED,      // Red color channel
        GREEN,    // Green color channel
        BLUE      // Blue color channel

    }

    void Update() {
        
        if (DataManager == null)
        {
            return;
        }

        _DataManager = DataManager.GetComponent<ServerDataManager>();
        if (_DataManager == null)
        {
            return;
        }

        if (_DataManager.isReaderClosed())
        {
            return;
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


        // 1) Send general data
        //CustomMessagesPointCloud.Instance.SendGeneralData(_DataManager.DSPWidth, _DataManager.DSPHeight);
        CustomMessagesPointCloud.Instance.SendDepthData(_DepthData);

        /*
        Debug.Log("DSPWidth: " + _DataManager.DSPWidth);
        Debug.Log("DSPHeight: " + _DataManager.DSPHeight);*/

        //CustomMessagesPointCloud.Instance.SendColorData(_ColorData);

        // 2) Send depth data 1

        // 3) Send depth data 2

        // 4) Send R data

        // 5) Send G data

        // 6) Send B data




    }
}