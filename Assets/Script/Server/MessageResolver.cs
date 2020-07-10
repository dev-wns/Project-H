using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class MessageResolver : MonoBehaviour
{
    // 전체 데이터 전송이 완료 됐을 때 호출 할 콜백함수
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
        // 현재 들어온 데이터의 위치 저장
        int srcPosition = offset;

        // 메세지가 완성되면 콜백함수 호출
        completeCallback = callback;

        // 처리해야될 메세지 양 저장
        remainBytes = transffered;

        if ( isHeadCompleted == false )
        {
            // 패킷의 헤더 데이터가 완성되지 않았으면 읽어 온 데이터로 헤더를 완성
            isHeadCompleted = ReadHead( buffer, ref srcPosition );

            // 읽어온 데이터로도 헤더를 완성 못하면 다음 데이터 전송을 기다림
            if ( isHeadCompleted == false ) return;

            // 헤더를 완성했으면 헤더 정보에 있는 데이터의 전체 양을 확인
            msgSize = GetBodySize();

            // 잘못된 데이터인지 확인, 현재 20K 까지만 받을 수 있음
            if ( msgSize < 0 || msgSize > Protocol.completeMessageSizeClient ) return;
        }

        if ( isTypeCompleted == false )
        {
            // 남은 데이터가 있으면 타입 정보를 완성
            isTypeCompleted = ReadType( buffer, ref srcPosition );

            // 타입 정보를 완성하지 못했으면 다음 메세지 전송을 기다림
            if ( isTypeCompleted == false ) return;

            // 타입 정보를 완성했으면 패킷 타입을 정의
            msgType = BitConverter.ToInt16( typeBuffer, 0 );

            // 잘못된 데이터인지 확인
            if ( msgType < 0 || msgType > ( int )PacketType.packetCount - 1 ) return;

            // 데이터가 미완성일 경우, 다음에 전송되었을 때를 위해 저장
            preType = ( PacketType )msgType;
        }

        if ( isCompleted == false )
        {
            // 남은 데이터가 있으면 데이터 완성과정을 진행
            isCompleted = ReadBody( buffer, ref srcPosition );
            if ( isCompleted == false ) return;
        }

        // 데이터가 완성 됬으면 패킷으로 만듬
        Packet packet = new Packet();
        packet.type = msgType;
        packet.SetData( msgBuffer, msgSize );

        // 패킷이 완성됬음을 알림
        completeCallback( packet );

        // 패킷을 만드는데 사용한 버퍼 초기화
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
        // 남은 데이터가 없으면 리턴
        if ( remainBytes < 0 ) return false;

        int copySize = toSize - destPosition;
        if ( remainBytes < copySize )
            copySize = remainBytes;

        Array.Copy( buffer, srcPosition, destBuffer, destPosition, copySize );

        // 시작위치 이동
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
