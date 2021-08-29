using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;

namespace Server
{

	class ClientSession : PacketSession
	{
		public int SessionID { get; set; }
		public GameRoom Room { get; set; }

		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");
			//Thread.Sleep(5000);
			//Disconnect();

			Program.Room.Push(() => Program.Room.Enter(this));

			//Program.Room.Enter(this);

		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		//// TEMP
		//public void Handle_PlayerInfoOk(ArraySegment<byte> buffer)
		//{

		//}

		public override void OnDisconnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnDisconnected : {endPoint}");
			
			SessionManager.Instance.Remove(this);
			if (Room != null)
			{
				//room이라는 변수로 참조가 망가지지 않게 도와주면 크래시가 나지 않을 것이다.
				GameRoom room = Room;
				room.Push(() => room.Leave(this));
				Room = null;
			}
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}
