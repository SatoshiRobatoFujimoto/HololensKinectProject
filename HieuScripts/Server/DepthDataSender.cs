/*
 * DepthDataSender.cs
 *
 * Broadcasts depth data over the network
 * Requires CustomMessages3.cs
 */

using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;

public class DepthDataSender : Singleton<DepthDataSender>
{
    public MultiSourceManager _MultiManager;

    void Update()
    {
        if (_MultiManager == null)
        {
            return;
        }

        ushort[] depthData = _MultiManager.GetDepthData();

        if (depthData == null)
        {
            return;
        }
        Message.Instance.SendDepthData(depthData);
        Message.Instance.SendDataSize(depthData.Length);
    }
}
