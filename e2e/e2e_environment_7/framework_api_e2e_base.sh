dotnet test \
 ./test/FrameworkAPI.E2E.Test/FrameworkAPI.E2E.Test.csproj \
 --test-adapter-path:. \
 --logger:"junit;LogFilePath=..\..\e2e\artifacts\{assembly}-test-result.xml;MethodFormat=Class;FailureBodyFormat=Verbose" \
 --logger:"console;verbosity=normal" \
 --filter: $1
 