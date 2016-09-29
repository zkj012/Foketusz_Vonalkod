del /q c:\zkj\*.txt
ver | find "XP" > nul
if %ERRORLEVEL% == 0 goto xpszekcio

if NOT %ERRORLEVEL% == 0 del /q c:\zkj\forfiles.exe

goto kozos

:xpszekcio
copy /y t:\forfiles.exe c:\zkj

:kozos
c:
cd \zkj
copy /y t:\mdb.zip c:\zkj
copy /y t:\vonalkodindit.cmd c:\zkj
copy /y t:\szigoruindit.cmd c:\zkj
copy /y t:\frissit.cmd c:\zkj
copy /y t:\urit.cmd c:\zkj
copy /y t:\csatol.cmd c:\zkj
copy /y t:\unzip.exe c:\zkj
c:\zkj\unzip -o c:\zkj\mdb.zip
copy t:\*.txt c:\zkj

