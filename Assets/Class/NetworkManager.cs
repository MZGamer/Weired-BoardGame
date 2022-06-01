using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;

public class NetworkManager : MonoBehaviour
{
    static Socket clinetSocket;
    private static byte[] result = new byte[2048];
    public static Queue<Package> sendingQueue;

    bool connect = false;

    // Start is called before the first frame update
    void Start()
    {
        StartConnect();
    }

    // Update is called once per frame
    void Update() {
        listenSocket();
    }

    void StartConnect() {
        IPAddress ip = IPAddress.Parse("25.9.176.234");
        IPEndPoint ipe = new IPEndPoint(ip, 1234);
        clinetSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try {
            clinetSocket.Connect(ipe);
            clinetSocket.Blocking = false;
            connect = true;
            Thread listening = new Thread(listenMessage);
        } catch (System.Net.Sockets.SocketException sockEx) {
            Debug.Log(sockEx);
            connect = false;
        }
        

    }

    void listenSocket() {
        if (!connect)
            return;
        try {
            //通過clientSocket接收資料
            int receiveNumber = clinetSocket.Receive(result);
            if (receiveNumber == 0) {
                connect = false;
                throw new Exception(String.Format("You lost connection with server"));
            } else {

            }

        } catch (System.Net.Sockets.SocketException sockEx) {
            Debug.Log(sockEx);
            connect = false;
        }
    }

    void listenMessage() {

    }
}
