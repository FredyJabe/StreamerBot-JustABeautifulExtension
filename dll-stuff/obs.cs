using System;
using System.Threading;
using OBSWebsocketDotNet;

namespace JabeDll {
    public class OBSConnection {
        protected OBSWebsocket _obs = new OBSWebsocket();

        private CancellationTokenSource _keepAliveTokenSource;
        private readonly int _keepAliveInterval = 500;

        private static void Connect() {
            if (!obs.IsConnected)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        _obs.ConnectAsync(txtServerIP.Text, txtServerPassword.Text);
                    }
                    catch (Exception ex)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            Data.Log("Connect failed : " + ex.Message);
                            return;
                        });
                    }
                });
            }
        }
    }
}