#!/usr/bin/env bash
set -euo pipefail

export DISPLAY="${DISPLAY:-:99}"
export DEBUG="${DEBUG:-pw:api}"
export PNH_ARTIFACT_DIR="${PNH_ARTIFACT_DIR:-/src/TestResults/playwright}"

screen_geometry="${SCREEN_GEOMETRY:-1440x1000x24}"
novnc_port="${NOVNC_PORT:-6080}"
vnc_port="${VNC_PORT:-5900}"

mkdir -p /src/TestResults "$PNH_ARTIFACT_DIR"

echo "Starting Xvfb on ${DISPLAY} with screen ${screen_geometry}."
Xvfb "$DISPLAY" -screen 0 "$screen_geometry" -ac +extension RANDR >/tmp/xvfb.log 2>&1 &
xvfb_pid=$!

echo "Starting Fluxbox window manager."
fluxbox >/tmp/fluxbox.log 2>&1 &
fluxbox_pid=$!

echo "Starting x11vnc on port ${vnc_port}."
x11vnc -display "$DISPLAY" -forever -shared -nopw -listen 0.0.0.0 -rfbport "$vnc_port" >/tmp/x11vnc.log 2>&1 &
x11vnc_pid=$!

echo "Starting noVNC on port ${novnc_port}."
websockify --web=/usr/share/novnc/ "0.0.0.0:${novnc_port}" "127.0.0.1:${vnc_port}" >/tmp/novnc.log 2>&1 &
novnc_pid=$!

cleanup() {
    local status=$?

    kill "$novnc_pid" "$x11vnc_pid" "$fluxbox_pid" "$xvfb_pid" 2>/dev/null || true
    wait "$novnc_pid" "$x11vnc_pid" "$fluxbox_pid" "$xvfb_pid" 2>/dev/null || true

    exit "$status"
}

trap cleanup EXIT INT TERM

sleep 2

echo "noVNC is available at http://localhost:${novnc_port}/vnc.html?autoconnect=1&resize=scale"
echo "VNC is available on localhost:${vnc_port}."
echo "Running: dotnet test pnh_automation.sln --no-restore --no-build --settings config/test-run.docker-debug.runsettings $*"

test_status=0
dotnet test pnh_automation.sln --no-restore --no-build --settings config/test-run.docker-debug.runsettings "$@" || test_status=$?

if [[ "${PNH_DEBUG_HOLD_OPEN:-0}" == "1" ]]; then
    echo "Test run finished with exit code ${test_status}. noVNC will stay open until the container is stopped."
    tail -f /dev/null &
    wait $!
fi

exit "$test_status"
