REM from https://superuser.com/questions/1226025/how-to-redirect-appdata-roaming-to-a-subdirectory-to-make-an-application-more
set APPDATA=%CD%\WebServer\AppData\Roaming>nul
start WebServer\fenix.exe