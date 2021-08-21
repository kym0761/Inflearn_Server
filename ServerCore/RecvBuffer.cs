using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
	public class RecvBuffer
	{
		// [rw][][][][]...
		// [r][][][][w][][][][][]
		// [][][][][rw][][]....
		// [rw][][][]
		ArraySegment<byte> _buffer;
		int _readPos; //r
		int _writePos; //w

		public RecvBuffer(int bufferSize)
		{
			_buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
		}

		public int DataSize { get { return _writePos - _readPos; } }
		public int FreeSize { get { return _buffer.Count - _writePos; } }

		//r~datasize
		public ArraySegment<byte> ReadSegment
		{
			get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
		}

		//w~freesize
		public ArraySegment<byte> WriteSegment
		{
			get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
		}

		//버퍼 사용완료된 부분 초기화.
		public void Clean()
		{
			int dataSize = DataSize;
			if (dataSize == 0) //r == w
			{
				// 남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
				_readPos = _writePos = 0;
			}
			else //[][][r][][][w]
			{
				// 남은 찌끄레기가 있으면 시작 위치로 복사
				Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
				_readPos = 0;
				_writePos = dataSize;
			}
		}

		public bool OnRead(int numOfBytes)
		{
			if (numOfBytes > DataSize) //error
				return false;

			_readPos += numOfBytes;
			return true;
		}

		public bool OnWrite(int numOfBytes)
		{
			if (numOfBytes > FreeSize) //error
				return false;

			_writePos += numOfBytes;
			return true;
		}
	}
}
