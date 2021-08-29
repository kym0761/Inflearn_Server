using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerCore;

public enum PacketID
{
	
C_Chat = 1,

	
S_Chat = 2,

	
}

interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}


class C_Chat : IPacket
{
	public string Chat;

    public ushort Protocol { get {return (ushort)PacketID.C_Chat; } }

	public ArraySegment<byte> Write()
    {
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;

		count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes((ushort)PacketID.C_Chat), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		
		ushort ChatLength = (ushort)Encoding.Unicode.GetBytes(Chat, 0, Chat.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		Array.Copy(BitConverter.GetBytes(ChatLength), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		count += ChatLength;
		
		Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

		return SendBufferHelper.Close(count);
	}

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;
		count += sizeof(ushort);
		count += sizeof(ushort);
		
		ushort ChatLength = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
		count += sizeof(ushort);
		Chat = Encoding.Unicode.GetString(segment.Array, segment.Offset + count, ChatLength);
		count += ChatLength;
		
	}
}

class S_Chat : IPacket
{
	public int PlayerID;
	public string Chat;

    public ushort Protocol { get {return (ushort)PacketID.S_Chat; } }

	public ArraySegment<byte> Write()
    {
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;

		count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_Chat), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes(PlayerID), 0, segment.Array, segment.Offset + count, sizeof(int));
		count += sizeof(int);
		
		ushort ChatLength = (ushort)Encoding.Unicode.GetBytes(Chat, 0, Chat.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		Array.Copy(BitConverter.GetBytes(ChatLength), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		count += ChatLength;
		
		Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

		return SendBufferHelper.Close(count);
	}

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;
		count += sizeof(ushort);
		count += sizeof(ushort);
		
		PlayerID = BitConverter.ToInt32(segment.Array, segment.Offset + count);
		count += sizeof(int);
		
		ushort ChatLength = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
		count += sizeof(ushort);
		Chat = Encoding.Unicode.GetString(segment.Array, segment.Offset + count, ChatLength);
		count += ChatLength;
		
	}
}

