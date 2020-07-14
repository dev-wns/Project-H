using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Program
{
    static List<User> userList;

    static void Main( string[] args )
    {
        PacketBufferManager.Initialize( 2000 );     
        userList = new List<User>();

        NetworkService service = new NetworkService();
        service.callbackSessionCreated += OnSessionCreated;
        service.Initialize();
        service.Listen( "0.0.0.0", 7979, 100 );

        Console.WriteLine( "Started! " );
        while( true )
        {
            System.Threading.Thread.Sleep( 1000 );
        }
    }

    static void OnSessionCreated( UserToken token )
    {
        User user = new User( token );

        lock( userList )
        {
            userList.Add( user );
        }
    }

    public static void RemoveUser( User user )
    {
        lock( userList )
        {
            userList.Remove( user );
        }
    }
}

