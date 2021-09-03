using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;



class PacketHandler
{
    public static void C_LeaveGameHandler(PacketSession session, IPacket packet)
    {
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
        {
            return;
        }

        //참조를 망가트리지 않기 위해서 임시 변수를 선언해준다.
        GameRoom room = clientSession.Room;
        room.Push(
            () => room.Leave(clientSession));
    }

    public static void C_MoveHandler(PacketSession session, IPacket packet)
    {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;
        
        if (clientSession.Room == null)
        {
            return;
        }

        Console.WriteLine($"{movePacket.PosX} {movePacket.PosY} {movePacket.PosZ}");

        //참조를 망가트리지 않기 위해서 임시 변수를 선언해준다.
        GameRoom room = clientSession.Room;
        room.Push(
            () => room.Move(clientSession, movePacket));
    }
}