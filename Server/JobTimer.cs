using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Server
{
    struct JobTimerElement : IComparable<JobTimerElement>
    {
        public int ExecTick;
        public Action _Action;


        public int CompareTo(JobTimerElement other)
        {
            return other.ExecTick - ExecTick;
        }
    }

    class JobTimer
    {
        PriorityQueue<JobTimerElement> _pq = new PriorityQueue<JobTimerElement>();

        object _Lock = new object();

        public static JobTimer Instance { get; } = new JobTimer();

        public void Push(Action action, int tickAfter = 0)
        {
            JobTimerElement job;

            job.ExecTick = System.Environment.TickCount + tickAfter;
            job._Action = action;

            lock (_Lock)
            {
                _pq.Push(job);
            }
        }

        public void Flush()
        {
            while (true)
            {
                int now = System.Environment.TickCount;

                JobTimerElement job;

                lock (_Lock)
                {

                    if (_pq.Count == 0)
                    {
                        break;
                    }

                    job = _pq.Peek();
                    if (job.ExecTick > now)
                    {
                        break;
                    }

                    _pq.Pop();
                }

                //실행
                job._Action.Invoke();
            }
        }

    }
}
