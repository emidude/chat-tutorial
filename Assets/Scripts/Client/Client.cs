using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour {

    public GameObject chatContainer;
    public GameObject messagePrefab;

    public string clientName;

    private bool socketReady = false;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    public void ConnectToServer()
    {
        //if already connected, ignore this function
        if (socketReady)
            return;

        //default host / port values
        string host = "127.0.0.1";
        int port = 6321;

        //overwrite default host/port values, if there is something in those boxes
        string h;
        int p;

        //try with find with tag later if everything working...........
        h = GameObject.Find("HostInput").GetComponent<InputField>().text;
        if (h != "")
        {
            host = h;
        }
        int.TryParse(GameObject.Find("PortInput").GetComponent<InputField>().text, out p);
        if (p != 0)
            port = p;

        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            socketReady = true;
        }
        catch(Exception e)
        {
            Debug.Log("socket error: " + e.Message);
            
        }
    }

	private void Update()
	{
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                    OnIncomingData(data);
            }
        }
	}

    private void OnIncomingData(string data)
    {
        if (data == "%NAME")
        {
            Send("&NAME|"+ clientName);
            return;
        }
        Debug.Log("Server: " + data);
        GameObject go = Instantiate(messagePrefab, chatContainer.transform) as GameObject;
        go.GetComponentInChildren<Text>().text = data;
    }

    private void Send(string data)
    {
        if (!socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }

    public void OnSendButton()
    {
        //FIND WITH TAG LATER? - LOOK UP WAYS OF OPTIMISING FIND
        string message = GameObject.Find("SendInput").GetComponent<InputField>().text;
        Send(message);
    }

    private void CloseSocket()
    {
        if (!socketReady)
            return;
        
        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
    }

	private void OnApplicationQuit()
	{
        CloseSocket();
	}

	private void OnDisable()
	{
        CloseSocket();
	}
}
