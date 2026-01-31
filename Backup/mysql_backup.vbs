Set WshShell = CreateObject("WScript.Shell")
WshShell.Run "D:\Selef Project\pos-service\Backup\mysql_backup.bat", 0, True
Set WshShell = Nothing