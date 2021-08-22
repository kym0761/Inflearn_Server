using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerCore;

namespace DummyClient
{
    public abstract class Packet
	{
		//ushort만으로도 64kb로 충분한 크기

		public ushort Size;
		public ushort PacketId;

		public abstract ArraySegment<byte> Write();
		public abstract void Read(ArraySegment<byte> s);
	}

	class PlayerInfoReq : Packet
	{
		public long PlayerId;
		public string Name;

		public struct SkillInfo
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

		public List<SkillInfo> Skills = new List<SkillInfo>();

		public PlayerInfoReq()
		{
			PacketId = (ushort)PacketID.PlayerInfoReq;
		}



		public override ArraySegment<byte> Write()
        {
			ArraySegment<byte> segment = SendBufferHelper.Open(4096);

			ushort count = 0;
			bool success = true;

			Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

			count += sizeof(ushort);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), PacketId);
			count += sizeof(ushort);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), PlayerId);
			
			count += sizeof(long);

			////string Name
			////[length] , [byte[]]
			//ushort nameLength = (ushort)Encoding.Unicode.GetByteCount(Name);
			//success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLength);
			//count += sizeof(ushort);
			
			//Array.Copy(Encoding.Unicode.GetBytes(Name), 0, segment.Array, count, nameLength);
			//count += nameLength;

			ushort nameLength = (ushort)Encoding.Unicode.GetBytes(Name, 0, Name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLength);
			count += sizeof(ushort);
			count += nameLength;


			//skill
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)Skills.Count);
			count += sizeof(ushort);
			foreach (SkillInfo skill in Skills)
			{
				success &= skill.Write(s, ref count);
			}


			success &= BitConverter.TryWriteBytes(s, count);


			if (success == false)
			{
				Console.WriteLine("Write Failed. so Write Segment will 0");
				return null;
			}

			ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);

			return sendBuff;
		}

		public override void Read(ArraySegment<byte> segment)
		{
			ushort count = 0;

			ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);


			//ushort size = BitConverter.ToUInt16(s.Array, s.Offset);
			count += sizeof(ushort);
			//ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count);
			count += sizeof(ushort);

			PlayerId =
			BitConverter.ToInt64(s.Slice(count, s.Length - count));
			count += sizeof(long);

			//string Name
			ushort nameLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
			count += sizeof(ushort);

			Name = Encoding.Unicode.GetString(s.Slice(count, nameLength));
			count += nameLength;

			//skill List
			Skills.Clear();
			ushort skillLength = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
			count += sizeof(ushort);


			for (int i = 0; i < skillLength; i++)
			{
				SkillInfo skill = new SkillInfo();
				skill.Read(s,ref count);
				Skills.Add(skill);
			}


		}
	}

	//class PlayerInfoOk : Packet
	//{
	//	public int hp;
	//	public int attack;
	//}

	public enum PacketID
	{
		PlayerInfoReq = 1,
		PlayerInfoOk = 2,
	}

	class ServerSession : Session
	{
		//unsafe는 포인터 쓰는 듯이 사용 가능.
		static unsafe void ToBytes(byte[] array, int offset, ulong value)
		{
			fixed (byte* ptr = &array[offset])
				*(ulong*)ptr = value;
		}

		static unsafe void ToBytes<T>(byte[] array, int offset, T value) where T : unmanaged
		{
			fixed (byte* ptr = &array[offset])
				*(T*)ptr = value;
		}

		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

			PlayerInfoReq packet = new PlayerInfoReq() {PlayerId = 1001, Name ="ABCDEF" };

			packet.Skills.Add(new PlayerInfoReq.SkillInfo() { ID = 101, Level = 1, Duration = 3.0f });
			packet.Skills.Add(new PlayerInfoReq.SkillInfo() { ID = 202, Level = 2, Duration = 5.0f });
			packet.Skills.Add(new PlayerInfoReq.SkillInfo() { ID = 203, Level = 3, Duration = 10.0f });


			// Send Test Code
			for (int i = 0; i < 5; i++)
			{
				ArraySegment<byte> s = packet.Write();

				if (s != null)
				{
					Send(s);
				}
			}

		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override int OnRecv(ArraySegment<byte> buffer)
		{
			string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
			Console.WriteLine($"[From Server] {recvData}");
			return buffer.Count;
		}

		public override void OnSend(int numOfBytes)
		{
			Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}

}
