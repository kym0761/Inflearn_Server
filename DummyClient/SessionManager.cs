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
                    C_Chat chatPacket = new C_Chat();
                    chatPacket.Chat = $"hello Server...!";
                    ArraySegment<byte> segment = chatPacket.Write();

                    s.Send(segment);

                }
            }
        }

    }
}
