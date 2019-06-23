#region usings
using System;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
#endregion

namespace Pochta.Server
{
    public class VeryImportantController : PochtaApiController
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetLogger(nameof(VeryImportantController));
        readonly Db db = new Db();

        public HttpResponseMessage VeryImportantAction()
        {
            string data = this.Request.Content.ReadAsStringAsync().Result;

            Logger.Debug("{0}: [{1}]", nameof(VeryImportantAction), data ?? "(null)");
            try
            {
                int requestId = SaveData(data);

                var ans = new { status = "ok", requestId };
                return Request.CreateResponse(HttpStatusCode.OK, ans);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Ошибка. Обратитесь в службу технической поддержки.");
            }
        }


        /// <exception cref="SqlException" />
        int SaveData(string data)
        {
            string ip = GetClientAddress();
            Logger.Info("{1} [{0}]", data, ip);

            return db.InsertIncomingRequest(data, ip);
        }
    }
}   
