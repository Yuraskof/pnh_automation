FROM mcr.microsoft.com/dotnet/sdk:10.0 AS test

WORKDIR /src

COPY ["pnh_automation.sln", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["src/PnhAutomation.Core/PnhAutomation.Core.csproj", "src/PnhAutomation.Core/"]
COPY ["tests/PnhAutomation.Tests/PnhAutomation.Tests.csproj", "tests/PnhAutomation.Tests/"]

RUN dotnet restore "pnh_automation.sln"

COPY . .

ENTRYPOINT ["dotnet", "test", "pnh_automation.sln", "--no-restore"]
