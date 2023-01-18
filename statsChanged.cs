using System;
using System.IO;
using System.Collections.Generic;

public class CPHInline
{
	public bool Execute()
	{
		//string follower = args["user"].ToString();
		string viewers = args["viewCount"].ToString();
        int messageToShow = 0;

		// On écrit le nombre de viewers actuel pour pouvoir s'en servir sur le stream
        using (StreamWriter writer = new StreamWriter(@"E:\Stream\Data\viewerCount.txt"))
        {
            writer.WriteLine(viewers);
        }

		return true;
	}
}