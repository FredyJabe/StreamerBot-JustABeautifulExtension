using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

public class CPHInline
{
    // OPTIONS
	private string pathMAIN, pathLOG, pathTXT, pathSFX, pathGFX, pathDATA, pathVIEWER, pathALERTS;

    private string user, userID, message, eventSource;
    private bool firstMessage, isVip, isSub, isModerator; 
    private bool hasOutput, isAction, isAnnounce, isTTS;
    private int viewerCount;

    public bool Execute() {
		VerifyFiles();
		
        // Start by setting the received message variables
        eventSource = args["triggerName"].ToString();

        switch(eventSource) {
            case "Chat Message":
                user = args["user"].ToString();
                userID = args["userId"].ToString();
                message = args["message"].ToString();
                firstMessage = (args["firstMessage"].ToString().ToLower() == "true") ? true : false;
                isVip = (args["isVip"].ToString().ToLower() == "true") ? true : false;
                isSub = (args["isSubscribed"].ToString().ToLower() == "true") ? true : false;
                isModerator = (args["isModerator"].ToString().ToLower() == "true") ? true : false;

                if (user.ToLower() == "jabenet") return false;
                Log($"{user}: {message}");

                // Checks if first message from a user
                List<string> usersThatSaidSomething = CPH.GetGlobalVar<List<string>>("usersThatSaidSomething");
                if (!usersThatSaidSomething.Contains(user)) {
                    string file = $"{CPH.GetGlobalVar<string>("pathAler")}Viewers\\{user.ToLower()}.mp3";
                    if (File.Exists(file)) {
                        int millisecondsToAdd = GetDuration(file);
                        ShowInObs(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceSFX"), $"{System.AppDomain.CurrentDomain.BaseDirectory}{file}", millisecondsToAdd);
                        CPH.SetGlobalVar("canPlayCommand", DateTime.Now.AddMilliseconds(millisecondsToAdd));
                    }
                    usersThatSaidSomething.Add(user);
                    CPH.SetGlobalVar("usersThatSaidSomething", usersThatSaidSomething);
                }
                
                // Checks if it's a command
                if (message.StartsWith("!")) {
                    string[] arguments = message.Split(' ');
                    string command = arguments[0].Replace("!","");
                    string commandPath = "";
                    int millisecondsToAdd = 0;

                    // Makes sure that it's a command and not an empty string, the empty string causes some random shits
                    if (command == "") return false;
                    
                    #region TXT
                    if (File.Exists(pathTXT + command + ".txt") && command != "mod" && command != "vip" && command != "sub") {
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
                        
                        Log(cmds[cmdToExecute]);

                        commandPath = cmds[cmdToExecute];
                        Log(commandPath);
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
                        ReadCommand(commandPath, arguments);
                    }

                    // Then deal with the price and cooldown for the executed command
                    int price = GetCommandPrice(command);

                    //if (DateTime.Compare(DateTime.Now, CPH.GetGlobalVar<DateTime>("canPlayCommand")) >= 0 && DateTime.Compare(DateTime.Now, CPH.GetGlobalVar<DateTime>("canPlayCommand" + command)) >= 0 && price <= GetUserPoints(userID)) {
                    if (DateTime.Compare(DateTime.Now, CPH.GetGlobalVar<DateTime>("canPlayCommand")) >= 0) {
                        // Charges the user X amount of points to call a command
                        //UpdateUserPoints(userID, -price); 

                        string sfx = pathSFX + command + ".mp3";
                        string gfx = pathGFX + command + ".mp4";
                        
                        #region SFX
                        if (File.Exists(sfx) && command != "vip" && command != "sub") {
                            // Is a SFX command
                            millisecondsToAdd = GetDuration(sfx);
                            ShowInObs(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceSFX"), $"{System.AppDomain.CurrentDomain.BaseDirectory}{sfx}", millisecondsToAdd);
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

                            ShowInObs(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceSFX"), $"{System.AppDomain.CurrentDomain.BaseDirectory}{cmds[cmdToExecute]}", millisecondsToAdd);
                        }
                        #endregion
                        #region GFX
                        else if (File.Exists(gfx) && command != "vip" && command != "sub") {
                            // Is a GFX command
                            int duration = GetDuration(gfx);

                            ShowInObs(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceGFX"), $"{System.AppDomain.CurrentDomain.BaseDirectory}{gfx}", duration);
                            millisecondsToAdd = duration;
                        }
                        #endregion

                        // Determines when the next command can be executed
                        //CPH.SetGlobalVar("canPlayCommand", DateTime.Now.AddMilliseconds(millisecondsToAdd));
                        //CPH.SetGlobalVar("canPlayCommand" + command, DateTime.Now.AddMilliseconds(millisecondsToAdd).AddSeconds(GetCooldown(command)));
                    }
                }
                else {
                    // When it's not a command, play a sound to make sure you don't miss the message
                    string chatMessageAlert = $"{pathALERTS}chat-notif.mp3";
                    if (File.Exists(chatMessageAlert)) {
                        CPH.PlaySound(chatMessageAlert);
                    }
                }
                break;
            case "TwitchRewardRedemption": break;
            case "Cheer": // When you receive cheers
                Log("TwitchCheer");

                int bits = (int)args["bits"];
                Log(bits.ToString());
                
                int currentBits = int.Parse(File.ReadAllText($"{pathDATA}bits.txt").Split(' ')[1]);
                currentBits += bits;
                
                if (currentBits >= 10000) {
                    currentBits -= 10000;
                    
                    using (StreamWriter writer = new StreamWriter($"{pathDATA}important.txt", true))
                        writer.WriteLine("BITS OBJECTIVE ACHIEVED");
                }
                
                using (StreamWriter writer = new StreamWriter($"{pathDATA}bits.txt"))
                        writer.WriteLine($"!bits: {currentBits.ToString()} / 10000");
                break;
            case "TwitchRaid":
                //
                break;
            case "Viewer Count Update": ViewerCountUpdate(args); break;
            // TODO: actions on timers (with delays?)
        }

        return true;
    }

    private void ReadCommand(string cmdFile, string[] arguments) {
        hasOutput = true;
		isAction = false;
		isAnnounce = false;
		isTTS = false;
        string[] lines = File.ReadAllLines(cmdFile);
        Dictionary<string, string> variables = new();

        // Reads every line from the command file
        for (int l=0; l<lines.Length; l++) {
            string output = lines[l];

            Log(output);
            bool inBalise = false;
            List<string> balise = new();
            int baliseAmt = 0;

            // TODO: mettre des commentaire ici
            foreach(char c in output) {
                if (c == '{') {
                    balise.Add($"");
                    if (inBalise) {
                        Log("Balise interne start");
                        baliseAmt ++;
                    }
                    else {
                        Log("Début balise");
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
                        Log($"Fin baliseinterne : {output}");
                        baliseAmt --;
                    }
                    else {
                        inBalise = false;

                        for(int i=(balise.Count-1); i>=0; i--) {
                            if (balise[i] != "") {
                                string tagHandled = HandleTag(balise[i], arguments, variables);
                                output = output.Replace(balise[i], tagHandled);
                            }
                        }

                        Log($"Fin balise: {output}");
                    }
                }
            }

            
            if (hasOutput && output != "") {
                // TODO: Check output's lenght and split it if necessary
				if (isAction) CPH.SendAction(output);
				else if (isAnnounce) CPH.TwitchAnnounce(output);
				else CPH.SendMessage(output);
            }
		}
    }

#region Methods
    // ChatMessage event
    private void ChatMessage(Dictionary<string,object> args) {
        
    }

    // ViewerCountUpdate event
    private void ViewerCountUpdate(Dictionary<string,object> args) {
        viewerCount = (int)args["viewerCount"];
        CPH.SetGlobalVar("currentViewerCount", viewerCount);
    }
    
    // Returns the cooldown for a specific command
    private int GetCooldown(string command) {
        int retVal = 0;

        try {
            string[] cmds = File.ReadAllLines(pathDATA + "cooldowns.txt");
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

    // Returns video duration in milliseconds
    private int GetDuration(string path) {
        var tfile = TagLib.File.Create(path);
        TimeSpan duration = tfile.Properties.Duration;
		
        return duration.Milliseconds + (duration.Seconds * 1000);
    }

    // Reads the price for a certain command
    private int GetCommandPrice(string command) {
        int retVal = 0;

        try {
            string[] cmds = File.ReadAllLines(pathDATA + "prices.txt");
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

    // Returns the amount of points the user have
    private int GetUserPoints(string uid) {
        string file = pathVIEWER + $"{uid}.txt";
        int retVal = 0;
        if (File.Exists(file)) {
            string value = File.ReadAllLines(file)[0];
            retVal = int.Parse(value);
        }
        return retVal;
    }

    // Updates the amount of points a user have
    private void UpdateUserPoints(string uid, int pts) {
        string file = pathVIEWER + $"{uid}.txt";

        // If the user exists, read his points and update
        int points = (File.Exists(file)) ? Int32.Parse(File.ReadAllLines(file)[0]) + pts : pts;
        
        // And saves the file again
        using (StreamWriter writer = new StreamWriter(file)) {
            writer.WriteLine(points.ToString());
        }
    }
	
	// Shows something in OBS
	private void ShowInObs(string nomScene, string nomSource, string toShow, int duration) {
        if (DateTime.Compare(DateTime.Now, CPH.GetGlobalVar<DateTime>("canPlayCommand")) >= 0) {
            // TODO: Find a way to know which type of scene it is and act accordingly
            // Instead of just calling both commands
            CPH.ObsSetSourceVisibility(nomScene, nomSource, true);
            CPH.ObsSetMediaSourceFile(nomScene, nomSource, toShow);
            CPH.ObsSetBrowserSource(nomScene, nomSource, toShow);
            CPH.SetGlobalVar("canPlayCommand", DateTime.Now.AddMilliseconds(duration));
            CPH.Wait(duration);
            CPH.ObsSetSourceVisibility(nomScene, nomSource, false);
        }
        
	}

    // Handles the tags management
    private string HandleTag(string _tag, string[] arguments, Dictionary<string, string> variables) {
        string retVal = "";
        string[] tag = _tag.Substring(1, _tag.Length - 2).Split(';');

        switch(tag[0]) {
            // Managing all possible variables in commands
            case "a": // Returns à specific argument
                int arg = int.Parse(tag[1]);
                if (arguments.Length >= arg){
                    retVal = arguments[arg];
                }
                break;
            case "r": // Returns a random number between 1 and X (defaults at 6)
                if (tag.Length > 1) {
                    int randomMax = Int32.Parse(tag[1]);
                    retVal = CPH.Between(1, randomMax).ToString();
                }
                else {
                    retVal = CPH.Between(1, 6).ToString();
                }
                break;
            case "rom": // Returns the rest of the message
                if (arguments.Length > 0) {
                    for(var i = 1; i < arguments.Length; i ++) {
                        retVal += arguments[i] + " ";
                    }
                }
                break;
            case "tts": // Make the bot say stuff.. VERBALLY
                if (tag.Length > 1) {
                    string tts = "";
                    for(var i=1; i<tag.Length; i++) {
                        tts += tag[i] + " ";
                    }
                    CPH.TtsSpeak("test", tts);
                }
                break;
            case "user": // Returns the user's name
                retVal = user;
                break;
            case "noOutput": // Prevents the command to output anything
                hasOutput = false;
                break;
            case "w": // Wait for X milliseconds
                if (tag.Length == 2) {
                    Thread.Sleep(Int32.Parse(tag[1]));
                }
                break;
            case "first": // The FIRST ONE
                if (tag.Length == 3) {
                    if (CPH.GetGlobalVar<string>("first") == null) {
                        CPH.SetGlobalVar("first", user);
                        retVal = tag[1];
                    }
                    else {
                        retVal = tag[2];
                    }
                }
                break;
            case "note": // Let's viewers save notes for the streamer
                if (arguments.Length > 0) {
                    string txt = "";
                    for(var i = 1; i < arguments.Length; i ++) {
                        txt += arguments[i] + " ";
                    }

                    using (StreamWriter writer = new StreamWriter(pathDATA + "notes.txt", true))
                        writer.WriteLine($"[{DateTime.Now.Day}-{DateTime.Now.Month}] {user}: {txt}");
                }
                break;
            case "webrequest": // Executes à webrequest
                if (tag.Length == 2) {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{tag[1]}");
                    request.Accept = "text/plain";

                    StreamReader objReader = new StreamReader(request.GetResponse().GetResponseStream());
                    retVal = objReader.ReadLine();
                }
                break;
            case "announce": // Uses the Twitch Announce command
                isAnnounce = true;
                break;
            case "tAction": // Uses the Twitch action command
                isAction = true;
                break;
            case "action": // Executes another StreamerBot Action
                if (tag.Length == 2) {
                    if (CPH.ActionExists(tag[1])) {
                        CPH.RunAction(tag[1], true);
                    }
                }
                break;
            case "clip": // Fetches target's clip within the last X most recent 
                // {clip:info:target:amount}
                string target = "";
                int amount = 10;
                if (tag.Length > 2) {
                    target = tag[2];
                    if (tag.Length == 4) {
                        Int32.TryParse(tag[3], out amount);
                    }
                    Random r = new();
                    var clips = CPH.GetClipsForUser(target, amount);
                    var clip = clips[r.Next(clips.Count)];
                    
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
                break;
            case "embed": // Embed a url to OBS
                // {embed:url:duration}
                if (tag.Length == 3) {
                    ShowInObs(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceEmbed"), $"{tag[1]}", (int)(float.Parse(tag[2]) * 1000f));
                }
                break;
            case "collab": // Shows collaboration link
                Log("Collab");
                if (tag.Length == 2) {
                    string collabLink = CPH.GetGlobalVar<string>("collabLink");
                    retVal = (collabLink == null) ? tag[1] : collabLink;
                }
                break;
            case "addquote": // Adds a quote to the quote folder
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
                break;
            case "getGlobalVar": // Reads StreamerBot GlobalVar
                Log("getGlobalVar");
                Log($"{tag.Length}");
                if (tag.Length == 2) {
                    retVal = CPH.GetGlobalVar<string>(tag[1]);
                }
                break;
            case "setGlobalVar": // Sets StreamerBot GlobalVar
                if (tag.Length == 3) {
                    CPH.SetGlobalVar(tag[1], tag[2]);
                }
                break;
            case "getLocalVar": // Reads current command local variable
                Log("getLocalVar");
                Log($"{tag.Length}");
                if (tag.Length == 2) {
                    Log($"{tag[1]}");
                    retVal = variables[tag[1]];
                }
                break;
            case "setLocalVar": // Sets current command local variable
                Log("setLocalVar");
                Log($"{tag.Length}");
                if (tag.Length == 3) {
                    Log($"{tag[1]}={tag[2]}");
                    variables.Add(tag[1], tag[2]);
                }
                break;
            case "collabstart": // Starts a collab
                if (tag.Length == 2) {
                    string collabUrl = $"https://multitwitch.tv/{args["broadcastUserName"]}/{tag[1].Replace(" ","/")}";
                    CPH.SetGlobalVar("collabLink", collabUrl);
                    retVal = collabUrl;
                }
                break;
            case "collabstop": // Stops a collab
                if (tag.Length == 1) {
                    CPH.SetGlobalVar("collabLink", null);
                }
                break;
            case "shoutout": // Uses the Twitch Shoutout command
                if (tag.Length == 2) {
                    CPH.TwitchSendShoutoutByLogin(tag[1].Replace("@", ""));
                }
                break;
            case "addcmd": // Add a command... FROM A COMMAND
                string commandText = "";
                for(var i = 2; i < arguments.Length; i ++) {
                    commandText += arguments[i] + " ";
                }
                
                File.WriteAllText($"{pathTXT}{arguments[1].ToLower()}.txt", commandText);
                break;
            case "rmcmd": // Remove a command... FROM A COMMAND
                if (arguments.Length > 1) {
                    string commandName = arguments[1].ToLower();
                    if (File.Exists($"{pathTXT}{commandName}.txt")) {
                        File.Delete($"{pathTXT}{commandName}.txt");
                    }
                }
                break;
            case "resetCd": // Resets the cooldown for commands
                CPH.SetGlobalVar("canPlayCommand", DateTime.Now);
                break;
            case "urlSafe": // Encodes a string to be safe to write in a URL
                if (tag.Length == 2) {
                    retVal = CPH.UrlEncode(tag[1]);
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
                if (tag.Length == 2) {
                    string sfxToPlay = tag[1];
                    if (File.Exists(sfxToPlay)) {
                        ShowInObs(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceSFX"), $"{sfxToPlay}", GetDuration(sfxToPlay));
                    }
                }
                break;
            case "playGfx": // Play a video file
                if (tag.Length == 2) {
                    string gfxToPlay = tag[1];
                    if (File.Exists(gfxToPlay)) {
                        ShowInObs(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceGFX"), $"{gfxToPlay}", GetDuration(gfxToPlay));
                    }
                }
                break;
            case "sbPath": // Returns the path where StreamerBot is
                if (tag.Length == 1) {
                    retVal = System.AppDomain.CurrentDomain.BaseDirectory;
                }
                break;
            // Listing stuff
            case "cmds": // Lists all available text commands
                //string[] cmds = Directory.GetFiles(pathTXT, "*.txt");
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
                break;
            case "sfx": // Lists all available SFXs
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
                break;
            case "gfx": // Lists all available GFXs
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
                break;
            // Custom Commands
            case "roulette": // 6 chances, 1 bullet
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
                break;
            case "readJson": // Outputs a Json node
            // {readJson;json;node[;node[;node]]}
                switch(tag.Length) {
                    case 3: retVal = JObject.Parse(tag[1]).GetValue(tag[2]).ToString(); break;
                    case 4: retVal = JObject.Parse(JObject.Parse(tag[1]).GetValue(tag[2]).ToString()).GetValue(tag[3]).ToString(); break;
                    case 5: retVal = JObject.Parse(JObject.Parse(JObject.Parse(tag[1]).GetValue(tag[2]).ToString()).GetValue(tag[3]).ToString()).GetValue(tag[4]).ToString(); break;
                }
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

    // Logs a line
    private void Log(string line) {
        File.AppendAllText($"{pathLOG}\\{DateTime.Now.ToShortDateString()}.log", DateTime.Now.ToString("hh:mm tt") + " | " + line + "\n");
    }
	
	// Checks if the paths exists, if not, create all necessary files and folders
	// Usefull for the first run
	private void VerifyFiles() {
		pathMAIN = CPH.GetGlobalVar<string>("pathMain");
		pathLOG = CPH.GetGlobalVar<string>("pathLogs");
		pathTXT = CPH.GetGlobalVar<string>("pathTXTs");
		pathSFX = CPH.GetGlobalVar<string>("pathSFXs");
		pathGFX = CPH.GetGlobalVar<string>("pathGFXs");
		pathDATA = CPH.GetGlobalVar<string>("pathData");
		pathVIEWER = CPH.GetGlobalVar<string>("pathView");
		pathALERTS = CPH.GetGlobalVar<string>("pathAler");
		
		if (!Directory.Exists(pathMAIN)) {
			// Generates the required folders
			Directory.CreateDirectory(pathMAIN);
			Directory.CreateDirectory(pathLOG);
			Directory.CreateDirectory(pathTXT);
			Directory.CreateDirectory($"{pathTXT}mod");
			Directory.CreateDirectory($"{pathTXT}quote");
            Directory.CreateDirectory($"{pathTXT}vip");
            Directory.CreateDirectory($"{pathTXT}sub");
			Directory.CreateDirectory(pathSFX);
            Directory.CreateDirectory($"{pathSFX}vip");
            Directory.CreateDirectory($"{pathSFX}sub");
			Directory.CreateDirectory(pathGFX);
            Directory.CreateDirectory($"{pathGFX}vip");
            Directory.CreateDirectory($"{pathGFX}sub");
			Directory.CreateDirectory(pathDATA);
			Directory.CreateDirectory(pathVIEWER);
			Directory.CreateDirectory(pathALERTS);
			
			Log("Generating necessary folders and files..");
			
			// And continues pis creating base files
			File.WriteAllText($"{pathTXT}mod\\addcmd.txt", "{addcommand}");
			File.WriteAllText($"{pathTXT}mod\\rmcmd.txt", "{addcommand}");
			File.WriteAllText($"{pathTXT}mod\\so.txt", "Shoutout to https://twitch.tv/{a;1}!!\n{shoutout}{a;1}");
			File.WriteAllText($"{pathTXT}mod\\collabstart.txt", "{collabstart}{rom}");
			File.WriteAllText($"{pathTXT}mod\\collabstop.txt", "{collabstop}");
			File.WriteAllText($"{pathTXT}commands.txt", "Here are the available commands: {cmds}");
			File.WriteAllText($"{pathTXT}collab.txt", "{collab}");
			File.WriteAllText($"{pathDATA}cooldowns.txt", "");
			File.WriteAllText($"{pathDATA}exercices.txt", "");
			File.WriteAllText($"{pathDATA}prices.txt", "");
			File.WriteAllText($"{pathDATA}bits.txt", "");
			File.WriteAllText($"{pathDATA}important.txt", "");
			
			Log("Done.");
		}
	}
#endregion
}
