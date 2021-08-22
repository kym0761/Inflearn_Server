using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
	class PacketFormat
	{

		//{0} 패킷 이름 / 번호 목록
		//{1} 패킷 목록
		public static string FileFormat =
			@"using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerCore;

public enum PacketID
{{
	{0}
}}

{1}
";

		//{0} packetName
		//{1} packetNumber
		public static string PacketEnumFormat =
			@"
{0} = {1},
";

		//{0} packetName
		//{1} memberVariables
		//{2} 멤버 변수 Read
		//{3} 멤머 변수 write
        public static string _PacketFormat =
@"
class {0}
{{
	{1}

	public ArraySegment<byte> Write()
    {{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;
		bool success = true;
		Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.{0});
		count += sizeof(ushort);
		{3}
		success &= BitConverter.TryWriteBytes(s, count);
		if (success == false)
		{{
			return null;
		}}
		ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);
		return sendBuff;
	}}

	public void Read(ArraySegment<byte> segment)
	{{
		ushort count = 0;
		ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
		count += sizeof(ushort);
		count += sizeof(ushort);
		{2}
	}}
}}
";
		//{0} 변수 형식
		//{1} 변수 이름
		public static string MemberFormat =@"public {0} {1};";

		//{0} 변수 이름
		//{1} to 변수 형식
		//{2} 변수 형식
		public static string ReadFormat = @"
{0} = BitConverter.{1}(s.Slice(count, s.Length - count));
count += sizeof({2});";

		//{0} 변수이름
		public static string ReadStringFormat =
@"
ushort {0}Length = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);
{0} = Encoding.Unicode.GetString(s.Slice(count, {0}Length));
count += {0}Length;
";
		//{0} 변수이름
		//{1} 변수형식
		public static string WriteFormat =
			@"
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0});
count += sizeof({1});";

		//{0} 변수이름
		public static string WriteStringFormat =
			@"
ushort {0}Length = (ushort)Encoding.Unicode.GetBytes({0}, 0, {0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), {0}Length);
count += sizeof(ushort);
count += {0}Length;
";

		// {0} 리스트 이름 대문자
		// {1} 리스트 이름 소문자
		// {2} 멤버 변수들
		// {3} 멤버 변수 Read
		// {4} 멤버 변수 Write
		public static string MemberListFormat =
			@"
public struct {0}
{{
	{2}

	public bool Write(Span<byte> s, ref ushort count)
	{{

		bool success = true;

		{4}

		return true;

	}}

	public void Read(ReadOnlySpan<byte> s, ref ushort count)
	{{

		{3}

	}}

}}

public List<{0}> {1}s = new List<{0}>();
";

		// {0} 리스트 이름 대문자
		// {1} 리스트 이름 소문자
		public static string ReadListFormat = @"
{1}s.Clear();
ushort {1}Length = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
count += sizeof(ushort);


for (int i = 0; i < {1}Length; i++)
{{
	{0} skill = new {0}();
	{1}.Read(s,ref count);
	{1}s.Add({1});
}}";

		// {0} 리스트 이름 대문자
		// {1} 리스트 이름 소문자
		public static string WriteListFormat = @"
success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort){1}s.Count);
count += sizeof(ushort);
foreach ({0} {1} in {1}s)
{{
	success &= {1}.Write(s, ref count);
}}";

		//{0} 변수이름
		//{1} 변수 형식
		public static string ReadByteFormat =
			@"
		{0} = ({1})segment.Array[segment.Offset + count];
		count += sizeof({1});
";
		//{0} 변수이름
		//{1} 변수 형식
		public static string WriteByteFormat =
			@"
		segment.Array[segment.Offset + count] = (byte){0};
		count += sizeof({1});
";


	}
}
