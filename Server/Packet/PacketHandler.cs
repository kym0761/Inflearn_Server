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

        clientSession.Room.Broadcast(clientSession,p.Chat);

        //Console.WriteLine($"Player Info Req : {p.PlayerID}, Name : {p.Name} ");

        //foreach (C_PlayerInfoReq.Skill skill in p.skills)
        //{
        //    Console.WriteLine($"Skill : {skill.ID}, {skill.Level}, {skill.Duration}");
        //}

    }
}