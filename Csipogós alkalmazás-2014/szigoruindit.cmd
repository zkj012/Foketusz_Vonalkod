setlocal EnableDelayedExpansion

net use t: /d /yes
net use t: \\srv-fs-02\Shared\Faliujsag\szigoru
c:
cd \zkj

ver | find "XP" > nul
if %ERRORLEVEL% == 0 goto xp


REM *** Windows Vista és utána ***
del /q c:\zkj\forfiles.exe
forfiles /P t:\ /M *.txt /C "cmd /c if not exist c:\zkj\@file c:\zkj\frissit.cmd"

goto kozos



:xp
REM *** Windows XP ***
copy /y t:\forfiles.exe c:\zkj
forfiles -pt:\ -m*.txt -v -c"CMD /C if NOT exist c:\ZKJ\@FILE frissit.cmd"


:kozos
cmd /c c:\zkj\Szigoru_sorszam_run.mdb
