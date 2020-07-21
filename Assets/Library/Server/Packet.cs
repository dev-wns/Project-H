using System;
using System.Collections.Generic;

// byte[] 버퍼를 참조로 보관하여 PopXXX메소드 호출 순서대로 데이터 변환을 수행한다.
public class Packet
{
    public IPeer owner { get; private set; }
    public byte[] buffer { get; private set; }
    public int position { get; private set; }

    Int16 Protocol_id;

    public static Packet Create( Int16 protocol_id )
    {
        Packet packet = PacketBufferManager.Pop();
        packet.SetProtocol( protocol_id );
        return packet;
    }

    public static void Destroy( Packet packet )
    {
        PacketBufferManager.Push( packet );
    }

    public Packet( byte[] buffer )
    {
        this.buffer = buffer;
        this.position = Defines.HEADERSIZE;
    }

    public Packet( byte[] buffer, IPeer owner )
    {
        // 참조로만 보관하여 작업합니다.
        // 복사가 필요하면 별도로 구현해야 합니다.
        this.buffer = buffer;

        // 헤더는 읽을 필요 없으니 그 이후부터 시작합니다.
        this.position = Defines.HEADERSIZE;
        this.owner = owner;
    }

    public Packet()
    {
        this.buffer = new byte[1024];
    }

    public Int16 PopProtocolID()
    {
        return PopInt16();
    }

    public Int16 PopInt16()
    {
        Int16 data = BitConverter.ToInt16( this.buffer, this.position );
        this.position += sizeof( Int16 );
        return data;
    }

    public Int32 PopInt32()
    {
        Int32 data = BitConverter.ToInt32( this.buffer, this.position );
        this.position += sizeof( Int32 );
        return data;
    }

    public string PopString()
    {
        // 문자열 길이는 최대 2바이트까지. 0 ~ 32767
        Int16 len = BitConverter.ToInt16( this.buffer, this.position );
        this.position += sizeof( Int16 );

        // 인코딩은 UTF8로 통일 합니다.
        string data = System.Text.Encoding.UTF8.GetString( this.buffer, this.position, len );
        this.position += len;

        return data;
    }

    public void SetProtocol( Int16 protocol_id )
    {
        this.Protocol_id = protocol_id;

        // 헤더는 나중에 넣을 것 이므로 데이터부터 넣을 수 있도록 위치를 점프 시켜놓는다.
        this.position = Defines.HEADERSIZE;
        Push( protocol_id );
    }

    public void RecordSize()
    {
        Int16 BodySize = ( Int16 )( this.position - Defines.HEADERSIZE );
        byte[] header = BitConverter.GetBytes( BodySize );
        header.CopyTo( this.buffer, 0 );
    }

    public void Push( Int16 data )
    {
        byte[] tempBuffer = BitConverter.GetBytes( data );
        tempBuffer.CopyTo( this.buffer, this.position );
        this.position += tempBuffer.Length;
    }
    
    public void Push( Int32 data )
    {
        byte[] tempBuffer = BitConverter.GetBytes( data );
        tempBuffer.CopyTo( this.buffer, this.position );
        this.position += tempBuffer.Length;
    }

    public void Push( string data )
    {
        byte[] tempBuffer = System.Text.Encoding.UTF8.GetBytes( data );
        Int16 len = ( Int16 )tempBuffer.Length;
        byte[] lenBuffer = BitConverter.GetBytes( len );
        lenBuffer.CopyTo( this.buffer, this.position );
        this.position += sizeof( Int16 );

        tempBuffer.CopyTo( this.buffer, this.position );
        this.position += tempBuffer.Length;
    }
}
