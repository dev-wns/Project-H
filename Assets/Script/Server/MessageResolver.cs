using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class MessageResolver : MonoBehaviour
{
    // ��ü ������ ������ �Ϸ� ���� �� ȣ�� �� �ݹ��Լ�
    public delegate void CompletedMessageCallback( Packet packet );

    int msgSize;
    byte[] msgBuffer    = new byte[1024 * 2000]; // 2000K
    byte[] headerBuffer = new byte[4];           // 4Byte;
    byte[] typeBuffer   = new byte[2];           // 2Byte;

    PacketType preType;

    int headPosition;
    int typePosition;
    int currentPosition;

    short msgType;
    int remainBytes;

    bool isHeadCompleted;
    bool isTypeCompleted;
    bool isCompleted;

    CompletedMessageCallback completeCallback;

    public MessageResolver()
    {
        ClearBuffer();
    }

    public void OnReceive( byte[] buffer, int offset, int transffered, CompletedMessageCallback callback )
    {
        // ���� ���� �������� ��ġ ����
        int srcPosition = offset;

        // �޼����� �ϼ��Ǹ� �ݹ��Լ� ȣ��
        completeCallback = callback;

        // ó���ؾߵ� �޼��� �� ����
        remainBytes = transffered;

        if ( isHeadCompleted == false )
        {
            // ��Ŷ�� ��� �����Ͱ� �ϼ����� �ʾ����� �о� �� �����ͷ� ����� �ϼ�
            isHeadCompleted = ReadHead( buffer, ref srcPosition );

            // �о�� �����ͷε� ����� �ϼ� ���ϸ� ���� ������ ������ ��ٸ�
            if ( isHeadCompleted == false ) return;

            // ����� �ϼ������� ��� ������ �ִ� �������� ��ü ���� Ȯ��
            msgSize = GetBodySize();

            // �߸��� ���������� Ȯ��, ���� 20K ������ ���� �� ����
            if ( msgSize < 0 || msgSize > Protocol.completeMessageSizeClient ) return;
        }

        if ( isTypeCompleted == false )
        {
            // ���� �����Ͱ� ������ Ÿ�� ������ �ϼ�
            isTypeCompleted = ReadType( buffer, ref srcPosition );

            // Ÿ�� ������ �ϼ����� �������� ���� �޼��� ������ ��ٸ�
            if ( isTypeCompleted == false ) return;

            // Ÿ�� ������ �ϼ������� ��Ŷ Ÿ���� ����
            msgType = BitConverter.ToInt16( typeBuffer, 0 );

            // �߸��� ���������� Ȯ��
            if ( msgType < 0 || msgType > ( int )PacketType.packetCount - 1 ) return;

            // �����Ͱ� �̿ϼ��� ���, ������ ���۵Ǿ��� ���� ���� ����
            preType = ( PacketType )msgType;
        }

        if ( isCompleted == false )
        {
            // ���� �����Ͱ� ������ ������ �ϼ������� ����
            isCompleted = ReadBody( buffer, ref srcPosition );
            if ( isCompleted == false ) return;
        }

        // �����Ͱ� �ϼ� ������ ��Ŷ���� ����
        Packet packet = new Packet();
        packet.type = msgType;
        packet.SetData( msgBuffer, msgSize );

        // ��Ŷ�� �ϼ������� �˸�
        completeCallback( packet );

        // ��Ŷ�� ����µ� ����� ���� �ʱ�ȭ
        ClearBuffer();
    }

    public void ClearBuffer()
    {
        Array.Clear( msgBuffer, 0, msgBuffer.Length );
        Array.Clear( headerBuffer, 0, headerBuffer.Length );
        Array.Clear( typeBuffer, 0, typeBuffer.Length );

        msgSize = 0;
        headPosition = 0;
        typePosition = 0;
        currentPosition = 0;
        msgType = 0;
        // remainBytes = 0;

        isHeadCompleted = false;
        isTypeCompleted = false;
        isCompleted = false;
    }

    private bool ReadHead( byte[]buffer, ref int srcPosition )
    {
        return ReadUntil( buffer, ref srcPosition, headerBuffer, ref headPosition, 4 );
    }

    private bool ReadType( byte[] buffer, ref int srcPosition )
    {
        return ReadUntil( buffer, ref srcPosition, typeBuffer, ref typePosition, 4 );
    }

    private bool ReadBody( byte[] buffer, ref int srcPosition )
    {
        return ReadUntil( buffer, ref srcPosition, msgBuffer, ref currentPosition, msgSize );
    }

    bool ReadUntil( byte[] buffer, ref int srcPosition, byte[] destBuffer, ref int destPosition, int toSize )
    {
        // ���� �����Ͱ� ������ ����
        if ( remainBytes < 0 ) return false;

        int copySize = toSize - destPosition;
        if ( remainBytes < copySize )
            copySize = remainBytes;

        Array.Copy( buffer, srcPosition, destBuffer, destPosition, copySize );

        // ������ġ �̵�
        srcPosition += copySize;
        destPosition += copySize;
        remainBytes -= copySize;

        return !( destPosition < toSize );
    }

    int GetBodySize()
    {
        Type type = Protocol.headSize.GetType();
        if ( type.Equals( typeof( Int16 ) ) == true )
            return BitConverter.ToInt16( headerBuffer, 0 );

        return BitConverter.ToInt32( headerBuffer, 0 );
    }
}
