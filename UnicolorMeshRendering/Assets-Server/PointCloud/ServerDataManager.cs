// DataManager.cs
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;
using System.IO;

/// <summary>
/// Retrieves the Kinect depth [mm] and color [RGB32] data
/// </summary>
public class ServerDataManager : MonoBehaviour
{

    private bool subtractBackground = false;

    private KinectSensor _Sensor;
    private MultiSourceFrameReader _Reader;

    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }
    public int DepthHeight { get; private set; }
    public int DepthWidth { get; private set; }

    // Downsampled width and height
    public int DSPWidth { get; private set; }
    public int DSPHeight { get; private set; }

    public int minDepthX;              // min clipped depth index
    public int maxDepthX;              // max clipped depth index

    /// <summary>
    /// Store original Kinect data
    /// </summary>
    private CoordinateMapper _Mapper;
    private Texture2D _ColorTexture;
    private ushort[] _DepthData;
    private byte[] _ColorData;
    private ColorSpacePoint[] _ColorPoints;

    private Texture2D _ChosenColorTexture;
    private ushort[] _ChosenDepthData;
    private byte[] _ChosenColorData;
    private byte[] _BodyData;

    // Save background data to subtract 
    private ushort[] _SavedDepthData;
    private byte[] _SavedColorData;

    private int BYTES_PER_PIXEL;

    public int DownsampleSize = 1;

    public int MAX_PACKET_SIZE;

    public bool isReaderClosed()
    {
        return (_Reader == null);
    }

    public int GetDownsampleSize()
    {
        return DownsampleSize;
    }

    /// <summary>
    /// Get unmodified kinect color texture
    /// </summary>
    public Texture2D GetColorTexture()
    {
        return _ColorTexture;
    }

    /// <summary>
    /// Get modified color texture
    /// </summary>
    public Texture2D GetChosenColorTexture()
    {
        return _ChosenColorTexture;
    }

    /// <summary>
    /// Public getter for depth data.
    /// </summary>
    /// <returns> Unmodified Kinect depth data in mm. </returns>
    public ushort[] GetChosenDepthData()
    {
        return _ChosenDepthData;
    }

    /// <summary>
    /// Public getter for color data.
    /// </summary>
    /// <returns> Unmodified Kinect color data in RGBA32. </returns>
    public byte[] GetChosenColorData()
    {
        return _ChosenColorData;
    }

    public byte[] GetSavedColorData()
    {
        return _SavedColorData;
    }

    public ushort[] GetSavedDepthData()
    {
        return _SavedDepthData;
    }


    void Start()
    {
        if (MAX_PACKET_SIZE == 0)
        {
            MAX_PACKET_SIZE = 60000;
            Debug.Log("ERROR. Automatically assigning max message size to 60,000 bytes.");
        }

        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {

            _Reader = _Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex);

            _Mapper = _Sensor.CoordinateMapper;

            var colorFrameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            ColorWidth = colorFrameDesc.Width;
            ColorHeight = colorFrameDesc.Height;

            BYTES_PER_PIXEL = (int)colorFrameDesc.BytesPerPixel; //should be 4 (RGBA)

            _ColorTexture = new Texture2D(colorFrameDesc.Width, colorFrameDesc.Height, TextureFormat.RGBA32, false);
            _ColorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];

            var depthFrameDesc = _Sensor.DepthFrameSource.FrameDescription;
            _DepthData = new ushort[depthFrameDesc.LengthInPixels];
            DepthWidth = depthFrameDesc.Width;
            DepthHeight = depthFrameDesc.Height;

            // Set downsampled frame parameters
            if (DownsampleSize == 1)
            {
                DownsampleSize = (int)Math.Ceiling(Math.Sqrt((DepthWidth * DepthHeight) / MAX_PACKET_SIZE));
            }
            DSPWidth = DepthWidth / DownsampleSize;
            DSPHeight = DepthHeight / DownsampleSize;

            Debug.Log("width: " + DSPWidth);
            Debug.Log("height: " + DSPHeight);

            _ColorPoints = new ColorSpacePoint[_DepthData.Length];

            // Downsample Data
            _ChosenDepthData = new ushort[DSPWidth * DSPHeight];
            _ChosenColorData = new byte[DSPWidth * DSPHeight * BYTES_PER_PIXEL];
            _ChosenColorTexture = new Texture2D(DSPWidth, DSPHeight, TextureFormat.RGBA32, false);

            _BodyData = new byte[DepthWidth * DepthHeight];

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }

    void Update()
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                var colorFrame = frame.ColorFrameReference.AcquireFrame();
                if (colorFrame != null)
                {
                    var depthFrame = frame.DepthFrameReference.AcquireFrame();
                    if (depthFrame != null)
                    {

                        // Populate arrays - Save all required data
                        colorFrame.CopyConvertedFrameDataToArray(_ColorData, ColorImageFormat.Rgba);
                        depthFrame.CopyFrameDataToArray(_DepthData);
                        
                        if (Input.GetMouseButtonDown(0))
                        {

                            // Allocate space for background depth
                            if (_SavedDepthData == null)
                            {
                                _SavedDepthData = new ushort[_DepthData.Length];
                            }

                            // Allocate space for color background
                            if (_SavedColorData == null)
                            {
                                _SavedColorData = new byte[_ColorData.Length];
                            }
                            
                            depthFrame.CopyFrameDataToArray(_SavedDepthData);
                            colorFrame.CopyConvertedFrameDataToArray(_SavedColorData, ColorImageFormat.Rgba);
                            subtractBackground = true;

                        }

                        // Map to color from depth frame
                        _Mapper.MapDepthFrameToColorSpace(_DepthData, _ColorPoints);

                        // Do the background subtraction
                        if (subtractBackground)
                        {
                            bool detectHuman = false;
                            var bodyIndexFrame = frame.BodyIndexFrameReference.AcquireFrame();
                            if (bodyIndexFrame != null)
                            {
                                bodyIndexFrame.CopyFrameDataToArray(_BodyData);
                                bodyIndexFrame.Dispose();
                                detectHuman = true;
                            }

                            ExtractBodySubtractBackground(detectHuman);
                        }
                        
                        // Get chosen depth data from initial depth data. 
                        DSPDepthData();

                        // Get chosen color data from initial color data.
                        DSPColorData();

                        // Apply Color Texture
                        _ColorTexture.LoadRawTextureData(_ColorData);
                        _ColorTexture.Apply();

                        // Apply Chosen Color Texture
                        _ChosenColorTexture.LoadRawTextureData(_ChosenColorData);
                        _ChosenColorTexture.Apply();

                        depthFrame.Dispose();
                        depthFrame = null;

                    }
                  

                    colorFrame.Dispose();
                    colorFrame = null;
                }

                frame = null;
            }
        }

    }

    /*
    void ExtractBody()
    {
        for (int y = 0; y < DepthHeight; y++)
        {
            for (int x = 0; x < DepthWidth; x++)
            {
                int depthIndex = y * (DepthWidth) + x;
                
                if (_BodyData[depthIndex] == 0xff)
                {
                    _DepthData[depthIndex] = 65535;
                }


            }
        }
    }*/

    void ExtractBodySubtractBackground(bool detectHuman)
    {
        int colorIndex = 0;
        for (int y = 0; y < DepthHeight; y++)
        {
            for (int x = 0; x < DepthWidth; x++)
            {
                int depthIndex = y * (DepthWidth) + x;

                if (compareDepth(_DepthData[depthIndex], _SavedDepthData[depthIndex]))
                {
                    _DepthData[depthIndex] = 65535;
                }
                if (detectHuman && (_BodyData[depthIndex] == 0xff))
                {
                    _DepthData[depthIndex] = 65535;
                }

                colorIndex += BYTES_PER_PIXEL;
                
            }
        }
    }

    bool compareDepth(ushort val1, ushort val2)
    {
        return (Math.Abs(val1 - val2) <= 5);
    }

    void DSPDepthData()
    {
        List<ushort> ChosenData = new List<ushort>();
        for (int y = 0; y < DepthHeight; y += DownsampleSize)
        {
            for (int x = 0; x < DepthWidth; x += DownsampleSize)
            {
                int depthIndex = y * (DepthWidth) + x;

                ChosenData.Add(_DepthData[depthIndex]);
            }
        }
        _ChosenDepthData = ChosenData.ToArray();
    }

    void DSPColorData()
    {
        int depthIndex = 0;
        int colorIndex = 0;
        int chosenColorIndex = 0;
        for (int y = 0; y < DepthHeight; y += DownsampleSize)
        {
            for (int x = 0; x < DepthWidth; x += DownsampleSize)
            {
                depthIndex = y * (DepthWidth) + x;

                ColorSpacePoint colorPoint = _ColorPoints[depthIndex];

                int colorX = (int)Math.Floor(colorPoint.X + 0.5);
                int colorY = (int)Math.Floor(colorPoint.Y + 0.5);
                
                colorIndex = ((colorY * ColorWidth) + colorX) * BYTES_PER_PIXEL;

                if ((colorX >= 0) && (colorX < ColorWidth) && (colorY >= 0) && (colorY < ColorHeight))
                {
                    _ChosenColorData[chosenColorIndex + 0] = _ColorData[colorIndex];            
                    _ChosenColorData[chosenColorIndex + 1] = _ColorData[colorIndex + 1]; 
                    _ChosenColorData[chosenColorIndex + 2] = _ColorData[colorIndex + 2];  
                    _ChosenColorData[chosenColorIndex + 3] = (byte)1;                     
                }
                chosenColorIndex += BYTES_PER_PIXEL;
            }
        }
    }

    /// <summary>
    /// Close Kinect connection when not in use.
    /// </summary>
    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}
