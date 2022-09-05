using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using RLparam;
using DWA;

public class ServerSide : MonoBehaviour
{
    private int m_Port = 4444;
    private TcpListener m_TcpListener;
    private List<TcpClient> m_Clients = new List<TcpClient>(new TcpClient[0]);
    private Thread m_ThrdtcpListener;
    private TcpClient m_Client;

    private GameObject gb;
    private WheelCollider wl, wr;    // for collecting rpm
    private float[] data = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f }; // x, y, heading angle, time

    public RL pyRL;

    void Start()
    {
        pyRL = new RL();
        gb = GameObject.Find("Tracer");
        wl = GameObject.Find("left_wheel").GetComponent<WheelCollider>();
        wr = GameObject.Find("right_wheel").GetComponent<WheelCollider>();

        m_ThrdtcpListener = new Thread(new ThreadStart(ListenForIncommingRequests));
        m_ThrdtcpListener.IsBackground = true;
        m_ThrdtcpListener.Start();
    }

    void Update()
    {
        // get rigid body data
        data[0] = gb.transform.position.x;
        data[1] = gb.transform.position.z;
        data[0] = wl.rpm * Mathf.PI / 30.0f;
        data[1] = wr.rpm * Mathf.PI / 30.0f;
        data[2] = gb.transform.rotation.eulerAngles.y;
        data[3] += Time.deltaTime;

        pyRL.recvAct();

        //Debug.Log(DynamicWindowApproach.v_objective[0].ToString("F3") + " " + DynamicWindowApproach.v_objective[1].ToString("F3") + " " + DynamicWindowApproach.v_objective[2].ToString("F3") + " " + DynamicWindowApproach.v_N.ToString());
        if (Input.GetKey(KeyCode.Q))
        {
            Application.Quit();
        }

        // turn into strings
        /*string result = "";
        for (int i = 0; i < data.Length; i++)
        {
            result += data[i].ToString("F4");

            if (i != data.Length - 1)
                result += " ";
            else
                result += "\n";
        }*/

        for (int i = 0; i < m_Clients.Count; i++)
        {
            if (!m_Clients[i].Connected)
            {
                m_Clients.RemoveAt(i);
                Debug.Log("(SERVER) Recent Client Disconnected...");
                DynamicWindowApproach.client_check = false;
            }

            else
            {
                DynamicWindowApproach.client_check = true;
                SendMessage(m_Clients[i], pyRL.sendObs()); // 보내는 값
                                                           //Debug.Log(result);
            }
        }
    }

    void OnApplicationQuit()
    {
        m_ThrdtcpListener.Abort();

        if (m_TcpListener != null)
        {
            m_TcpListener.Stop();
            m_TcpListener = null;
        }
        Debug.Log("Quiting");
    }

    void ListenForIncommingRequests()
    {
        m_TcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), m_Port);
        m_TcpListener.Start();
        Debug.Log("(SERVER) Listening Start...");
        ThreadPool.QueueUserWorkItem(ListenerWorker, null);
    }

    void ListenerWorker(object token)
    {
        while (m_TcpListener != null)
        {
            m_Client = m_TcpListener.AcceptTcpClient();
            m_Clients.Add(m_Client);
            Debug.Log("(SERVER): Client found");
            ThreadPool.QueueUserWorkItem(HandleClientWorker, m_Client);
        }
    }

    void HandleClientWorker(object token)
    {
        Byte[] bytes = new Byte[1024];
        using (var client = token as TcpClient)
        using (var stream = client.GetStream())
        {
            int length;

            while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                var incommingData = new byte[length];
                Array.Copy(bytes, 0, incommingData, 0, length);
                string clientMessage = Encoding.Default.GetString(incommingData);

                if (clientMessage == "q")   // resetting env
                {
                    DynamicWindowApproach.reset_env();
                    DynamicWindowApproach.check_reset = true;
                    Debug.Log("(SERVER) RESET ENV...!!");
                    //return;
                }
                else
                {
                    //Debug.Log("(SERVER)RL Parameter Get Success");
                    RL.msg = clientMessage;
                    // from python, update alpha, gamma, beta, n-predict
                    //pyRL.recvAct(clientMessage);
                }
                // from python, update alpha, gamma, beta, n-predict
                //pyRL.recvAct(clientMessage);

                //Debug.Log("(SERVER) Recieved: " + clientMessage); // 받은 자료
            }

            if (m_Client == null)
            {
                return;
            }
        }
    }

    void SendMessage(object token, string message)
    {
        if (m_Client == null)
            return;

        //else
        //Debug.Log("Clien Number: "+m_Clients.Count);


        var client = token as TcpClient;
        {
            try
            {
                NetworkStream stream = client.GetStream();
                if (stream.CanWrite)
                {
                    byte[] serverMessageAsByteArray = Encoding.Default.GetBytes(message);
                    stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                }
            }

            catch (SocketException ex)
            {
                Debug.Log(ex);
                return;
            }
        }
    }
}
/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using RLparam;
using DWA;

public class ServerSide : MonoBehaviour
{
    #region private members 	
    /// <summary> 	
    /// TCPListener to listen for incomming TCP connection 	
    /// requests. 	
    /// </summary> 	
    private TcpListener tcpListener;
    /// <summary> 
    /// Background thread for TcpServer workload. 	
    /// </summary> 	
    private Thread tcpListenerThread;
    /// <summary> 	
    /// Create handle to connected tcp client. 	
    /// </summary> 	
    private TcpClient connectedTcpClient;
    
    public RL pyRL;
    #endregion

    // Use this for initialization
    void Start()
    {
        // Start TcpServer background thread 		
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
        pyRL = new RL();
    }

    // Update is called once per frame
    void Update()
    {
        //SendMessage();
    }

    /// <summary> 	
    /// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
    /// </summary> 	
    private void ListenForIncommingRequests()
    {
        try
        {
            // Create listener on localhost port 8052. 			
            tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 4444);
            tcpListener.Start();
            Debug.Log("Server is listening");
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                using (connectedTcpClient = tcpListener.AcceptTcpClient())
                {
                    // Get a stream object for reading 					
                    using (NetworkStream stream = connectedTcpClient.GetStream())
                    {
                        int length;
                        // Read incomming stream into byte arrary. 						
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            // Convert byte array to string message. 							
                            string clientMessage = Encoding.ASCII.GetString(incommingData);
                            if (clientMessage == "q")   // resetting env
                            {
                                DynamicWindowApproach.reset_env();
                                DynamicWindowApproach.check_reset = true;
                                Debug.Log("RESET ENV...!!");
                            }
                            else
                            {
                                // from python, update alpha, gamma, beta, n-predict
                                pyRL.recvAct(clientMessage);

                                Debug.Log("recieved: " + clientMessage); // 받은 자료
                            }
                            
                            Debug.Log("client message received as: " + clientMessage);
                        }
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }
    /// <summary> 	
    /// Send message to client using socket connection. 	
    /// </summary> 	
    private void SendMessage()
    {
        if (connectedTcpClient == null)
        {
            return;
        }

        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = connectedTcpClient.GetStream();
            if (stream.CanWrite)
            {
                string serverMessage = "This is a message from your server.";
                // Convert string message to byte array.                 
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage);
                // Write byte array to socketConnection stream.               
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                Debug.Log("Server sent his message - should be received by client");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }
}*/