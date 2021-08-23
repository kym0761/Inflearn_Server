using ServerCore;
using System;
using System.Collections.Generic;

 class PacketManager
{
    #region singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance { get { return _instance; } }
    #endregion

    PacketManager() // 생성자에서 Register 실행함.
    {
        Register();
    }

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _OnRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _Handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {
				_OnRecv.Add((ushort)PacketID.C_Chat, MakePacket<C_Chat>);
			_Handler.Add((ushort)PacketID.C_Chat,PacketHandler.C_ChatHandler);


    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;

        if (_OnRecv.TryGetValue(id, out action))
        {
            action.Invoke(session, buffer);
        }

    }

    public void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        T packet = new T();
        packet.Read(buffer);

        Action<PacketSession, IPacket> action = null;
        if (_Handler.TryGetValue(packet.Protocol, out action))
        {
            action.Invoke(session, packet);
        }
    }
}
