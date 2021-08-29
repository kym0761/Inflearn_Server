using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServerCore
{
	public class SendBufferHelper
	{
		//멀티쓰레드 환경이므로, 쓰레드들마다 sendbuffer는 threadlocal로 만듬.
		public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

		public static int ChunkSize { get; set; } = 65535 * 100;

		public static ArraySegment<byte> Open(int reserveSize)
		{
			//한번도 사용안했으니 일단 만듬.
			if (CurrentBuffer.Value == null)
				CurrentBuffer.Value = new SendBuffer(ChunkSize);

			//크기가 부족하면, 새로운 크기로 변경.
			if (CurrentBuffer.Value.FreeSize < reserveSize)
				CurrentBuffer.Value = new SendBuffer(ChunkSize);

			return CurrentBuffer.Value.Open(reserveSize);
		}

		public static ArraySegment<byte> Close(int usedSize)
		{
			return CurrentBuffer.Value.Close(usedSize);
		}
	}

	//sendbuffer를 Session에 멤버 변수로 쓰지 않는 이유는
	//유저가 100명 있으면, 100명분을 미리 준비해주고 사용해야함.
	//세션마다 100개가 있으면, 유저가 100명일때 세션이 100개 -> 총 1만개 필요
	//sendbuffer를 따로 내부에서 선언해 사용하고, 연결된 유저에 대해 데이터를 보내기를 하면됨
	//필요한 만큼만 사용하면 session안에 존재하는 멤버변수일 때보단 성능이 나음.
	//sendbuffer는 다 보내기 전까지는 계속 사용할 예정이기 때문에 recvbuffer때 있던 clean은 없음.
	//애초에 1회용으로 쓸 버퍼 클래스임.
	public class SendBuffer
	{
		// [u][][][][][][][][][]
		// [][][][][][][][][u][]
		byte[] _buffer;
		int _usedSize = 0;

		public int FreeSize { get { return _buffer.Length - _usedSize; } }

		public SendBuffer(int chunkSize)
		{
			_buffer = new byte[chunkSize];
		}

		//일단 크기를 확정하기 전에 선언하는 역할.
		public ArraySegment<byte> Open(int reserveSize)
		{
			if (reserveSize > FreeSize)
				return null;

			return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
		}

		//사용할 크기를 확정하는 역할.
		public ArraySegment<byte> Close(int usedSize)
		{
			ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
			_usedSize += usedSize;
			return segment;
		}
	}
}
