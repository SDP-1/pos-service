@echo off
REM ===================================================
REM MySQL Backup Script (Encrypted Login Version)
REM ===================================================

REM --- mysql_config_editor set --login-path=backup_user --host=localhost --user=root --password ---

REM --- CONFIGURATION ---
REM Use double quotes for paths with spaces
SET "MYSQL_DATABASE=pos-system"
SET "MYSQLDUMP_PATH=C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe"
SET "BACKUP_FOLDER=D:\Auto backup"
SET "LOGIN_PATH=backup_user"

REM --- Create backup folder if it doesn't exist ---
IF NOT EXIST "%BACKUP_FOLDER%" (
    mkdir "%BACKUP_FOLDER%"
)

REM --- Generate filename with timestamp ---
REM This format: YYYY-MM-DD_HH-MM
SET "MYDATE=%date:~10,4%-%date:~4,2%-%date:~7,2%"
SET "MYTIME=%time:~0,2%-%time:~3,2%"
SET "MYTIME=%MYTIME: =0%"

SET "FILE_NAME=%MYSQL_DATABASE%_%MYDATE%_%MYTIME%.sql"
SET "FULL_PATH=%BACKUP_FOLDER%\%FILE_NAME%"

REM --- Run mysqldump ---
REM Note: No -u or -p needed because they are inside --login-path
"%MYSQLDUMP_PATH%" --login-path=%LOGIN_PATH% %MYSQL_DATABASE% --routines --triggers --single-transaction --result-file="%FULL_PATH%"

REM --- Log results ---
IF %ERRORLEVEL% EQU 0 (
    echo [%DATE% %TIME%] Success: Backup created at "%FULL_PATH%" >> "%BACKUP_FOLDER%\backup.log"
) ELSE (
    echo [%DATE% %TIME%] ERROR: Backup failed with Exit Code %ERRORLEVEL% >> "%BACKUP_FOLDER%\backup.log"
)

exit /b %ERRORLEVEL%