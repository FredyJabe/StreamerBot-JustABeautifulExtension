using System;
using System.IO;
using System.Net;
using System.Web;

namespace JabeDll {
    public static class Communication {
        public static void OutputToStreamerbot(string stuff) {
            Data.Log("HttpHandlerUrl: " + Settings.HttpHandlerUrl);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Settings.HttpHandlerUrl);
            request.Method = "POST";
            request.ContentType = "application/json";
            
            string data = "{"
                        + "\"action\":{"
                        + "\"id\" : \"2c8d1a1b-e948-4965-a670-38acd0a10f1b\","
                        + "\"name\" : \"Websocket Handle\""
                        + "},"
                        + "\"args\" : {"
                        + "\"test\" : \"" + stuff + "\""
                        + "}"
                        + "}";
            Data.Log(data);

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream())) {
                writer.Write(data);
            }

            var response = (HttpWebResponse)request.GetResponse();
        }

        public static void OutputToLumiastream(string platform, string stuff) {
            if (stuff != "" && platform != "") {
                Data.Log("Lumia Stuff: " + Settings.LumiaAPIUrl + Settings.LumiaAPIToken);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Settings.LumiaAPIUrl + Settings.LumiaAPIToken);
                request.Method = "POST";
                request.ContentType = "application/json";
                
                string data = "{"
                            + "\"type\" : \"chatbot-message\","
                            + "\"params\" : {"
                            + "\"value\" : \"" + stuff + "\" , "
                            + "\"platform\" : \"" + platform + "\""
                            + "}"
                            + "}";
                            Data.Log(data);

                using (StreamWriter writer = new StreamWriter(request.GetRequestStream())) {
                    writer.Write(data);
                }
                var response = (HttpWebResponse)request.GetResponse();
            }
        }
    }
}