@echo off
REM ===========================
REM MySQL Backup Script
REM ===========================

REM --- CONFIGURATION ---
SET MYSQL_USER=root
SET MYSQL_PASSWORD=1234
SET MYSQL_DATABASE=pos-system
SET MYSQLDUMP_PATH=D:\Manual_backup\mysqldump.exe
SET BACKUP_FOLDER=D:\Manual_backup
REM Folder to store backups

REM --- Create backup folder if it doesn't exist ---
IF NOT EXIST "%BACKUP_FOLDER%" (
    mkdir "%BACKUP_FOLDER%"
)

REM --- Generate filename with timestamp ---
FOR /F "tokens=1-4 delims=/ " %%i IN ('date /t') DO SET DATE=%%l-%%j-%%k
FOR /F "tokens=1-2 delims=: " %%i IN ('time /t') DO SET TIME=%%i-%%j
SET FILE_NAME=%MYSQL_DATABASE%_%DATE%_%TIME%.sql
SET FULL_PATH=%BACKUP_FOLDER%\%FILE_NAME%

REM --- Run mysqldump ---
"%MYSQLDUMP_PATH%" -u %MYSQL_USER% -p%MYSQL_PASSWORD% %MYSQL_DATABASE% --routines --triggers --single-transaction --result-file="%FULL_PATH%"

REM --- Optional: log success/failure ---
IF %ERRORLEVEL% EQU 0 (
    echo Backup succeeded: "%FULL_PATH%" >> "%BACKUP_FOLDER%\backup.log"
) ELSE (
    echo Backup FAILED: "%FULL_PATH%" >> "%BACKUP_FOLDER%\backup.log"
)

echo Backup completed.
pause
