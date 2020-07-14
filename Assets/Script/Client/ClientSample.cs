using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.UI;

public class ClientSample : MonoBehaviour
{
    public InputField input;

    static List<IPeer> servers = new List<IPeer>();
    NetworkService service;
    Connector connector;

    private void Awake()
    {
        PacketBufferManager.Initialize( 2000 );

        service = new NetworkService();

        connector = new Connector( service );
        connector.callbackConnected += OnConnectedServer;
        IPEndPoint endPoint = new IPEndPoint( IPAddress.Parse( "127.0.0.1" ), 7979 );
        connector.Connect( endPoint );
    }

    public void ChatEnter()
    {
        Packet msg = Packet.Create( ( short )PROTOCOL.CHAT_MSG_REQ );
        msg.Push( input.text );
        servers[0].Send( msg );
        input.text = "";
    }

    public static void OnConnectedServer( UserToken token )
    {
        lock( servers )
        {
            IPeer server = new RemoteServerPeer( token );
            servers.Add( server );
            Debug.Log( " Connected! " );
        }
    }
}
