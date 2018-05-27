using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{

    private List<ServerClient> clients;
    private List<ServerClient> disconnectList;

    public int port = 6321;
    private TcpListener server;
    private bool serverStarted;

    private void Start()
    {
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start(); //starts listening

            startListening();
            serverStarted = true;

            Debug.Log("server has been started on port: " + port.ToString());

        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }

    }

    private void Update()
    {
        if (!serverStarted)
            return;

        foreach (ServerClient c in clients)
        {
            //is the clinet still connected?
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                disconnectList.Add(c);
                continue;
            }

            //check for messages from the client
            else
            {
                NetworkStream s = c.tcp.GetStream();

                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(c, data);
                    }
                }
            }
        }

        for (int i = 0; i < disconnectList.Count - 1; i++)
        {
            Broadcast(disconnectList[i].clientName + "has disconnected", clients);

            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);

           
        }

    }

    private void OnIncomingData(ServerClient c, string data)
    {
        if (data.Contains("&NAME"))
        {
            c.clientName = data.Split('|')[1];
            Broadcast(c.clientName + "has connected", clients);
            return;
        }
        Debug.Log(c.clientName + " has sent the following message: " + data);
        Broadcast(c.clientName + " : " + data, clients);
    }

    private void Broadcast(string data, List<ServerClient> cl)
    {
        foreach (ServerClient c in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error " + e.Message + " to client " + c.clientName);
            }
        }
    }

    private bool IsConnected(TcpClient tcp)
    {
        // throw new NotImplementedException();
        try
        {
            if (tcp != null && tcp.Client != null && tcp.Client.Connected)
            {
                //mystery code - put this in when everything works
                //if (tcp.Client.Poll(0, SelectMode.SelectRead))
                //{
                //    return !(tcp.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                //}
                return true;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void startListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar))); //add the connected client to the list of clients   
        startListening();

        //send a message to everyone that someone has connected
       // Broadcast(clients[clients.Count - 1].clientName + " has connected.", clients);
        Broadcast("%NAME", new List<ServerClient> { clients[clients.Count - 1] });
    }
}

public class ServerClient
{
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient clientSocket)
    {
        clientName = "Guest";
        tcp = clientSocket;
    }

}