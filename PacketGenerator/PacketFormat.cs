using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
	class PacketFormat
	{
		//{0} Packet 등록
		public static string ManagerFormat =
			@"using ServerCore;
using System;
using System.Collections.Generic;

public class PacketManager
{{
    #region singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance {{ get {{ return _instance; }} }}
    #endregion

    PacketManager() // 생성자에서 Register 실행함.
    {{
        Register();
    }}

    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _MakeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _Handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {{
		{0}
    }}

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession,IPacket> onRecvCallback = null)
    {{
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
        if (_MakeFunc.TryGetValue(id, out func))
        {{
            IPacket packet = func.Invoke(session, buffer);

            if (onRecvCallback != null)
            {{
                onRecvCallback.Invoke(session, packet);
            }}
            else 
            {{
                HandlePacket(session, packet);
            }}
        }}

    }}

    public T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {{
        T packet = new T();
        packet.Read(buffer);

        return packet;
    }}


    public void HandlePacket(PacketSession session, IPacket packet)
    {{
        Action<PacketSession, IPacket> action = null;
        if (_Handler.TryGetValue(packet.Protocol, out action))
        {{
            action.Invoke(session, packet);
        }}
    }}

}}
";
		//{0} 패킷 이름
		public static string ManagerRegisterFormat =
			@"	_MakeFunc.Add((ushort)PacketID.{0}, MakePacket<{0}>);
	_Handler.Add((ushort)PacketID.{0},PacketHandler.{0}Handler);";



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

public interface IPacket
{{
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
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
public class {0} : IPacket
{{
	{1}

    public ushort Protocol {{ get {{return (ushort)PacketID.{0}; }} }}

	public ArraySegment<byte> Write()
    {{
		ArraySegment<byte> segment = SendBufferHelper.Open(4096);
		ushort count = 0;

		count += sizeof(ushort);
		Array.Copy(BitConverter.GetBytes((ushort)PacketID.{0}), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		{3}
		Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

		return SendBufferHelper.Close(count);
	}}

	public void Read(ArraySegment<byte> segment)
	{{
		ushort count = 0;
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
{0} = BitConverter.{1}(segment.Array, segment.Offset + count);
count += sizeof({2});";

		//{0} 변수이름
		public static string ReadStringFormat =
@"
ushort {0}Length = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
count += sizeof(ushort);
{0} = Encoding.Unicode.GetString(segment.Array, segment.Offset + count, {0}Length);
count += {0}Length;
";
		//{0} 변수이름
		//{1} 변수형식
		public static string WriteFormat =
			@"Array.Copy(BitConverter.GetBytes({0}), 0, segment.Array, segment.Offset + count, sizeof({1}));
count += sizeof({1});";

		//{0} 변수이름
		public static string WriteStringFormat =
			@"
ushort {0}Length = (ushort)Encoding.Unicode.GetBytes({0}, 0, {0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));
Array.Copy(BitConverter.GetBytes({0}Length), 0, segment.Array, segment.Offset + count, sizeof(ushort));
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

	public bool Write(ArraySegment<byte> segment, ref ushort count)
	{{

		bool success = true;

		{4}

		return true;

	}}

	public void Read(ArraySegment<byte> segment, ref ushort count)
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
ushort {1}Length = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
count += sizeof(ushort);"
;

//for (int i = 0; i < {1}Length; i++)
//{{
//	{0} skill = new {0}();
//	{1}.Read(s,ref count);
//	{1}s.Add({1});
//}}";

		// {0} 리스트 이름 대문자
		// {1} 리스트 이름 소문자
		public static string WriteListFormat = @"
Array.Copy(BitConverter.GetBytes((ushort){1}s.Count), 0, segment.Array, segment.Offset + count, sizeof(ushort));
count += sizeof(ushort);
foreach ({0} {1} in {1}s)
{{
	{1}.Write(segment, ref count);
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
