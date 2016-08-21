using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;
using System;

public class MultiManager : MonoBehaviour
{
    /*
     * Define variables
     */
    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }
    public int DepthHeight { get; private set; }
    public int DepthWidth { get; private set; }
    public int minDepthX { get; private set; }              // Resize Depth Frame
    public int maxDepthX { get; private set; }              // Resize Depth Frame
    public int minColorX { get; private set; }              // Resize Color Frame
    public int maxColorY { get; private set; }              // Resize Color Frame

    private KinectSensor _Sensor;
    private MultiSourceFrameReader _Reader;
    private Texture2D _ColorTexture;
    private Texture2D _ChosenColorTexture;
    private CoordinateMapper _Mapper;
    private ColorSpacePoint[] _ColorPoints;
    private ushort[] _DepthData;
    private ushort[] _ChosenDepthData;
    private byte[] _ColorData;
    private byte[] _ChosenColorData;
    private int BytesPerPixel = 4;

    /*
 * Define methods
 */
    public Texture2D GetColorTexture()
    {
        return _ColorTexture;
    }

    public Texture2D GetChosenColorTexture()
    {
        return _ChosenColorTexture;
    }

    public ColorSpacePoint[] GetColorSpacePoints()
    {
        return _ColorPoints;
    }

    public ushort[] GetDepthData()
    {
        return _DepthData;
    }

    public ushort[] GetChosenDepthData()
    {
        return _ChosenDepthData;
    }

    void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);
            _Mapper = _Sensor.CoordinateMapper;

            // How to get a new Frame Description for thissss????? 
            var colorFrameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            ColorWidth = colorFrameDesc.Width;
            ColorHeight = colorFrameDesc.Height;
            Debug.Log("ColorWidth: " + ColorWidth);
            Debug.Log("ColorHeight: " + ColorHeight);

            _ColorTexture = new Texture2D(colorFrameDesc.Width, colorFrameDesc.Height, TextureFormat.RGBA32, false);
            _ColorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];

            var depthFrameDesc = _Sensor.DepthFrameSource.FrameDescription;
            DepthWidth = depthFrameDesc.Width;
            DepthHeight = depthFrameDesc.Height;

            minDepthX = DepthWidth / 3;
            maxDepthX = DepthWidth - minDepthX;

            _DepthData = new ushort[depthFrameDesc.LengthInPixels];
            Debug.Log("DepthData Size: " + _DepthData.Length);

            _ColorPoints = new ColorSpacePoint[_DepthData.Length];
            _ChosenDepthData = new ushort[DepthHeight * (DepthWidth - 2 * minDepthX)];                     // Resize the chosen depth frame to 1/3 
            _ChosenColorData = new byte[DepthHeight * (DepthWidth - 2 * minDepthX) * BytesPerPixel];       // Get the chosen color frame
            _ChosenColorTexture = new Texture2D(DepthWidth - 2 * minDepthX, DepthHeight, TextureFormat.RGBA32, false);

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
                        colorFrame.CopyConvertedFrameDataToArray(_ColorData, ColorImageFormat.Rgba);

                        Debug.Log("ColorData Size: " + _ColorData.Length);

                        depthFrame.CopyFrameDataToArray(_DepthData);

                        // Coordinate Mapping    ->   This is buggy.
                        _Mapper.MapDepthFrameToColorSpace(_DepthData, _ColorPoints);

                        // Get chosen depth data from initial depth data. 
                        EvaluateDepthData(_DepthData, _ChosenDepthData);

                        // Get chosen color data from initial color data.
                        EvaluateColorData(_ColorData, _ChosenColorData);

                        // Apply Color Texture
                        _ColorTexture.LoadRawTextureData(_ColorData);
                        _ColorTexture.Apply();

                        // Apply Chosen Color Texture
                        _ChosenColorTexture.LoadRawTextureData(_ChosenColorData);   // this is too much????
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

    // Get chosen depth data from initial data. 
    void EvaluateDepthData(ushort[] DepthData, ushort[] ChosenDepthData)
    {
        List<ushort> ChosenData = new List<ushort>();

        for (int y = 0; y < DepthHeight; y++)
        {
            for (int x = 0; x < DepthWidth; x++)
            {
                int depthIndex = y * (DepthWidth) + x;

                if (x >= minDepthX && x < maxDepthX)
                {
                    ChosenData.Add(DepthData[depthIndex]);
                }
            }
        }
        ChosenDepthData = ChosenData.ToArray();
        Debug.Log("ChosenDepthData Size: " + ChosenData.Count);
    }

    // Get chosen color data from initial data -> This is not correct because I must get color data from the depth data.
    //void EvaluateColorData(byte[] ColorData, byte[] ChosenColorData)
    //{
    //    for (int i = 0; i <= ChosenColorData.Length - 4; i += 4)
    //    {
    //        int chosenColorIndex = i;                                           // Run through the ChosenColorData,
    //        int depthIndex = chosenColorIndex / BytesPerPixel;                  // get its chosenColorIndex and its related
    //                                                                            // depthIndex by using BytesPerPixel. Using        
    //        ColorSpacePoint colorPoint = _ColorPoints[depthIndex];              // Coordinate Mapping technique. Get Chosen- 
    //                                                                            // ColorData in ColorData with chosenColorIndex.
    //        int colorX = (int)Math.Floor(colorPoint.X + 0.5);
    //        int colorY = (int)Math.Floor(colorPoint.Y + 0.5);
    //        // try to add array of colorX and colorY to get colorFrameDesc.
    //        int colorIndex = ((colorY + ColorWidth) + colorX) * BytesPerPixel;

    //        ChosenColorData[chosenColorIndex + 0] = ColorData[colorIndex];      // R       
    //        ChosenColorData[chosenColorIndex + 1] = ColorData[colorIndex + 1];  // G
    //        ChosenColorData[chosenColorIndex + 2] = ColorData[colorIndex + 2];  // B
    //        ChosenColorData[chosenColorIndex + 3] = 255;                        // A
    //    }
    //    Debug.Log("ChosenColorData Chosen Size: " + ChosenColorData.Length);
    //}

    void EvaluateColorData(byte[] ColorData, byte[] ChosenColorData)
    {
        for (int y = 0; y < DepthHeight; y++)
        {
            for (int x = minDepthX; x < maxDepthX; x++)
            {
                int depthIndex = y * (DepthWidth) + x;

                ColorSpacePoint colorPoint = _ColorPoints[depthIndex];

                int colorX = (int)Math.Floor(colorPoint.X + 0.5);
                int colorY = (int)Math.Floor(colorPoint.Y + 0.5);

                // try to add array of colorX and colorY to get colorFrameDesc.
                int colorIndex = ((colorY + ColorWidth) + colorX) * BytesPerPixel;
                int chosenColorIndex = colorIndex * BytesPerPixel;                  // sure??? only * BytesPerPixel??? 

                ChosenColorData[chosenColorIndex + 0] = ColorData[colorIndex];      // R       
                ChosenColorData[chosenColorIndex + 1] = ColorData[colorIndex + 1];  // G
                ChosenColorData[chosenColorIndex + 2] = ColorData[colorIndex + 2];  // B
                ChosenColorData[chosenColorIndex + 3] = 255;                        // A
            }
        }
        Debug.Log("ChosenColorData Chosen Size: " + ChosenColorData.Length);
    }

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
