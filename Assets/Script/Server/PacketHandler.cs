using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
public struct TestStructData
{
    public PacketType testEnum;

    public long datalong;
    public float datafloat;
    public bool databool;

    [MarshalAs( UnmanagedType.ByValTStr, SizeConst = 20 )]
    public string name;

    public TestStructData(long l, string _name, float f, bool b, PacketType type )
    {
        datalong = l;
        name = _name;
        datafloat = f;
        databool = b;
        testEnum = type;
    }

}

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public  class TestPacketReq : Data<TestPacketReq>
{
    public long testLongData;
    public TestStructData testData;

    [MarshalAs( UnmanagedType.ByValArray, SizeConst = 10 )]
    public TestStructData[] testDataArray = new TestStructData[10];

    public TestPacketReq() { }
}

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
public class TestPacketRes : Data<TestPacketRes>
{
    public bool isSuccess;
    public int testIntValue;

    [MarshalAs( UnmanagedType.ByValTStr, SizeConst = Protocol.maxPacketStringLength )]
    public string message;

    public TestPacketRes() { }
}

public class PacketHandler
{
    Network network;

    public void Init( Network net)
    {
        network = net;    
    }

    public void ParsePacket( Packet packet )
    {
        switch( ( PacketType )packet.type )
        {
            case PacketType.chatMessage:
                {
                    TestPacketRes( packet );
                } break;
        }
    }

    public void TestPacketRes( Packet packet )
    {
        // 역직렬화해서 원래 데이터로 만듬
        TestPacketRes notify = Data<TestPacketRes>.Deserialize( packet.data );
    }
}
