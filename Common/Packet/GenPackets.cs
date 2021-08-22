using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerCore;

public enum PacketID
{
	
PlayerInfoReq = 1,

	
}


class PlayerInfoReq
{
	public long PlayerID;
	public string Name;
	
	public struct Skill
	{
		public int ID;
		public short Level;
		public float Duration;
	
		public bool Write(Span<byte> s, ref ushort count)
		{
	
			bool success = true;
	
			
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), ID);
			count += sizeof(int);
			
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), Level);
			count += sizeof(short);
			
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), Duration);
			count += sizeof(float);
	
			return true;
	
		}
	
		public void Read(ReadOnlySpan<byte> s, ref ushort count)
		{
	
			
			ID = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			
			Level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
			count += sizeof(short);
			
			Duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
	
		}
	
	}
	
	public List<Skill> skills = new List<Skill>();
	

	public ArraySegment<byte> Write()
    {
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;
		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
		count += sizeof(ushort);
		
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), PlayerID);
		count += sizeof(long);
		
		ushort NameLength = (ushort)Encoding.Unicode.GetBytes(Name, 0, Name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), NameLength);
		count += sizeof(ushort);
		count += NameLength;
		
		
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
		count += sizeof(ushort);
		foreach (Skill skill in skills)
		{
			success &= skill.Write(s, ref count);
		}
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
		
		PlayerID = BitConverter.ToInt64(s.Slice(count, s.Length - count));
		count += sizeof(long);
		
		ushort NameLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		Name = Encoding.Unicode.GetString(s.Slice(count, NameLength));
		count += NameLength;
		
		
		skills.Clear();
		ushort skillLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		
		
		for (int i = 0; i < skillLength; i++)
		{
			Skill skill = new Skill();
			skill.Read(s,ref count);
			skills.Add(skill);
		}
	}
}

