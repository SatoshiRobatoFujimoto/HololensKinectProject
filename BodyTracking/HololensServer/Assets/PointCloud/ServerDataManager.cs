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
public class ServerDataManager : MonoBehaviour {

    private KinectSensor _Sensor;
    private MultiSourceFrameReader _Reader;
    
    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }
    public int DepthHeight { get; private set; }
    public int DepthWidth { get; private set; }

    // Clippped width and height
    public int ClipWidth { get; private set; }
    public int ClipHeight { get; private set; }

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

    /// <summary>
    /// Store clipped or downsampled Kinect data
    /// </summary>
    private Texture2D _ClippedColorTexture;
    private ushort[] _ClippedDepthData;
    private byte[] _ClippedColorData;

    private Texture2D _ChosenColorTexture;
    private ushort[] _ChosenDepthData;
    private byte[] _ChosenColorData;

    private int BYTES_PER_PIXEL;

    private int DownsampleSize = 1;

    public int MAX_PACKET_SIZE;

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
    public Texture2D GetClippedColorTexture()
    {
        return _ClippedColorTexture;
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
    /// Public getter for modified Kinect depth data.
    /// </summary>
    /// <returns> Downsampled or clipped Kinect depth data in mm. </returns>
    public ushort[] GetClippedDepthData()
    {
        return _ClippedDepthData;
    }

    /// <summary>
    /// Public getter for color data.
    /// </summary>
    /// <returns> Unmodified Kinect color data in RGBA32. </returns>
    public byte[] GetChosenColorData()
    {
        return _ChosenColorData;
    }

    /// <summary>
    /// Public getter for modified Kinect color data.
    /// </summary>
    /// <returns> Downsampled or clipped Kinect color data in RGBA32. </returns>
    public byte[] GetClippedColorData()
    {
        return _ClippedColorData;
    }

    void Start () 
    {
        if (MAX_PACKET_SIZE == 0)
        {
            MAX_PACKET_SIZE = 60000;
            Debug.Log("ERROR. Automatically assigning max message size to 60,000 bytes.");
        }

        _Sensor = KinectSensor.GetDefault();
        
        if (_Sensor != null) 
        {

            _Reader = _Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);

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

            // Select clipping points
            minDepthX = 0; 
            maxDepthX = 512; 
            
            ClipWidth = maxDepthX - minDepthX;
            ClipHeight = DepthHeight;

            if ((ClipWidth * ClipHeight) >= MAX_PACKET_SIZE)
            {
                DownsampleSize = (int)Math.Ceiling(Math.Sqrt((ClipWidth * ClipHeight) / MAX_PACKET_SIZE));
                DSPWidth = ClipWidth / DownsampleSize;   
                DSPHeight = ClipHeight / DownsampleSize;
            }
            
            _ColorPoints = new ColorSpacePoint[_DepthData.Length];

            _ClippedDepthData = new ushort[ClipHeight * ClipWidth];                  // Resize the Clipped depth  
            _ClippedColorData = new byte[_ClippedDepthData.Length * BYTES_PER_PIXEL];  // Get the Clipped color frame
            _ClippedColorTexture = new Texture2D(ClipWidth, ClipHeight, TextureFormat.RGBA32, false);

            // Downsample Data
            _ChosenDepthData = new ushort[DSPWidth * DSPHeight];                 
            _ChosenColorData = new byte[DSPWidth * DSPHeight * BYTES_PER_PIXEL];     
            _ChosenColorTexture = new Texture2D(DSPWidth, DSPHeight, TextureFormat.RGBA32, false);

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }
    
    void Update () 
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

                        // Map to color from depth frame
                        _Mapper.MapDepthFrameToColorSpace(_DepthData, _ColorPoints);

                        // Clip the depth frame
                        EvaluateDepthData();

                        // Clip the color frame
                        EvaluateColorData();

                        // Get chosen depth data from initial depth data. 
                        DSPDepthData();

                        // Get chosen color data from initial color data.
                        DSPColorData();

                        // Apply Color Texture
                        _ColorTexture.LoadRawTextureData(_ColorData);
                        _ColorTexture.Apply();

                        // Apply Clipped Color Texture
                        _ClippedColorTexture.LoadRawTextureData(_ClippedColorData); 
                        _ClippedColorTexture.Apply();

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

    /// <summary>
    /// Clips the depth data frame.
    /// </summary>
    void EvaluateDepthData()
    {
        List<ushort> ChosenData = new List<ushort>();

        for (int y = 0; y < DepthHeight; y++)
        {
            for (int x = 0; x < DepthWidth; x++)
            {
                int depthIndex = y * (DepthWidth) + x;

                if (x >= minDepthX && x < maxDepthX)
                {
                    ChosenData.Add(_DepthData[depthIndex]);
                }
            }
        }
        _ClippedDepthData = ChosenData.ToArray();
    }

    void EvaluateColorData()
    {
        int depthIndex = 0;
        int colorIndex = 0;
        int chosenColorIndex = 0;
        for (int y = 0; y < DepthHeight; y++)
        {
            for (int x = minDepthX; x < maxDepthX; x++)
            {
                depthIndex = y * (DepthWidth) + x;

                ColorSpacePoint colorPoint = _ColorPoints[depthIndex];

                int colorX = (int)Math.Floor(colorPoint.X + 0.5);
                int colorY = (int)Math.Floor(colorPoint.Y + 0.5);
                
                colorIndex = ((colorY * ColorWidth) + colorX) * BYTES_PER_PIXEL;

                if ((colorX >= 0) && (colorX < ColorWidth) && (colorY >= 0) && (colorY < ColorHeight))
                {
                    _ClippedColorData[chosenColorIndex + 0] = _ColorData[colorIndex];      // R       
                    _ClippedColorData[chosenColorIndex + 1] = _ColorData[colorIndex + 1];  // G
                    _ClippedColorData[chosenColorIndex + 2] = _ColorData[colorIndex + 2];  // B
                    _ClippedColorData[chosenColorIndex + 3] = (byte)1;                     // A
                }
                chosenColorIndex += BYTES_PER_PIXEL;
            }
        }
    }


    void DSPDepthData()
    {
        List<ushort> ChosenData = new List<ushort>();

        for (int y = 0; y < ClipHeight; y += DownsampleSize)
        {
            for (int x = 0; x < ClipWidth; x += DownsampleSize)
            {
                int depthIndex = y * (ClipWidth) + x;
                ChosenData.Add(_ClippedDepthData[depthIndex]);
            }
        }
        _ChosenDepthData = ChosenData.ToArray();
    }

    void DSPColorData()
    {
        int depthIndex = 0;
        int colorIndex = 0;
        int chosenColorIndex = 0;
        for (int y = 0; y < ClipHeight; y += DownsampleSize)
        {
            for (int x = 0; x < ClipWidth; x += DownsampleSize)
            {
                depthIndex = y * (DepthWidth) + x + minDepthX;

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
