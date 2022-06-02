using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;
using System.Text;

public class NetworkManager : MonoBehaviour
{
    static Socket clinetSocket;
    private static byte[] result = new byte[2048];
    public static Queue<Package> sendingQueue = new Queue<Package>();

    private delegate void DelTalkMessageUpdate(string s);

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
                string json = Encoding.Unicode.GetString(result);
                UI_Manager.animationQueue.Enqueue(JsonUtility.FromJson<Package>(json));
            }

        } catch (System.Net.Sockets.SocketException sockEx) {
            Debug.Log(sockEx);
            connect = false;
        }
    }

    void listenMessage() {
        UdpClient udpClient = new UdpClient(1235);
        try {
            udpClient.Connect("25.9.176.234", 1235);

            //IPEndPoint object will allow us to read datagrams sent from any source.
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            // Blocks until a message returns on this socket from a remote host.
            Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);
            MessageUpdate(returnData);

            // Uses the IPEndPoint object to determine which of these two hosts responded.
            Console.WriteLine("This is the message you received " +
                                         returnData.ToString());
            Console.WriteLine("This message was sent from " +
                                        RemoteIpEndPoint.Address.ToString() +
                                        " on their port number " +
                                        RemoteIpEndPoint.Port.ToString());

        } catch (Exception e) {
            Debug.Log(e.ToString());
        }
    }
    public void MessageUpdate(string message) {
        UI_Manager.talkMessage += message;
    }
}
