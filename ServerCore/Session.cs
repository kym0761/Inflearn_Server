using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
	public abstract class PacketSession : Session
	{
		public static readonly int HeaderSize = 2;

		// [size(2)][packetId(2)][옵션 ... ] , [size(2)][packetId(2)][옵션 ... ]
		public sealed override int OnRecv(ArraySegment<byte> buffer)
		{
			int processLen = 0;

			while (true)
			{
				// 최소한 헤더는 파싱할 수 있는지 확인
				if (buffer.Count < HeaderSize)
					break;

				// 패킷이 완전체로 도착했는지 확인
				ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
				if (buffer.Count < dataSize)
					break;

				// 여기까지 왔으면 패킷 조립 가능
				OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));
				
				processLen += dataSize;

				//버퍼 사용됐다면 위치 옮겨야할걸
				buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
			}

			return processLen;
		}

		public abstract void OnRecvPacket(ArraySegment<byte> buffer);
	}

	public abstract class Session
	{
		Socket _socket;
		int _disconnected = 0;

		RecvBuffer _recvBuffer = new RecvBuffer(1024);

		object _lock = new object();

		// ArraySegment를 사용하는 이유는, 배열을 쓸 때 포인터를 썼다면 쓰려는 포인트 주소 위치 넘기고 땡인데
		// C#에는 그게 없으므로, ArraySegment를 통해서 array, 시작위치, 사용할 길이 등을 넘겨서 사용해야함.
		Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
		List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
		
		SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
		SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

		public abstract int  OnRecv(ArraySegment<byte> buffer);
		public abstract void OnSend(int numOfBytes);
		public abstract void OnDisconnected(EndPoint endPoint);
		public abstract void OnConnected(EndPoint endPoint);
			   
		public void Start(Socket socket)
		{
			_socket = socket;

			_recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
			_sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

			RegisterRecv();
		}

		public void Send(ArraySegment<byte> sendBuff)
		{
			lock (_lock)
			{
				_sendQueue.Enqueue(sendBuff);
				if (_pendingList.Count == 0)
				{
					RegisterSend();
				}
			}
		}

		public void Disconnect()
		{
			if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            {
				return;
			}

			OnDisconnected(_socket.RemoteEndPoint);
			_socket.Shutdown(SocketShutdown.Both);
			_socket.Close();
		}

		#region 네트워크 통신

		void RegisterSend()
		{
			while (_sendQueue.Count > 0)
			{
				ArraySegment<byte> buff = _sendQueue.Dequeue();
				_pendingList.Add(buff);
			}
			_sendArgs.BufferList = _pendingList;

			bool pending = _socket.SendAsync(_sendArgs);
			if (pending == false)
			{
				OnSendCompleted(null, _sendArgs);
			}
		}

		void OnSendCompleted(object sender, SocketAsyncEventArgs args)
		{
			lock (_lock)
			{
				if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
				{
					try
					{
						//보내는게 완료 됐다면, SendArgs의 BufferList를 초기화
						//pendingList도 비움.
						_sendArgs.BufferList = null;
						_pendingList.Clear();

						OnSend(_sendArgs.BytesTransferred);

						//누군가 sendQueue에 메시지 추가했으면 다시 보낼 준비해야함.
						if (_sendQueue.Count > 0)
						{
							RegisterSend();
						}		
					}
					catch (Exception e)
					{
						Console.WriteLine($"OnSendCompleted Failed {e}");
					}
				}
				else
				{
					Disconnect();
				}
			}
		}

		void RegisterRecv()
		{
			//recvBuffer Setting.
			_recvBuffer.Clean();
			ArraySegment<byte> segment = _recvBuffer.WriteSegment;
			_recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

			bool pending = _socket.ReceiveAsync(_recvArgs);
			if (pending == false)
			{
				OnRecvCompleted(null, _recvArgs);
			}
		}

		void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
		{
			if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
			{
				try
				{
					// Write 커서 이동
					if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
					{
						Console.WriteLine("Write Cursor Failed");
						Disconnect();
						return;
					}

					// 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다
					int processLen = OnRecv(_recvBuffer.ReadSegment);

					if (processLen < 0 || _recvBuffer.DataSize < processLen)
					{
						Console.WriteLine("Data Processing Failed");
						Disconnect();
						return;
					}

					// Read 커서 이동
					if (_recvBuffer.OnRead(processLen) == false)
					{
						Console.WriteLine("Read Cursor Failed");
						Disconnect();
						return;
					}

					RegisterRecv();
				}
				catch (Exception e)
				{
					Console.WriteLine($"OnRecvCompleted Failed {e}");
				}
			}
			else
			{
				Console.WriteLine("OnRecvCompleted Failed");
				Console.WriteLine($"{args.SocketError}");
				Console.WriteLine($"{args.BytesTransferred}");
				Disconnect();
			}
		}

		#endregion
	}
}
