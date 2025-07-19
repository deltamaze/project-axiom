@echo off
echo Starting Project Axiom Dedicated Server...

REM Set environment variables for local testing
set PF_TITLE_ID=
set GamePort=7777
set PF_SERVER_INSTANCE_NUMBER=local-1

REM Start the server
dotnet run --project project-axiom-server.csproj

pause
