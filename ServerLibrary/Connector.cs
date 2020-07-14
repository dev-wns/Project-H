using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


// EndPoint ������ �޾Ƽ� ������ �����Ѵ�.
// �����Ϸ��� ���� �ϳ��� �ν��Ͻ� �Ѱ��� �����Ͽ� ����ϸ� �ȴ�.
public class Connector
{
    public delegate void ConnectedHandler( UserToken token );
    public ConnectedHandler callbackConnected { get; set; }

    // ������ �������� ������ ���� ����
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

        // �񵿱� ������ ���� Event Args.
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
            // ���� ����
            UserToken token = new UserToken();

            // ������ ���� �غ�
            this.networkService.OnConnectCompleted( this.client, token );

            if ( this.callbackConnected != null )
            {
                this.callbackConnected( token );
            }
        }
        else
        {
            // ���� ����
            Console.WriteLine( string.Format( "Failed to connect. {0}", args.SocketError ) );
        }
    }
}
