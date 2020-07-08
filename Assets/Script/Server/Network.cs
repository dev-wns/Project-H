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
        // TCP통신으로 연결
        socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

        // 버퍼에 데이터를 쌓아서 한번에 전송하는 것이 아니라 바로바로 전송
        socket.NoDelay = true;

        // 연결할 서버의 ip와 포트 설정
        IPEndPoint endPoint = new IPEndPoint( IPAddress.Parse( address ), port );
        
        // 비동기 접속을 위한 event args
        // 비동기로 통신
        SocketAsyncEventArgs eventArguments = new SocketAsyncEventArgs();
        eventArguments.Completed += OnConnected;
        eventArguments.RemoteEndPoint = endPoint;

        // 비동기 함수로 연결을 시도 했지만
        // 동기적으로 바로 응답이 온 경우 == false
        if ( socket.ConnectAsync( eventArguments ) == false )
        {
            OnConnected( null, eventArguments );
        }
    }

    private void OnConnected( object sender, SocketAsyncEventArgs eventArguments )
    {
        if ( eventArguments.SocketError == SocketError.Success )
        {
            // 연결 성공
        }
        else
        {
            // 연결 실패
        }
    }

    public void Send( Packet packet )
    {
        // 소켓이 연결 안된 상태
        if ( socket == null || socket.Connected == false ) return;

        SocketAsyncEventArgs sendEventArguments = new SocketAsyncEventArgs();
        if ( sendEventArguments == null ) return;

        // 전송 완료 됐을 때 이벤트 등록
        sendEventArguments.Completed += OnSendComplected;
        sendEventArguments.UserToken = this;

        // 전송할 데이터 byte배열을 SocketAsyncEventArgs객체 버퍼에 복사한다.
        byte[] sendData = packet.GetSendBytes();
        sendEventArguments.SetBuffer( sendData, 0, sendData.Length );

        if ( socket.SendAsync( sendEventArguments ) == false )
            OnSendComplected( null, sendEventArguments );
    }

    private void OnSendComplected( object sender, SocketAsyncEventArgs eventArguments )
    {
        if ( eventArguments.SocketError == SocketError.Success )
        {
            // 전송 성공
        }
        else
        {
            // 전송 실패
        }


    }
}
