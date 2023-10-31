using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

public class CPHInline
{
    // OPTIONS
	private string pathMAIN = $"JabeChatCommands\\", pathLOG, pathTXT, pathSFX, pathGFX, pathDATA, pathEVENTS, pathALERTS, pathREDEEM, pathTIMERS, pathQUOTES,
                   obsSceneEffects = "Component Overlay Effects",
                   obsSourceSFX = "SFX",
                   obsSourceGFX = "GFX",
                   obsSourceEmbed = "Embed",
                   obsSourceSongRequest = "SongRequest";

    private string user, message;
    private bool isVip, isSub, isModerator; 
    private bool hasOutput, isAction, isAnnounce;

    public bool Execute() {
        try {
            VerifyFiles();

            switch(args["triggerName"].ToString()) {
                #region Generic
                case "Streamer.bot Started": BotStarted(); break;
                #endregion
                #region Twitch
                case "Chat Message": ChatMessage(); break;
                case "Reward Redemption": RewardRedemption(); break;
                case "Viewer Count Update": ViewerCountUpdate(); break;
                case "Present Viewers": PresentViewers(); break;
                case "Timed Actions": break;
                case "Follow":
                case "Subscription":
                case "Resubscription":
                case "Gift Subscription": 
                case "Raid": 
                case "Cheer": 
                case "Hype Train Start": 
                case "Hype Train End": EventHandle(); break;
                #endregion
                #region Youtube
                case "Message": ChatMessage(); break;
                case "Super Chat":
                case "Super Sticker":
                case "New Subscriber": EventHandle(); break;
                #endregion
            }
        }
        catch(Exception e) {
            Log($"ERROR: {e.ToString()}");
        }

        return true;
    }

    #region EVENT: StreamerBot started
    private void BotStarted() {
        try {
            // Resets the stream specific variables
            CPH.SetGlobalVar("first", null, false);
            CPH.SetGlobalVar("chanceRoulette", 5, false);
            CPH.SetGlobalVar("collabLink", null, false);
            CPH.SetGlobalVar("usersThatSaidSomething", new List<string>(), false);
            CPH.SetGlobalVar("currentViewerCount", 0, false);

            CPH.SetGlobalVar("currentViewers", new List<string>(), false);
            CPH.SetGlobalVar("currentViewersVip", new List<string>(), false);
            CPH.SetGlobalVar("currentViewersSub", new List<string>(), false);
            CPH.SetGlobalVar("currentViewersMod", new List<string>(), false);
            CPH.SetGlobalVar("currentViewersTotal", new List<string>(), false);

            // TODO : Check current version versus the one on github and warn if update is available
            //MessageBox.Show("Hello World");
            // Version check
            if (File.Exists($"{pathDATA}version.txt")) {
                string currentVersion = File.ReadAllText($"{pathDATA}version.txt");
                string updatedVersion = currentVersion; // TODO : Read the version from Github file
            }
        }
        catch(Exception e) {
            Log($"ERROR: {e.ToString()}");
        }
    }
    #endregion
    #region EVENT: Chat Message
    private void ChatMessage() {
        try {
            user = args["user"].ToString();
            message = args["message"].ToString();
            isSub = (args["isSubscribed"].ToString().ToLower() == "true") ? true : false;
            isModerator = (args["isModerator"].ToString().ToLower() == "true") ? true : false;
            
            if (args["eventSource"].ToString() == "twitch") {
                isVip = (args["isVip"].ToString().ToLower() == "true") ? true : false;
            }

            if (user.ToLower() == "jabenet") return;
            Log($"ChatMessage: {user}: {message}");

            // Checks if first message from a user
            List<string> usersThatSaidSomething = CPH.GetGlobalVar<List<string>>("usersThatSaidSomething", false);
            if (!usersThatSaidSomething.Contains(user)) {
                // Makes sure the sound only plays once by adding the user to the list before doing anything else
                usersThatSaidSomething.Add(user);
                CPH.SetGlobalVar("usersThatSaidSomething", usersThatSaidSomething, false);

                // Then checks if the file/directory exists and play it if it does
                string file = $"{pathALERTS}Viewers\\{user.ToLower()}.mp3";
                Log($"  File: {file}");
                if (File.Exists($"{file}")) {
                    Log($"    File exists");
                    ShowInObs(obsSceneEffects, obsSourceSFX, $"{System.AppDomain.CurrentDomain.BaseDirectory}{file}.mp3", GetDuration($"{file}.mp3"));
                }
                // If it's a folder, play a random file in there
                else if (Directory.Exists(file)) {
                    Log($"    Directory exists");
                    Random r = new Random();
                    string[] cmds = Directory.GetFiles(file);
                    int cmdToExecute = r.Next(cmds.Length);

                    int duration = GetDuration($"{cmds[cmdToExecute]}");
                    ShowInObs(obsSceneEffects, obsSourceSFX, $"{System.AppDomain.CurrentDomain.BaseDirectory}{cmds[cmdToExecute]}", duration);
                }
            }
            
            // Checks if it's a command
            if (message.StartsWith("!")) {
                string[] arguments = message.Split(' ');
                string command = arguments[0].Replace("!","").ToLower();
                string commandPath = "";
                int millisecondsToAdd = 0;
                Log($"  Command: {command}");

                // Makes sure that it's a command and not an empty string, the empty string causes some random shits
                if (command == "") return;
                
                if (DateTime.Compare(DateTime.Now, CPH.GetGlobalVar<DateTime>($"canPlayCommand{command}", false)) >= 0) {
                    #region Custom alerts
                    if (command == user.ToLower()) {
                        Log($"    Custom Alert");
                        string alertPath = $"{pathALERTS}Viewers\\{command}";
                        Log($"    alertPath: {alertPath}");

                        if (File.Exists($"{alertPath}.mp3")) {
                            Log($"      File exists");
                            millisecondsToAdd = GetDuration($"{alertPath}.mp3");
                            CPH.SetGlobalVar($"canPlayCommand{command}", DateTime.Now.AddMilliseconds(millisecondsToAdd).AddSeconds(GetCooldown(command)), false);
                            ShowInObs(obsSceneEffects, obsSourceSFX, $"{System.AppDomain.CurrentDomain.BaseDirectory}{alertPath}.mp3", millisecondsToAdd);
                        }
                        else if (Directory.Exists(alertPath)) {
                            Log($"      Directory Exists");
                            Random r = new Random();
                            string[] cmds = Directory.GetFiles(alertPath);
                            int cmdToExecute = r.Next(cmds.Length);

                            int duration = GetDuration($"{cmds[cmdToExecute]}");
                            Log($"Duration: {duration}");
                            CPH.SetGlobalVar($"canPlayCommand{command}", DateTime.Now.AddMilliseconds(millisecondsToAdd).AddSeconds(GetCooldown(command)), false);
                            ShowInObs(obsSceneEffects, obsSourceSFX, $"{System.AppDomain.CurrentDomain.BaseDirectory}{cmds[cmdToExecute]}", duration);
                        }
                    }
                    #endregion
                    #region TXT
                    else if (File.Exists(pathTXT + command + ".txt") && command != "mod" && command != "vip" && command != "sub") {
                        // Is a TXT command
                        commandPath = pathTXT + command + ".txt";
                    }
                    #endregion
                    #region Random TXT
                    else if (Directory.Exists(pathTXT + command) && command != "mod" && command != "vip" && command != "sub") {
                        // Is a TXT command but runs a random file in that folder
                        Random r = new Random();
                        string[] cmds = Directory.GetFiles(pathTXT + command);
                        int cmdToExecute = r.Next(cmds.Length);
                        commandPath = cmds[cmdToExecute];
                    }
                    #endregion
                    #region Vip+ TXT
                    else if (File.Exists(pathTXT + @"vip\" + command + ".txt") && (isModerator || isVip)) {
                        // Is a TXT command but only available to MODs and VIPs
                        commandPath = pathTXT + @"vip\" + command + ".txt";
                        Log(pathTXT + @"vip\" + command + ".txt");
                    }
                    #endregion
                    #region Subs+ TXT
                    else if (File.Exists(pathTXT + @"sub\" + command + ".txt") && (isModerator || isSub)) {
                        // Is a TXT command but only available to MODs and Subs
                        commandPath = pathTXT + @"sub\" + command + ".txt";
                        Log(pathTXT + @"sub\" + command + ".txt");
                    }
                    #endregion
                    #region Moderator Only TXT
                    // Moderator only commands
                    else if (File.Exists(pathTXT + @"mod\" + command + ".txt") && isModerator) {
                        // Is a TXT command but only available to MODs
                        commandPath = pathTXT + @"mod\" + command + ".txt";
                    }
                    #endregion
                    if (commandPath != "") {
                        CPH.SetGlobalVar($"canPlayCommand{command}", DateTime.Now.AddMilliseconds(millisecondsToAdd).AddSeconds(GetCooldown(command)), false);
                        ReadCommand(commandPath, arguments);
                    }

                    if (DateTime.Compare(DateTime.Now, CPH.GetGlobalVar<DateTime>("canPlayCommand")) >= 0) {

                        string sfx = pathSFX + command + ".mp3";
                        string gfx = pathGFX + command + ".mp4";
                        
                        #region SFX
                        if (File.Exists(sfx) && command != "vip" && command != "sub") {
                            // Is a SFX command
                            millisecondsToAdd = GetDuration(sfx);
                            CPH.SetGlobalVar($"canPlayCommand{command}", DateTime.Now.AddMilliseconds(millisecondsToAdd).AddSeconds(GetCooldown(command)), false);
                            ShowInObs(obsSceneEffects, obsSourceSFX, $"{System.AppDomain.CurrentDomain.BaseDirectory}{sfx}", millisecondsToAdd);
                        }
                        #endregion
                        #region Random SFX 
                        else if (Directory.Exists(pathSFX + command) && command != "vip" && command != "sub") {
                            // Is a SFX command but runs a random file in that folder
                            Random r = new Random();
                            string[] cmds = Directory.GetFiles(pathSFX + command);
                            int cmdToExecute = r.Next(cmds.Length);
                            CPH.LogDebug(cmds[cmdToExecute]);
                            millisecondsToAdd = GetDuration(cmds[cmdToExecute]);

                            CPH.SetGlobalVar($"canPlayCommand{command}", DateTime.Now.AddMilliseconds(millisecondsToAdd).AddSeconds(GetCooldown(command)), false);
                            ShowInObs(obsSceneEffects, obsSourceSFX, $"{System.AppDomain.CurrentDomain.BaseDirectory}{cmds[cmdToExecute]}", millisecondsToAdd);
                        }
                        #endregion
                        #region GFX
                        else if (File.Exists(gfx) && command != "vip" && command != "sub") {
                            // Is a GFX command
                            int duration = GetDuration(gfx);

                            CPH.SetGlobalVar($"canPlayCommand{command}", DateTime.Now.AddMilliseconds(millisecondsToAdd).AddSeconds(GetCooldown(command)), false);
                            ShowInObs(obsSceneEffects, obsSourceGFX, $"{System.AppDomain.CurrentDomain.BaseDirectory}{gfx}", duration);
                        }
                        #endregion
                    }
                }
            }
            else {
                // When it's not a command, play a sound to make sure you don't miss the message
                string chatMessageAlert = $"{pathALERTS}chat-notif.mp3";
                if (File.Exists(chatMessageAlert)) {
                    CPH.PlaySound(chatMessageAlert);
                }
            }
        }
        catch(Exception e) {
            Log($"ERROR: {e.ToString()}");
        }
    }
    #endregion
    #region EVENT: Viewer Count Update
    private void ViewerCountUpdate() {
        CPH.SetGlobalVar("currentViewerCount", (int)args["viewerCount"]);
    }
    #endregion
    #region EVENT: Present Viewers
    private void PresentViewers() {
        try {
            // Resets the user lists
            List<string> viewers = new(), viewersVip = new(), viewersSub = new(), viewersMod = new(), viewersTotal = new();

            // Cycle throught all users and add
            foreach(Dictionary<string,object> u in (List<Dictionary<string,object>>)args["users"]) {
                int uRole = (int)u["role"];
                string uName = u["display"].ToString();
                if ((bool)u["isSubscribed"]) { viewersSub.Add(uName); }
                if (uRole == 2) { viewersVip.Add(uName); }
                else if (uRole == 3 || uRole == 4) { viewersMod.Add(uName); }
                else { viewers.Add(uName); }

                viewersTotal.Add(uName);
            }

            // Then saves the values so you ccan use them somewhere else
            CPH.SetGlobalVar("currentViewers", viewers, false);
            CPH.SetGlobalVar("currentViewersVip", viewersVip, false);
            CPH.SetGlobalVar("currentViewersSub", viewersSub, false);
            CPH.SetGlobalVar("currentViewersMod", viewersMod, false);
            CPH.SetGlobalVar("currentViewersTotal", viewersTotal, false);
        }
        catch(Exception e) {
            Log($"ERROR: {e.ToString()}");
        }
    }
    #endregion
    #region EVENT: Timed Actions
    private void TimedAction() {
        // TODO: Timed Actions
    }
    #endregion
    #region EVENT: Reward Redemption
    private void RewardRedemption() {
        string redeemFile = $"{pathREDEEM}\\{args["rewardName"]}.txt";
        Log(redeemFile);
        if (File.Exists(redeemFile)) {
            ReadCommand(redeemFile, new string[1]);
        }
    }
    #endregion
    #region EVENT: Follow, Subscription, Resubscription, Gift Subscription, Raid, Cheer, Hype Train Start, Hype Train End
    private void EventHandle() {
        string eventFile = $"{pathEVENTS}\\{args["eventSource"].ToString()}\\{args["triggerName"].ToString()}.txt";
        string eventSound = $"{pathEVENTS}\\{args["eventSource"].ToString()}\\{args["triggerName"].ToString()}.mp3";
        string eventVideo = $"{pathEVENTS}\\{args["eventSource"].ToString()}\\{args["triggerName"].ToString()}.mp4";
        Log($"eventFile: {eventFile}");
        Log($"eventSound: {eventSound}");
        Log($"eventVideo: {eventVideo}");
        if (File.Exists(eventFile)) {
            ReadCommand(eventFile, new string[1]);
        }
        if (File.Exists(eventSound)) {
            int duration = GetDuration($"{eventSound}");
            ShowInObs(obsSceneEffects, obsSourceSFX, $"{System.AppDomain.CurrentDomain.BaseDirectory}{eventSound}", duration);
        }
        if (File.Exists(eventVideo)) {
            int duration = GetDuration($"{eventVideo}");
            ShowInObs(obsSceneEffects, obsSourceGFX, $"{System.AppDomain.CurrentDomain.BaseDirectory}{eventVideo}", duration);
        }
    }
    #endregion

#region Methods
    #region ReadCommand - Reads the command to execute it
    private void ReadCommand(string cmdFile, string[] arguments) {
        hasOutput = true;
		isAction = false;
		isAnnounce = false;
        string[] lines = File.ReadAllLines(cmdFile);
        Dictionary<string, string> variables = new();

        // Reads every line from the command file
        for (int l=0; l<lines.Length; l++) {
            string output = lines[l];

            bool inBalise = false;
            List<string> balise = new();
            int baliseAmt = 0;

            // If starting with ::, don't do anything, it's a comment
            if (!output.StartsWith("::")) {
                // TODO: mettre des commentaire ici
                foreach(char c in output) {
                    if (c == '{') {
                        balise.Add($"");
                        if (inBalise) {
                            baliseAmt ++;
                        }
                        else {
                            inBalise = true;
                        }
                    }
                    if (inBalise) {
                        for (int i=0; i<=baliseAmt; i++) {
                            balise[i] += c;
                        }
                    }
                    if (c == '}') {
                        if (baliseAmt > 0) {
                            string tagHandled = HandleTag(balise[baliseAmt], arguments, variables);

                            for(int i=1;i<=baliseAmt;i++) {
                                balise[baliseAmt-i] = balise[baliseAmt-i].Replace(balise[baliseAmt], tagHandled);
                            }

                            output = output.Replace(balise[baliseAmt], tagHandled);
                            balise[baliseAmt] = "";
                            baliseAmt --;
                        }
                        else {
                            inBalise = false;

                            for(int i=(balise.Count-1); i>=0; i--) {
                                if (balise[i] != "") {
                                    string tagHandled = HandleTag(balise[i], arguments, variables);
                                    output = output.Replace(balise[i], tagHandled);
                                    balise[i] = "";
                                }
                            }
                        }
                    }
                }

                
                if (hasOutput && output != "") {
                    // TODO: Check output's lenght and split it if necessary
                    SendMessage(args["eventSource"].ToString(), output, isAction, isAnnounce);
                }
            }
		}
    }
    #endregion
    #region HandleTag - Handles the tags management
    private string HandleTag(string _tag, string[] arguments, Dictionary<string, string> variables) {
        string retVal = "";
        string[] tag = _tag.Substring(1, _tag.Length - 2).Split(';');

        switch(tag[0]) {
            // Managing all possible variables in commands
            case "a": // Returns à specific argument
                try {
                    int arg = int.Parse(tag[1]);
                    if (arguments.Length >= arg){
                        retVal = arguments[arg].Replace("@","");
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "r": // Returns a random number between 1 and X (defaults at 6)
                try {
                    if (tag.Length > 1) {
                        int randomMax = Int32.Parse(tag[1]);
                        retVal = CPH.Between(1, randomMax).ToString();
                    }
                    else {
                        retVal = CPH.Between(1, 6).ToString();
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "rom": // Returns the rest of the message
                try {
                    if (arguments.Length > 0) {
                        for(var i = 1; i < arguments.Length; i ++) {
                            retVal += arguments[i] + " ";
                        }
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "tts": // Make the bot say stuff.. VERBALLY
                try {
                    if (tag.Length > 2) {
                        string tts = "";
                        for(var i=2; i<tag.Length; i++) {
                            tts += tag[i] + " ";
                        }
                        CPH.TtsSpeak(tag[1], tts);
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "user": // Returns the user's name
                try {
                    retVal = args["user"].ToString();
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "noOutput": // Prevents the command to output anything
                hasOutput = false;
                break;
            case "w": // Wait for X milliseconds
                try {
                    if (tag.Length == 2) {
                        Thread.Sleep(Int32.Parse(tag[1]));
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "first": // The FIRST ONE
                try {
                    if (tag.Length == 3) {
                        if (CPH.GetGlobalVar<string>("first", false) == null) {
                            CPH.SetGlobalVar("first", user, false);
                            retVal = tag[1];
                        }
                        else {
                            retVal = tag[2];
                        }
                    }
                }
                catch (Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "webrequest": // Executes à webrequest
                try {
                    if (tag.Length == 2) {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{tag[1]}");
                        request.Accept = "text/plain";

                        StreamReader objReader = new StreamReader(request.GetResponse().GetResponseStream());
                        retVal = objReader.ReadLine();
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "tAnnounce": // Uses the Twitch Announce command
                isAnnounce = true;
                break;
            case "tAction": // Uses the Twitch action command
                isAction = true;
                break;
            case "action": // Executes another StreamerBot Action
                try {
                    if (tag.Length == 2) {
                        if (CPH.ActionExists(tag[1])) {
                            CPH.RunAction(tag[1], true);
                        }
                    }
                }
                catch (Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "clip": // Fetches target's clip within the last X most recent 
                // {clip:info:target:amount}
                try {
                    string target = "";
                    int amount = 10;
                    if (tag.Length > 2) {
                        target = tag[2];
                        if (tag.Length == 4) {
                            Int32.TryParse(tag[3], out amount);
                        }
                        Random r = new();
                        var clips = CPH.GetClipsForUser(target, amount);
                        // Only continue if at least 1 clip is found
                        if (clips.Count > 1) {
                            var clip = clips[r.Next(clips.Count - 1)];
                        
                            string[] info = tag[1].Split(',');
                            for(int i=0; i<info.Length; i++) {
                                if (i != 0) {
                                    retVal += ";";
                                }
                                
                                switch(info[i]) {
                                    default:
                                    case "url": retVal += clip.Url; break;
                                    case "embed": retVal += clip.EmbedUrl + "&parent=localhost&autoplay=true"; break;
                                    case "title": retVal += clip.Title; break;
                                    case "duration": retVal += clip.Duration.ToString(); break;
                                }
                            }
                        }
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: ${e.ToString()}");
                }
                break;
            case "embed": // Embed a url to OBS
                // {embed:url:duration}
                try {
                    if (tag.Length == 3) {
                        ShowInObs(obsSceneEffects, obsSourceEmbed, $"{tag[1]}", (int)(float.Parse(tag[2]) * 1000f));
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "collab": // Shows collaboration link
                try {
                    if (tag.Length == 2) {
                        string collabLink = CPH.GetGlobalVar<string>("collabLink");
                        retVal = (collabLink == null) ? tag[1] : collabLink;
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "addquote": // Adds a quote to the quote folder
                try {
                    if (tag.Length > 1) {
                        string quoteFileName = "";

                        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create()) {
                            //byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(tag[1]);
                            byte[] hashBytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(tag[1]));
                            //quoteFileName = Convert.ToHexString(hashBytes);
                            StringBuilder sb = new System.Text.StringBuilder();
                            for (int i = 0; i < hashBytes.Length; i++) {
                                sb.Append(hashBytes[i].ToString("X2"));
                            }
                            quoteFileName = sb.ToString();
                        }

                        using (StreamWriter writer = new StreamWriter($"{pathTXT}quote\\{quoteFileName}.txt", false))
                            writer.WriteLine($" {tag[1]} - {DateTime.Now.Day.ToString("00")}/{DateTime.Now.Month.ToString("00")}");
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "getGlobalVar": // Reads StreamerBot GlobalVar
                try {
                    if (tag.Length == 2) {
                        retVal = CPH.GetGlobalVar<string>(tag[1]).ToString();
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "setGlobalVar": // Sets StreamerBot GlobalVar
                try {
                    if (tag.Length == 3) {
                        CPH.SetGlobalVar(tag[1], tag[2]);
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "getLocalVar": // Reads current command local variable
                try {
                    if (tag.Length == 2) {
                        Log($"{tag[1]}");
                        retVal = variables[tag[1]];
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "setLocalVar": // Sets current command local variable
                try {
                    if (tag.Length == 3) {
                        Log($"{tag[1]}={tag[2]}");
                        variables.Add(tag[1], tag[2]);
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "getUserVar": // Reads current command local variable
            // {getUserVar;User;Variable}
                try {
                    if (tag.Length == 3) {
                        if (args["eventSource"].ToString() == "twitch") {
                            CPH.GetTwitchUserVar<string>(tag[1], tag[2]);
                        }
                        else {
                            CPH.GetYouTubeUserVar<string>(tag[1], tag[2]);
                        }
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "setUserVar": // Sets current command local variable
            // {setUserVar;User;Variable;Value}
                try {
                    if (tag.Length == 4) {
                        if (args["eventSource"].ToString() == "twitch") {
                            CPH.SetTwitchUserVar(tag[1], tag[2], tag[3]);
                        }
                        else {
                            CPH.SetYouTubeUserVar(tag[1], tag[2], tag[3]);
                        }
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "collabstart": // Starts a collab
                try {
                    if (tag.Length == 2) {
                        string collabUrl = $"https://multitwitch.tv/{args["broadcastUserName"]}/{tag[1].Replace(" ","/")}";
                        CPH.SetGlobalVar("collabLink", collabUrl);
                        retVal = collabUrl;
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "collabstop": // Stops a collab
                try {
                    if (tag.Length == 1) {
                        CPH.SetGlobalVar("collabLink", null);
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "tShoutout": // Uses the Twitch Shoutout command
                try {
                    if (tag.Length == 2) {
                        CPH.TwitchSendShoutoutByLogin(tag[1].Replace("@", ""));
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "addcmd": // Add a command... FROM A COMMAND
                try {
                    if (tag.Length > 2) {
                        string commandText = "";
                        for(var i = 2; i < arguments.Length; i ++) {
                            commandText += arguments[i] + " ";
                        }
                        
                        File.WriteAllText($"{pathTXT}{arguments[1].ToLower()}.txt", commandText);
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "rmcmd": // Remove a command... FROM A COMMAND
                try {
                    if (arguments.Length > 1) {
                        string commandName = arguments[1].ToLower();
                        if (File.Exists($"{pathTXT}{commandName}.txt")) {
                            File.Delete($"{pathTXT}{commandName}.txt");
                        }
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "resetCd": // Resets the cooldown for commands
                try {
                    CPH.SetGlobalVar("canPlayCommand", DateTime.Now);
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "urlSafe": // Encodes a string to be safe to write in a URL
            //tag[1]: string stringToEncode
                try {
                    if (tag.Length == 2) {
                        retVal = CPH.UrlEncode(tag[1]);
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "aSplit": // Splits every arguments with a comma
                if (arguments.Length > 0) {
                    for(var i = 1; i < arguments.Length; i ++) {
                        retVal += arguments[i] + ",";
                    }
                }
                break;
            case "playSfx": // Play a sound
            //tag[1]: string soundPath
                try {
                    if (tag.Length == 2) {
                        string sfxToPlay = tag[1];
                        if (File.Exists(sfxToPlay)) {
                            ShowInObs(obsSceneEffects, obsSourceSFX, $"{sfxToPlay}", GetDuration(sfxToPlay));
                        }
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "playGfx": // Play a video file
            //tag[1]: string videoPath
                try {
                    if (tag.Length == 2) {
                        string gfxToPlay = tag[1];
                        if (File.Exists(gfxToPlay)) {
                            ShowInObs(obsSceneEffects, obsSourceGFX, $"{gfxToPlay}", GetDuration(gfxToPlay));
                        }
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "sbPath": // Returns the path where StreamerBot is
                if (tag.Length == 1) {
                    retVal = System.AppDomain.CurrentDomain.BaseDirectory;
                }
                break;
            case "readFile": // Reads a line in a text file
            //tag[1]: string fileToRead
            //tag[2]: int lineToRead
                try {
                    if (tag.Length == 3) {
                        if (File.Exists(tag[1])) {
                            string[] linesFromTextFile = File.ReadAllLines(tag[1]);
                            int lineToRead = int.Parse(tag[2]);
                            if (linesFromTextFile.Length >= lineToRead) {
                                retVal = linesFromTextFile[lineToRead-1];
                            }
                        }
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: tag readFile: {e.ToString()}");
                }
                break;
            case "writeFile": // Writes in a text file
            //tag[1]: string fileToRead
            //tag[2]: string textToWrite
            //tag[3]: int lineToWrite
                try {
                    if (tag.Length == 4) {
                        int lineToWrite = int.Parse(tag[3]);
                        string[] linesToWrite = new string[lineToWrite];
                        if (File.Exists(tag[1])) {
                            string[] linesRead = File.ReadAllLines(tag[1]);
                            if (linesRead.Length > linesToWrite.Length) {
                                linesToWrite = linesRead;
                            }
                            else {
                                for(int i=0; i<linesRead.Length; i++) {
                                    linesToWrite[i] = linesRead[i];
                                }
                            }
                        }
                        linesToWrite[lineToWrite-1] = tag[2];

                        File.WriteAllLines(tag[1], linesToWrite);
                    }
                    else if (tag.Length == 3) { // ignore line number to write a t the end of the file
                        using (StreamWriter writer = new StreamWriter(tag[1], true))
                            writer.WriteLine(tag[2]);
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: tag readFile: {e.ToString()}");
                }
                break;
            case "day": // Outputs current day
                retVal = DateTime.Now.Day.ToString("00");
                break;
            case "month": // Outputs current month
                retVal = DateTime.Now.Month.ToString("00");
                break;
            case "year": // Outputs current year
                retVal = DateTime.Now.Year.ToString("0000");
                break;
            case "isVip": // Outputs if the current use is VIP
                retVal = isVip.ToString();
                break;
            case "isSub": // Outputs if the curent user is Subbed
                retVal = isSub.ToString();
                break;
            case "sA": // Returns StreamerBot args[]
                try {
                    if (tag.Length == 2) {
                        retVal = args[tag[1]].ToString();
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            // Listing stuff
            case "cmds": // Lists all available text commands
                //string[] cmds = Directory.GetFiles(pathTXT, "*.txt");
                try {
                    string[] cmds = Directory.GetFileSystemEntries(pathTXT);
                    string[] cmdsVip = Directory.GetFileSystemEntries(pathTXT + "vip\\");
                    string[] cmdsSub = Directory.GetFileSystemEntries(pathTXT + "sub\\");
                    foreach(string c in cmds) {
                        int i = c.Split('\\').Length;
                        string t = c.Split('\\')[i-1].Replace(".txt","");
                        if (t != "mod" && t != "vip" && t != "sub") {
                            retVal += $"!{t} ";
                        }
                    }
                    if (isVip) {
                        foreach(string c in cmdsVip) {
                            int i = c.Split('\\').Length;
                            string t = c.Split('\\')[i-1].Replace(".txt","");
                            retVal += $"!{t} ";
                        }
                    }
                    if (isSub) {
                        foreach(string c in cmdsSub) {
                            int i = c.Split('\\').Length;
                            string t = c.Split('\\')[i-1].Replace(".txt","");
                            retVal += $"!{t} ";
                        }
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "sfx": // Lists all available SFXs
                try {
                    string[] sfx = Directory.GetFileSystemEntries(pathSFX);
                    string[] sfxVip = Directory.GetFileSystemEntries(pathSFX + "vip\\");
                    string[] sfxSub = Directory.GetFileSystemEntries(pathSFX + "sub\\");
                    foreach(string c in sfx) {
                        int i = c.Split('\\').Length;
                        string t = c.Split('\\')[i-1].Replace(".mp3","");
                        if (t != "vip" && t != "sub") {
                            retVal += $"!{t} ";
                        }
                    }
                    if (isVip) {
                        foreach(string c in sfxVip) {
                            int i = c.Split('\\').Length;
                            string t = c.Split('\\')[i-1].Replace(".mp3","");
                            retVal += $"!{t} ";
                        }
                    }
                    if (isSub) {
                        foreach(string c in sfxSub) {
                            int i = c.Split('\\').Length;
                            string t = c.Split('\\')[i-1].Replace(".mp3","");
                            retVal += $"!{t} ";
                        }
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "gfx": // Lists all available GFXs
                try {
                    string[] gfx = Directory.GetFiles(pathGFX);
                    string[] gfxVip = Directory.GetFileSystemEntries(pathGFX + "vip\\");
                    string[] gfxSub = Directory.GetFileSystemEntries(pathGFX + "sub\\");
                    foreach(string c in gfx) {
                        int i = c.Split('\\').Length;
                        string t = c.Split('\\')[i-1].Replace(".mp4","");
                        if (t != "vip" && t != "sub") {
                            retVal += $"!{t} ";
                        }
                    }
                    if (isVip) {
                        foreach(string c in gfxVip) {
                            int i = c.Split('\\').Length;
                            string t = c.Split('\\')[i-1].Replace(".mp4","");
                            retVal += $"!{t} ";
                        }
                    }
                    if (isSub) {
                        foreach(string c in gfxSub) {
                            int i = c.Split('\\').Length;
                            string t = c.Split('\\')[i-1].Replace(".mp4","");
                            retVal += $"!{t} ";
                        }
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            // Custom Commands
            case "roulette": // 6 chances, 1 bullet
                try {
                    if (tag.Length == 3) {
                        int chanceRoulette = CPH.GetGlobalVar<int>("chanceRoulette");
                        if (CPH.Between(0, chanceRoulette+1) == 0) {
                            chanceRoulette = 5;
                            CPH.SetGlobalVar("chanceRoulette", chanceRoulette);
                            retVal = tag[1];
                        }
                        else {
                            chanceRoulette --;
                            CPH.SetGlobalVar("chanceRoulette", chanceRoulette);
                            retVal = tag[2];
                        }
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            case "readJson": // Outputs a Json node
            // {readJson;json;node[;node[;node]]}
                try {
                    switch(tag.Length) {
                        case 3: retVal = JObject.Parse(tag[1]).GetValue(tag[2]).ToString(); break;
                        case 4: retVal = JObject.Parse(JObject.Parse(tag[1]).GetValue(tag[2]).ToString()).GetValue(tag[3]).ToString(); break;
                        case 5: retVal = JObject.Parse(JObject.Parse(JObject.Parse(tag[1]).GetValue(tag[2]).ToString()).GetValue(tag[3]).ToString()).GetValue(tag[4]).ToString(); break;
                    }
                }
                catch(Exception e) {
                    Log($"ERROR: {e.ToString()}");
                }
                break;
            // DEV STUFF
            case "test":
                //ShowInObs2(obsSceneEffects, obsSourceEmbed, "test", 1000);
                break;
            default: // Anything else
                Log("Unknown tag.");
                break;
        }

        /*
        #region run - Execute a file
            if (output.Contains("{run}")) {
                string file = output.Replace("{run}", "");
                //TODO Finish the run function
            }
        #endregion

        #region massfart - MOD - Farts a bunch of time depending on the amount of viewers
        if (output.Contains("{massfart}")) {
            int amount = Int32.Parse(File.ReadAllLines(pathDATA + "viewerCount.txt")[0]) * 2;

            for(var i=0; i<amount; i++)
            {
                // Is a SFX command but runs a random file in that folder
                Random r = new Random();
                string[] fartSounds = Directory.GetFiles(pathSFX + "fart");
                int cmdToExecute = r.Next(fartSounds.Length);
                CPH.LogDebug(fartSounds[cmdToExecute]);
                CPH.PlaySound(fartSounds[cmdToExecute]);

                CPH.Wait(CPH.Between(250, 1000));
            }
        }
        #endregion
        */

        Log($"retVal: {retVal}");
        return retVal;
    }
    #endregion
    #region SendMessage - Sends a message to tchat, to the good platform
    private void SendMessage(string platform, string message, bool action = false, bool announce = false) {
        if (platform == "twitch") {
            if (action) CPH.SendAction(message);
            else if (announce) CPH.TwitchAnnounce(message);
            else CPH.SendMessage(message);
        }
        else if (platform == "youtube") {
            CPH.SendYouTubeMessage(message);
        }
    }
    #endregion
    #region GetCooldown - Returns the cooldown for a specific command
    private int GetCooldown(string command) {
        int retVal = 0;

        try {
            string[] cmds = File.ReadAllLines($"{pathDATA}cooldowns.txt");
            foreach(string l in cmds) {
                if (l.Contains(command)) {
                    retVal = int.Parse(l.Split('=')[1]);
                    break;
                }
            }
        }
        catch (Exception e) {
            Log("ERROR: " + command);
            Log(e.ToString());
        }

        return retVal;
    }
    #endregion
    #region GetDuration - Returns video duration in milliseconds
    private int GetDuration(string path) {
        /*
        Assembly TagLib = Assembly.Load("taglib-sharp");

        // get the File type
        var fileType = TagLib.GetType("TagLib.File");
        // get the overloaded File.Create method
        var createMethod = fileType.GetMethod("Create", new[] { typeof(string) });

        // get the TagTypes method that contains Id3v2 field
        Type tagTypes = TagLib.GetType("TagLib.TagTypes");
        // get the overloaded File.GetTag method
        var getTagMethod = fileType.GetMethod("GetProperties", new[] {tagTypes});
        // obtain the file
        //dynamic file = createMethod.Invoke(null, new[] { "C:\\temp\\some.mp3" });
        dynamic file = createMethod.Invoke(null, new[] { path });
        // obtain the Id3v2 field value
        //FieldInfo Id3TagField = tagTypes.GetField("Id3v2");
        FieldInfo Id3TagField = tagTypes.GetField("Id3v2");
        //var Id3Tag = Id3TagField.GetValue(tagTypes);

        // obtain the actual tag of the file
        //var tag = getTagMethod.Invoke(file, new[] { Id3Tag });
*/

        var tfile = TagLib.File.Create(path);
        TimeSpan duration = tfile.Properties.Duration;
		
        return duration.Milliseconds + (duration.Seconds * 1000);
    }
	#endregion
	#region ShowInObs - Shows something in OBS
	private void ShowInObs(string nomScene, string nomSource, string toShow, int duration) {
        Log("ShowInObs");
        Log($"  nomScene: {nomScene}");
        Log($"  nomSource: {nomSource}");
        Log($"  toShow: {toShow}");
        Log($"  duration: {duration.ToString()}");
        Log($"  Now: {DateTime.Now.ToString()}");
        Log($"  canPlayCommand: {CPH.GetGlobalVar<DateTime>("canPlayCommand").ToString()}");
        Log($"  Compare: {DateTime.Compare(DateTime.Now, CPH.GetGlobalVar<DateTime>("canPlayCommand"))}");
        try {
            if (DateTime.Compare(DateTime.Now, CPH.GetGlobalVar<DateTime>("canPlayCommand")) >= 0) {
                // TODO: Find a way to know which type of scene it is and act accordingly
                // Instead of just calling both commands

                CPH.ObsSetMediaSourceFile(nomScene, nomSource, toShow);
                CPH.ObsSetBrowserSource(nomScene, nomSource, toShow);
                CPH.ObsSetSourceVisibility(nomScene, nomSource, true);
                CPH.SetGlobalVar("canPlayCommand", DateTime.Now.AddMilliseconds(duration));
                CPH.Wait(duration);
                CPH.ObsSetSourceVisibility(nomScene, nomSource, false);
            }
        }
        catch(Exception e) {
            Log($"ERROR: {e.ToString()}");
        }
	}
    #endregion
    #region Log - Logs a line
    private void Log(string line) {
        File.AppendAllText($"{pathLOG}\\{DateTime.Now.Year.ToString("0000")}-{DateTime.Now.Month.ToString("00")}-{DateTime.Now.Day.ToString("00")}.log", DateTime.Now.ToString("hh:mm tt") + " | " + line + "\n");
    }
	#endregion
	#region VerifyFiles - Checks if the basic paths exists, if not, create all necessary files and folders
	// Usefull for the first run
	private void VerifyFiles() {
        pathLOG = $"{pathMAIN}\\Logs\\";
        pathTXT = $"{pathMAIN}\\TXTs\\";
        pathSFX = $"{pathMAIN}\\SFXs\\";
        pathGFX = $"{pathMAIN}\\GFXs\\";
        pathDATA = $"{pathMAIN}\\Data\\";
        pathEVENTS = $"{pathMAIN}\\Events\\";
        pathALERTS = $"{pathEVENTS}Alerts\\";
        pathREDEEM = $"{pathEVENTS}Redeems\\";
        pathTIMERS = $"{pathEVENTS}Timers\\";
        pathQUOTES = $"{pathTXT}quote\\";

		if (!Directory.Exists(pathMAIN)) {
			// Generates the required folders
			Directory.CreateDirectory(pathMAIN);
			Directory.CreateDirectory(pathLOG);
            Log("Generating necessary folders and files..");
            Log($" {pathMAIN}");
            Log($" {pathLOG}");
			Directory.CreateDirectory(pathTXT);
            Log($" {pathTXT}");
			File.WriteAllText($"{pathTXT}commands.txt", "Here are the available commands: {cmds}");
            Log($"  {pathTXT}commands.txt");
			File.WriteAllText($"{pathTXT}collab.txt", "{collab;Currently no collab.}");
            Log($"  {pathTXT}collab.txt");
			Directory.CreateDirectory($"{pathTXT}mod");
            Log($"  {pathTXT}mod");
			File.WriteAllText($"{pathTXT}mod\\addcmd.txt", "{addcmd}");
            Log($"   {pathTXT}mod\\addcmd.txt");
			File.WriteAllText($"{pathTXT}mod\\rmcmd.txt", "{rmcmd}");
            Log($"   {pathTXT}mod\\rmcmd.txt");
			File.WriteAllText($"{pathTXT}mod\\so.txt", "Shoutout to https://twitch.tv/{a;1}!!\n{shoutout;{a;1}}");
            Log($"   {pathTXT}mod\\so.txt");
			File.WriteAllText($"{pathTXT}mod\\collabstart.txt", "{collabstart;{rom}}Collab started, click here to see everyone: {collab}");
            Log($"   {pathTXT}mod\\collabstart.txt");
			File.WriteAllText($"{pathTXT}mod\\collabstop.txt", "{collabstop}Collab ended! :(");
            Log($"   {pathTXT}mod\\collabstop.txt");
			Directory.CreateDirectory($"{pathQUOTES}");
            Log($"  {pathTXT}quote");
            Directory.CreateDirectory($"{pathTXT}vip");
            Log($"  {pathTXT}vip");
            Directory.CreateDirectory($"{pathTXT}sub");
            Log($"  {pathTXT}sub");
			Directory.CreateDirectory(pathSFX);
            Log($" {pathSFX}");
            Directory.CreateDirectory($"{pathSFX}vip");
            Log($"  {pathSFX}vip");
            Directory.CreateDirectory($"{pathSFX}sub");
            Log($"  {pathSFX}sub");
			Directory.CreateDirectory(pathGFX);
            Log($" {pathGFX}");
            Directory.CreateDirectory($"{pathGFX}vip");
            Log($"  {pathGFX}vip");
            Directory.CreateDirectory($"{pathGFX}sub");
            Log($"  {pathGFX}sub");
			Directory.CreateDirectory(pathDATA);
            Log($" {pathDATA}");
			File.WriteAllText($"{pathDATA}cooldowns.txt", "");
            Log($"  {pathDATA}cooldowns.txt");
            File.WriteAllText($"{pathDATA}version.txt", "");
            Log($"  {pathDATA}version.txt");
			Directory.CreateDirectory(pathEVENTS);
            Log($" {pathEVENTS}");
			Directory.CreateDirectory($"{pathEVENTS}twitch\\");
            Log($" {pathEVENTS}twitch");
			Directory.CreateDirectory($"{pathEVENTS}youtube\\");
            Log($" {pathEVENTS}youtube");
            File.WriteAllText($"{pathEVENTS}{args["eventSource"].ToString()}\\Cheer.txt", "");
            Log($"  {pathEVENTS}{args["eventSource"].ToString()}\\Cheer.txt");
            File.WriteAllText($"{pathEVENTS}{args["eventSource"].ToString()}\\Follow.txt", "");
            Log($"  {pathEVENTS}{args["eventSource"].ToString()}\\Follow.txt");
            File.WriteAllText($"{pathEVENTS}{args["eventSource"].ToString()}\\Gift Subscription.txt", "");
            Log($"  {pathEVENTS}{args["eventSource"].ToString()}\\Gift Subscription.txt");
            File.WriteAllText($"{pathEVENTS}{args["eventSource"].ToString()}\\Hype Train Start.txt", "");
            Log($"  {pathEVENTS}{args["eventSource"].ToString()}\\Hype Train Start.txt");
            File.WriteAllText($"{pathEVENTS}{args["eventSource"].ToString()}\\Hype Train End.txt", "");
            Log($"  {pathEVENTS}{args["eventSource"].ToString()}\\Hype Train End.txt");
            File.WriteAllText($"{pathEVENTS}{args["eventSource"].ToString()}\\Raid.txt", "");
            Log($"  {pathEVENTS}{args["eventSource"].ToString()}\\Raid.txt");
            File.WriteAllText($"{pathEVENTS}{args["eventSource"].ToString()}\\Resubscription.txt", "");
            Log($"  {pathEVENTS}{args["eventSource"].ToString()}\\Resubscription.txt");
            File.WriteAllText($"{pathEVENTS}{args["eventSource"].ToString()}\\Subscription.txt", "");
            Log($"  {pathEVENTS}{args["eventSource"].ToString()}\\Subscription.txt");
			Directory.CreateDirectory(pathALERTS);
            Log($" {pathALERTS}");
			Directory.CreateDirectory($"{pathALERTS}Viewers");
            Log($"  {pathALERTS}Viewers");
            Directory.CreateDirectory(pathREDEEM);
            Log($" {pathREDEEM}");
            Directory.CreateDirectory(pathTIMERS);
            Log($" {pathTIMERS}");
			
			Log("Done.");
		}
	}
    #endregion
#endregion
}
