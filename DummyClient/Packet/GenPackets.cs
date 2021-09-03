using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerCore;

public enum PacketID
{
	
S_BroadcastEnterGame = 1,

	
C_LeaveGame = 2,

	
S_BroadcastLeaveGame = 3,

	
S_PlayerList = 4,

	
C_Move = 5,

	
S_BroadcastMove = 6,

	
}

public interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}


public class S_BroadcastEnterGame : IPacket
{
	public int PlayerID;
	public float PosX;
	public float PosY;
	public float PosZ;

    public ushort Protocol { get {return (ushort)PacketID.S_BroadcastEnterGame; } }

	public ArraySegment<byte> Write()
    {
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;

		count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_BroadcastEnterGame), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes(PlayerID), 0, segment.Array, segment.Offset + count, sizeof(int));
		count += sizeof(int);
		Array.Copy(BitConverter.GetBytes(PosX), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(PosY), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(PosZ), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
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
		
		PosX = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		count += sizeof(float);
		
		PosY = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		count += sizeof(float);
		
		PosZ = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		count += sizeof(float);
	}
}

public class C_LeaveGame : IPacket
{
	

    public ushort Protocol { get {return (ushort)PacketID.C_LeaveGame; } }

	public ArraySegment<byte> Write()
    {
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;

		count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes((ushort)PacketID.C_LeaveGame), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		
		Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

		return SendBufferHelper.Close(count);
	}

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;
		count += sizeof(ushort);
		count += sizeof(ushort);
		
	}
}

public class S_BroadcastLeaveGame : IPacket
{
	public int PlayerID;

    public ushort Protocol { get {return (ushort)PacketID.S_BroadcastLeaveGame; } }

	public ArraySegment<byte> Write()
    {
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;

		count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_BroadcastLeaveGame), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes(PlayerID), 0, segment.Array, segment.Offset + count, sizeof(int));
		count += sizeof(int);
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
	}
}

public class S_PlayerList : IPacket
{
	
	public struct Player
	{
		public bool IsSelf;
		public int PlayerID;
		public float PosX;
		public float PosY;
		public float PosZ;
	
		public bool Write(ArraySegment<byte> segment, ref ushort count)
		{
	
			bool success = true;
	
			Array.Copy(BitConverter.GetBytes(IsSelf), 0, segment.Array, segment.Offset + count, sizeof(bool));
			count += sizeof(bool);
			Array.Copy(BitConverter.GetBytes(PlayerID), 0, segment.Array, segment.Offset + count, sizeof(int));
			count += sizeof(int);
			Array.Copy(BitConverter.GetBytes(PosX), 0, segment.Array, segment.Offset + count, sizeof(float));
			count += sizeof(float);
			Array.Copy(BitConverter.GetBytes(PosY), 0, segment.Array, segment.Offset + count, sizeof(float));
			count += sizeof(float);
			Array.Copy(BitConverter.GetBytes(PosZ), 0, segment.Array, segment.Offset + count, sizeof(float));
			count += sizeof(float);
	
			return true;
	
		}
	
		public void Read(ArraySegment<byte> segment, ref ushort count)
		{
	
			
			IsSelf = BitConverter.ToBoolean(segment.Array, segment.Offset + count);
			count += sizeof(bool);
			
			PlayerID = BitConverter.ToInt32(segment.Array, segment.Offset + count);
			count += sizeof(int);
			
			PosX = BitConverter.ToSingle(segment.Array, segment.Offset + count);
			count += sizeof(float);
			
			PosY = BitConverter.ToSingle(segment.Array, segment.Offset + count);
			count += sizeof(float);
			
			PosZ = BitConverter.ToSingle(segment.Array, segment.Offset + count);
			count += sizeof(float);
	
		}
	
	}
	
	public List<Player> players = new List<Player>();
	

    public ushort Protocol { get {return (ushort)PacketID.S_PlayerList; } }

	public ArraySegment<byte> Write()
    {
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;

		count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_PlayerList), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		
		Array.Copy(BitConverter.GetBytes((ushort)players.Count), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		foreach (Player player in players)
		{
			player.Write(segment, ref count);
		}
		Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

		return SendBufferHelper.Close(count);
	}

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;
		count += sizeof(ushort);
		count += sizeof(ushort);
		
		players.Clear();
		ushort playerLength = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
		count += sizeof(ushort);
	}
}

public class C_Move : IPacket
{
	public float PosX;
	public float PosY;
	public float PosZ;

    public ushort Protocol { get {return (ushort)PacketID.C_Move; } }

	public ArraySegment<byte> Write()
    {
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;

		count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes((ushort)PacketID.C_Move), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes(PosX), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(PosY), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(PosZ), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

		return SendBufferHelper.Close(count);
	}

	public void Read(ArraySegment<byte> segment)
	{
		ushort count = 0;
		count += sizeof(ushort);
		count += sizeof(ushort);
		
		PosX = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		count += sizeof(float);
		
		PosY = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		count += sizeof(float);
		
		PosZ = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		count += sizeof(float);
	}
}

public class S_BroadcastMove : IPacket
{
	public int PlayerID;
	public float PosX;
	public float PosY;
	public float PosZ;

    public ushort Protocol { get {return (ushort)PacketID.S_BroadcastMove; } }

	public ArraySegment<byte> Write()
    {
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;

		count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes((ushort)PacketID.S_BroadcastMove), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes(PlayerID), 0, segment.Array, segment.Offset + count, sizeof(int));
		count += sizeof(int);
		Array.Copy(BitConverter.GetBytes(PosX), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(PosY), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(PosZ), 0, segment.Array, segment.Offset + count, sizeof(float));
		count += sizeof(float);
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
		
		PosX = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		count += sizeof(float);
		
		PosY = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		count += sizeof(float);
		
		PosZ = BitConverter.ToSingle(segment.Array, segment.Offset + count);
		count += sizeof(float);
	}
}

