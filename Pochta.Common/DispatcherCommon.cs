#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using NLog;
#endregion


namespace Pochta.Common
{
    public abstract class DispatcherCommon
    {
        static readonly Logger _logger = LogManager.GetLogger(nameof(DispatcherCommon));
        protected readonly ManualResetEvent StopEvent = new ManualResetEvent(false);

        readonly List<Thread> _workingThreads = new List<Thread>();
        readonly List<Thread> _oneusingThreads = new List<Thread>();


        public abstract void Start();


        #region add thread
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Имя новой нити</param>
        /// <param name="action">выйти должны только фатальные исключения. Остальные должны внутри проглотиться.</param>
        /// <param name="beforeFirstSleepSeconds">Сколько времени спать перед запуском</param>
        /// <param name="priority"></param>
        protected void AddAndStartOneusingThread(string name, Action<ManualResetEvent> action,
            int beforeFirstSleepSeconds, ThreadPriority priority = ThreadPriority.Normal)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (beforeFirstSleepSeconds < 0)
                throw new ArgumentException($"{nameof(beforeFirstSleepSeconds)}: {beforeFirstSleepSeconds}");

            _logger.Debug("AddAndStartOneusingThread {0}:{1} ({2}) sleepSeconds=0 beforeFirstSleepSeconds={3}",
                GetType().Name, name, action.Method.Name, beforeFirstSleepSeconds);
            var t = new Thread(Dispatch) { Name = name, Priority = priority };
            t.Start(new object[]
                {
                    action,
                    0,
                    beforeFirstSleepSeconds
                });
            _oneusingThreads.Add(t);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Имя новой нити</param>
        /// <param name="action">выйти должны только фатальные исключения. Остальные должны внутри проглотиться.</param>
        /// <param name="sleepSeconds">Сколько времени спать перед каждым следующим запуском</param>
        /// <param name="beforeFirstSleepSeconds">Сколько времени спать перед первым запуском</param>
        /// <param name="priority"></param>
        protected void AddAndStartUsualThread(string name, Action<ManualResetEvent> action, int sleepSeconds,
            int beforeFirstSleepSeconds = 0, ThreadPriority priority = ThreadPriority.Normal)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (sleepSeconds < 1)
                throw new ArgumentException($"{nameof(sleepSeconds)}: {sleepSeconds} (must be >= 1)");
            if (beforeFirstSleepSeconds < 0)
                throw new ArgumentException($"{nameof(beforeFirstSleepSeconds)}: {beforeFirstSleepSeconds}");

            _logger.Debug("AddAndStartUsualThread {0}:{1} ({2}) sleepSeconds={3} beforeFirstSleepSeconds={4}",
                GetType().Name, name, action.Method.Name, sleepSeconds, beforeFirstSleepSeconds);
            var t = new Thread(Dispatch) { Name = name, Priority = priority };
            t.Start(new object[]
                {
                    action,
                    sleepSeconds,
                    null,
                    beforeFirstSleepSeconds
                });
            _workingThreads.Add(t);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Имя новой нити</param>
        /// <param name="action">выйти должны только фатальные исключения. Остальные должны внутри проглотиться.</param>
        /// <param name="schedCron">Расписание запуска в формате cron</param>
        /// <param name="priority"></param>
        protected void AddAndStartUsualThread(string name, Action<ManualResetEvent> action, string schedCron,
            ThreadPriority priority = ThreadPriority.Normal)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (string.IsNullOrWhiteSpace(schedCron)) throw new ArgumentNullException(nameof(schedCron));

            _logger.Debug("AddAndStartUsualThread {0}:{1} ({2}) schedCron={3}",
                GetType().Name, name, action.Method.Name, schedCron);
            try
            {
                int? beforeFirstSleepSeconds = CalcSleepSeconds(schedCron);
                if (beforeFirstSleepSeconds == null)
                {
                    // do not start
                    _logger.Error("can not parse cron string {schedCron}", schedCron);
                    return;
                }

                var t = new Thread(Dispatch) { Name = name, Priority = priority };
                t.Start(new object[]
                    {
                        action,
                        0,
                        schedCron,
                        beforeFirstSleepSeconds
                    });
                _workingThreads.Add(t);
            }
            catch (CronFormatException ex)
            {
                _logger.Error(ex, "can not parse cron string {schedCron}", schedCron);
            }
        }
        #endregion


        // ==null on first exec
        DateTime? _currentOccurence;


        /// <exception cref="CronFormatException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        int? CalcSleepSeconds(string schedCron)
        {
            CronExpression expression = CronExpression.Parse(schedCron);
            DateTimeOffset? nextOccurrence =
                expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local, _currentOccurence == null);
            if (nextOccurrence == null)
                return null;
            _currentOccurence = nextOccurrence.Value.DateTime;

            int sleepSeconds = (int)(_currentOccurence.Value - DateTime.Now).TotalSeconds + 1;
            // +1 because of rounding - float part is just discarded and the thread wakes up before the time
            return sleepSeconds;
        }


        /// <summary>
        ///     Is executed in two separate theads. Parameter: (action, sleepSeconds, schedCron, beforeFirstSleepSeconds)
        /// </summary>
        void Dispatch(object o)
        {
            try
            {
                _logger.Trace("Dispatch");

                var action = (Action<ManualResetEvent>)((object[])o)[0];
                var sleepSeconds = (int)((object[])o)[1];
                var schedCron = (string)((object[])o)[2];
                var beforeFirstSleepSeconds = (int)((object[])o)[3];

                int sleepSecondsCurrent;

                _logger.Debug("{0} sleepSeconds {1}, beforeFirstSleepSeconds {2}", GetType().Name, sleepSeconds,
                    beforeFirstSleepSeconds);

                if (!StopEvent.WaitOne(beforeFirstSleepSeconds * 1000))
                    do
                    {
                        _logger.Trace("{0} to exec {1} ...", GetType().Name, action.Method.Name);
                        if (StopEvent.WaitOne(0))
                        {
                            _logger.Warn("{0} Received stop event, breaking", GetType().Name);
                            break;
                        }

                        // выйти должны только фатальные исключения. Остальные должны внутри проглотиться.
                        action(StopEvent);

                        if (sleepSeconds == 0 && schedCron == null)
                        {
                            _logger.Warn("{0} {1} ended, because of sleepSeconds==0", GetType().Name,
                                action.Method.Name);
                            return;
                        }

                        sleepSecondsCurrent = sleepSeconds;
                        if (schedCron != null)
                        {
                            int? addonSleepSeconds = CalcSleepSeconds(schedCron);
                            if (addonSleepSeconds == null)
                            {
                                // do not start
                                _logger.Error("{type} can not parse cron string {schedCron}", GetType().Name,
                                    schedCron);
                                return;
                            }
                            sleepSecondsCurrent += addonSleepSeconds.Value;
                        }
                        if (sleepSecondsCurrent < 1) sleepSecondsCurrent = 1;

                        _logger.Trace("{0} ({1}) ended, sleeping for {2} seconds", action.Method.Name, GetType().Name,
                            sleepSecondsCurrent);
                    } while (!StopEvent.WaitOne(sleepSecondsCurrent * 1000));

                _logger.Debug("end {0} by stop event", Thread.CurrentThread.Name);
            }
            catch (ThreadAbortException ex)
            {
                _logger.Warn(ex, "ThreadAbort received");
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex);
            }
        }


        /// <summary>
        ///     Is executed in another thread
        /// </summary>
        public void Stop()
        {
            _logger.Warn("{0} Stop service", GetType().Name);
            StopEvent.Set();

            foreach (Thread t in _oneusingThreads)
                StopThread(t, 10000);
            foreach (Thread t in _workingThreads)
                StopThread(t, 10000);

            _logger.Info("{0} all thread was ended or aborted", GetType().Name);
        }


        protected void StopThread(Thread t, int msToJoin)
        {
            if (t != null && t.ThreadState != ThreadState.Unstarted && !t.Join(msToJoin))
            {
                _logger.Warn("{0} Aborting [{1}] thread", GetType().Name, t.Name);
                t.Abort();
            }
        }
    }
}