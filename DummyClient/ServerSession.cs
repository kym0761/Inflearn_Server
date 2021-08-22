using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerCore;

namespace DummyClient
{

	class PlayerInfoReq
	{
		public byte testByte;
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

			segment.Array[segment.Offset + count] = (byte)testByte;
			count += sizeof(byte);


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

			testByte = (byte)segment.Array[segment.Offset + count];
			count += sizeof(byte);


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
				skill.Read(s, ref count);
				skills.Add(skill);
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

			PlayerInfoReq packet = new PlayerInfoReq() {PlayerID = 1001, Name ="ABCDEF" };

			packet.skills.Add(new PlayerInfoReq.Skill() { ID = 101, Level = 1, Duration = 3.0f });
			packet.skills.Add(new PlayerInfoReq.Skill() { ID = 202, Level = 2, Duration = 5.0f });
			packet.skills.Add(new PlayerInfoReq.Skill() { ID = 203, Level = 3, Duration = 10.0f });


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
