@ECHO OFF
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /r:../References/TagLibSharp.dll,../References/System.Data.SQLite.dll /target:library /out:./output/JabeDll.dll *.cs
