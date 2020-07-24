using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.UI;

public class Server : MonoBehaviour
{
    private static List<IPeer> servers = new List<IPeer>();
    private NetworkService service;
    private Connector connector;

    public static IPeer GetServer()
    {
        if ( servers.Count == 0 ) return null;

        return servers[0];
    }

    private void Awake()
    {
        PacketBufferManager.Initialize( 2000 );

        service = new NetworkService();

        connector = new Connector( service );
        connector.callbackConnected += OnConnectedServer;
        IPEndPoint endPoint = new IPEndPoint( IPAddress.Parse( "127.0.0.1" ), 7979 );
        connector.Connect( endPoint );
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
