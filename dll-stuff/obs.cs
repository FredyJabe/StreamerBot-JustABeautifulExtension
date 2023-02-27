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
    }
}