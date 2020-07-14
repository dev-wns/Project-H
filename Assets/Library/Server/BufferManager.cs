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

    // totalSize = �ִ� ������ * �Ѱ��ǹ��ۻ����� * 2( ����, ���� )
    public BufferManager( int totalBytes, int bufferSize )
    {
        this.numBytes = totalBytes;
        this.currentIndex = 0;
        this.bufferSize = bufferSize;
        this.freeIndexPool = new Stack<int>();
    }

    public void InitBuffer()
    {
        // �Ŵ��� ����Ʈ �迭 ����
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
            // ���� ���� ���� �ε��� ���� �������� ���� ���� ��ġ�� ����ų �� �ֵ��� ��.
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
