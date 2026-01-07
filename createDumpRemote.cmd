@echo off
rem ***********************************************************************
rem This cmd file connects to a remote docker host and creats a dumpfile.
rem After that the debug file will opened with lldb to analyse it
rem This expects the following format
rem createDumpRemote <hostname>
rem                   Hostname of the remote docker host
rem ***********************************************************************
"C:\Program Files\Docker\Docker\resources\bin\Docker.exe" ^
--tls --tlscacert="%APPDATA%\Docker\certs.d\ca.pem" --tlscert="%APPDATA%\Docker\certs.d\cert.pem" --tlskey="%APPDATA%\Docker\certs.d\key.pem" ^
-H %1:2376 exec -it isp_frameworkapi ^
bash -c "export DOTNETVER=$(ls /usr/share/dotnet/shared/Microsoft.NETCore.App/); /usr/share/dotnet/shared/Microsoft.NETCore.App/$DOTNETVER/createdump 1; echo \"lldb-3.9 -O 'settings set target.exec-search-paths /usr/share/dotnet/shared/Microsoft.NETCore.App/$DOTNETVER' -o 'plugin load /usr/share/dotnet/shared/Microsoft.NETCore.App/$DOTNETVER/libsosplugin.so' --core /tmp/coredump.1 /usr/bin/dotnet\" >lldb.sh; chmod +x lldb.sh; ./lldb.sh"