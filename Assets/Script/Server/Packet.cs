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
        // 복사할데이터, 복사받는데이터, 길이
        Array.Copy( _data, data, _length );
    }

    public byte[] GetSendBytes()
    {
        // BitConverter.GetBytes 지정된 값을 바이트 배열로 변환해줌
        byte[] typeBytes = BitConverter.GetBytes( type );
        int headerSize = ( int )( data.Length );
        byte[] headerBytes = BitConverter.GetBytes( headerSize );
        byte[] sendBytes = new byte[headerBytes.Length + typeBytes.Length + data.Length];

        // 복사할 데이터, 시작인덱스, 받는데이터, 시작인덱스, 길이
        Array.Copy( headerBytes, 0, sendBytes, 0, headerBytes.Length );
        Array.Copy( typeBytes, 0, sendBytes, headerBytes.Length, typeBytes.Length );
        Array.Copy( data, 0, sendBytes, headerBytes.Length + typeBytes.Length, data.Length );

        return sendBytes;
    }
}

// 객체를 저장하거나 메모리, 데이터베이스 같은 곳으로 전송할 때
// 바이트 스트림으로 변환해주는 프로세스.
// 필요할 때 다시 객체로 만들 수 있도록 상태를 저장.
// 이 클래스 내부에는 직렬화 가능한 타입만 존재 해야함.
// [Serializable]이 붙은 구조체나 원시타입( int, float ) 같은거.
[Serializable] // 직렬화 가능한 구조체
// Sequential 데이터를 내보낼 때 순차적으로 내보냄
// Pack = 1 데이트 크기를 1Btyte로 맞춰줌 #pragma pack( push, 1)
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Data<Type> where Type : class  // where T : class ( Type에 클래스만 올수있음 )
{
    public Data() {  }

    // 직렬화 ( 객체를 Byte배열로 )
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

    // 역 직렬화 ( Byte배열을 객체로 )
    public static Type Deserialize( byte[] array )
    {
        int size = Marshal.SizeOf( typeof( Type ) );
        // 지정된 바이트 사이즈만큼 관리되지않는 메모리영역에 할당
        // 반환값은 새로 할당된 메모리에 대한 포인터
        // GC한테 메모리에서 객체를 이동시키지 말라고 알려줌
        IntPtr ptr = Marshal.AllocHGlobal( size );
        Marshal.Copy( array, 0, ptr, size );
        Type obj = ( Type )Marshal.PtrToStructure( ptr, typeof( Type ) );
        Marshal.FreeHGlobal( ptr );
        return obj;
    }
}

