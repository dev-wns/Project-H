using System;
using System.Collections.Generic;
using System.Net.Sockets;

public class UserToken 
{
    public Socket socket { get; set; }

    public SocketAsyncEventArgs receiveEventArgs { get; private set; }
    public SocketAsyncEventArgs sendEventArgs { get; private set; }

    // ����Ʈ�� ��Ŷ �������� �ؼ����ִ� �ؼ���.
    MessageResolver messageResolver;

    // session��ü. ���ø����̼� ������ �����Ͽ� ���.
    IPeer peer;

    // ������ ��Ŷ�� �����س��� ť 1-Send�� ó���ϱ� ���� ť�Դϴ�.
    Queue<Packet> sendingQueue;

    // sendingQueue lockó���� ���Ǵ� ��ü
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

    // ��Ŷ�� �����մϴ�.
    // ť�� ��� ���� ��쿡�� ť�� �߰��� �� �ٷ� SendAsync�޼ҵ带 ȣ���ϰ�,
    // �����Ͱ� ������� ��쿡�� ���� �߰��� �մϴ�.
    
    // ť�׵� ��Ŷ�� ���� ���� :
    // ���� �������� SendAsync�� �Ϸ�Ǿ��� �� ť�� �˻��Ͽ� ������ ��Ŷ�� �����մϴ�.
    public void Send( Packet msg )
    {
        lock( this.csSendingQueue )
        {
            // ť�� ��� �ִٸ� ť�� �߰��ϰ� �ٷ� �񵿱� ���� �޼ҵ带 ȣ���մϴ�.
            if ( this.sendingQueue.Count <= 0 )
            {
                this.sendingQueue.Enqueue( msg );
                StartSend();
                return;
            }

            // ť�� ���𰡰� ��� �ִٸ� ���� ���� ������ �Ϸ�������� �����̹Ƿ� ť�� �߰��� �ϰ� �����մϴ�.
            // ���� �������� SendAsync�� �Ϸ�� ���Ŀ� ť�� �˻��Ͽ� �����Ͱ� ������ SendAsync�� ȣ���Ͽ� �������ݴϴ�.
            this.sendingQueue.Enqueue( msg );
        }
    }

    // �񵿱� ������ �����մϴ�.
    private void StartSend()
    {
        lock( this.csSendingQueue )
        {
            // ���� ������ �Ϸ�� ���°� �ƴϹǷ� �����͸� �������� ť���� �������� �ʽ��ϴ�.
            Packet msg = this.sendingQueue.Peek();

            // ����� ��Ŷ ����� ����մϴ�.
            msg.RecordSize();

            // �̹��� ���� ��Ŷ �����ŭ ���� ũ�⸦ �����ϰ�
            this.sendEventArgs.SetBuffer( this.sendEventArgs.Offset, msg.position );
            // ��Ŷ ������ SocketAsyncEventArgs���ۿ� �����մϴ�.
            Array.Copy( msg.buffer, 0, this.sendEventArgs.Buffer, this.sendEventArgs.Offset, msg.position );

            // �񵿱� ���� ����
            bool pending = this.socket.SendAsync( this.sendEventArgs );
            if ( pending == false )
            {
                ProcessSend( this.sendEventArgs );
            }
        }
    }

    static int sendCount = 0;
    static object csCount = new object();

    // �񵿱� ���� �Ϸ� �� ȣ��Ǵ� �ݹ� �޼ҵ�
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

            // ��Ŷ �ϳ��� �� ������ ���
            int size = this.sendingQueue.Peek().position;
            if ( args.BytesTransferred != size )
            {
                Console.WriteLine( string.Format( "Need to send more! transferred {0},  packet size {1}", args.BytesTransferred, size ) );

                // ���� ������ ��ŭ ���۸� �缳�� ���ְ� �ٽ� �����մϴ�.
                args.SetBuffer( args.Offset, size - args.BytesTransferred );
                bool pending = this.socket.SendAsync( args );
                if ( pending == false )
                {
                    ProcessSend( this.sendEventArgs );
                }

                return;
            }
        }

        // �ܼ� ��¿�.
        lock ( csCount)
        {
            ++sendCount;
            Console.WriteLine( string.Format( "process send : {0}, transferred {1}, sent count {2}",
            args.SocketError, args.BytesTransferred, sendCount ) );
        }

        // ���� �Ϸ�� ��Ŷ�� ť���� �����մϴ�.
        Packet packet = this.sendingQueue.Dequeue();
        Packet.Destroy( packet );

        // ���� ���۵��� �ʰ� ������� ��Ŷ�� ������ �ٽ��ѹ� ������ ��û�մϴ�.
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
