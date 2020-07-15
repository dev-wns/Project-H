using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class NetworkService
{
    int connectCount;
    Listener clientListener;

    // �޼��� ����, ���Ž� �ʿ��� ������Ʈ
    SocketAsyncEventArgsPool receiveEventArgsPool; // �޼��� ���ſ� Ǯ
    SocketAsyncEventArgsPool sendEventArgsPool;    // �޼��� ���ۿ� Ǯ

    // �޼��� ����, ���Ž� .Net�񵿱� �Ұٿ��� ����� ���۸� �����ϴ� ��ü
    BufferManager bufferManager;

    // Ŭ���̾�Ʈ ������ �̷������ �� ȣ��Ǵ� ��������Ʈ
    public delegate void SessionHandler( UserToken token );
    public SessionHandler callbackSessionCreated{ get; set; }

    int maxConnections;
    int bufferSize;
    readonly int preAllocCount = 2; // read, write

    public NetworkService()
    {
        this.connectCount = 0;
        this.callbackSessionCreated = null;
    }

    public void Initialize()
    {
        this.maxConnections = 10000;
        this.bufferSize = 1024;

        this.bufferManager = new BufferManager( this.maxConnections * this.bufferSize * this.preAllocCount, this.bufferSize );
        // �ִ� ���� �� ��ŭ ����
        this.receiveEventArgsPool = new SocketAsyncEventArgsPool( this.maxConnections );
        this.sendEventArgsPool = new SocketAsyncEventArgsPool( this.maxConnections );

        // �ſ� ū ���� ����Ʈ �迭 ����
        this.bufferManager.InitBuffer();

        SocketAsyncEventArgs args;
        for ( int count = 0; count < this.maxConnections; count++ )
        {
            // ������ ���Ͽ� send receive�� �ϹǷ�
            // UserToken�� ���Ǻ��� �ϳ����� ��������
            // receive send EventArgs���� ������ token�� �����ϵ��� ����
            UserToken token = new UserToken();

            // Receive Pool
            args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>( ReceiveCompleted );
            args.UserToken = token;

            this.bufferManager.SetBuffer( args );
            this.receiveEventArgsPool.Push( args );

            // Send Pool
            args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>( SendCompleted );
            args.UserToken = token;

            this.bufferManager.SetBuffer( args );
            this.sendEventArgsPool.Push( args );
        }
    }

    public void OnConnectCompleted( Socket socket, UserToken token )
    {
        // SocketAsyncEventArgsPool���� ������ �ʰ� �׶��׶� �Ҵ��ؼ� ����Ѵ�.
        // Ǯ�� �������� Ŭ���̾�Ʈ���� ��ſ����θ� ������ ���� �� �̱� �����̴�.
        // Ŭ���̾�Ʈ ���忡�� ������ ��� �� ���� ������ ������ �ΰ��� EventArgs�� ������ �Ǳ� �����̴�.
        // ������ ���ῡ���� ����������.
        // Ǯ�� ó���� �Ϸ��� client -> server�� ���� ������ Ǯ�� ���� ��� �Ѵ�.

        SocketAsyncEventArgs receiveArgs = new SocketAsyncEventArgs();
        receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>( ReceiveCompleted );
        receiveArgs.UserToken = token;
        receiveArgs.SetBuffer( new byte[1024], 0, 1024 );

        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>( SendCompleted );
        sendArgs.UserToken = token;
        sendArgs.SetBuffer( new byte[1024], 0, 1024 );

        BeginReceive( socket, receiveArgs, sendArgs );
    }

    // ReceiveAsync ���� �ݹ��Լ��� ȣ��Ǵ� �޼ҵ�
    void ReceiveCompleted( object sender, SocketAsyncEventArgs args )
    {
        if ( args.LastOperation == SocketAsyncOperation.Receive )
        {
            ProcessReceive( args );
            return;
        }
        throw new ArgumentException( "The last operation completed on the socket was not a receive." );
    }

    void SendCompleted( object sender, SocketAsyncEventArgs args )
    {
        UserToken token = args.UserToken as UserToken;
        token.ProcessSend( args );
    }

    public void Listen( string host, int port, int backlog )
    {
        this.clientListener = new Listener();
        this.clientListener.callbackOnNewClient += OnNewClient;
        this.clientListener.Start( host, port, backlog );
    }

    // ���ο� Ŭ���̾�Ʈ�� ���� �������� �� ȣ��˴ϴ�.
    private void OnNewClient( Socket clientSocket, object token )
    {
        Interlocked.Increment( ref this.connectCount );

        Console.WriteLine( string.Format( "[{0}] A client connected. handle {1}, count{2}",
            Thread.CurrentThread.ManagedThreadId, clientSocket.Handle, this.connectCount ) );

        // Ǯ���� �ϳ� ������ ����մϴ�.
        SocketAsyncEventArgs receiveArgs = this.receiveEventArgsPool.Pop();
        SocketAsyncEventArgs sendArgs = this.sendEventArgsPool.Pop();

        UserToken userToken = null;
        if ( this.callbackSessionCreated != null )
        {
            // as UserToken : UserToken���� ����ȯ �����Ѱ� �ȵǸ� null
            userToken = receiveArgs.UserToken as UserToken;
            this.callbackSessionCreated( userToken );
        }

        BeginReceive( clientSocket, receiveArgs, sendArgs );
    }

    private void BeginReceive( Socket socket, SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs )
    {
        // receive, send Args �� �� �ƹ��������� �����͵� �˴ϴ�.
        // ������ �Ѵ� ���� UserToken�� ������ �ֽ��ϴ�.
        UserToken token = receiveArgs.UserToken as UserToken;
        token.SetEventArgs( receiveArgs, sendArgs );

        // ������ Ŭ���̾�Ʈ ������ ������ ���� ����� �� ����մϴ�.
        token.socket = socket;

        bool pending = socket.ReceiveAsync( receiveArgs );
        if ( pending == false )
        {
            ProcessReceive( receiveArgs );
        }
    }

    private void ProcessReceive( SocketAsyncEventArgs args )
    {
        UserToken token = args.UserToken as UserToken;
        if ( args.BytesTransferred > 0 && args.SocketError == SocketError.Success )
        {
            // Buffer : Ŭ���̾�Ʈ�κ��� ���ŵ� ������
            // Offset : ���ŵ� ������ ������
            // BytesTransferred : ���ŵ� �������� ����Ʈ ��
            token.OnReceive( args.Buffer, args.Offset, args.BytesTransferred );

            // �����͸� �ѹ� ������ �� �ٽ� ȣ���ؾ� ��� �����͸� ���� �� �ֽ��ϴ�.
            // �ѹ��� ReceiveAsync�� ��� �����͸� ���� ���ϱ� ���� �Դϴ�.
            bool pending = token.socket.ReceiveAsync( args );
            if ( pending == false )
            {
                ProcessReceive( args );
            }
        }
        else
        {
            Console.WriteLine( string.Format( "errer {0}, transferred{1}", args.SocketError, args.BytesTransferred ) );
            CloseClientSocket( token );
        }
    }

    public void CloseClientSocket( UserToken token )
    {
        token.OnRemoved();

        // ���۸� ��ȯ�� �ʿ䰡 ����. SocketAsyncEventArg�� ���۸� ���� �ֱ� ������
        // �̰��� ���� �� �� ���� �ִ� ���۸� �״�� ����ϸ� �Ǳ� �����̴�.
        if ( this.receiveEventArgsPool != null )
        {
            this.receiveEventArgsPool.Push( token.receiveEventArgs );
        }

        if ( this.sendEventArgsPool != null )
        {
            this.sendEventArgsPool.Push( token.sendEventArgs );
        }
    }
}
