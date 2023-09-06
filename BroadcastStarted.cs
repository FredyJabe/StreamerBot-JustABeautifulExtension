using System;
using System.IO;

public class CPHInline
{
	public bool Execute()
	{ 
		// Sets the general paths
		CPH.SetGlobalVar("pathMain", @"JabeChatCommands");
		CPH.SetGlobalVar("pathLogs", @"JabeChatCommands\Logs\");
		CPH.SetGlobalVar("pathTXTs", @"JabeChatCommands\TXTs\");
		CPH.SetGlobalVar("pathSFXs", @"JabeChatCommands\SFXs\");
		CPH.SetGlobalVar("pathGFXs", @"JabeChatCommands\GFXs\");
		CPH.SetGlobalVar("pathData", @"JabeChatCommands\Data\");
		CPH.SetGlobalVar("pathView", @"JabeChatCommands\Data\Viewers\");
		CPH.SetGlobalVar("pathAler", @"JabeChatCommands\Alerts\");
		
		CPH.SetGlobalVar("obsSceneEffects", "Component Overlay Effects");
		CPH.SetGlobalVar("obsSourceSFX", "SFX");
		CPH.SetGlobalVar("obsSourceGFX", "GFX");
		CPH.SetGlobalVar("obsSourceEmbed", "Embed");
	
	        // Resets the first and the roulette
	        CPH.SetGlobalVar("first", null);
	        CPH.SetGlobalVar("chanceRoulette", 6);
	        CPH.SetGlobalVar("collabLink", "");
	        //CPH.SetGlobalVar("currentVip", File.ReadAllText(""));

		return true;
	}
}
