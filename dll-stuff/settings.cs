using System;

namespace JabeDll {
    public static class Settings {
        private static string _httpHandlerUrl = @"127.0.0.1:7474";
        private static string _lumiaAPIUrl = @"127.0.0.1:39231";
        private static string _lumiaAPIToken = @"";
        private static string _actionId = "";
        private static string _actionName = "";
        private static string _pathLOG = @"D:\Stream\Logs\" + DateTime.Now.ToShortDateString() + ".log";
        private static string _pathTXT = @"D:\Stream\Commands\";
        private static string _pathSFX = @"D:\Stream\Sounds\";
        private static string _pathGFX = @"D:\Stream\Gifs\";
        private static string _pathDATA = @"D:\Stream\Data\";

        public static string HttpHandlerUrl {
            get { return "http://" + _httpHandlerUrl + "/DoAction"; }
        }
        public static string LumiaAPIUrl {
            get { return "http://" + _lumiaAPIUrl + "/api/send?token="; }
        }
        public static string LumiaAPIToken {
            get { return _lumiaAPIToken; }
        }
        public static string ActionId {
            get { return _actionId; }
        }
        public static string actionName {
            get { return _actionName; }
        }
        public static string PathLog {
            get { return _pathLOG; }
        }
        public static string PathTxt {
            get { return _pathTXT; }
        }
        public static string PathSfx {
            get { return _pathSFX; }
        }
        public static string PathGfx {
            get { return _pathGFX; }
        }
        public static string PathData {
            get { return _pathDATA; }
        }

        public static void Initialize() {
            string settingsPath = @"JabeDLL/Settings.cfg";
        }
    }
}