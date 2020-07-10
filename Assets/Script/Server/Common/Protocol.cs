using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class Protocol
{
    private const ushort kbyte = 2;
    public const uint socketBufferSize = 1024 * 4;
    public const uint headSize = 4;

    public const uint completeMessageSizeClient = 7;
    public const int maxPacketStringLength = 1024 * kbyte;
}

[Serializable]
public enum PacketType
{
    None = -1,
    chatMessage = 1000,
    chatMessageAck = 2000, chatMessageReq,

    packetCount
}