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
		bool success = true;
		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_Chat);
		count += sizeof(ushort);
		
		ushort ChatLength = (ushort)Encoding.Unicode.GetBytes(Chat, 0, Chat.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), ChatLength);
		count += sizeof(ushort);
		count += ChatLength;
		
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
		{
			return null;
		}
		ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);
		return sendBuff;
	}

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;
		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		
		ushort ChatLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		Chat = Encoding.Unicode.GetString(s.Slice(count, ChatLength));
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
		bool success = true;
		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_Chat);
		count += sizeof(ushort);
		
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), PlayerID);
		count += sizeof(int);
		
		ushort ChatLength = (ushort)Encoding.Unicode.GetBytes(Chat, 0, Chat.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), ChatLength);
		count += sizeof(ushort);
		count += ChatLength;
		
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
		{
			return null;
		}
		ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);
		return sendBuff;
	}

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;
		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		
		PlayerID = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		
		ushort ChatLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		Chat = Encoding.Unicode.GetString(s.Slice(count, ChatLength));
		count += ChatLength;
		
	}
}

