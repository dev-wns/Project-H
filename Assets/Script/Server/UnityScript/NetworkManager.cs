using System;
using System.Collections;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    UnityService gameServer;
    string receivedMessage;

    private void Awake()
    {
        this.receivedMessage = "";

        // 네트워크 통신을 위해 UnityService객체를 추가합니다.
        this.gameServer = gameObject.AddComponent<UnityService>();

        // 상태 변화( 접속, 끊김 등 )를 통보 받을 델리게이트를 설정합니다.
        this.gameServer.appcallbackOnStatusChanged += OnStatusChanged;

        // 패킷 수신용 델리게이트를 설정합니다.
        this.gameServer.appcallbackOnMessage += OnMessage;
    }

    private void Start()
    {
        this.gameServer.Connect( "127.0.0.1", 7979 );
    }

    void OnStatusChanged( NETWORK_EVENT status )
    {
        switch( status )
        {
            case NETWORK_EVENT.CONNECTED:
                {
                    LogManager.log( "On Connected!" );
                    this.receivedMessage += "On Connected\n";

                    Packet msg = Packet.Create( ( short )PROTOCOL.CHAT_MSG_REQ );
                    msg.Push( "HELLO!!!" );
                    this.gameServer.Send( msg );
                } break;
            case NETWORK_EVENT.DISCONNECTED:
                {
                    LogManager.log( "DisConnected!" );
                    this.receivedMessage += "DisConnected\n";
                } break;

            default: break;
        }
    }

    void OnMessage( Packet msg )
    {
        PROTOCOL protocol_id = ( PROTOCOL )msg.PopProtocolID();

        switch( protocol_id )
        {
            case PROTOCOL.CHAT_MSG_ACK:
                {
                    string text = msg.PopString();
                    GameObject.Find( "ServerMain" ).GetComponent<GameMain>().MakeMessage( text );
                } break;
            case PROTOCOL.MOVE_ACK:
                {

                } break;
            default: break;
        }
    }

    public void Send( Packet msg )
    {
        this.gameServer.Send( msg );
    }
}
