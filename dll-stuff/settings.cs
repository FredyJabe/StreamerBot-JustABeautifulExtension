using System;

namespace JabeDLL {
    public static class Settings {
        private static string _httpHandlerUrl = @"http://127.0.0.1:7474/DoAction";
        private static string _actionId = "";
        private static string _actionName = "";
        private static string _pathLOG = @"D:\Stream\Logs\" + DateTime.Now.ToShortDateString() + ".log";
        private static string _pathTXT = @"D:\Stream\Commands\";
        private static string _pathSFX = @"D:\Stream\Sounds\";
        private static string _pathGFX = @"D:\Stream\Gifs\";
        private static string _pathDATA = @"D:\Stream\Data\";

        public static string HttpHandlerUrl {
            get => _httpHandlerUrl;
        }
        public static string ActionId {
            get => _actionId;
        }
        public static string actionName {
            get => _actionName;
        }
        public static string PathLog {
            get => _pathLOG;
        }
        public static string PathTxt {
            get => _pathTXT;
        }
        public static string PathSfx {
            get => _pathSFX;
        }
        public static string PathGfx {
            get => _pathGFX;
        }
        public static string PathData {
            get => _pathDATA;
        }

        public static void Initialize() {
            string settingsPath = @"JabeDLL/";
        }
    }
}