using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WinServiceConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Factory.StartNew(async () => 
            {                
                while (true)
                {
                    var server = new NamedPipeServerStream("WinServiceConsole");
                    server.WaitForConnection();
                    StreamReader reader = new StreamReader(server);
                    StreamWriter writer = new StreamWriter(server);
                    var line = reader.ReadLine();                    
                    Console.WriteLine(line);
                    PostRequestAsync(line);
                    writer.Flush();
                    server.Disconnect();
                    server.Close();
                }
            });

            Console.ReadLine();
        }

        static void PostRequestAsync(string message)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://localhost:1234/users");
            request.Method = "POST"; // для отправки используется метод Post
            request.Timeout = 5000;
            request.KeepAlive = false;
            // данные для отправки
            string data = GetJSONMessage(message);
            //string data = string.Format(@"{'{0}': 'user3', ""password"": ""333456"", ""RecDateText"": ""2020-03-12 10:33:17""}", "Login:234");
            // преобразуем данные в массив байтов
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(data);
            // устанавливаем тип содержимого - параметр ContentType
            request.ContentType = "application/json";
            // Устанавливаем заголовок Content-Length запроса - свойство ContentLength
            request.ContentLength = byteArray.Length;

            try
            {
                using (Stream output = request.GetRequestStream())
                    output.Write(byteArray, 0, byteArray.Length);

                var response = request.GetResponse() as HttpWebResponse;
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        Console.WriteLine(reader.ReadToEnd());
                    }
                }
                response.Close();
                Console.WriteLine("Запрос выполнен...");
            }
            catch (WebException wE)
            {
                Console.WriteLine(wE.Message);
            }
        }

        static string GetJSONMessage(string message)
        {
            var result = message;

            char[] trimChars = { '{', '}', ' ', ';'};
            message = message.Trim(trimChars);
            string[] message_arr = message.Split(',').Select(it => it.Substring(it.IndexOf(':') + 1)).ToArray();
            //Dictionary<string, string> json_str = new Dictionary<string, string>();
            //foreach (var str in message_arr)
            //{                
            //    string[] messages = str.Trim(trimChars).Split(new char[] { ':' });

            //    json_str.Add(messages[0].Trim(trimChars), messages[1].Trim(trimChars));
            //}

            //var result = string.Format("{\"Login\":\"{0}\"," + "\"Password\":\"{1}\"}" + "\"RecDateText\":\"{2}\"}",
            //    message_arr[0].Trim(trimChars), message_arr[1].Trim(trimChars), message_arr[2].Trim(trimChars))

            //var result = $@"{{""Login"":""{message_arr[0].Trim(trimChars)}""," + 
            //             $@"""Password"":""{message_arr[1].Trim(trimChars)}""," +  
            //             $@"""RecDateText':""{message_arr[2].Trim(trimChars)}""";
            
            if (message_arr.Length == 3)
            {
                result = "{" + "\"" + "Login" + "\"" + " : " + "\"" + message_arr[0].Trim(trimChars) + "\"" + ", " +
                               "\"" + "Password" + "\"" + " : " + "\"" + message_arr[1].Trim(trimChars) + "\"" + ", " +
                               "\"" + "RecDateText" + "\"" + " : " + "\"" + message_arr[2].Trim(trimChars) + "\"" + "}";
            }

            return result;
        }
    }
}
