using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        object _Lock = new object();
       
        //누군가가 들어오거나, 나가는 중에 이 행동을 하면 문제가 생김. 그러므로 lock이 필요함.
        public void Enter(ClientSession session)
        {
            lock (_Lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock (_Lock)
            {
                _sessions.Remove(session);
            }
        }

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.PlayerID = session.SessionID;
            packet.Chat = chat + $" I am {packet.PlayerID}";
            ArraySegment<byte> segment = packet.Write();

            //모든 클라이언트 세션에 대해 메시지 보냄.
            lock (_Lock)
            {
                foreach (ClientSession s in _sessions)
                {
                    s.Send(segment);
                }
            }


        }


    }
}
