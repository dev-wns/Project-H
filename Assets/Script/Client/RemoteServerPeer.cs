using System;
using System.Collections.Generic;

public class RemoteServerPeer : IPeer
{
    public UserToken token { get; private set; }

    public RemoteServerPeer( UserToken token )
    {
        this.token = token;
        this.token.SetPeer( this );
    }

    void IPeer.OnMessage( Const<byte[]> buffer )
    {
        Packet msg = new Packet( buffer.value, this );
        PROTOCOL protocol_id = ( PROTOCOL )msg.PopProtocolID();
        switch( protocol_id)
        {
            case PROTOCOL.CHAT_MSG_ACK:
                {
                    string text = msg.PopString();
                    ChatManager.AddPacket( text );
                    // Console.WriteLine( string.Format( "text {0}", text ) );
                } break;
            default: break;
        }
    }

    void IPeer.OnRemoved()
    {
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
