# StreamerBot-DLL

A little DLL that im using with StreamerBot to manage all the commands in my streams.

Not really a DLL yet because of how StreamerBot works, so every file is the action linked to a single event.
Will check options to transform into a working DLL eventually.
I really need to check if it's possible to call CPH methods from a DLL..

## What does it do?
The system let's the streamer creates new chat commands as easily as acreating a new text file! Also enables SFXs and GFX to be added as simply as a new file in a folder.

## Which platform can I use that with?
Currently, it's only setup to work with YouTube, but I can easily make some changes to enable Twitch too if interest there is.

## How can I use that?
It's super simple to use, create 5 actions with a single `Execute C# Code` sub-action in all of them! For the C# that you'll need, every file in the repo represents an action, so simply copy-paste what you want! Now we need to link these Actions with actual events, to do so, simply go the the Platforms tab and assign your actions to their respective events!

Screenshots and more information coming soon

*Don't mind the `dll-stuff` folder, it's a work in progress*

## Problems?
If you find any bug, have a feature request or anything like, feel free to open an Issue here on the repo and I'll see what I can do to fix it.