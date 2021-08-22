using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerCore;

namespace DummyClient
{

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
