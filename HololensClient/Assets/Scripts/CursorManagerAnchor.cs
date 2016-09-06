using UnityEngine;
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Input;
using System.Collections;
using HoloToolkit.Unity;
using System;

/// <summary>
/// CursorManager class takes Cursor GameObjects.
/// One that is on Holograms and another off Holograms.
/// 1. Shows the appropriate Cursor when a Hologram is hit.
/// 2. Places the appropriate Cursor at the hit position.
/// 3. Matches the Cursor normal to the hit surface.
/// </summary>
public class CursorManagerAnchor : Singleton<CursorManagerAnchor>
{
    public GameObject Cursor;

    public GameObject PointCloud;

    private bool anchoredPointCloud = false;

    public float DistanceFromCam;

    GestureRecognizer recognizer;

    void Start()
    {
        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            anchorPointCloud();
            if (anchoredPointCloud == true)
            {
                anchoredPointCloud = false;
                Cursor.SetActive(true);
            }
            else
            {
                anchoredPointCloud = true;
                Cursor.SetActive(false);
            }
        };
        Cursor.SetActive(true);
        recognizer.StartCapturingGestures();
    }

    void Awake()
    {
        if (Cursor == null)
        {
            return;
        }

        // Hide the Cursors to begin with.
        Cursor.SetActive(true);
    }

    void LateUpdate()
    {

        if (Cursor == null)
        {
            return;
        }
        PointCloud.transform.rotation = Camera.main.transform.rotation;// Quaternion.Euler(Vector3.up * Camera.main.transform.rotation.eulerAngles.y);
        PointCloud.transform.position = Camera.main.transform.position + Camera.main.transform.forward * DistanceFromCam + Camera.main.transform.up * 0.02f; // + Camera.main.transform.;
        this.gameObject.transform.position = PointCloud.transform.position; // new Vector3(-0.5f, 0.74f, 0); + new Vector3(-0.5f, 0.2f, 0)
        this.gameObject.transform.rotation = PointCloud.transform.rotation;

    }

    void anchorPointCloud()
    {
        var anchor = PointCloud.GetComponent<WorldAnchor>();
        if (anchor == null)
        {
            PointCloud.AddComponent<WorldAnchor>();
        }
        else
        {
            GameObject.DestroyImmediate(anchor);
        }
    }

}