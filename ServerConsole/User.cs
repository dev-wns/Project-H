using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class User : IPeer
{
    UserToken token;

    public User( UserToken token )
    {
        this.token = token;
        this.token.SetPeer( this );
    }

    void IPeer.OnMessage(Const<byte[]> buffer)
    {
        Packet msg = new Packet( buffer.value, this );
        PROTOCOL protocol = ( PROTOCOL )msg.PopProtocolID();
        Console.WriteLine( "------------------------------------------------------" );
        Console.WriteLine( "protocol id " + protocol );

        switch( protocol )
        {
            case PROTOCOL.CHAT_MSG_REQ:
                {
                    string text = msg.PopString();
                    Console.WriteLine( string.Format( "text {0}", text ) );

                    Packet response = Packet.Create( ( short )PROTOCOL.CHAT_MSG_ACK );
                    response.Push( text );
                    Send( response );
                } break;
            default: break;
        }
    }
    void IPeer.OnRemoved()
    {
        Console.WriteLine( "The client disconnected." );

        Program.RemoveUser( this );
    }

    public void Send( Packet msg )
    {
        this.token.Send( msg );
    }

    void IPeer.DisConnect()
    {
        this.token.socket.Disconnect( false );
    }

    void IPeer.ProcessUserOperation( Packet msg )
    {
    }
}
