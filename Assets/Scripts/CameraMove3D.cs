using UnityEngine;
using System.Collections;
using System;
using System.Net.Sockets;
using System.Net;

public class CameraMove3D : MonoBehaviour
{
    public float MaxDistance = 3;
    public float MinDistance = 1;
    public int FrameRate = 30;
    public float scale = 100;
    public float up = 0;
    protected float size;
    protected Vector3 old_mouse_pos;
    public float distance, alpha, beta;

    private Socket clientSocket;
    void Start()
    {
        Application.targetFrameRate = FrameRate;
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        OBJ model = GameObject.Find("model").GetComponent<OBJ>();
        try
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            clientSocket.Connect(new IPEndPoint(ip, model.myPort));
        }
        catch 
        {
            Debug.Log("Socket connection failed");
            return;
        }
    }

    // Use this for initialization
    public void StartObserve()
    {
        GameObject model = GameObject.Find("model");
        SkinnedMeshRenderer mf = model.GetComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
        if (mf == null)
            Debug.Log("wrong");
        size = MovePara.normalHeight;
        
        size = size / 1.5f;
        distance = 2 * size;
        beta = 0;
        alpha = Mathf.PI/2;
        transform.position = new Vector3(distance * Mathf.Cos(beta) * Mathf.Cos(alpha),
               distance * Mathf.Sin(beta), distance * Mathf.Cos(beta) * Mathf.Sin(alpha));
        transform.LookAt(new Vector3(0, up, 0));
    }

    // Update is called once per frame
    void Update()
    {           
        Vector3 delta = new Vector3(0, 0, 0);
        
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (Input.GetMouseButtonDown(0))
            old_mouse_pos = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            delta = Input.mousePosition - old_mouse_pos;
            old_mouse_pos = Input.mousePosition;
        }        
#endif

#if UNITY_ANDROID
        if (Input.touchCount>0 && Input.GetTouch(0).phase==TouchPhase.Moved) 
        {
            delta = Input.GetTouch(0).deltaPosition;
        }
#endif
        alpha -= delta.x / scale;
        beta -= delta.y / scale;

        transform.position = new Vector3(distance * Mathf.Cos(beta) * Mathf.Cos(alpha),
               distance * Mathf.Sin(beta), distance * Mathf.Cos(beta) * Mathf.Sin(alpha));
        beta = Mathf.Clamp(beta, -1.3f, 1.3f);
        transform.LookAt(new Vector3(0, up, 0));
    }

    public void ZoomIn()
    {
        distance -= size * 0.4f;
        distance = Mathf.Clamp(distance, MinDistance*size, MaxDistance*size);
    }

    public void ZoomOut()
    {
        distance += size * 0.4f;
        distance = Mathf.Clamp(distance, MinDistance*size, MaxDistance*size);
    }

    public void LookUp()
    {
        if (up < size / 3)
            up += size / 2;
        else
            up = -size / 2;
    }

    public void OnObj1()
    {
        string s;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        s ="Load file://" + Application.streamingAssetsPath + "/model3.obj\n";
#else
#if UNITY_ANDROID
        s = "Load " + Application.streamingAssetsPath + "/model3.obj\n";
#endif
#endif
        clientSocket.Send(System.Text.Encoding.ASCII.GetBytes(s));
    }

    public void OnObj2()
    {
        string s;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        s = "Load file://" + Application.streamingAssetsPath + "/model2.obj\n";
#else
#if UNITY_ANDROID
        s = "Load " + Application.streamingAssetsPath + "/model2.obj\n";
#endif
#endif
        clientSocket.Send(System.Text.Encoding.ASCII.GetBytes(s));
    }

    public void OnObj3()
    {
        string s;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        s = "Load file://" + Application.streamingAssetsPath + "/mod2el.obj\n";
#else
#if UNITY_ANDROID
        s= "Load file://" +"/sdcard/model.obj";        
#endif
#endif
        clientSocket.Send(System.Text.Encoding.ASCII.GetBytes(s));
    }

    public void OnQuit()
    {
        GameObject model = GameObject.Find("model");
        Destroy(model);
        Application.Quit();
    }
}
