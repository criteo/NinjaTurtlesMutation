@echo off
if exist tozip rd -s -q tozip
if not exist tozip md tozip
cd tozip
copy /Y ..\NinjaTurtles\bin\Debug\*.* .
copy /Y ..\NinjaTurtles.Console\bin\Debug\*.* .
del *.zip