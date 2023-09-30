using System;
using System.IO;
using System.Collections.Generic;

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
		CPH.SetGlobalVar("pathRede", @"JabeChatCommands\Redeems\");
		CPH.SetGlobalVar("pathTime", @"JabeChatCommands\Timers\");
		
		// OBS stuff
		CPH.SetGlobalVar("obsSceneEffects", "Component Overlay Effects");
		CPH.SetGlobalVar("obsSourceSFX", "SFX");
		CPH.SetGlobalVar("obsSourceGFX", "GFX");
		CPH.SetGlobalVar("obsSourceEmbed", "Embed");
	
        // Resets the stream specific variables
        CPH.SetGlobalVar("first", null);
        CPH.SetGlobalVar("chanceRoulette", 5);
        CPH.SetGlobalVar("collabLink", null);
		CPH.SetGlobalVar("usersThatSaidSomething", new List<string>());
		CPH.SetGlobalVar("currentViewerCount", 0);

		return true;
	}
}
