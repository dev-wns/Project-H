using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;

public class Network
{
    private Socket socket;

    public Network() { }

    public void Connect( string address, int port )
    {
        // TCP������� ����
        socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

        // ���ۿ� �����͸� �׾Ƽ� �ѹ��� �����ϴ� ���� �ƴ϶� �ٷιٷ� ����
        socket.NoDelay = true;

        // ������ ������ ip�� ��Ʈ ����
        IPEndPoint endPoint = new IPEndPoint( IPAddress.Parse( address ), port );
        
        // �񵿱� ������ ���� event args
        // �񵿱�� ���
        SocketAsyncEventArgs eventArguments = new SocketAsyncEventArgs();
        eventArguments.Completed += OnConnected;
        eventArguments.RemoteEndPoint = endPoint;

        // �񵿱� �Լ��� ������ �õ� ������
        // ���������� �ٷ� ������ �� ��� == false
        if ( socket.ConnectAsync( eventArguments ) == false )
        {
            OnConnected( null, eventArguments );
        }
    }

    private void OnConnected( object sender, SocketAsyncEventArgs eventArguments )
    {
        if ( eventArguments.SocketError == SocketError.Success )
        {
            // ���� ����
        }
        else
        {
            // ���� ����
        }
    }

    public void Send( Packet packet )
    {
        // ������ ���� �ȵ� ����
        if ( socket == null || socket.Connected == false ) return;

        SocketAsyncEventArgs sendEventArguments = new SocketAsyncEventArgs();
        if ( sendEventArguments == null ) return;

        // ���� �Ϸ� ���� �� �̺�Ʈ ���
        sendEventArguments.Completed += OnSendComplected;
        sendEventArguments.UserToken = this;

        // ������ ������ byte�迭�� SocketAsyncEventArgs��ü ���ۿ� �����Ѵ�.
        byte[] sendData = packet.GetSendBytes();
        sendEventArguments.SetBuffer( sendData, 0, sendData.Length );

        if ( socket.SendAsync( sendEventArguments ) == false )
            OnSendComplected( null, sendEventArguments );
    }

    private void OnSendComplected( object sender, SocketAsyncEventArgs eventArguments )
    {
        if ( eventArguments.SocketError == SocketError.Success )
        {
            // ���� ����
        }
        else
        {
            // ���� ����
        }


    }
}
