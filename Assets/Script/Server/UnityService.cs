using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/*
 * 네트워크 엔진(라이브러리)과 유니티를 이어주는 클래스 입니다. 
 * 엔진에서 받은 접속 이벤트, 메세지 수신 이벤트 등을 유니티로 전달하는 역할을 하는데
 * MonoBehaviour를 상속받아 유니티와 동일한 스레드에서 작동하도록 구현했습니다.
 * 따라서 이 클래스의 콜백 메소드에서 유니티 오브젝트에 접근할 때 별도의 동기화 처리는 필요없습니다.
 */


// 네트워크 라이브러리와 유니티 프로젝트 연결
class UnityService : MonoBehaviour
{
    EventManager eventManager;

    // 연결된 게임 서버 객체
    IPeer gameServer;

    // TCP통신을 위한 서비스 객체
    NetworkService service;

    // 접속 완료시 호출되는 델리게이트, 어플리케이션에서 콜백 메소드를 설정하여 사용합니다.
    public delegate void StatusChangedHandler( NETWORK_EVENT status );
    public StatusChangedHandler appcallbackOnStatusChanged;

    // 네트워크 메세지 수신시 호출되는 델리게이트, 어플리케이션에서 콜백 메소드를 설정하여 사용합니다.
    public delegate void MessageHandler( Packet msg );
    public MessageHandler appcallbackOnMessage;

    private void Awake()
    {
        PacketBufferManager.Initialize( 10 );
        this.eventManager = new EventManager();
    }

    public void Connect( string host, int port )
    {
        // NetworkService객체는 메세지의 비동기 송,수신 처리를 수행합니다.
        this.service = new NetworkService();

        // endpoint정보를 갖고있는 Connector생성 하고 만들어둔 NetworkService객체를 넣어줍니다.
        Connector connector = new Connector( service );
        // 접속 성공시 호출될 콜백 메소드 지정.
        connector.callbackConnected += OnConnectedGameServer;
        IPEndPoint endPoint = new IPEndPoint( IPAddress.Parse( host ), port );
        connector.Connect( endPoint );
    }

    void OnConnectedGameServer( UserToken token )
    {
        this.gameServer = new RemoteServerPeer( token );
        ( ( RemoteServerPeer )this.gameServer ).SetEventManager( this.eventManager );

        // 유니티로 이벤트를 넘겨주기 위해서 매니저에 큐잉 시켜준다.
        this.eventManager.EnqueueNetworkEvent( NETWORK_EVENT.CONNECTED );
    }

    // 네트워크에서 발생하는 모든 이벤트를 클라이언트에게 알려주는 역할을 Update에서 진행합니다.
    // 네트워크엔진의 메세지 송수신 처리는 워커스레드에서 수행하지만 유니티의 로직 처리는 메인 스레드에서 수행되므로
    // 큐잉처리를 통하여 메인 스레드에서 모든 로직 처리가 이루어지도록 구성했습니다.
    private void Update()
    {
        // 수신된 메세지에 대한 콜백
        if ( this.eventManager.hasMessage() )
        {
            Packet msg = this.eventManager.DequeueNetworkMessage();
            if ( this.appcallbackOnMessage != null )
            {
                this.appcallbackOnMessage( msg );
            }
        }

        // 네트워크 발생 이벤트에 대한 콜백
        if ( this.eventManager.hasEvent() )
        {
            NETWORK_EVENT status = this.eventManager.DequeueNetworkEvent();
            if( this.appcallbackOnStatusChanged != null )
            {
                this.appcallbackOnStatusChanged( status );
            }
        }
    }

    public void Send( Packet msg )
    {
        try
        {
            this.gameServer.Send( msg );
        }
        catch( Exception ex )
        {
            Debug.LogError( ex.Message );
        }
    }

    private void OnApplicationQuit()
    {
        if ( this. gameServer != null )
        {
            ( ( RemoteServerPeer )this.gameServer ).token.Disconnect();
        }
    }
}
