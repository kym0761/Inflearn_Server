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
            //플레이어 추가
            _sessions.Add(session);
            session.Room = this;

            //들어온 플레이어에게 모든 플레이어 목록 전송

            S_PlayerList playerList = new S_PlayerList();
            foreach (ClientSession s in _sessions)
            {
                playerList.players.Add(new S_PlayerList.Player()
                {
                    IsSelf = (s == session),
                    PlayerID = s.SessionID,
                    PosX = s.PosX,
                    PosY = s.PosY,
                    PosZ = s.PosZ
                });
                    
            }

            session.Send(playerList.Write());


            //모든 플레이어에게 들어온 플레이어를 알림.
            S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
            enter.PlayerID = session.SessionID;
            enter.PosX = 0;
            enter.PosY = 0;
            enter.PosZ = 0;
            Broadcast(enter.Write());



        }

        public void Leave(ClientSession session)
        {
            //플레이어 제거
            _sessions.Remove(session);

            //모든 플레이어에게 제거된 플레이어를 알림
            S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();

            leave.PlayerID = session.SessionID;
            Broadcast(leave.Write());

        }

        public void Broadcast(ArraySegment<byte> segment)
        {
            _PendingList.Add(segment);
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

            //Console.WriteLine($"flushed Item number : {_PendingList.Count}");

            _PendingList.Clear();
        }

        public void Move(ClientSession session, C_Move packet)
        {
            //좌표를 바꾸고
            session.PosX = packet.PosX;
            session.PosY = packet.PosY;
            session.PosZ = packet.PosZ;

            //broadcast한다.
            S_BroadcastMove move = new S_BroadcastMove();
            move.PlayerID = session.SessionID;
            move.PosX = session.PosX;
            move.PosY = session.PosY;
            move.PosZ = session.PosZ;

            Broadcast(move.Write());

        }


    }
}
