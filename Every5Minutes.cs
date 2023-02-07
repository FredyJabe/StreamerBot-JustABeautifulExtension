using System;
using System.IO;
using System.Collections.Generic;

public class CPHInline
{
    private static string viewerFolder = @"D:\Stream\Data\Viewers";

	public bool Execute()
	{
        List<Dictionary<string,object>> users = (List<Dictionary<string,object>>)args["users"];

        // On écrit le nombre de viewers actuel pour pouvoir s'en servir sur le stream
        using (StreamWriter writer = new StreamWriter(@"E:\Stream\Data\viewerCount.txt")) {
            writer.WriteLine(users.Count);
        }

        // Increment & save the viewers points
        foreach(Dictionary<string,object> d in users) {
            string id = d["id"].ToString();
            string userFile = $"{viewerFolder}\\{id}.txt";

            // If the user exists, read his points and update
            int points = (File.Exists(userFile)) ? Int32.Parse(File.ReadAllLines(userFile)[0]) + 5 : 5;
            

            // And saves the file again
            using (StreamWriter writer = new StreamWriter(userFile)) {
                writer.WriteLine(points.ToString());
            }
        }

		return true;
	}
}
