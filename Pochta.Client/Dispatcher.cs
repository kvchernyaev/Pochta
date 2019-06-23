#region usings
using NLog;
using Pochta.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Pochta.Client
{
    class Dispatcher : DispatcherCommon
    {
        static readonly Logger Logger = LogManager.GetLogger(nameof(Dispatcher));
        /// <summary>
        /// Чтобы сделать юнит-тестирование, этот объект надо будет заменить на унаследованный от HttpSender мок и этот класс не надо трогать
        /// </summary>
        HttpSender httpSender = new HttpSender();


        public override void Start()
        {
            if (!int.TryParse(ConfigurationManager.AppSettings["dispatcherSleepSeconds"], out int dispatcherSleepSeconds))
                dispatcherSleepSeconds = 10;

            base.AddAndStartUsualThread(nameof(Dispatcher), Watch, dispatcherSleepSeconds, 0, ThreadPriority.AboveNormal);
        }


        readonly Db db = new Db();


        void Watch(ManualResetEvent stopEvent)
        {
            // выйти должны только фатальные исключения. Остальные должны внутри проглотиться.

            try
            {
                for (; ; )
                {
                    List<QueueItem> qis = db.GetNextQueueItem();
                    if (qis == null || qis.Count == 0)
                        break; /*sleep to next iteration*/
                    // а, если был хотя бы 1 елемент, выбирать их, пока не кончатся, и только потом спать
                    foreach(QueueItem qi in qis)
                    {
                        bool isOk = Send(qi);

                        if (isOk)
                            db.DeleteQueueItem(qi.Id);
                        // если неуспешно - оно перепошлется через 10 секунд
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // do not end dispatcher's work - try again in interval
                Logger.Error(ex);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="qi"></param>
        /// <returns>Whether sending was successfull</returns>
        private bool Send(QueueItem qi)
        {
            try
            {
                var ans = httpSender.SendHttp(HttpMethod.Post, ConfigurationManager.AppSettings["serverUrl"], body: qi.Message);

                Logger.Info("({1}) [{0}] is sent: {2} {3}", qi.Message, qi.Id, ans.Item1, ans.Item2);
                return true;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error("failure sending ({1}) [{0}] {2}", qi.Message, qi.Id, ex.GetWholeMessage());
                return false;
            }
            catch (WebException ex)
            {
                Logger.Error("failure sending ({1}) [{0}] {2}", qi.Message, qi.Id, ex.GetWholeMessage());
                return false;
            }
        }
    }
}
