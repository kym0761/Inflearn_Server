using ServerCore;
using System;
using System.Collections.Generic;

public class PacketManager
{
    #region singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance { get { return _instance; } }
    #endregion

    PacketManager() // 생성자에서 Register 실행함.
    {
        Register();
    }

    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _MakeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _Handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {
			_MakeFunc.Add((ushort)PacketID.C_LeaveGame, MakePacket<C_LeaveGame>);
	_Handler.Add((ushort)PacketID.C_LeaveGame,PacketHandler.C_LeaveGameHandler);
	_MakeFunc.Add((ushort)PacketID.C_Move, MakePacket<C_Move>);
	_Handler.Add((ushort)PacketID.C_Move,PacketHandler.C_MoveHandler);

    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession,IPacket> onRecvCallback = null)
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;
        if (_MakeFunc.TryGetValue(id, out func))
        {
            IPacket packet = func.Invoke(session, buffer);

            if (onRecvCallback != null)
            {
                onRecvCallback.Invoke(session, packet);
            }
            else 
            {
                HandlePacket(session, packet);
            }
        }

    }

    public T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        T packet = new T();
        packet.Read(buffer);

        return packet;
    }


    public void HandlePacket(PacketSession session, IPacket packet)
    {
        Action<PacketSession, IPacket> action = null;
        if (_Handler.TryGetValue(packet.Protocol, out action))
        {
            action.Invoke(session, packet);
        }
    }

}
