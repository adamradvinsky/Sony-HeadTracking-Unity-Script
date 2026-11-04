using UnityEngine;
using System.Net.Sockets;
using System;
using System.Net;
using System.Text;
using System.Threading;
using Unity.VisualScripting;



public class SonyHeadTracker : MonoBehaviour
{




    private bool exit = false;
    private static Mutex exit_mutex = new Mutex();
    Payload data;
    private static Mutex payload_mutex = new Mutex();
    private Thread t;
    void Awake()
    {
        data = new Payload();
        Debug.Log("creating a new thread");
        // set this up on a new thread
        t = new Thread(new ThreadStart(ThreadProcess));
        t.Start();


    }

    //public static UdpClient udpClient;
    void ThreadProcess()
    {

        // set up socket 
        UdpClient udpClient = new UdpClient(4243);


        PortListener(udpClient);
    }


    private static Payload payload = null;

    void PortListener(UdpClient udpClient)
    {
        bool listen = true;

        while (listen == true)
        {

            exit_mutex.WaitOne();

            if (exit == true)
            {
                listen = false;

                exit_mutex.ReleaseMutex();
                break;
            }

            exit_mutex.ReleaseMutex();


            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Byte[] recieveBytes = udpClient.Receive(ref remoteIpEndPoint);
            string json = Encoding.UTF8.GetString(recieveBytes);
            //Debug.Log(json);

            // lock
            payload_mutex.WaitOne();

            // convert the json into an instance of the Payload class and the info is automatically filled in
            payload = JsonUtility.FromJson<Payload>(json);

            // unlock
            payload_mutex.ReleaseMutex();

            //Debug.Log($"the version is{payload.version}");
            //Debug.Log($"the rotation vector is{payload.rotationVector[0]}");


        }
        udpClient.Close();
    }


    public static event Action<Payload> OnPayloadUpdate;

    void Update()
    {

        // lock
        payload_mutex.WaitOne();

        data = payload;

        // unlock
        payload_mutex.ReleaseMutex();


        if (data != null)
        {
            OnPayloadUpdate?.Invoke(data);
        }
    }

    private void OnApplicationQuit()
    {


        exit_mutex.WaitOne();
        exit = true;
        exit_mutex.ReleaseMutex();

    }

    private void OnDisable()
    {
        exit_mutex.WaitOne();
        exit = true;
        exit_mutex.ReleaseMutex();
    }

    public void subscribeToPayload(Action<Payload> actionToAdd)
    {
        OnPayloadUpdate += actionToAdd;
    }

    public void unSubscribeToPayload(Action<Payload> actionToRemove)
    {
        OnPayloadUpdate -= actionToRemove;
    }



}
