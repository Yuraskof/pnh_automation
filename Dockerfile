FROM mcr.microsoft.com/playwright/dotnet:v1.59.0-noble AS test

ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH="${PATH}:/root/.dotnet/tools"

WORKDIR /src

RUN dotnet --list-sdks | grep -q '^10\.' \
    || (curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
    && bash /tmp/dotnet-install.sh --install-dir /usr/share/dotnet --channel 10.0 \
    && rm /tmp/dotnet-install.sh)

COPY ["pnh_automation.sln", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["src/PnhAutomation.Core/PnhAutomation.Core.csproj", "src/PnhAutomation.Core/"]
COPY ["tests/PnhAutomation.Tests/PnhAutomation.Tests.csproj", "tests/PnhAutomation.Tests/"]

RUN dotnet restore "pnh_automation.sln"

COPY . .

RUN dotnet build "tests/PnhAutomation.Tests/PnhAutomation.Tests.csproj" --no-restore \
    && pwsh ./tests/PnhAutomation.Tests/bin/Debug/net10.0/playwright.ps1 install chrome

ENTRYPOINT ["dotnet", "test", "pnh_automation.sln", "--no-restore", "--no-build"]
