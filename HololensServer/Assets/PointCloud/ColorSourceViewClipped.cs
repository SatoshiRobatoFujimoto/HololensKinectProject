using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class ColorSourceViewClipped : MonoBehaviour
{
    public GameObject DataManager;
    private ServerDataManager _DataManager;
    
    void Start ()
    {
        gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }
    
    void Update()
    {
        if (DataManager == null)
        {
            return;
        }
        
        _DataManager = DataManager.GetComponent<ServerDataManager>();
        if (_DataManager == null)
        {
            return;
        }
        
        gameObject.GetComponent<Renderer>().material.mainTexture = _DataManager.GetChosenColorTexture();
    }
}
