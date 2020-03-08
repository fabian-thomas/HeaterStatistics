set APPDATA=%CD%\WebServer\AppData\Roaming>nul
start WebServer\fenix.exe
start /D DataProcessing DataProcessing\HeaterListener.exe