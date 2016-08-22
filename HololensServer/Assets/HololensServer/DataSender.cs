// DataSender.cs
using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Broadcasts depth data and color data over the network.
///  Requires CustomMessagesPointCloud.cs
/// </summary>
public class DataSender : Singleton<DataSender> {
    
    public GameObject DataManager;
    private ServerDataManager _DataManager;

    private ushort[] _DepthData;
    private byte[] _ColorData;

    private bool firstPass = true;

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
        
        if (firstPass) // If first time transmitting, send general info 
        {
            CustomMessagesPointCloud.Instance.SendGeneralData(_DataManager.ClipWidth, _DataManager.ClipHeight);
            firstPass = false;
        } else         // Else just send depth & RGB each frame
        {
            CustomMessagesPointCloud.Instance.SendDepthData(_DepthData);
            CustomMessagesPointCloud.Instance.SendColorData(_ColorData);
        }

    }
}