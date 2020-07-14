using System;
using System.Collections.Generic;
using System.Net.Sockets;

// ���Ϻ��� �ΰ��� SocketAsyncEventArgs�� �ʿ��մϴ�. ( ���ۿ�, ���ſ� )
// SocketAsyncEventArgs���� ���۸� �ʿ�� �ϴµ�
// �ᱹ �ϳ��� ���Ͽ� ���ۿ� ���� �Ѱ�, ���ſ� ���� �Ѱ� �� �ΰ��� ���۸� �ʿ��մϴ�.
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
