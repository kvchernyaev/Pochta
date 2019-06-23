#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using NLog;
#endregion

namespace Pochta.Common
{
    public class HttpSender
    {
        static readonly Logger _logger = LogManager.GetLogger(nameof(HttpSender));


        /// <exception cref="T:System.Net.Http.HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public Tuple<HttpStatusCode, string> SendHttp(HttpMethod m, string serverUrl, string actionPath = null,
            string qsParams = null,
            string contentType = "application/json", string body = null, string authBearer = null)
        {
            Tuple<HttpResponseMessage, string> t = SendHttpInternal(m, serverUrl, actionPath, qsParams, contentType,
                body, authBearer);

            return new Tuple<HttpStatusCode, string>(t.Item1.StatusCode, t.Item2);
        }


        /// <exception cref="T:System.Net.Http.HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public Tuple<HttpStatusCode, T> SendHttp<T>(HttpMethod m, string serverUrl, string actionPath = null,
            string qsParams = null,
            string contentType = "application/json", string body = null, string authBearer = null)
        {
            Tuple<HttpResponseMessage, string> t = SendHttpInternal(m, serverUrl, actionPath, qsParams, contentType,
                body, authBearer);

            var o = new JavaScriptSerializer().Deserialize<T>(t.Item2);
            return new Tuple<HttpStatusCode, T>(t.Item1.StatusCode, o);
        }


        /// <exception cref="T:System.Net.Http.HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        Tuple<HttpResponseMessage, string> SendHttpInternal(HttpMethod m, string serverUrl, string actionPath,
            string qsParams,
            string contentType = "application/json", string body = null, string authBearer = null)
        {
            //specify to use TLS 1.2 as default connection
            System.Net.ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            string url = $@"{serverUrl}{actionPath}?{qsParams}";
            var message = new HttpRequestMessage(m, url);
            if (!string.IsNullOrWhiteSpace(authBearer))
                message.Headers.Add("authorization", "Bearer " + authBearer);

            if (m != HttpMethod.Get && !string.IsNullOrWhiteSpace(body))
                message.Content = new System.Net.Http.StringContent(body, Encoding.UTF8, contentType);

            try
            {
                using (var client = new HttpClient())
                {
                    HttpResponseMessage response = client.SendAsync(message).Result;
                    string res = response.Content.ReadAsStringAsync().Result;

                    if (response.StatusCode != HttpStatusCode.OK)
                        _logger.Error("http fail {url} status {status} answer text [{result}], body [{body}]",
                            url, response.StatusCode, res, body);

                    return new Tuple<HttpResponseMessage, string>(response, res);
                }
            }
            catch (AggregateException ex)
            {
                // Эта фигня появляется из-за асинхронности client.SendAsync, оно из таска там перекидывается
                throw ex.InnerException;
            }
        }
    }
}