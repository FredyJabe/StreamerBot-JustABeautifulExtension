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

    private DateTime canPlayCommand = DateTime.Now;

    private string user, userID, message;
    private bool isModerator;

    public bool Execute() {
        // Start by setting the received message variables
        user = args["user"].ToString();
        userID = args["userId"].ToString();
        message = args["message"].ToString();
        isModerator = (args["isModerator"].ToString().ToLower() == "true") ? true : false;

        Log("user: " + user);
        Log("userID: " + userID);
        Log("message: " + message);

        // Checks if it's a command
        if (message.StartsWith("!")) {
            string[] arguments = message.Split(' ');
            string command = arguments[0].Replace("!","");
            int millisecondsToAdd = 0;

            Log("command: " + command);
            
            #region TXT
            Log(pathTXT + command + ".txt");
            Log(pathTXT + @"mod\" + command + ".txt");
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

            Log("now: " + DateTime.Now.ToString());
            Log("time: " + canPlayCommand.ToString());

            if (DateTime.Now >= canPlayCommand) {
                #region SFX
                if (File.Exists(pathSFX + command + ".mp3")) {
                    // Is a SFX command
                    millisecondsToAdd = GetDuration(pathSFX + command + ".mp3");
                }
                #endregion
                #region Random SFX
                else if (Directory.Exists(pathSFX + command)) {
                    // Is a SFX command but runs a random file in that folder
                    Random r = new Random();
                    string[] cmds = Directory.GetFiles(pathSFX + command);
                    int cmdToExecute = r.Next(cmds.Length);
                    millisecondsToAdd = GetDuration(cmds[cmdToExecute]);
                }
                #endregion
                #region GFX
                else if (File.Exists(pathGFX + command + ".txt")) {
                    // Is a GFX command
                }
                #endregion

                // Determines when the next command can be executed
                canPlayCommand = DateTime.Now.AddMilliseconds(millisecondsToAdd);
            }
        }

        return true;
    }

    private void ReadCommand(string cmdFile) {
        Log("Command received");
        bool hasOutput = true;
        string[] lines = File.ReadAllLines(cmdFile);

        foreach(string l in lines) {
            if (l.Contains("{user}")) {
                l.Replace("{user}", user);
                Log("  {user} -> " + user);
            }
            if (l.Contains("{noOutput")) {
                hasOutput = false;
                Log("  No output");
            }
            if (l.Contains("{w}")) {
                int t = Int32.Parse(l.Replace("{w}", ""));
                Log("  Wait " + t.ToString() + "ms");
                CPH.Wait(t);
                hasOutput = false;
            }

            if (hasOutput) {
                Log("    Sending message: " + l);
                //CPH.SendYouTubeMessage(l);
                CPH.SendMessage(l);
            }
        }
    }

    // Returns video duration in milliseconds
    private int GetDuration(string path)
    {
        var tfile = TagLib.File.Create(path);
        TimeSpan duration = tfile.Properties.Duration;
        return duration.Milliseconds + (duration.Seconds * 1000);
    }

    private void Log(string line) {
        //using(TextWriter tw = new StreamWriter(pathLOG, true)) {
        //    tw.WriteLine(DateTime.Now.ToShortTimeString() + " | " + line);
        //}
        File.AppendAllText(pathLOG, DateTime.Now.ToShortTimeString() + " | " + line + "\n");
        //using (StreamWriter writer = new StreamWriter(pathLOG, true)) {
        //    writer.WriteLine(DateTime.Now.ToShortTimeString() + " | " + line);
        //}
    }

    /*

    private static int chanceRoulette = 6;

    public bool Execute()
    {
        try
        {

            // Then check the command
            if (wholeMessage.StartsWith("!"))
            {
                string cmd = wholeMessage.Split(' ')[0].Substring(1).ToLower();
                commands = Directory.GetFiles(commandsFolder);
                gifs = Directory.GetFiles(gifsFolder);
                var snds = new List<string>();
                snds.AddRange(Directory.GetFiles(soundsFolder));
                snds.AddRange(Directory.GetDirectories(soundsFolder));
                sounds = snds.ToArray();

                // Text commands
                AddLog("Loading chat commands");
                for (var i = 0; i < commands.Length; i++) commands[i] = commands[i].Split('\\')[3].Split('.')[0];

                // Sound commands
                AddLog("Loading sound commands");
                for (var i = 0; i < sounds.Length; i++) sounds[i] = sounds[i].Split('\\')[3].Split('.')[0];

                // Gif commands
                AddLog("Loading gif commands");
                for (var i = 0; i < gifs.Length; i++) gifs[i] = gifs[i].Split('\\')[3].Split('.')[0];
                AddLog($"Loaded {commands.Length + sounds.Length + gifs.Length} commands");

                // Checks if the command actually exists
                if (modo && File.Exists($"{commandsFolder}\\mod\\{cmd}.txt"))
                {
                    AddLog("Mod command used");
                    AddLog($"  {cmd}");
                    ReadCommand($"{commandsFolder}\\mod\\{cmd}.txt", wholeMessage);
                }
                else if (Array.IndexOf(commands, cmd) >= 0)
                {
                    AddLog("Command used");
                    AddLog($"  {cmd}");
                    ReadCommand($"{commandsFolder}\\{cmd}.txt", wholeMessage);
                }
                // Commandes sonores
                else if (Array.IndexOf(sounds, cmd) >= 0)
                {
                    AddLog("Sound played");
                    AddLog($"  {cmd}");
                    if (canPlaySoundOrGif)
                    {
                        canPlaySoundOrGif = false;
                        switch (cmd)
                        {
                            case "plaisir":
                                if (canPlayPlaisir)
                                {
                                    canPlayPlaisir = false;
                                    CPH.PlaySound($"{soundsFolder}\\{cmd}.ogg", 0.3F, true);
                                    canPlaySoundOrGif = true;

                                    plaisirCooldown.Start();

                                    //CPH.Wait(300000);
                                    //canPlayPlaisir = true;
                                }
                                //else CPH.SendMessage("/me plaisir est en cooldown!");
                                else
                                {
                                    SendToChat("Plaisir est en cooldown.");
                                }
                                break;
                            default:
                                if (Directory.Exists($"{soundsFolder}\\{cmd}"))
                                {
                                    CPH.PlaySoundFromFolder($"{soundsFolder}\\{cmd}", 0.5F, false, true);
                                }
                                else
                                {
                                    // On vérifie les multiples extensions
                                    string[] exts = { "ogg","mp3", "wav" };
                                    string ext = "";
                                    foreach(string e in exts)
                                    {
                                        if (File.Exists($"{soundsFolder}\\{cmd}.{e}"))
                                        {
                                            ext = e;
                                            break;
                                        }
                                    }

                                    CPH.PlaySound($"{soundsFolder}\\{cmd}.{ext}", 0.5F, true);
                                }

                                break;
                        }

                        canPlaySoundOrGif = true;
                    }
                    //else CPH.SendMessage("/me un sfx/gfx est déjà en cours.");
                    else
                    {
                        SendToChat("Un SFX/GFX est déjà en cours.");
                    }
                }
                else if (Array.IndexOf(gifs, cmd) >= 0)
                {
                    if (canPlaySoundOrGif)
                    {
                        // On vérifie les multiples extensions
                        string[] exts = { "webm","mp4","avi" };
                        string ext = "";
                        foreach(string e in exts)
                        {
                            if (File.Exists($"{gifsFolder}\\{cmd}.{e}"))
                            {
                                ext = e;
                                break;
                            }
                        }

                        canPlaySoundOrGif = false;
                        CPH.ObsSetBrowserSource("Component Overlay Effects", "Gifs", $"{gifsFolder}\\{cmd}.{ext}");
                        CPH.ObsSetSourceVisibility("Component Overlay Effects", "Gifs", true);
                        CPH.Wait(GetVideoDuration($"{gifsFolder}\\{cmd}.{ext}"));
                        CPH.ObsSetSourceVisibility("Component Overlay Effects", "Gifs", false);
                        canPlaySoundOrGif = true;
                    }
                    //else CPH.SendMessage("/me un sfx/gfx est déjà en cours.");
                    else
                    {
                        SendToChat("Un SFX/GFX est déjà en cours.");
                    }
                }

                AddLog("Command execution complete");
            }
            else
            {
                // Incremente les viewer points si c'est pas une commande
                UpdateUserPoints(1);
            }
        }
        catch (Exception e)
        {
            AddLog("ERROR");
            AddLog(e.ToString());
        }
        finally
        {
            using (StreamWriter writer = new StreamWriter(logFile, false))
            {
                writer.WriteLine(logContent);
            }
        }

        return true;
    }

    // Log une ligne
    public static void AddLog(string t)
    {
        logContent += t + "\n";
    }

    // Methode pour lire et traiter les ficheirs de commandes
    private void ReadCommand(string cmdFile, string wholeText)
    {
        string output = "";
        bool hasOutput = true;
        string[] arg = wholeText.Split(' ');
        string toAdd = "";
        string sender = user;
        string[] lines = File.ReadAllLines(cmdFile);
        for (var l = 0; l < lines.Length; l++)
        {
            string line = lines[l];
            // Empêche le bot de répondre
            if (line.Contains("{noOutput}")) hasOutput = false;
            // Affiche le nom de la personne qui a fait la commande
            if (line.Contains("{sender}")) line = line.Replace("{sender}", sender);
            // Retourne le nombre de points de l'utilisateur
            if (line.Contains("{points}")) line = line.Replace("{points}", GetUserPoints().ToString());
            // Commande custom de roulette russe
            if (line.Contains("{customRoulette}"))
            {
                // On le fait juste si pas Modo ou +, parce que ça enlève le role!
                if (role < 3 && source == "twitch") CommandCustomRoulette(sender);
            }
            // Commande custom pour changer le label en haut du stream
            if (line.Contains("{changeText}")) CommandChangeText(wholeText.Substring(arg[0].Length));
            // Commande custom pour prendre des notes
            if (line.Contains("{note}")) CommandTakeNote(sender, wholeText.Substring(arg[0].Length));
            // Affiche la liste des commandes disponibles
            if (line.Contains("{cmds}"))
            {
                string cmdList = "";
                foreach (string c in commands)
                    cmdList += $"!{c} ";
                line = line.Replace("{cmds}", cmdList);
            }
            // Effectue PLEINS de bruits de pets
            if (line.Contains("{massFart}")) CommandMassFart();
            // Gère toutes les fonctionnalités d'un giveaway
            if (line.Contains("{giveAway}")) CommandGiveAway(arg);

            // Affiche la liste des SFX disponible
            if (line.Contains("{sounds}"))
            {
                string sndsList = "";
                foreach (string s in sounds)
                    sndsList += "!" + s.Split('.')[0] + " ";
                line = line.Replace("{sounds}", sndsList);
            }

            // Affiche la liste des gifs disponible
            if (line.Contains("{gifs}"))
            {
                string gifsList = "";
                foreach (string g in gifs)
                    gifsList += "!" + g.Split('.')[0] + " ";
                line = line.Replace("{gifs}", gifsList);
            }

            // Change les arguments dynamiquement
            for (var i = 1; i <= arg.Length; i++)
            {
                if (line.Contains("{" + i.ToString() + "}"))
                    line = line.Replace("{" + i.ToString() + "}", arg[i].Replace("@", ""));
            }

            // Wait un nombre de milisecondes
            if (line.Contains("{w}"))
            {
                line = line.Replace("{w}", "");
                CPH.Wait(Int32.Parse(line));
                //CPH.SendMessage(lines[l + 1]);
				SendToChat(lines[l+1]);
                line = "";
                hasOutput = false;
            }

            output += line;
        }

        if (hasOutput)
        {
            //CPH.SendMessage(output);
            SendToChat(output);
        }
    }

    // Commande custom : Roulette
    private void CommandCustomRoulette(string sender)
    {
        //CPH.SendMessage($"/me pointe l'arme vers {sender} et appuie sur la détente...");
        SendToChat($"On point l'arme sur {sender} et appuie sur la détente...");
        CPH.Wait(1000);
        if (CPH.Between(1, chanceRoulette) == 1)
        {
            chanceRoulette = 6;
            SendToChat("POW! Timeout 69 secondes!");
            SendToChat($"/timeout {sender} 69 Roulette Russe");
            //CPH.SendMessage("/me tire et TO pour 69 secondes!");
            //CPH.SendMessage($"/timeout {sender} 69 Roulette Russe");
        }
        else
        {
            chanceRoulette--;
            SendToChat("C'est un tir à blanc...");
            //CPH.SendMessage("/me tire à blanc...");
        }
    }

    // Commande text
    private void CommandChangeText(string txt)
    {
        var filePath = dataFolder + "\\label.txt";
        using (StreamWriter writer = new StreamWriter(filePath))
            writer.WriteLine(txt);
    }

    // Commande text
    private void CommandTakeNote(string sender, string txt)
    {
        var filePath = dataFolder + "\\notes.txt";
        using (StreamWriter writer = new StreamWriter(filePath, true))
            writer.WriteLine(sender + ": " + txt);
    }

    // Commande de massfart
    private void CommandMassFart()
    {
        if (canPlaySoundOrGif)
        {
            canPlaySoundOrGif = false;
            int amount = Int32.Parse(File.ReadAllLines(@"E:\Stream\Data\viewerCount.txt")[0]) * 2;

            for(var i=0; i<amount; i++)
            {
                CPH.PlaySoundFromFolder($"{soundsFolder}/fart");
                CPH.Wait(CPH.Between(250, 1000));
            }

            canPlaySoundOrGif = true;
        }
    }

    // Giveaway!!
    private void CommandGiveAway(string[] arguments)
    {
        bool currentlyActive = CPH.GetGlobalVar<bool>("giveawayActive");
        List<string> participants = CPH.GetGlobalVar<List<string>>("participantsGiveaway");

        if (arguments.Length > 1)
        {
            switch(arguments[1])
            {
                case "start":
                    if (role >= 3)
                    {
                        if (currentlyActive)
                        {
                            //CPH.SendMessage("/me Un tirage déjà en cours!");
                            SendToChat("Un tirage déjà en cours!");
                        }
                        else
                        {
                            CPH.SetGlobalVar("giveawayActive", true);
                            //CPH.SendMessage($"/me Un tirage vient de commencer, tapes \"{arguments[0]} X\" ou X est le nombre de billets que tu souhaites acheter! (10 JabeCoins par billet)");
                            SendToChat($"Un tirage vient de commencer, tapes \"{arguments[0]} X\" ou X est le nombre de billets que tu souhaites acheter! (10 JabeCoins par billet)");

                            CPH.SetGlobalVar("participantsGiveaway", new List<string>());
                        }
                    }
                    else
                    {
                        //CPH.SendMessage("/me Seul les mods peuvent starter un tirage!");
                        SendToChat("Seul les mods peuvent starter un tirage!");
                    }
                    break;
                case "pick":
                    if (role >= 3)
                    {
                        if (currentlyActive)
                        {
                            int qtyWinner = 1;
                            int chosenWinners = 0;
                            if (arguments.Length > 2) Int32.TryParse(arguments[2], out qtyWinner);

                            // Choose the winner(s) and make sure that nobody wins more than once per giveaway
                            string winner = "";
                            if (participants.Count > 0) 
                            {
                                while(participants.Count != 0 && chosenWinners != qtyWinner)
                                {
                                    int winnerCheckId = CPH.Between(0,participants.Count-1);
                                    string tmpWinner = participants[winnerCheckId];

                                    if (winner.Contains(tmpWinner)) participants.RemoveAt(winnerCheckId);
                                    else
                                    {
                                        winner += (chosenWinners == 0) ? tmpWinner : ", " + tmpWinner;
                                        chosenWinners ++;
                                    }
                                }
                            }
                            // Makes sure there's at least 1 winner and if none, say so
                            if (winner != "")
                            {
                                if (qtyWinner == 1)
                                {
                                    //CPH.SendMessage($"/me {winner} est l'heureux gagnant du tirage!");
                                    SendToChat($"{winner} est l'heureux gagnant du tirage!");
                                }
                                else
                                {
                                    //CPH.SendMessage($"/me Les gagnants du tirage sont {winner}!");
                                    SendToChat($"Les gagnants du tirage sont {winner}!");
                                }
                            }
                            else
                            {
                                //CPH.SendMessage("/me Pas assez de participants pour déterminer un gagnant :(");
                                SendToChat("Pas assez de participants pour déterminer un gagnant :(");
                            }

                            CPH.SetGlobalVar("giveawayActive", false);
                        }
                        else
                        {
                            //CPH.SendMessage($"/me Aucun tirage n'est en cours.");
                            SendToChat($"Aucun tirage n'est en cours.");
                        }
                    }
                    else
                    {
                        //CPH.SendMessage($"/me Un tirage est en cours, tapes \"{arguments[0]} X\" ou X est le nombre de billets que tu souhaites acheter! (10 JabeCoins par billet)");
                        SendToChat($"Un tirage est en cours, tapes \"{arguments[0]} X\" ou X est le nombre de billets que tu souhaites acheter! (10 JabeCoins par billet)");
                    }
                    break;
                case "remove":
                    if (currentlyActive)
                    {
                        int amt = 0;

                        if (arguments.Length > 2)
                        {

                            while(participants.Contains(arguments[2]))
                            {
                                amt += 1;
                                participants.Remove(arguments[2]);
                            }

                            CPH.SetGlobalVar("participantsGiveaway", participants);
                        }

                        UpdateUserPoints(amt * 10, arguments[2]);
                        //CPH.SendMessage($"/me {amt} billets ont été remboursés.");
                        SendToChat($"{amt} billets ont été remboursés.");
                    }
                    break;
                default: // Acheter des billets
                    string msg = "";
                    if (currentlyActive)
                    {
                        int billets = 0;
                        if (Int32.TryParse(arguments[1], out billets))
                        {
                            if (billets > 0)
                            {
                                int pts = GetUserPoints();
                                if (pts >= (billets * 10))
                                {
                                    UpdateUserPoints(-(billets*10));
                                    for(var b=0; b<billets; b++) participants.Add(user); 
                                    
                                    int totalBillets = 0;
                                    foreach(string p in participants)
                                    {
                                        if (p == user) totalBillets ++;
                                    }

                                    CPH.SetGlobalVar("participantsGiveaway", participants);
                                    msg = $"{user} a {totalBillets} billets! (+{billets})";
                                }
                                else msg = $"{user} n'a pas assez de JabeCoins! ({pts}/{billets*10})";
                            }
                            else msg = $"Tu dois acheter au moins 1 billet, {user}!";
                        }
                        else msg = $"Tu peux juste acheter un \"nombre\" de billet, {user}!";
                    }
                    else msg = "Aucun tirage n'est en cours.";

                    //CPH.SendMessage(msg);
                    SendToChat(msg);
                    break;
            }
        }
        else
        {
            string answer = "Aucun tirage n'est actif pour le moment.";
            if (currentlyActive) answer = $"Un tirage est en cours, tapes \"{arguments[0]} X\" ou X est le nombre de billets que tu souhaites acheter! (10 JabeCoins par billet)";

            //CPH.SendMessage(answer);
            SendToChat(answer);
        }
    }

    // Command to know the amount of points you have
    private int GetUserPoints()
    {
        int retVal = 0;

        string userFile = $"{viewerFolder}\\{user}.txt";
        if (File.Exists(userFile))
        {
            // Userfile format:
            // 0 : points
            retVal = Int32.Parse(File.ReadAllLines(userFile)[0]);
        }

        return retVal;
    }

    // Adds or Removes X points to user
    private void UpdateUserPoints(int changement, string u = "")
    {
        u = (u == "") ? user : u;

        string userFile = $"{viewerFolder}\\{u}.txt";
        int points = changement;
        // If the user exists, read his points and update
        if (File.Exists(userFile))
        {
            // Userfile format:
            // 0 : points
            points = Int32.Parse(File.ReadAllLines(userFile)[0]) + changement;
        }

        // And saves the file again
        using (StreamWriter writer = new StreamWriter(userFile))
        {
            writer.WriteLine(points.ToString());
        }
    }

    // Retourne la durée en millisecondes d'un video
    private int GetVideoDuration(string path)
    {
        var tfile = TagLib.File.Create(path);
        TimeSpan duration = tfile.Properties.Duration;
        return duration.Milliseconds + (duration.Seconds * 1000);
    }

    // Generic cooldown for the SFXs/GFXs
    public static void GenericEffectCooldown(int ms) 
    {
        Thread.Sleep(ms);
        canPlaySoundOrGif = true;
    }

    // Cooldown for the PLAISIR SFX
    public static void PlaisirEffectCooldown() 
    {
        Thread.Sleep(300000);
        canPlayPlaisir = true;
    }

    private void SendToChat(string msg)
    {
        switch(source)
        {
            case "twitch":
                CPH.SendMessage(msg);
                break;
            case "youtube":
                CPH.SendYouTubeMessage(msg);
                break;
        }
    }
    */
}
