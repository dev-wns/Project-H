using System;
using System.Collections.Generic;

class Defines
{
    public static readonly short HEADERSIZE = 2;
}

// [header][body] 구조를 갖는 데이터를 파싱하는 클래스
// - header : 데이터 사이즈. Defines.HEADERSIZER에 정의된 타입만큼의 크기를 갖는다.
//            2바이트일 경우 Int16, 4바이트일 경우 Int32로 처리하면 된다.
//            본문의 크기가 Int16.Max값을 넘지 않는다면 2바이트로 처리하는 것이 좋을 것 같다.
//
// - body   : 메세지 본문
public class MessageResolver
{
    public delegate void CompletedMessageACallback( Const<byte[]> buffer );

    // 메세지 사이즈
    int messageSize;

    // 진행중인 버퍼
    byte[] messageBuffer = new byte[1024];

    // 현재 진행중인 버퍼의 인덱스를 가리키는 변수.
    // 패킷 하나를 완성한 뒤에는 0으로 초기화 시켜줘야 한다.
    int currentPosition;

    // 읽어와야 할 목표 위치
    int positionToRead;

    // 남은 사이즈
    int remainBytes;

    // 목표지점으로 설정된 위치까지의 바이트를 원본 버퍼로부터 복사한다.
    // 데이터가 모자랄 경우 현재 남은 바이트 까지만 복사한다.
    // <return> 다 읽었으면 ture, 데이터가 모자라서 못읽었으면 false를 리턴한다.
    bool ReadUntil( byte[] buffer, ref int srcPosition, int offset, int transferred )
    {
        if ( this.currentPosition >= offset + transferred )
        {
            // 들어온 데이터 만큼 다 읽은 상태이므로 더이상 읽을 데이터가 없다.
            return true;
        }

        // 읽어와야 할 바이트
        // 데이터가 분리되어 올 경우 이전에 읽어놓은 값을 뺴줘서 부족한 만큼 읽어올 수 있도록 계산해 준다.
        int copySize = this.positionToRead - this.currentPosition;

        // 남은 데이터가 더 적다면 가능한 만큼만 복사한다.
        if ( this.remainBytes < copySize )
        {
            copySize = this.remainBytes;
        }

        // 버퍼에 복사
        Array.Copy( buffer, srcPosition, this.messageBuffer, this.currentPosition, copySize );

        // 원본 버퍼 포지션 이동.
        srcPosition += copySize;

        // 타겟 버퍼 포지션 이동
        this.currentPosition += copySize;

        // 남은 바이트 수
        this.remainBytes -= copySize;

        // 목표지점에 도달 못했으면 false
        if ( this.currentPosition < this.positionToRead )
        {
            return false;
        }

        return true;
    }

    // 소켓 버퍼로부터 데이터를 수신할 때마다 호출된다.
    // 데이터가 남아 있을 때까지 계속 패킷을 만들어 callback함수를 호출한다.
    // 하나의 패킷을 완성하지 못했다면 버퍼에 보관해 놓은 뒤 사음 수신을 기다린다.
    public void OnReceive( byte[] buffer, int offset, int transferred, CompletedMessageACallback callback )
    {
        // 이번 receive로 읽어오게 될 바이트 수
        this.remainBytes = transferred;

        // 원본 버퍼의 포지션값.
        // 패킷이 여러개 뭉쳐 올 경우 원본 버퍼의 포지션은 계속 앞으로 가야하는데 그 처리를 위한 변수
        int srcPosition = offset;

        // 남은 데이터가 있다면 계속 반복한다.
        while ( this.remainBytes > 0 )
        {
            bool completed = false;

            // 헤더만큼 못읽은 경우 헤더를 먼저 읽는다.
            if ( this.currentPosition < Defines.HEADERSIZE )
            {
                // 목표지점 설정 ( 헤더 위치까지 도달하도록 설정 )
                this.positionToRead = Defines.HEADERSIZE;

                completed = ReadUntil( buffer, ref srcPosition, offset, transferred );
                if ( completed == false )
                {
                    // 아직 다 못읽었으므로 다음 receive를 기다린다.
                    return;
                }

                // 헤더 하나를 온전히 읽어왔으므로 메세지 사이즈를 구한다.
                this.messageSize = GetBodySize();

                // 다음 목표 지점 ( 헤더 + 메세지 사이즈 )
                this.positionToRead = this.messageSize + Defines.HEADERSIZE;
            }

            // 메세지를 읽는다.
            completed = ReadUntil( buffer, ref srcPosition, offset, transferred );

            if ( completed == true )
            {
                // 패킷 하나를 완성 했다.

                callback( new Const<byte[]>( this.messageBuffer ) );

                ClearBuffer();
            }
        }
    }

    int GetBodySize()
    {
        // 헤더 타입의 바이트만큼을 읽어와 메세지 사이즈를 리턴한다.
        Type type = Defines.HEADERSIZE.GetType();
        if ( type.Equals( typeof( Int16 ) ) == true )
        {
            return BitConverter.ToInt16( this.messageBuffer, 0 );
        }
        return BitConverter.ToInt32( this.messageBuffer, 0 );
    }

    void ClearBuffer()
    {
        Array.Clear( this.messageBuffer, 0, this.messageBuffer.Length );

        this.currentPosition = 0;
        this.messageSize = 0;
    }
}
