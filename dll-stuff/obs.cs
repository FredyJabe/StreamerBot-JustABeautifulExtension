using System;
using System.Threading;
using OBSWebsocketDotNet;

namespace JabeDll {
    public class OBSConnection {
        protected OBSWebsocket _obs = new OBSWebsocket();

        private CancellationTokenSource _keepAliveTokenSource;
        private readonly int _keepAliveInterval = 500;

        private static bool Connect() {
            bool retVal = false;

            if (!obs.IsConnected) {
                System.Threading.Tasks.Task.Run(() => {
                    try {
                        _obs.ConnectAsync(Settings.ObsUrl, Settings.ObsPassword);
                    }
                    catch (Exception ex) {
                        BeginInvoke((MethodInvoker)delegate {
                            Data.Log("Connect failed : " + ex.Message);
                            return;
                        });
                    }
                });
            }
        }

        public static void SendRaw(string data) {
            // TODO handle sending message to OBS

            _obs.SendRequest();

            //public JObject SendRequest(string requestType, JObject additionalFields = null)
            //{
            //    return SendRequest(MessageTypes.Request, requestType, additionalFields, true);
            //}

            // <param name="operationCode">Type/OpCode for this messaage</param>
            // <param name="requestType">obs-websocket request type, must be one specified in the protocol specification</param>
            // <param name="additionalFields">additional JSON fields if required by the request type</param>
            // <param name="waitForReply">Should wait for reply vs "fire and forget"</param>
            // <returns>The server's JSON response as a JObject</returns>
            //internal JObject SendRequest(MessageTypes operationCode, string requestType, JObject additionalFields = null, bool waitForReply = true)
        }
    }
}