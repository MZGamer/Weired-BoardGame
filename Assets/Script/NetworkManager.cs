using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;
using System.Text;
using UnityEngine.SceneManagement;
using TMPro;

public class NetworkManager : MonoBehaviour
{
    static Socket clinetSocket;
    List<Socket> gateway = new List<Socket>();
    private static byte[] result = new byte[2048];
    public static Queue<Package> sendingQueue = new Queue<Package>();
    public static bool connectSettingComplete = false;
    public static string ip;
    public static int port;
    public static string messageToSend;
    private delegate void DelTalkMessageUpdate(string s);
    Socket udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    bool connect = false;
    public static bool gameOver;
    public static bool gameResult;
    string temp = "";
    public GameObject gameOverObj;
    public TextMeshProUGUI WinText;
    public TextMeshProUGUI AlertText;
    Thread talkSystem;

    // Start is called before the first frame update
    void Start()
    {
        messageToSend = "";
        Application.runInBackground = true;
        //StartConnect();
        gameOver = false;
        connectSettingComplete = false;
        gameResult = false;
    }

    // Update is called once per frame
    void Update() {

        if (connectSettingComplete)
            StartConnect();
        //listenSocket();
        packageSend();
        if(connect && messageToSend != "") {
            sendMessage(); 
        }
        if(gameOver) {
            Disconnected();
            SceneManager.LoadScene("SampleScene");
        }
    }

    public void StartConnect() {
        if (connect)
            return;
        connectSettingComplete = false;
        IPEndPoint ipe;
        try {
            IPAddress ipAddress = IPAddress.Parse(ip);
            ipe = new IPEndPoint(ipAddress, port);
        } catch {
            return;
        }

        try {

            clinetSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clinetSocket.ReceiveBufferSize = 2048;
            clinetSocket.Connect(ipe);
            //clinetSocket.Blocking = false;
            connect = true;
            IPEndPoint target = clinetSocket.LocalEndPoint as IPEndPoint;
            udpClient.Bind(new IPEndPoint(target.Address, port +1));


            StartCoroutine(listenSocket());
            talkSystem = new Thread(listenMessage);
            talkSystem.Start();
        } catch (System.Net.Sockets.SocketException sockEx) {
            Debug.Log(sockEx);
            Disconnected();
            connect = false;
        }
        

    }


    IEnumerator listenSocket() {
        while (true) {
            if (!connect)
                break;
            gateway.Add(clinetSocket);
            Socket.Select(gateway,null,null,100);
            //Debug.Log(gateway.Count);
            try {
                if (gateway.Count != 0) {
                    //通過clientSocket接收資料
                    int receiveNumber = clinetSocket.Receive(result);
                    if (receiveNumber <= 0 && !gameResult) {
                        //connect = false;
                        WinText.text = " You lost connection with server";
                        AlertText.text = "Disconnected";
                        gameOverObj.SetActive(true);
                    } else {

                        string json = Encoding.Unicode.GetString(result);
                        json = json.Replace("\0", string.Empty);
                        Debug.Log("Receive : " + json);
                        Array.Clear(result, 0, result.Length);
                        for (int i = 0; i < json.Length; i++) {
                            if (json[i] != '{' && temp == "") {
                                temp = "";
                                Debug.LogWarning("JunkPackage");
                                break;
                            }
                            if (json[i] != '#') {
                                temp += json[i];
                                if (temp.Length == 7 && temp != "{\"src\":") {
                                    temp = "";
                                    Debug.LogWarning("JunkPackage");
                                    break;
                                }
                            } else {
                                Debug.LogWarning("Package Get" + temp);
                                UI_Manager.animationQueue.Enqueue(JsonUtility.FromJson<Package>(temp));
                                temp = "";
                            }
                        }
                    }
                }

            } catch (System.Net.Sockets.SocketException sockEx) {
                Debug.Log(sockEx);
                Disconnected();
            }
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
    }

    void packageSend() {
        if (!connect)
            return;

        while(sendingQueue.Count != 0) {
            try {
                Package pkg = sendingQueue.Dequeue();
                string json = JsonUtility.ToJson(pkg) + '#';
                clinetSocket.Send(Encoding.Unicode.GetBytes(json));
            } catch (System.Net.Sockets.SocketException sockEx) {
                Debug.Log(sockEx);
                Disconnected();
            }

        }

    }

    void listenMessage() {
        // Blocks until a message returns on this socket from a remote host.
        byte[] receiveBytes = new byte[4096];
        while (true) {
            try {



                int chk = udpClient.Receive(receiveBytes);
                string returnData = Encoding.Unicode.GetString(receiveBytes);
                returnData = returnData.Replace("\0", string.Empty);
                MessageUpdate(returnData);

                // Uses the IPEndPoint object to determine which of these two hosts responded.
                Console.WriteLine("This is the message you received " +
                                                returnData.ToString());
                Array.Clear(receiveBytes, 0, receiveBytes.Length);
                    
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }

        }



    }
    void sendMessage() {
        try {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint target =new IPEndPoint(ipAddress, port * 2);
           Byte[] sendBytes = Encoding.Unicode.GetBytes(messageToSend);
            messageToSend = "";
            udpClient.SendTo(sendBytes, target);
            Array.Clear(sendBytes, 0, sendBytes.Length);
        } catch (Exception e) {
            Debug.Log(e.ToString());
        }
    }
    public void MessageUpdate(string message) {
        UI_Manager.talkMessage += message;
    }

    private void OnApplicationQuit() {
        Disconnected();


    }
    private void Disconnected() {
        if (connect) {
            if(talkSystem.IsAlive)
                talkSystem.Abort();
            try {
                udpClient.Dispose();
            } catch {

            }
            try {
                clinetSocket.Dispose();
            } catch {

            }
            Debug.Log("Disconnected");
        }
    }
}
