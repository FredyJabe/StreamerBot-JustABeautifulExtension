using System;
using System.IO;

public class CPHInline
{
	private string likeFile = @"D:\Stream\Data\likes.txt";
	private string pathALERT = @"D:\Stream\Alertes\";

	public bool Execute()
	{
		int likes = int.Parse(args["likeCount"].ToString());
		int oldLikes = int.Parse(File.ReadAllText(likeFile));

		// Writes the new like count
        using (StreamWriter writer = new StreamWriter(likeFile)) {
            writer.WriteLine(likes);
        }

		if (oldLikes < likes) {
			CPH.PlaySound(pathALERT + "like.wav");
		}

		return true;
	}
}
