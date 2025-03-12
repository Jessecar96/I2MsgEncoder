using System.Net.Http;

namespace I2MsgEncoder
{
    class HttpClientHolder
    {

        static readonly HttpClient client = new HttpClient();

        public static HttpClient GetClient() => client;

    }
}
