using System;
using System.IO;
using System.Windows.Forms;

namespace JabeDll {
    public static class Settings {
        private static string _httpHandlerUrl = @"127.0.0.1:7474";
        private static string _lumiaAPIUrl = @"127.0.0.1:39231";
        private static string _lumiaAPIToken = @"";
        private static string _actionId = "";
        private static string _actionName = "";
        private static string _pathDATA = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"\JabeDll\";
        private static string _configFile = _pathDATA + @"Settings.cfg";

        public static string HttpHandlerUrl {
            get {
                    return @"http://" + GetSettingValue("HttpHandlerUrl", _httpHandlerUrl) + @"/DoAction";
            }
        }
        public static string LumiaAPIUrl {
            get { 
                return  @"http://" + GetSettingValue("LumiaAPIUrl", _lumiaAPIUrl) + @"/api/send?token=";
            }
        }
        public static string LumiaAPIToken {
            get {
                return GetSettingValue("LumiaAPIToken", _lumiaAPIToken);
            }
        }
        public static string ActionId {
            get {
                return GetSettingValue("ActionId", _actionId);
            }
        }
        public static string actionName {
            get {
                return GetSettingValue("ActionName", _actionName);
            }
        }
        public static string PathData {
            get { return _pathDATA; }
        }
        public static string PathLog {
            get { return PathData + @"Logs\" + DateTime.Now.ToShortDateString() + ".log"; }
        }
        public static string PathTxt {
            get { return PathData + @"Commands\"; }
        }
        public static string PathSfx {
            get { return PathData + @"SFXs\"; }
        }
        public static string PathGfx {
            get { return PathData + @"GFXs\"; }
        }

        // Retrieves a specific setting value
        private static string GetSettingValue(string key, string defaultValue) {
            string retVal = defaultValue;

            if (File.Exists(_configFile)) {
                foreach(string s in File.ReadAllLines(_configFile)) {
                    if (!s.StartsWith("#") && s != "") {
                        string key2 = s.Split(',')[0];
                        string value = s.Split(',')[1];

                        if (key2 == key) {
                            retVal = value;
                            break;
                        }
                    }
                }
            }

            return retVal;
        }
    }
}