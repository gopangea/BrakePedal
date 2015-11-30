SOLUTIONFILE := src/BrakePedal.sln
TESTPROJECT := src/BrakePedal.Tests/BrakePedal.Tests.csproj
TESTDLL := src/BrakePedal.Tests/bin/Debug/BrakePedal.Tests.dll

COREPROJECT := src/BrakePedal/BrakePedal.csproj
HTTPPROJECT := src/BrakePedal.Http/BrakePedal.Http.csproj
REDISPROJECT := src/BrakePedal.Redis/BrakePedal.Redis.csproj

build:
	mono tools/nuget/nuget.exe restore $(SOLUTIONFILE)
	xbuild $(SOLUTIONFILE)

test:
	mono tools/nuget/nuget.exe restore $(SOLUTIONFILE)
	xbuild $(TESTPROJECT)
	# Must use -noshadow switch for the runner to work in Mono as of 4.2.1
	mono tools/xunit/xunit.console.exe $(TESTDLL) -noshadow

package-core:
	xbuild $(COREPROJECT) /p:Configuration=Release
	mono tools/nuget/nuget.exe Pack BrakePedal.nuspec

package-http:
	xbuild $(HTTPPROJECT) /p:Configuration=Release
	mono tools/nuget/nuget.exe Pack BrakePedal.Http.nuspec

package-redis:
	xbuild $(REDISPROJECT) /p:Configuration=Release
	mono tools/nuget/nuget.exe Pack BrakePedal.Redis.nuspec
