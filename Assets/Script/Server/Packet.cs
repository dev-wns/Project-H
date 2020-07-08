using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Packet
{
    public int type { get; set; }
    public byte[] data { get; set; }

    public Packet() {  }

    public void SetData( byte[] _data, int _length )
    {
        data = new byte[_length];
        // �����ҵ�����, ����޴µ�����, ����
        Array.Copy( _data, data, _length );
    }

    public byte[] GetSendBytes()
    {
        // BitConverter.GetBytes ������ ���� ����Ʈ �迭�� ��ȯ����
        byte[] typeBytes = BitConverter.GetBytes( type );
        int headerSize = ( int )( data.Length );
        byte[] headerBytes = BitConverter.GetBytes( headerSize );
        byte[] sendBytes = new byte[headerBytes.Length + typeBytes.Length + data.Length];

        // ������ ������, �����ε���, �޴µ�����, �����ε���, ����
        Array.Copy( headerBytes, 0, sendBytes, 0, headerBytes.Length );
        Array.Copy( typeBytes, 0, sendBytes, headerBytes.Length, typeBytes.Length );
        Array.Copy( data, 0, sendBytes, headerBytes.Length + typeBytes.Length, data.Length );

        return sendBytes;
    }
}

// ��ü�� �����ϰų� �޸�, �����ͺ��̽� ���� ������ ������ ��
// ����Ʈ ��Ʈ������ ��ȯ���ִ� ���μ���.
// �ʿ��� �� �ٽ� ��ü�� ���� �� �ֵ��� ���¸� ����.
// �� Ŭ���� ���ο��� ����ȭ ������ Ÿ�Ը� ���� �ؾ���.
// [Serializable]�� ���� ����ü�� ����Ÿ��( int, float ) ������.
[Serializable] // ����ȭ ������ ����ü
// Sequential �����͸� ������ �� ���������� ������
// Pack = 1 ����Ʈ ũ�⸦ 1Btyte�� ������ #pragma pack( push, 1)
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Data<Type> where Type : class  // where T : class ( Type�� Ŭ������ �ü����� )
{
    public Data() {  }

    // ����ȭ ( ��ü�� Byte�迭�� )
    public byte[] Serialize()
    {
        var size = Marshal.SizeOf( typeof( Type ) );
        var array = new byte[size];
        var ptr = Marshal.AllocHGlobal( size );
        Marshal.StructureToPtr( this, ptr, true );
        Marshal.Copy( ptr, array, 0, size );
        Marshal.FreeHGlobal( ptr );
        return array;
    }

    // �� ����ȭ ( Byte�迭�� ��ü�� )
    public static Type Deserialize( byte[] array )
    {
        int size = Marshal.SizeOf( typeof( Type ) );
        // ������ ����Ʈ �����ŭ ���������ʴ� �޸𸮿����� �Ҵ�
        // ��ȯ���� ���� �Ҵ�� �޸𸮿� ���� ������
        // GC���� �޸𸮿��� ��ü�� �̵���Ű�� ����� �˷���
        IntPtr ptr = Marshal.AllocHGlobal( size );
        Marshal.Copy( array, 0, ptr, size );
        Type obj = ( Type )Marshal.PtrToStructure( ptr, typeof( Type ) );
        Marshal.FreeHGlobal( ptr );
        return obj;
    }
}

