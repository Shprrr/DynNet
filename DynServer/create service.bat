sc.exe create DynServer binpath="%~dp0DynServer.exe"
sc.exe failure DynServer reset=0 actions=restart/60000/restart/60000/restart/60000
pause