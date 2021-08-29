using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
 
        JobQueue _JobQueue = new JobQueue();

        List<ArraySegment<byte>> _PendingList = new List<ArraySegment<byte>>();

        //누군가가 들어오거나, 나가는 중에 이 행동을 하면 문제가 생김. 그러므로 lock이 필요함.
        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
        }

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.PlayerID = session.SessionID;
            packet.Chat = chat + $" I am {packet.PlayerID}";
            ArraySegment<byte> segment = packet.Write();


            _PendingList.Add(segment);

            ////모든 클라이언트 세션에 대해 메시지 보냄. n^2
            //foreach (ClientSession s in _sessions)
            //{
            //    s.Send(segment);
            //}
        }

        public void Push(Action job)
        {
            _JobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (ClientSession s in _sessions)
            {
                s.Send(_PendingList);
            }

            Console.WriteLine($"flushed Item number : {_PendingList.Count}");

            _PendingList.Clear();

        }

        //public Action Pop()
        //{
        //    return null;
        //}
    }
}
