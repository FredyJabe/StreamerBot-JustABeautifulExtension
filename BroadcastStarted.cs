using System;
using System.IO;

public class CPHInline
{
    private string webhookUrl = "WEBHOOK_URL";
    private string content = "<@&986710953611636746> Mais si c'est pas le Frèdé qui est LIVE toé chose!\nhttps://www.youtube.com/@FredyJabe/live";
    private string username = "Fredy NEWS";
    
	public bool Execute()
	{
        // Warns people that we're live!
        CPH.DiscordPostTextToWebhook(webhookUrl, content, username);
        
        // Makes sure everytime we start the stream, the credits resets
        CPH.ResetCredits();

        // Adds all my supporters to the credits
        foreach( string l in File.ReadAllLines(@"D:\Stream\Data\supporters.txt")) {
            CPH.AddToCredits("supporters", l, false);
        }

		return true;
	}
}
