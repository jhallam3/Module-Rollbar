using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rollbar.Classes
{
    public class Message
    {
        public string body { get; set; }
    }

    public class Body
    {
        public Message message { get; set; }
    }

    public class Request
    {
        public string user_ip { get; set; }
    }

    public class Person
    {
        public string id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
    }

    public class Server
    {
        public string cpu { get; set; }
        public string ram { get; set; }
        public string host { get; set; }
        public string code_version { get; set; }
    }

    public class Data
    {
        public string environment { get; set; }
        public Body body { get; set; }
        public string level { get; set; }
        public string code_version { get; set; }
        public Request request { get; set; }
        public Person person { get; set; }
        public Server server { get; set; }
    }

    public class JsonRollbarRootObject
    {
        public string access_token { get; set; }
        public Data data { get; set; }
    }
}
