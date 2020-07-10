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
            // GamePacketHandler ��ü���� ��Ŷó�� 
            foreach ( Packet packet in receivePacketList )
                packetHandler.ParsePacket( packet );

            receivePacketList.Clear();
        }
    }

    public void Init()
    {
        // ���� byte�迭�� ��Ŷ���� ����� ����Ʈ�� �ְ� gamePacketHandler���� ó��
        receivePacketList = new LinkedList<Packet>();
        receiveBuffer = new byte[Protocol.socketBufferSize];
        msgResolver = new MessageResolver();

        // ���۵� ��Ŷ�� ó��
        packetHandler = new PacketHandler();
        packetHandler.Init( this );

        // �޼��� ���� ���� ���� ( 4K )
        // ���� 10K�� [ 4, 4, 2 ]�� 3���� ������ �̺�Ʈ �߻�
        receiveEventArgs = new SocketAsyncEventArgs();
        receiveEventArgs.Completed += OnReceiveComplected;
        receiveEventArgs.UserToken = this;
        receiveEventArgs.SetBuffer( receiveBuffer, 0, 1024 * 4 );
    }

    // �޼��� �ޱ� ����
    public void StartReceive()
    {
        // ������ ����� ���¿��� �� �Լ��� ȣ���Ͽ� �޼����� ���⸦ ��ٸ���.
        // �޼����� ���� onReceiveComplected �ݹ��Լ��� ȣ���
        if ( socket.ReceiveAsync( receiveEventArgs ) == false )
            OnReceiveComplected( this, receiveEventArgs );
    }

    // �޼����� ���� �� �����ϴ� �ݹ��Լ�
    void OnReceiveComplected( object sender, SocketAsyncEventArgs socketEventArgs )
    {
        if ( socketEventArgs.BytesTransferred > 0 &&
             socketEventArgs.SocketError == SocketError.Success )
        {
            // ���� ����
            // byte�迭�� �����͸� �ٽ� ��Ŷ���� ������ش�.
            msgResolver.OnReceive( socketEventArgs.Buffer, socketEventArgs.Offset, 
                                   socketEventArgs.BytesTransferred, OnMessageComplected );

            // ���ο� �޼����� �޴´�
            StartReceive();
        }
        else
        {
            // ���� ����, ������ �����ų� ����� �Ұ��� �� ��
        }
    }
}