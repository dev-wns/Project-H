using System;
using System.Collections.Generic;
using System.Net.Sockets;

// 소켓별로 두개의 SocketAsyncEventArgs가 필요합니다. ( 전송용, 수신용 )
// SocketAsyncEventArgs마다 버퍼를 필요로 하는데
// 결국 하나의 소켓에 전송용 버퍼 한개, 수신용 버퍼 한개 총 두개의 버퍼만 필요합니다.
public class SocketAsyncEventArgsPool
{
    Stack<SocketAsyncEventArgs> pool;

    public SocketAsyncEventArgsPool( int capacity )
    {
        pool = new Stack<SocketAsyncEventArgs>( capacity );
    }

    public void Push( SocketAsyncEventArgs item )
    {
        if ( item == null )
        {
            throw new ArgumentException( "Items added to a SocketAsyncEventArgsPool cannot be null" );
        }

        lock ( pool )
        {
            pool.Push( item );
        }
    }

    public SocketAsyncEventArgs Pop()
    {
        lock ( pool )
        {
            return pool.Pop();
        }
    }

    public int Count {  get { return pool.Count; } }
}
