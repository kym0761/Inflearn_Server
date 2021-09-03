using System;
using System.Collections.Generic;
using System.Text;

namespace DummyClient
{
    class SessionManager
    {
        static SessionManager _Session = new SessionManager();
        public static SessionManager Instance { get { return _Session; } }

        List<ServerSession> _Sessions = new List<ServerSession>();
        object _Lock = new object();
        Random _Rand = new Random();
        public ServerSession Generate()
        {
            lock (_Lock)
            {
                ServerSession session = new ServerSession();
                _Sessions.Add(session);


                return session;
            }
        }

        public void SendForEach()
        {
            lock (_Lock)
            {
                foreach (ServerSession s in _Sessions)
                {
                    C_Move packet = new C_Move();
                    packet.PosX = _Rand.Next(-50, 50);
                    packet.PosY = 0;
                    packet.PosZ = _Rand.Next(-50, 50);

                    s.Send(packet.Write());

                }
            }
        }

    }
}
