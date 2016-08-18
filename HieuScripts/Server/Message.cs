/*
 * CustomMessages2.cs
 *
 * Allows for sending body data as custom messages to the Hololens
 * Requires a SharingStage GameObject
 */

using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;

public class Message : Singleton<Message>
{

    // <summary>
    // Message enum containing information bytes to share
    // The first message type has to start with UserMessageIDStart
    // so as not to conflict with HoloToolkit internal messages
    // </summary>
    public enum TestMessageID : byte
    {
        DepthData = MessageID.UserMessageIDStart,
        Max
    }

    public enum UserMessageChannels
    {
        Anchors = MessageChannel.UserMessageChannelStart,
    }

    // <summary>
    // Cache the local user's ID to use when sending messages
    // </summary>
    public long localUserID
    {
        get; set;
    }

    public delegate void MessageCallback(NetworkInMessage msg);
    private Dictionary<TestMessageID, MessageCallback> _MessageHandlers =
        new Dictionary<TestMessageID, MessageCallback>();

    public Dictionary<TestMessageID, MessageCallback> MessageHandlers
    {
        get
        {
            return _MessageHandlers;
        }
    }

    // <summary>
    // Helper object that we use to route incoming message callbacks to the member
    // functions of this class
    // </summary>
    NetworkConnectionAdapter connectionAdapter;

    // <summary>
    // Cache the connection object for the sharing service
    // </summary>
    NetworkConnection serverConnection;

    void Start()
    {
        InitializeMessageHandlers();
    }

    void InitializeMessageHandlers()
    {

        SharingStage sharingStage = SharingStage.Instance;

        if (sharingStage == null)
        {
            Debug.Log("Cannot Initialize CustomMessages. No SharingStage instance found.");
            return;
        }

        serverConnection = sharingStage.Manager.GetServerConnection();
        if (serverConnection == null)
        {
            Debug.Log("Cannot initialize CustomMessages. Cannot get a server connection.");
            return;
        }

        connectionAdapter = new NetworkConnectionAdapter();
        connectionAdapter.MessageReceivedCallback += OnMessageReceived;

        // <summary>
        // Cache the local user ID
        // </summary>
        this.localUserID = SharingStage.Instance.Manager.GetLocalUser().GetID();

        for (byte index = (byte)TestMessageID.DepthData; index < (byte)TestMessageID.Max; index++)
        {

            if (MessageHandlers.ContainsKey((TestMessageID)index) == false)
            {
                MessageHandlers.Add((TestMessageID)index, null);
            }

            serverConnection.AddListener(index, connectionAdapter);
        }
    }

    private NetworkOutMessage CreateMessage(byte MessageType)
    {
        NetworkOutMessage msg = serverConnection.CreateMessage(MessageType);
        msg.Write(MessageType);
        return msg;
    }

    // Send size of the depth data. -> CHECK!!!
    public void SendDataSize(int size)
    {
        // If we are connected to a session, broadcast our info
        if (this.serverConnection != null && this.serverConnection.IsConnected())
        {
            // Create an outgoing network message to contain all the info we want to send
            NetworkOutMessage msg = CreateMessage((byte)TestMessageID.DepthData);               //check

            msg.Write(size);

            // Send the message as a broadcast
            this.serverConnection.Broadcast(
                msg,
                MessagePriority.Immediate,
                MessageReliability.UnreliableSequenced,
                MessageChannel.Avatar);
        }
    }

    // <summary>
    // Send depth data
    // </summary>
    public void SendDepthData(ushort[] DepthData)
    {
        // If we are connected to a session, broadcast our info
        if (this.serverConnection != null && this.serverConnection.IsConnected())
        {
            // Create an outgoing network message to contain all the info we want to send
            NetworkOutMessage msg = CreateMessage((byte)TestMessageID.DepthData);               //check

            //    msg.Write(trackingID);   -> msg.Write((byte)colorData);
            foreach (ushort data in DepthData)
            {
                AppendUshort(msg, data);
            }

            // Send the message as a broadcast
            this.serverConnection.Broadcast(
                msg,
                MessagePriority.Immediate,
                MessageReliability.UnreliableSequenced,
                MessageChannel.Avatar);
        }
    }

    void OnDestroy()
    {
        if (this.serverConnection != null)
        {
            for (byte index = (byte)TestMessageID.DepthData; index < (byte)TestMessageID.Max; index++)
            {
                this.serverConnection.RemoveListener(index, this.connectionAdapter);
            }
            this.connectionAdapter.MessageReceivedCallback -= OnMessageReceived;
        }
    }

    void OnMessageReceived(NetworkConnection connection, NetworkInMessage msg)
    {

        byte messageType = msg.ReadByte();
        MessageCallback messageHandler = MessageHandlers[(TestMessageID)messageType];
        if (messageHandler != null)
        {
            messageHandler(msg);
        }
    }

    #region HelperFunctionsForWriting

    void AppendUshort(NetworkOutMessage msg, ushort data)
    {
        msg.Write(data);
    }

    #endregion HelperFunctionsForWriting

    #region HelperFunctionsForReading

    public short ReadDepth(NetworkInMessage msg)
    {
        return (short) msg.ReadInt16();  
    }

    #endregion HelperFunctionsForReading

    public double ReadSize(NetworkInMessage msg)
    {
        return msg.ReadDouble();
    }
}