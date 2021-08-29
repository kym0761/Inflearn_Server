using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;



class PacketHandler
{
    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat p = packet as C_Chat;

        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
        {
            return;
        }

        //참조를 망가트리지 않기 위해서 임시 변수를 선언해준다.
        GameRoom room = clientSession.Room;
        room.Push(
            () => room.Broadcast(clientSession, p.Chat));

        //clientSession.Room.Broadcast(clientSession,p.Chat);
    }
}