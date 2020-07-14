using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class BufferManager 
{
    int numBytes;
    byte[] buffer;
    Stack<int> freeIndexPool;
    int currentIndex;
    int bufferSize;

    // totalSize = 최대 동접수 * 한개의버퍼사이즈 * 2( 전송, 수신 )
    public BufferManager( int totalBytes, int bufferSize )
    {
        this.numBytes = totalBytes;
        this.currentIndex = 0;
        this.bufferSize = bufferSize;
        this.freeIndexPool = new Stack<int>();
    }

    public void InitBuffer()
    {
        // 거대한 바이트 배열 생성
        buffer = new byte[numBytes];
    }

    public bool SetBuffer( SocketAsyncEventArgs args )
    {
        if ( freeIndexPool.Count > 0 )
        {
            args.SetBuffer( buffer, freeIndexPool.Pop(), bufferSize );
        }
        else
        {
            if ( ( numBytes - bufferSize ) < currentIndex ) 
            {
                return false;
            }
            // 버퍼 설정 이후 인덱스 값을 증가시켜 다음 버퍼 위치를 가르킬 수 있도록 함.
            args.SetBuffer( buffer, currentIndex, bufferSize );
            currentIndex += bufferSize;
        }

        return true;
    }

    public void FreeBuffer( SocketAsyncEventArgs args )
    {
        freeIndexPool.Push( args.Offset );
        args.SetBuffer( null, 0, 0 );
    }
}
