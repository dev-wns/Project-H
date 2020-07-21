using System;
using System.Collections.Generic;


public class RemoteServerPeer : IPeer
{
    public UserToken token { get; private set; }
    WeakReference networkEventManager;

    public RemoteServerPeer( UserToken token )
    {
        this.token = token;
        this.token.SetPeer( this );
    }

    public void SetEventManager( EventManager eventManager )
    {
        this.networkEventManager = new WeakReference( eventManager );
    }

    void IPeer.OnMessage( Const<byte[]> buffer )
    {
        // ���۸� ������ �� PakcetŬ������ ���� �� �Ѱ��ݴϴ�.
        // Packet Ŭ���� ���ο����� �����θ� ��� �ֽ��ϴ�.
        byte[] appBuffer = new byte[buffer.value.Length];
        Array.Copy( buffer.value, appBuffer, buffer.value.Length );
        Packet msg = new Packet( appBuffer );
        ( this.networkEventManager.Target as EventManager ).EnqueueNetworkMessage( msg );

        // Packet msg = new Packet( buffer.value, this );
        // PROTOCOL protocol_id = ( PROTOCOL )msg.PopProtocolID();
        // switch( protocol_id)
        // {
        //     case PROTOCOL.CHAT_MSG_ACK:
        //         {
        //             string text = msg.PopString();
        //             ChatManager.AddPacket( text );
        //             // Console.WriteLine( string.Format( "text {0}", text ) );
        //         } break;
        //     default: break;
        // }
    }

    void IPeer.OnRemoved()
    {
        ( this.networkEventManager.Target as EventManager ).EnqueueNetworkEvent( NETWORK_EVENT.DISCONNECTED );
        Console.WriteLine( "Server Removed. ");
    }

    void IPeer.Send( Packet msg )
    {
        this.token.Send( msg );
    }

    void IPeer.DisConnect()
    {
        this.token.socket.Disconnect( false );
    }

    void IPeer.ProcessUserOperation( Packet msg ) { }
}
