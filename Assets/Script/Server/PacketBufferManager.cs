using System;
using System.Collections.Generic;

public class PacketBufferManager 
{
    static object csBuffer = new object();
    static Stack<Packet> pool;
    static int poolCapacity;

    public static void Initialize( int capacity )
    {
        pool = new Stack<Packet>();
        poolCapacity = capacity;
        Allocate();
    }

    static void Allocate()
    {
        for( int count = 0; count < poolCapacity; ++count )
        {
            pool.Push( new Packet() );
        }
    }

    public static void Push( Packet packet )
    {
        lock( csBuffer )
        {
            pool.Push( packet );
        }
    }

    public static Packet Pop()
    {
        lock ( csBuffer)
        {
            // 풀 다 쓰면 새로 할당
            if ( pool.Count <= 0 )
            {
                Console.WriteLine( "Reallocate." );
                Allocate();
            }

            return pool.Pop();
        }
    }
}
