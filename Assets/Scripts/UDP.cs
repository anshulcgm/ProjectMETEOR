//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using UnityEngine;

//public class UDP
//{
//    private const int PORT_NUMBER = 15000;
//    private Thread t = null;
//    private string broadcastAddr = null;

//    public string mostRecentMessage = null;

//    public void Start()
//    {
//        if (t != null)
//        {
//            throw new Exception("Already started, stop first");
//        }
//        Debug.Log("Started listening");
//        string [] locIpSplit = GetLocalIPAddress().Split('.');
//        broadcastAddr = locIpSplit[0] + '.' + locIpSplit[1] + '.' + locIpSplit[2] + ".255";
//        Debug.Log(broadcastAddr);
//        StartListening();
//    }

//    public void Stop()
//    {
//        try
//        {
//            udp.Close();
//            Debug.Log("Stopped listening");
//        }
//        catch { /* don't care */ }
//    }

//    private readonly UdpClient udp = new UdpClient(PORT_NUMBER);

//    IAsyncResult ar_ = null;

//    private void StartListening()
//    {
//        ar_ = udp.BeginReceive(Receive, new object());
//    }

//    private void Receive(IAsyncResult ar)
//    {
//        IPEndPoint ip = new IPEndPoint(IPAddress.Any, PORT_NUMBER);
//        byte[] bytes = udp.EndReceive(ar, ref ip);
//        mostRecentMessage = Encoding.ASCII.GetString(bytes);
//        Debug.Log("From" + ip.Address.ToString() +  "received: " + mostRecentMessage);
//        StartListening();
//    }

//    public void SendBroadcastOnLAN(string message)
//    {
//        UdpClient client = new UdpClient();
//        IPEndPoint ip = new IPEndPoint(IPAddress.Parse(broadcastAddr), PORT_NUMBER);
//        byte[] bytes = Encoding.ASCII.GetBytes(message);
//        client.Send(bytes, bytes.Length, ip);
//        client.Close();
//        Debug.Log("Broadcasted: " + message);
//    }

//    public void Send(string message, string ipAddr)
//    {
//        UdpClient client = new UdpClient();
//        IPEndPoint ip = new IPEndPoint(IPAddress.Parse(ipAddr), PORT_NUMBER);
//        byte[] bytes = Encoding.ASCII.GetBytes(message);
//        client.Send(bytes, bytes.Length, ip);
//        client.Close();
//        Debug.Log("Sent: " + message + " to " + ipAddr);
//    }

//    public static string GetLocalIPAddress()
//    {
//        var host = Dns.GetHostEntry(Dns.GetHostName());
//        foreach (var ip in host.AddressList)
//        {
//            if (ip.AddressFamily == AddressFamily.InterNetwork)
//            {
//                return ip.ToString();
//            }
//        }
//        throw new Exception("No network adapters with an IPv4 address in the system!");
//    }
//}

