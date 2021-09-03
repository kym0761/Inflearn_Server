using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;
using DummyClient;

class PacketHandler
{
    public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastEnterGame enterPacket = packet as S_BroadcastEnterGame;

        ServerSession serverSession = session as ServerSession;
    }
    public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastLeaveGame leavePacket = packet as S_BroadcastLeaveGame;

        ServerSession serverSession = session as ServerSession;
    }

    public static void S_PlayerListHandler(PacketSession session, IPacket packet)
    {
        S_PlayerList playerListPacket = packet as S_PlayerList;
        
        ServerSession serverSession = session as ServerSession;
    }

    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastMove movePacket = packet as S_BroadcastMove;

        ServerSession serverSession = session as ServerSession;
    }
}