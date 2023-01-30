using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

public class CPHInline
{
    private string pathLOG = @"E:\Stream\Logs\" + DateTime.Now.ToShortDateString() + ".log";
    private string pathTXT = @"E:\Stream\Commands\";
    private string pathSFX = @"E:\Stream\Sounds\";
    private string pathGFX = @"E:\Stream\Gifs\";
    private string pathDATA = @"E:\Stream\Data\";
    private string pathVIEWER = @"E:\Stream\Data\Viewers\";

    private string user, userID, message;
    private bool isModerator;

    public bool Execute() {
        // Start by setting the received message variables
        user = args["user"].ToString();
        userID = args["userId"].ToString();
        message = args["message"].ToString();
        isModerator = (args["isModerator"].ToString().ToLower() == "true") ? true : false;

        CPH.LogInfo($"{user}: {message}");

        // Checks if it's a command
        if (message.StartsWith("!")) {
            string[] arguments = message.Split(' ');
            string command = arguments[0].Replace("!","");
            int millisecondsToAdd = 0;
            CPH.LogInfo($"  {command}");
            
            #region TXT
            if (File.Exists(pathTXT + command + ".txt") && command != "mod") {
                // Is a TXT command
                ReadCommand(pathTXT + command + ".txt");
            }
            #endregion
            #region Random TXT
            else if (Directory.Exists(pathTXT + command)) {
                // Is a TXT command but runs a random file in that folder
                Random r = new Random();
                string[] cmds = Directory.GetFiles(pathTXT + command);
                int cmdToExecute = r.Next(cmds.Length);

                ReadCommand(pathTXT + command + @"\" + cmds[cmdToExecute] + ".txt");
            }
            #endregion
            #region Moderator Only TXT
            // Moderator only commands
            else if (File.Exists(pathTXT + @"mod\" + command + ".txt") && isModerator) {
                // Is a TXT command but runs a random file in that folder
            }
            #endregion

            int price = GetCommandPrice(command);

            if (DateTime.Compare(DateTime.Now, CPH.GetGlobalVar<DateTime>("canPlayCommand")) >= 0 && DateTime.Compare(DateTime.Now, CPH.GetGlobalVar<DateTime>("canPlayCommand" + command)) >= 0 && price <= GetUserPoints(userID)) {
                
                // Charges the user X amount of points to call a command
                UpdateUserPoints(userID, -price);

                #region SFX
                string sfx = pathSFX + command + ".mp3";
                string gfx = pathGFX + command + ".mp4";
                if (File.Exists(sfx)) {
                    // Is a SFX command
                    millisecondsToAdd = GetDuration(sfx);
                    CPH.PlaySound(sfx);
                }
                #endregion
                #region Random SFX
                else if (Directory.Exists(pathSFX + command)) {
                    // Is a SFX command but runs a random file in that folder
                    Random r = new Random();
                    string[] cmds = Directory.GetFiles(pathSFX + command);
                    int cmdToExecute = r.Next(cmds.Length);
                    CPH.LogDebug(cmds[cmdToExecute]);
                    millisecondsToAdd = GetDuration(cmds[cmdToExecute]);
                    CPH.PlaySound(cmds[cmdToExecute]);
                }
                #endregion
                #region GFX
                else if (File.Exists(gfx)) {
                    // Is a GFX command
                    int duration = GetDuration(gfx);

                    CPH.ObsSetBrowserSource("Component Overlay Effects", "Gifs", gfx);
                    CPH.ObsSetSourceVisibility("Component Overlay Effects", "Gifs", true);
                    CPH.Wait(duration);
                    CPH.ObsSetSourceVisibility("Component Overlay Effects", "Gifs", false);
                    millisecondsToAdd = duration;
                }
                #endregion

                // Determines when the next command can be executed
                CPH.SetGlobalVar("canPlayCommand", DateTime.Now.AddMilliseconds(millisecondsToAdd));
                CPH.SetGlobalVar("canPlayCommand" + command, DateTime.Now.AddMilliseconds(millisecondsToAdd).AddSeconds(GetCooldown(command)));
            }
        }

        return true;
    }

    private void ReadCommand(string cmdFile) {
        bool hasOutput = true;
        string[] lines = File.ReadAllLines(cmdFile);

        //foreach(string l in lines) {
        for (int l=0; l<lines.Length; l++) {
            string output = lines[l];

            // Returns the user name
            if (output.Contains("{sender}")) {
                output = output.Replace("{sender}", user);
            }
            // Prevents the command to output anything
            if (output.Contains("{noOutput}")) {
                hasOutput = false;
            }
            // Makes the command wait for X milliseconds
            if (output.Contains("{w}")) {
                Log("WAIT");
                int t = Int32.Parse(output.Replace("{w}", ""));
                Log(t.ToString());
                CPH.Wait(t);
                output = "";
            }
            // Reads and returns user points
            if (output.Contains("{points}")) {
                output = output.Replace("{points}", GetUserPoints(userID).ToString());
            }
            // The FIRST ONE
            if (output.Contains("{first}")) {
                if (CPH.GetGlobalVar<string>("first") == null) {
                     CPH.SetGlobalVar("first", user);
                     CPH.AddToCredits("first", user, false);
                }
            }

            // Lists stuff
            if (output.Contains(@"{cmds}")) {
                output = output.Replace(@"{cmds}", " ");
                string[] cmds = Directory.GetFiles(pathTXT, "*.txt");
                foreach(string c in cmds) {
                    int i = c.Split('\\').Length;
                    output += "!" + c.Split('\\')[i-1].Replace(".txt"," ");
                }
            }
            if (output.Contains("{sfx}")) {
                output = output.Replace("{sfx}", " ");
                string[] cmds = Directory.GetFiles(pathSFX);
                foreach(string c in cmds) {
                    int i = c.Split('\\').Length;
                    output += "!" + c.Split('\\')[i-1].Replace(".mp3"," ");
                }
            }
            if (output.Contains("{gfx}")) {
                output = output.Replace("{gfx}", " ");
                string[] cmds = Directory.GetFiles(pathGFX);
                foreach(string c in cmds) {
                    int i = c.Split('\\').Length;
                    output += "!" + c.Split('\\')[i-1].Replace(".mp4"," ");
                }
            }

            if (hasOutput) {
                CPH.SendYouTubeMessage(output);
            }
        }
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

    private void Log(string line) {
        File.AppendAllText(pathLOG, DateTime.Now.ToShortTimeString() + " | " + line + "\n");
    }
}