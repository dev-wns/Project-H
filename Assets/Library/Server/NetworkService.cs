using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class NetworkService
{
    int connectCount;
    Listener clientListener;

    // 메세지 전송, 수신시 필요한 오브젝트
    SocketAsyncEventArgsPool receiveEventArgsPool; // 메세지 수신용 풀
    SocketAsyncEventArgsPool sendEventArgsPool;    // 메세지 전송용 풀

    // 메세시 전송, 수신시 .Net비동기 소겟에서 사용할 버퍼를 관리하는 객체
    BufferManager bufferManager;

    // 클라이언트 접속이 이루어졌을 때 호출되는 델리게이트
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
        // 최대 동접 수 만큼 생성
        this.receiveEventArgsPool = new SocketAsyncEventArgsPool( this.maxConnections );
        this.sendEventArgsPool = new SocketAsyncEventArgsPool( this.maxConnections );

        // 매우 큰 버퍼 바이트 배열 생성
        this.bufferManager.InitBuffer();

        SocketAsyncEventArgs args;
        for ( int count = 0; count < this.maxConnections; count++ )
        {
            // 동일한 소켓에 send receive를 하므로
            // UserToken은 세션별로 하나씩만 만들어놓고
            // receive send EventArgs에서 동일한 token을 참조하도록 구성
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
        // SocketAsyncEventArgsPool에서 뺴오지 않고 그때그때 할당해서 사용한다.
        // 풀은 서버에서 클라이언트와의 통신용으로만 쓰려고 만든 것 이기 때문이다.
        // 클라이언트 입장에서 서버와 통신 할 때는 접속한 서버당 두개의 EventArgs만 있으면 되기 때문이다.
        // 서버간 연결에서도 마찬가지다.
        // 풀링 처리를 하려면 client -> server로 가는 별도의 풀을 만들어서 써야 한다.

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

    // ReceiveAsync 이후 콜백함수로 호출되는 메소드
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

    // 새로운 클라이언트가 접속 성공했을 때 호출됩니다.
    private void OnNewClient( Socket clientSocket, object token )
    {
        Interlocked.Increment( ref this.connectCount );

        Console.WriteLine( string.Format( "[{0}] A client connected. handle {1}, count{2}",
            Thread.CurrentThread.ManagedThreadId, clientSocket.Handle, this.connectCount ) );

        // 풀에서 하나 꺼내서 사용합니다.
        SocketAsyncEventArgs receiveArgs = this.receiveEventArgsPool.Pop();
        SocketAsyncEventArgs sendArgs = this.sendEventArgsPool.Pop();

        UserToken userToken = null;
        if ( this.callbackSessionCreated != null )
        {
            // as UserToken : UserToken으로 형변환 가능한가 안되면 null
            userToken = receiveArgs.UserToken as UserToken;
            this.callbackSessionCreated( userToken );
        }

        BeginReceive( clientSocket, receiveArgs, sendArgs );
    }

    private void BeginReceive( Socket socket, SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs )
    {
        // receive, send Args 둘 중 아무곳에서나 꺼내와도 됩니다.
        // 어차피 둘다 같은 UserToken을 가지고 있습니다.
        UserToken token = receiveArgs.UserToken as UserToken;
        token.SetEventArgs( receiveArgs, sendArgs );

        // 생성된 클라이언트 소켓을 보관해 놓고 통신할 때 사용합니다.
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
            // Buffer : 클라이언트로부터 수신된 데이터
            // Offset : 수신된 버퍼의 포지션
            // BytesTransferred : 수신된 데이터의 바이트 수
            token.OnReceive( args.Buffer, args.Offset, args.BytesTransferred );

            // 데이터를 한번 수신한 후 다시 호출해야 계속 데이터를 받을 수 있습니다.
            // 한번의 ReceiveAsync로 모든 데이터를 받지 못하기 때문 입니다.
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

        // 버퍼를 반환할 필요가 없다. SocketAsyncEventArg가 버퍼를 물고 있기 때문에
        // 이것을 재사용 할 때 물고 있는 버퍼를 그대로 사용하면 되기 때문이다.
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
