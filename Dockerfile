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

FROM test AS debug

RUN set -eux; \
    sed -i \
        -e 's|http://archive.ubuntu.com/ubuntu|https://archive.ubuntu.com/ubuntu|g' \
        -e 's|http://security.ubuntu.com/ubuntu|https://security.ubuntu.com/ubuntu|g' \
        /etc/apt/sources.list /etc/apt/sources.list.d/*.sources 2>/dev/null || true; \
    apt-get \
        -o Acquire::Retries=5 \
        -o Acquire::http::Timeout=60 \
        -o Acquire::https::Timeout=60 \
        -o Acquire::http::Pipeline-Depth=0 \
        update; \
    apt-get \
        -o Acquire::Retries=5 \
        -o Acquire::http::Timeout=60 \
        -o Acquire::https::Timeout=60 \
        -o Acquire::http::Pipeline-Depth=0 \
        install -y --no-install-recommends \
        fluxbox \
        novnc \
        websockify \
        x11vnc \
        xvfb; \
    rm -rf /var/lib/apt/lists/*

COPY ["scripts/docker-debug-entrypoint.sh", "/usr/local/bin/pnh-docker-debug-entrypoint"]

RUN chmod +x /usr/local/bin/pnh-docker-debug-entrypoint

ENV DISPLAY=:99 \
    NOVNC_PORT=6080 \
    PNH_ARTIFACT_DIR=/src/TestResults/playwright \
    SCREEN_GEOMETRY=1440x1000x24 \
    VNC_PORT=5900

EXPOSE 6080 5900

ENTRYPOINT ["pnh-docker-debug-entrypoint"]
CMD ["--filter", "FullyQualifiedName~PublicHomeSmokeTests", "--logger", "console;verbosity=detailed"]

FROM test AS default
