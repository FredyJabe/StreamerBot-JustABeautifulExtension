using System;
using System.IO;
using System.Net;
using System.Web;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

using Newtonsoft.Json.Linq;

public class CPHInline
{
    // OPTIONS
	private string pathMAIN, pathLOG, pathTXT, pathSFX, pathGFX, pathDATA, pathVIEWER, pathALERTS;

    private string user, userID, message, source;
    private bool isModerator; 

    public bool Execute() {
		VerifyFiles();
		
        // Start by setting the received message variables
        user = args["user"].ToString();
        userID = args["userId"].ToString();
        message = args["message"].ToString();
        source = args["eventSource"].ToString();
        isModerator = (args["isModerator"].ToString().ToLower() == "true") ? true : false;

        //CPH.LogInfo($"{user}: {message}");
        Log($"{user}: {message}");
		
        // Checks if it's a command
        if (message.StartsWith("!")) {
            string[] arguments = message.Split(' ');
            string command = arguments[0].Replace("!","");
            string commandPath = "";
            int millisecondsToAdd = 0;
            
            #region TXT
            if (File.Exists(pathTXT + command + ".txt") && command != "mod") {
                // Is a TXT command
                commandPath = pathTXT + command + ".txt";
            }
            #endregion
            #region Random TXT
            else if (Directory.Exists(pathTXT + command) && command != "mod") {
                // Is a TXT command but runs a random file in that folder
                Random r = new Random();
                string[] cmds = Directory.GetFiles(pathTXT + command);
                int cmdToExecute = r.Next(cmds.Length);
				
				Log(cmds[cmdToExecute]);

                //ReadCommand(pathTXT + command + @"\" + cmds[cmdToExecute] + ".txt", arguments);
                commandPath = cmds[cmdToExecute];
                //commandPath = pathTXT + command + @"\" + cmds[cmdToExecute] + ".txt";
				Log(commandPath);
            }
            #endregion
            #region Moderator Only TXT
            // Moderator only commands
            else if (File.Exists(pathTXT + @"mod\" + command + ".txt") && isModerator) {
                // Is a TXT command but runs a random file in that folder
                commandPath = pathTXT + @"mod\" + command + ".txt";
                //ReadCommand(pathTXT + @"mod\" + command + ".txt", arguments);
            }
            #endregion
            if (commandPath != "") {
                //List<string> output = ReadCommand(commandPath, arguments);
                ReadCommand(commandPath, arguments);
            }

            // Then deal with the price and cooldown for the executed command
            int price = GetCommandPrice(command);

            if (DateTime.Compare(DateTime.Now, CPH.GetGlobalVar<DateTime>("canPlayCommand")) >= 0 && DateTime.Compare(DateTime.Now, CPH.GetGlobalVar<DateTime>("canPlayCommand" + command)) >= 0 && price <= GetUserPoints(userID)) {
                
                // Charges the user X amount of points to call a command
                UpdateUserPoints(userID, -price); 

				string sfx = pathSFX + command + ".mp3";
                string gfx = pathGFX + command + ".mp4";
				
                #region SFX
                if (File.Exists(sfx)) {
                    // Is a SFX command
                    millisecondsToAdd = GetDuration(sfx);
                    CPH.ObsSetSourceVisibility(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceSFX"), false);
                    CPH.ObsSetMediaSourceFile(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceSFX"), $"{System.AppDomain.CurrentDomain.BaseDirectory}{sfx}");
                    CPH.ObsSetSourceVisibility(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceSFX"), true);
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

                    CPH.ObsSetSourceVisibility(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceSFX"), false);
                    CPH.ObsSetMediaSourceFile(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceSFX"), $"{System.AppDomain.CurrentDomain.BaseDirectory}{cmds[cmdToExecute]}");
                    CPH.ObsSetSourceVisibility(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceSFX"), true);
                }
                #endregion
                #region GFX
                else if (File.Exists(gfx)) {
                    // Is a GFX command
                    int duration = GetDuration(gfx);

					ShowInObs(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceGFX"), $"{System.AppDomain.CurrentDomain.BaseDirectory}{gfx}", duration);
                    millisecondsToAdd = duration;
                }
                #endregion

                // Determines when the next command can be executed
                CPH.SetGlobalVar("canPlayCommand", DateTime.Now.AddMilliseconds(millisecondsToAdd));
                CPH.SetGlobalVar("canPlayCommand" + command, DateTime.Now.AddMilliseconds(millisecondsToAdd).AddSeconds(GetCooldown(command)));
            }
        }
		else {
			// When it's not a command, play a sound to make sure you don't miss the message
			string chatMessageAlert = $"{pathALERTS}chat-notif.mp3";
			if (File.Exists(chatMessageAlert)) {
				CPH.PlaySound(chatMessageAlert);
			}
		}

        return true;
    }

    private void ReadCommand(string cmdFile, string[] arguments) {
        bool hasOutput = true;
		bool isAction = false;
		bool isAnnounce = false;
		bool isTTS = false;
        string[] lines = File.ReadAllLines(cmdFile);

        //foreach(string l in lines) {
        for (int l=0; l<lines.Length; l++) {
            string output = lines[l];

            // Managing all possible variables in commands
            #region args - Returns a specific argument
            for(var i = 1; i < arguments.Length; i ++) {
                if (output.Contains("{" + i.ToString() + "}")) {
					if (output.Contains("{webrequest}") && !cmdFile.Contains("addcmd")) {
						output = output.Replace("{" + i.ToString() + "}", CPH.UrlEncode(arguments[i]));
					}
					else {
						output = output.Replace("{" + i.ToString() + "}", arguments[i]);
					}
                }
            }
            #endregion
            #region rom - Returns the rest of the message
            if (output.Contains("{rom}")) {
                string rom = "";
                for(var i = 1; i < arguments.Length; i ++) {
                    rom += arguments[i] + " ";
                }
				
				if (output.Contains("{webrequest}") && !cmdFile.Contains("addcmd")) {
					output = output.Replace("{rom}", CPH.UrlEncode(rom));
				}
				else {
					output = output.Replace("{rom}", rom);
				}
            }
            #endregion
			#region tts - Make the bot say stuff
			if (output.Contains("{tts}")) {
				output = output.Replace("{tts}","");
				isTTS = true;
			}
			#endregion
            #region user - Returns the user name
            if (output.Contains("{user}")) {
                output = output.Replace("{user}", user);
            }
            #endregion
            #region noOutput - Removes any output the command has
            if (output.Contains("{noOutput}")) {
                hasOutput = false;
            }
            #endregion
            #region w - Wait for X milliseconds
            if (output.Contains("{w}")) {
                Log("WAIT");
                int t = Int32.Parse(output.Replace("{w}", ""));
                Log(t.ToString());
                //CPH.Wait(t);
                Thread.Sleep(t);
                output = "";
            }
            #endregion
            #region r - Random from 1 to X
            if (output.Contains("{r}")) {
                int randomMax = Int32.Parse(output.Replace("{r}", ""));
                output = CPH.Between(1, randomMax).ToString();
            }
            #endregion
            #region points - Reads and returns user points
            if (output.Contains("{points}")) {
                output = output.Replace("{points}", GetUserPoints(userID).ToString());
            }
            #endregion
            #region first - The FIRST ONE
            if (output.Contains("{first}")) {
                if (CPH.GetGlobalVar<string>("first") == null) {
                     CPH.SetGlobalVar("first", user);
                     CPH.AddToCredits("first", user, false);
                     UpdateUserPoints(userID, 15);
                     output = output.Substring(0, output.IndexOf("{first}"));
                }
                else {
                    output = output.Substring(output.IndexOf("{first}") + 7);
                }
            }
            #endregion
            #region note - Let's viewers save notes for the streamer
            if (output.Contains("{note}")) {
                string txt = "";

                for(var i = 1; i < arguments.Length; i ++) {
                    txt += arguments[i] + " ";
                }

                using (StreamWriter writer = new StreamWriter(pathDATA + "notes.txt", true))
                    writer.WriteLine($"[{DateTime.Now.Day}-{DateTime.Now.Month}] {user}: {txt}");
            }
            #endregion
			#region webrequest - Executes à webrequest
			if (output.Contains("{webrequest}")) {
				string rUrl = output.Replace("{webrequest}", "");
				
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create((rUrl.IndexOf("{") != -1) ? rUrl.Substring(0, rUrl.IndexOf("{")) : rUrl);
				request.Accept = "text/plain";

				StreamReader objReader = new StreamReader(request.GetResponse().GetResponseStream());
				output = output.Replace($"{request.Address}", "").Replace("{webrequest}", objReader.ReadLine());
			}
			#endregion
			#region announce - Uses the Twitch Announce command
			if (output.Contains("{announce}")) {
				output = output.Replace("{announce}", "");
				isAnnounce = true;
			}
			#endregion
			#region action - Uses the Twitch Shoutout command
			if (output.Contains("{action}")) {
					output = output.Replace("{action}", "");
					isAction = true;
			}
			#endregion
			#region clip - Fetches target's clip
			if (output.Contains("{clip}")) {
				string target = output.Replace("{clip}", "").Replace("@", "");
				output = output.Replace(target, "");
		
				Random r = new();
				
				var clips = CPH.GetClipsForUser(target, 10);
				var clip = clips[r.Next(clips.Count)];
				
				output = output.Replace("{clip}", $"{clip.Title} : {clip.Url}");
			}
			#endregion
			#region embed - Embed a url to OBS
			if (output.Contains("{embed}")) {
				string target = output.Replace("{embed}", "").Replace("@", "");
				output = output.Replace("{embed}", "").Replace(target, "");
		
				Random r = new();
				
				var clips = CPH.GetClipsForUser(target, 10);
				var clip = clips[r.Next(clips.Count)];
				
				ShowInObs(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceEmbed"), $"{clip.EmbedUrl}&parent=localhost&autoplay=true", (int)(clip.Duration * 1000f));
			}
			#endregion
			#region exercices
			if (output.Contains("{exercices}")) {
				output = output.Replace("{exercices}", "");
				string[] linesToWrite = { "situps 0", "squats 0", "pushups 0"};
				File.WriteAllLines($"{pathDATA}exercices.txt", linesToWrite);
			}
			#endregion
			#region addquote - Adds a quote to the quote folder
			if (output.Contains("{addquote}")) {
				output = output.Replace("{addquote}", "");
				hasOutput = false;
				int quotesAmount = Directory.GetFiles(pathTXT + "quote").Length;
				
				using (StreamWriter writer = new StreamWriter($"{pathTXT}quote\\{quotesAmount + 1}.txt", false))
                    writer.WriteLine($" {output} - {DateTime.Now.Day.ToString("00")}/{DateTime.Now.Month.ToString("00")}");
			}
			#endregion
            #region run - Execute a file
                if (output.Contains("{run}")) {
                    string file = output.Replace("{run}", "");
                    //TODO Finish the run function
                }
            #endregion
			#region collab - Shows collaboration link
			if (output.Contains("{collab}")) {
				output = output.Replace("{collab}", CPH.GetGlobalVar<string>("collabLink"));
			}
			#endregion
			#region collabstart - MOD - Starts a collab
			if (output.Contains("{collabstart}")) {
				output = output.Replace("{collabstart}", "");
				string collabUrl = $"https://multitwitch.tv/{args["broadcastUserName"].ToString()}";
				
				foreach(string s in output.Split(' ')) {
					collabUrl += $"/{s}";
				}
				
				CPH.SetGlobalVar("collabLink", collabUrl);
				output = $"Collab starté! {collabUrl}";
			}
			#endregion
			#region collabstop - MOD - Stops a collab
			if (output.Contains("{collabstop}")) {
				output = "Collab stoppée!";
				CPH.SetGlobalVar("collabLink", "");
			}
			#endregion
			#region shoutout - MOD - Uses the twitch Shoutout command
			if (output.Contains("{shoutout}")) {
					output = output.Replace("{shoutout}", "");
					if (output != "") {
						CPH.TwitchSendShoutoutByLogin(output.Replace("@", ""));
						output = "";
					}
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
            #region resetCD - MOD - Resets the cooldown of a specific command
            if (output.Contains("{resetcd}")) {
                if (arguments.Length > 0) {
                    CPH.SetGlobalVar("canPlayCommand" + arguments[1], DateTime.Now);
                }
            }
            #endregion
            #region addcmd - MOD - Add a command... FROM A COMMAND
			if (output.Contains("{addcommand}")) {
				output = output.Replace("{addcommand}", "");
				string commandName = arguments[1].ToLower();
				string commandText = "";
				for(var i = 2; i < arguments.Length; i ++) {
                    commandText += arguments[i] + " ";
                }
				
				File.WriteAllText($"{pathTXT}{commandName}.txt", commandText);
			}
            #endregion
            #region rmcmd - MOD - Remove a command... FROM A COMMAND
			if (output.Contains("{removecommand}")) {
				output = output.Replace("{removecommand}", "");
				if (arguments.Length > 1) {
					string commandName = arguments[1].ToLower();
					if (File.Exists($"{pathTXT}{commandName}.txt")) {
						File.Delete($"{pathTXT}{commandName}.txt");
					}
				}
			}
            #endregion

            // Lists stuff
            #region cmds - Lists all available text commands
            if (output.Contains(@"{cmds}")) {
                output = output.Replace(@"{cmds}", " ");
                string[] cmds = Directory.GetFiles(pathTXT, "*.txt");
                foreach(string c in cmds) {
                    int i = c.Split('\\').Length;
                    output += c.Split('\\')[i-1].Replace(".txt"," ");
                }
            }
            #endregion
            #region sfx - Lists all available SFXs
            if (output.Contains("{sfx}")) {
                output = output.Replace("{sfx}", " ");
                
                //string[] cmds = Directory.GetFiles(pathSFX);
                string[] cmds = Directory.GetFileSystemEntries(pathSFX);
                foreach(string c in cmds) {
                    int i = c.Split('\\').Length;
                    output += c.Split('\\')[i-1].Replace(".mp3","") + " ";
                }
            }
            #endregion
            #region gfx - Lists all available GFXs
            if (output.Contains("{gfx}")) {
                output = output.Replace("{gfx}", " ");
                string[] cmds = Directory.GetFiles(pathGFX);
                foreach(string c in cmds) {
                    int i = c.Split('\\').Length;
                    output += c.Split('\\')[i-1].Replace(".mp4"," ");
                }
            }
            #endregion

            // Custom commands
            #region roulette - 6 chances, 1 bullet
            if (output.Contains("{roulette}")) {
                output = output.Replace("{roulette}", "");

                int chanceRoulette = CPH.GetGlobalVar<int>("chanceRoulette");

                if (CPH.Between(1, chanceRoulette) == 1) {
                    chanceRoulette = 6;
                    output = $"explose la tronche de {user}!!";
                }
                else {
                    chanceRoulette --;
                    output = $"tire à blanc... il reste {chanceRoulette} chances...";
                }

                CPH.SetGlobalVar("chanceRoulette", chanceRoulette);
            }
            #endregion
			#region gif - shows a random gif depending on the parameter
			if (output.Contains("{randomgif}")) {
				output = output.Replace("{randomgif}", "");
				
				string embed_url = JObject.Parse(JObject.Parse(output).GetValue("data").ToString()).GetValue("embed_url").ToString();
				
				CPH.SetGlobalVar("canPlayCommandgif", DateTime.Now.AddMilliseconds(15000));
				
				ShowInObs(CPH.GetGlobalVar<string>("obsSceneEffects"), CPH.GetGlobalVar<string>("obsSourceGFX"), embed_url, 5000);
			}
			#endregion

            // Outputs ALL available commands for users to a textfile
            #region outputCommandList - DEBUG - Outputs every commands with cooldowns and prices
            if (output.Contains("{outputTextfile}")) {
                hasOutput = false;
                output = "";

                string[] folders = { pathTXT, pathSFX, pathGFX };
                foreach(string p in folders) {
                    string[] cmds = Directory.GetFiles(p);
                    output += "#### " + p + " ####\n";
                    foreach(string c in cmds) {
                        int i = c.Split('\\').Length;
                        string cmd = c.Split('\\')[i-1].Split('.')[0];
                        string price = GetCommandPrice(cmd).ToString();
                        string cd = GetCooldown(cmd).ToString();
                        output += cmd + $" (cd:{cd} p:{price})\n";
                    }
                }

                File.WriteAllText(pathDATA + "TEST.txt", output);
            }
            #endregion

            
            if (hasOutput && output != "") {
				if (isAction) CPH.SendAction(output);
				else if (isAnnounce) CPH.TwitchAnnounce(output);
				else if (isTTS) CPH.TtsSpeak("test", output);
				else CPH.SendMessage(output);
            }
		}
    }

#region Methods
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
		CPH.ObsSetBrowserSource(nomScene, nomSource, toShow);
		CPH.ObsSetSourceVisibility(nomScene, nomSource, true);
		
		CPH.Wait(duration);
		CPH.ObsSetSourceVisibility(nomScene, nomSource, false);
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
			Directory.CreateDirectory(pathSFX);
			Directory.CreateDirectory(pathGFX);
			Directory.CreateDirectory(pathDATA);
			Directory.CreateDirectory(pathVIEWER);
			Directory.CreateDirectory(pathALERTS);
			
			Log("Generating necessary folders and files..");
			
			// And continues pis creating base files
			File.WriteAllText($"{pathTXT}mod\\addcmd.txt", "{addcommand}");
			File.WriteAllText($"{pathTXT}mod\\rmcmd.txt", "{addcommand}");
			File.WriteAllText($"{pathTXT}mod\\so.txt", "Shoutout to https://twitch.tv/{1}!!\n{shoutout}{1}");
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
