using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class SessionManager
    {
        static SessionManager _Session = new SessionManager();
        public static SessionManager Instance { get { return _Session; } }

        int _SessionID = 0;
        Dictionary<int, ClientSession> _Sessions = new Dictionary<int, ClientSession>();
        object _Lock = new object();

        public ClientSession Generate()
        {
            lock (_Lock)
            {
                int sessionID = ++_SessionID;

                ClientSession session = new ClientSession();
                session.SessionID = sessionID;
                _Sessions.Add(sessionID, session);

                Console.WriteLine($"connected : {sessionID}");


                return session;
            }
        }

        public ClientSession Find(int ID)
        {
            lock (_Lock)
            {
                ClientSession session = null;
                _Sessions.TryGetValue(ID, out session);

                return session;
            }
        }

        public void Remove(ClientSession ToRemove)
        {
            lock (_Lock)
            {
                _Sessions.Remove(ToRemove.SessionID);
            }
        }

    }
}
