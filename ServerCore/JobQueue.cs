using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action job);
        //Action Pop();
    }

    public class JobQueue : IJobQueue
    {
        Queue<Action> _JobQueue = new Queue<Action>();
        object _Lock = new object();
        bool _Flush = false;
        public void Push(Action job)
        {
            bool flush = false;

            lock (_Lock)
            {
                _JobQueue.Enqueue(job);

                if (_Flush == false)
                {
                    flush = _Flush = true;
                }

            }

            if (flush)
            {
                Flush();
            }

        }

        void Flush()
        {
            while (true)
            {

                Action action = Pop();

                if (action == null)
                {
                    return;
                }

                action.Invoke();
            }
        }

        public Action Pop()
        {
            lock (_Lock)
            {
                if (_JobQueue.Count == 0)
                {
                    _Flush = false;
                    return null;
                }

                return _JobQueue.Dequeue();
            }
        }

    }
}
