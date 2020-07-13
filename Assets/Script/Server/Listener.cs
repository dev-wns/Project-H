using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Listener
{    
    // �񵿱� Accept�� ���� EventArgs.
    SocketAsyncEventArgs acceptArgs;
    
    Socket listenSocket;

    // Acceptó���� ������ �����ϱ� ���� �̺�Ʈ ����.
    // ManualResetEvent : ���� Reset�޼ҵ带 ȣ������ �ʴ� �� ��� Set���·� �����ֽ��ϴ�.
    // AutoResetEvent   : �ѹ� Set�� ���� �ڵ����� Reset���·� ������ݴϴ�.
    AutoResetEvent flowControlEvent;
    
    // ���ο� Ŭ���̾�Ʈ�� �������� �� ȣ��Ǵ� �ݹ�.
    public delegate void NewclientHandler( Socket clientSocket, object token );
    public NewclientHandler callbackOnNewClient;
    
    public Listener()
    {
        this.callbackOnNewClient = null;
    }
    
    public void Start( string host, int port, int backlog )
    {
        this.listenSocket = new Socket( AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp );
    
        IPAddress address;
        if ( host == "0.0.0.0" )
        {
            address = IPAddress.Any;
        }
        else
        {
            address = IPAddress.Parse( host );
        }
        IPEndPoint endPoint = new IPEndPoint( address, port );
    
        try
        {
            listenSocket.Bind( endPoint );
            listenSocket.Listen( backlog );
    
            this.acceptArgs = new SocketAsyncEventArgs();
            this.acceptArgs.Completed += new EventHandler<SocketAsyncEventArgs>( OnAcceptCompleted );
    
            Thread listenThread = new Thread( DoListen );
            listenThread.Start();
        }
        catch ( Exception except )
        {
            Console.WriteLine(except.Message);
        }
    }
    
    // ������ ���� Ŭ���̾�Ʈ�� �޾Ƶ��Դϴ�.
    // �ϳ��� ���� ó���� �Ϸ�� �� ���� accept�� �����ϱ� ���ؼ� event��ü�� ���� �帧�� �����ϵ��� �����Ǿ� �ֽ��ϴ�.
    void DoListen()
    {
        this.flowControlEvent = new AutoResetEvent( false );
    
        while ( true )
        {
            // SocketAsyncEventArgs�� ���� �ϱ� ���ؼ� null�� ����� �ش�.
            this.acceptArgs.AcceptSocket = null;
    
            bool pending = true;
            try
            {
                // �񵿱� accept�� ȣ���Ͽ� Ŭ���̾�Ʈ�� ������ �޾Ƶ��Դϴ�.
                // �񵿱� �żҵ� ������ ���������� ������ �Ϸ�� ��쵵 ������
                // ���ϰ��� Ȯ���Ͽ� �б���Ѿ� �մϴ�.
                pending = listenSocket.AcceptAsync( this.acceptArgs );
            }
            catch ( Exception except )
            {
                Console.WriteLine( except.Message );
                continue;
            }
    
            // ��� �Ϸ� �Ǹ� �̺�Ʈ�� �߻����� �����Ƿ� ���ϰ��� false�� ��� �ݹ� �żҵ带 ���� ȣ���� �ݴϴ�.
            // pending���¶�� �񵿱� ��û�� �� �����̹Ƿ� �ݹ� �żҵ带 ��ٸ��� �˴ϴ�.
            // http://msdn.microsoft.com/ko-kr/library/system.net.sockets.socket.acceptasync%28v=vs.110%29.aspx
            if ( pending == false )
            {
                OnAcceptCompleted( null, this.acceptArgs );
            }
    
            // Ŭ���̾�Ʈ ���� ó���� �Ϸ�Ǹ� �̺�Ʈ ��ü�� ��ȣ�� ���޹޾� �ٽ� ������ �����ϵ��� �մϴ�.
            this.flowControlEvent.WaitOne();
    
            // *�� : �ݵ�� WaitOne -> Set ������ ȣ�� �Ǿ� �ϴ� ���� �ƴմϴ�.
            //      Accept�۾��� ������ ���� ������ Set -> WaitOne ������ ȣ��ȴٰ� �ϴ��� 
            //      ���� Accept ȣ�� ���� ���� ���� �̷�� ���ϴ�.
            //      WaitOne�żҵ尡 ȣ��� �� �̺�Ʈ ��ü�� �̹� signalled ���¶�� �����带 ��� ���� �ʰ� ��� �����ϱ� �����Դϴ�.
        }
    }
    
    // AcceptAsync�� �ݹ� �żҵ�
    void OnAcceptCompleted( object sender, SocketAsyncEventArgs args )
    {
        if ( args.SocketError == SocketError.Success )
        {
            // ���� ���� ������ ������ ������~
            Socket client_socket = args.AcceptSocket;
    
            // ���� ������ �޾Ƶ��δ�.
            this.flowControlEvent.Set();
    
            // �� Ŭ���������� accept������ ���Ҹ� �����ϰ� Ŭ���̾�Ʈ�� ���� ������ ó����
            // �ܺη� �ѱ�� ���ؼ� �ݹ� �żҵ带 ȣ���� �ֵ��� �մϴ�.
            // ������ ���� ó���ο� ������ �����θ� �и��ϱ� �����Դϴ�.
            // ������ �����κ��� ���� �ٲ� ���ɼ��� ������, ���� Accept�κ��� ��������� ������ ���� �κ��̱� ������
            // ������ �и������ִ°��� �����ϴ�.
            // ���� Ŭ���� ���� ��ħ�� ���� Listen�� ���õ� �ڵ常 �����ϵ��� �ϱ� ���� ������ �ֽ��ϴ�.
            if ( this.callbackOnNewClient != null )
            {
                this.callbackOnNewClient( client_socket, args.UserToken );
            }
    
            return;
        }
        else
        {
            // Accept ����
            // Console.WriteLine("Failed To Accept Client");
        }
    
        // ���� ������ �޾Ƶ��δ�.
        this.flowControlEvent.Set();
    }
}
