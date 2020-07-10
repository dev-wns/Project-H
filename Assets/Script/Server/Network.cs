using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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

    SocketAsyncEventArgs receiveEventArgs;
    MessageResolver msgResolver;
    LinkedList<Packet> receivePacketList;
    PacketHandler packetHandler;
    byte[] receiveBuffer;
    private Mutex mutexReceivePacketList;

    void OnMessageComplected( Packet packet )
    {
        PushPacket( packet );
    }

    private void PushPacket( Packet packet )
    {
        lock ( mutexReceivePacketList )
        {
            receivePacketList.AddLast( packet );
        }
    }

    public void Processpackets()
    {
        lock( mutexReceivePacketList )
        {
            // GamePacketHandler 객체에서 패킷처리 
            foreach ( Packet packet in receivePacketList )
                packetHandler.ParsePacket( packet );

            receivePacketList.Clear();
        }
    }

    public void Init()
    {
        // 받은 byte배열을 패킷으로 만들어 리스트에 넣고 gamePacketHandler에서 처리
        receivePacketList = new LinkedList<Packet>();
        receiveBuffer = new byte[Protocol.socketBufferSize];
        msgResolver = new MessageResolver();

        // 전송된 패킷을 처리
        packetHandler = new PacketHandler();
        packetHandler.Init( this );

        // 메세지 받을 버퍼 설정 ( 4K )
        // 만약 10K면 [ 4, 4, 2 ]로 3번에 나눠서 이벤트 발생
        receiveEventArgs = new SocketAsyncEventArgs();
        receiveEventArgs.Completed += OnReceiveComplected;
        receiveEventArgs.UserToken = this;
        receiveEventArgs.SetBuffer( receiveBuffer, 0, 1024 * 4 );
    }

    // 메세지 받기 시작
    public void StartReceive()
    {
        // 서버가 연결된 상태에서 이 함수를 호출하여 메세지가 오기를 기다리자.
        // 메세지가 오면 onReceiveComplected 콜백함수가 호출됨
        if ( socket.ReceiveAsync( receiveEventArgs ) == false )
            OnReceiveComplected( this, receiveEventArgs );
    }

    // 메세지가 왔을 때 동작하는 콜백함수
    void OnReceiveComplected( object sender, SocketAsyncEventArgs socketEventArgs )
    {
        if ( socketEventArgs.BytesTransferred > 0 &&
             socketEventArgs.SocketError == SocketError.Success )
        {
            // 전송 성공
            // byte배열의 데이터를 다시 패킷으로 만들어준다.
            msgResolver.OnReceive( socketEventArgs.Buffer, socketEventArgs.Offset, 
                                   socketEventArgs.BytesTransferred, OnMessageComplected );

            // 새로운 메세지를 받는다
            StartReceive();
        }
        else
        {
            // 전송 실패, 서버가 닫혔거나 통신이 불가능 할 때
        }
    }
}