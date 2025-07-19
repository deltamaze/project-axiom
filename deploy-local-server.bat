@echo off
echo Building and deploying Server to LocalMultiplayerAgent...
dotnet-script DeployLocalServer.cs
if %ERRORLEVEL% EQU 0 (
    echo.
    echo Deployment successful! Starting LocalMultiplayerAgent...
    cd /d "C:\Main\Tools\LocalMultiplayerAgent"
    start LocalMultiplayerAgent.exe
) else (
    echo.
    echo Deployment failed. Check the error messages above.
    pause
)
