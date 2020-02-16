//using System.Collections;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Sockets;
//using UnityEngine;

//public class HandTracking : MonoBehaviour
//{
//    public UDP udp = null;

//	// Use this for initialization
//	public void Start () {
//        udp = new UDP();
        
//        udp.Start();
//	}
	
//	// Update is called once per frame
//	public void Update ()
//    {
//        //throw out messages not sent by PC. Get actual message, remove "JASK" indicator.
//        string mostRecentMessage = udp.mostRecentMessage;
//        if(mostRecentMessage == null)
//        {
//            return;
//        }

//        string[] messageParsed = mostRecentMessage.Split(' ');
//        if (messageParsed.Length != 2 || !messageParsed[0].Equals("JASK"))
//        {
//            return;
//        }
//        string message = messageParsed[1];

//        //if the PC is requesting an ip, send the ip
//        if (message.Equals("send_ip"))
//        {
//            Debug.Log("ip request recieved. Broadcasting local ip.");
//            //send local ip address so that the server can just send to us, not to everybody.
//            udp.SendBroadcastOnLAN(UDP.GetLocalIPAddress());
//            //set the most recent message to null so we don't send the ip address again.
//            udp.mostRecentMessage = null;
//        }
//        //if the message isn't an ip request, display it.
//        else
//        {
//            Debug.Log(message);
//        }
//    }
//}
