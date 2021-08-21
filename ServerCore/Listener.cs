using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
	public class Listener
	{
		Socket _listenSocket;
		Func<Session> _sessionFactory;

		public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
		{
			_listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			_sessionFactory += sessionFactory;

			// 문지기 교육
			_listenSocket.Bind(endPoint);

			// 영업 시작
			// backlog : 최대 대기수
			_listenSocket.Listen(10);

			//init에서 1번 선언하고 계속 재사용할 args.
			SocketAsyncEventArgs args = new SocketAsyncEventArgs();
			args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
			//일단 처음에 딱 1번만 RegisterAccept를 호출함.
			RegisterAccept(args);
		}

		void RegisterAccept(SocketAsyncEventArgs args)
		{
			//아직 accept 안한 상태라, 기존의 것을 사용한다면, 연결이 존재할 시 초기화.
			args.AcceptSocket = null;

			//비동기 accept.
			bool pending = _listenSocket.AcceptAsync(args);
			if (pending == false)
				OnAcceptCompleted(null, args);
		}

		//메인의 while과 독립적으로 다른 스레드로 실행되는 함수로, 나중에 같은 자원을 접근하면 race condition이 일어날 수 있음.
		void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
		{
			//accept가 된다면 작동하는 함수.

			if (args.SocketError == SocketError.Success)
			{
				//세션의 종류에 상관없이 모든 세션에 대해서 처리할 수 있도록 Func<Session>을 사용해서 필요한 세션을 만듬.
				Session session = _sessionFactory.Invoke();
				session.Start(args.AcceptSocket);
				session.OnConnected(args.AcceptSocket.RemoteEndPoint);
			}
			else
			{
				Console.WriteLine(args.SocketError.ToString());
			}

			//다음에 접속할 것들을 위해서 다시 registerAccept로 돌아감.
			RegisterAccept(args);
		}
	}
}
