using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


// EndPoint 정보를 받아서 서버에 접속한다.
// 접속하려는 서버 하나당 인스턴스 한개씩 생성하여 사용하면 된다.
public class Connector
{
    public delegate void ConnectedHandler( UserToken token );
    public ConnectedHandler callbackConnected { get; set; }

    // 원격지 서버와의 연결을 위한 소켓
    Socket client;

    NetworkService networkService;

    public Connector( NetworkService networkService )
    {
        this.networkService = networkService;
        this.callbackConnected = null;
    }

    public void Connect( IPEndPoint remoteEndPoint )
    {
        this.client = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

        // 비동기 접속을 위한 Event Args.
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.Completed += OnConnectCompleted;
        args.RemoteEndPoint = remoteEndPoint;

        bool pending = this.client.ConnectAsync( args );
        if ( pending == false )
        {
            OnConnectCompleted( null, args );
        }
    }

    void OnConnectCompleted( object sender, SocketAsyncEventArgs args )
    {
        if ( args.SocketError == SocketError.Success )
        {
            // 연결 성공
            UserToken token = new UserToken();

            // 데이터 수신 준비
            this.networkService.OnConnectCompleted( this.client, token );

            if ( this.callbackConnected != null )
            {
                this.callbackConnected( token );
            }
        }
        else
        {
            // 연결 실패
            Console.WriteLine( string.Format( "Failed to connect. {0}", args.SocketError ) );
        }
    }
}
