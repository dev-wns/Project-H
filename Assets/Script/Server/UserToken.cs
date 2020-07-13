using System;
using System.Collections.Generic;
using System.Net.Sockets;

public class UserToken 
{
    public Socket socket { get; set; }

    public SocketAsyncEventArgs receiveEventArgs { get; private set; }
    public SocketAsyncEventArgs sendEventArgs { get; private set; }

    // 바이트를 패킷 형식으로 해석해주는 해석기.
    MessageResolver messageResolver;

    // session객체. 어플리케이션 딴에서 구현하여 사용.
    IPeer peer;

    // 전송할 패킷을 보관해놓는 큐 1-Send로 처리하기 위한 큐입니다.
    Queue<Packet> sendingQueue;

    // sendingQueue lock처리에 사용되는 객체
    private object csSendingQueue;

    public UserToken()
    {
        this.csSendingQueue = new object();
        this.messageResolver = new MessageResolver();
        this.peer = null;
        this.sendingQueue = new Queue<Packet>();
    }

    public void SetPeer( IPeer peer )
    {
        this.peer = peer;
    }

    public void SetEventArgs( SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs )
    {
        receiveEventArgs = receiveArgs;
        sendEventArgs = sendEventArgs;
    }

    public void OnReceive( byte[] buffer, int offset, int transfered )
    {
        this.messageResolver.OnReceive( buffer, offset, transfered, OnMessage );
    }

    private void OnMessage( Const<byte[]> buffer )
    {
        if( this.peer != null )
        {
            this.peer.OnMessage( buffer );
        }
    }

    public void OnRemoved()
    {
        this.sendingQueue.Clear();
        if ( this.peer != null )
        {
            this.peer.OnRemoved();
        }
    }

    // 패킷을 전송합니다.
    // 큐가 비어 있을 경우에는 큐에 추가한 뒤 바로 SendAsync메소드를 호출하고,
    // 데이터가 들어있을 경우에는 새로 추가만 합니다.
    
    // 큐잉된 패킷의 전송 시점 :
    // 현재 전송중인 SendAsync가 완료되었을 때 큐를 검사하여 나머지 패킷을 전송합니다.
    public void Send( Packet msg )
    {
        lock( this.csSendingQueue )
        {
            // 큐가 비어 있다면 큐에 추가하고 바로 비동기 전송 메소드를 호출합니다.
            if ( this.sendingQueue.Count <= 0 )
            {
                this.sendingQueue.Enqueue( msg );
                StartSend();
                return;
            }

            // 큐에 무언가가 들어 있다면 아직 이전 전송이 완료되지않은 상태이므로 큐에 추가만 하고 리턴합니다.
            // 현재 수행중인 SendAsync가 완료된 이후에 큐를 검사하여 데이터가 있으면 SendAsync를 호출하여 전송해줍니다.
            this.sendingQueue.Enqueue( msg );
        }
    }

    // 비동기 전송을 시작합니다.
    private void StartSend()
    {
        lock( this.csSendingQueue )
        {
            // 아직 전송이 완료된 상태가 아니므로 데이터만 가져오고 큐에서 제거하진 않습니다.
            Packet msg = this.sendingQueue.Peek();

            // 헤더에 패킷 사이즈를 기록합니다.
            msg.RecordSize();

            // 이번에 보낼 패킷 사이즈만큼 버퍼 크기를 설정하고
            this.sendEventArgs.SetBuffer( this.sendEventArgs.Offset, msg.position );
            // 패킷 내용을 SocketAsyncEventArgs버퍼에 복사합니다.
            Array.Copy( msg.buffer, 0, this.sendEventArgs.Buffer, this.sendEventArgs.Offset, msg.position );

            // 비동기 전송 시작
            bool pending = this.socket.SendAsync( this.sendEventArgs );
            if ( pending == false )
            {
                ProcessSend( this.sendEventArgs );
            }
        }
    }

    static int sendCount = 0;
    static object csCount = new object();

    // 비동기 전송 완료 시 호출되는 콜백 메소드
    public void ProcessSend( SocketAsyncEventArgs args )
    {
        if ( args.BytesTransferred <= 0 || args.SocketError != SocketError.Success )
        {
            return;
        }

        lock( this.csSendingQueue )
        {
            if (this.sendingQueue.Count <= 0 )
            {
                throw new Exception( "Sending queue count is less than zero!" );
            }

            // 패킷 하나를 다 못보낸 경우
            int size = this.sendingQueue.Peek().position;
            if ( args.BytesTransferred != size )
            {
                Console.WriteLine( string.Format( "Need to send more! transferred {0},  packet size {1}", args.BytesTransferred, size ) );

                // 남은 데이터 만큼 버퍼를 재설정 해주고 다시 전송합니다.
                args.SetBuffer( args.Offset, size - args.BytesTransferred );
                bool pending = this.socket.SendAsync( args );
                if ( pending == false )
                {
                    ProcessSend( this.sendEventArgs );
                }

                return;
            }
        }

        // 콘솔 출력용.
        lock ( csCount)
        {
            ++sendCount;
            Console.WriteLine( string.Format( "process send : {0}, transferred {1}, sent count {2}",
            args.SocketError, args.BytesTransferred, sendCount ) );
        }

        // 전송 완료된 패킷을 큐에서 제거합니다.
        Packet packet = this.sendingQueue.Dequeue();
        Packet.Destroy( packet );

        // 아직 전송되지 않고 대기중인 패킷이 있으면 다시한번 전송을 요청합니다.
        if ( this.sendingQueue.Count > 0 )
        {
            StartSend();
        }
    }

    public void Disconnect()
    {
        try
        {
            this.socket.Shutdown( SocketShutdown.Send );
        }
        catch ( Exception except ) { Console.WriteLine( except.Message ); }
        this.socket.Close();
    }

    public void StartKeepalive()
    {
        System.Threading.Timer keepalive = new System.Threading.Timer( ( object e ) =>
         {
             Packet msg = Packet.Create( 0 );
             msg.Push( 0 );
             Send( msg );
         }, null, 0, 3000 );
    }
}
