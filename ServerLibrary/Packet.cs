using System;
using System.Collections.Generic;

// byte[] ���۸� ������ �����Ͽ� PopXXX�޼ҵ� ȣ�� ������� ������ ��ȯ�� �����Ѵ�.
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
        // �����θ� �����Ͽ� �۾��մϴ�.
        // ���簡 �ʿ��ϸ� ������ �����ؾ� �մϴ�.
        this.buffer = buffer;

        // ����� ���� �ʿ� ������ �� ���ĺ��� �����մϴ�.
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
        // ���ڿ� ���̴� �ִ� 2����Ʈ����. 0 ~ 32767
        Int16 len = BitConverter.ToInt16( this.buffer, this.position );
        this.position += sizeof( Int16 );

        // ���ڵ��� UTF8�� ���� �մϴ�.
        string data = System.Text.Encoding.UTF8.GetString( this.buffer, this.position, len );
        this.position += len;

        return data;
    }

    public void SetProtocol( Int16 protocol_id )
    {
        this.Protocol_id = protocol_id;

        // ����� ���߿� ���� �� �̹Ƿ� �����ͺ��� ���� �� �ֵ��� ��ġ�� ���� ���ѳ��´�.
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
