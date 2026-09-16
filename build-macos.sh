#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TOOL_DIR="$ROOT/.tools"
DIST_DIR="$ROOT/dist"
PROJECT="$ROOT/src/NotificationHapticSafePlugin.csproj"
LOGI_PLUGIN_TOOL_VERSION="${LOGI_PLUGIN_TOOL_VERSION:-6.1.4.22672}"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "ERROR: .NET 8 SDK is required." >&2
  exit 1
fi

PLUGIN_API_DLL="${PLUGIN_API_DLL:-/Applications/Utilities/LogiPluginService.app/Contents/MonoBundle/PluginApi.dll}"
if [[ ! -f "$PLUGIN_API_DLL" ]]; then
  echo "ERROR: PluginApi.dll not found at: $PLUGIN_API_DLL" >&2
  echo "Install/update Logi Options+, or set PLUGIN_API_DLL=/path/to/PluginApi.dll" >&2
  exit 1
fi

PLUGIN_API_DIR="$(cd "$(dirname "$PLUGIN_API_DLL")" && pwd)/"
mkdir -p "$TOOL_DIR" "$DIST_DIR"

if [[ ! -x "$TOOL_DIR/logiplugintool" ]]; then
  dotnet tool install LogiPluginTool --tool-path "$TOOL_DIR" --version "$LOGI_PLUGIN_TOOL_VERSION"
fi

rm -rf "$ROOT/build" "$DIST_DIR/NotificationHapticSafe_1_0_0.lplug4" "$DIST_DIR/NotificationHapticSafe_1_0_0.lplug4.sha256"

dotnet build "$PROJECT" -c Release /p:PluginApiDir="$PLUGIN_API_DIR"

"$TOOL_DIR/logiplugintool" pack "$ROOT/build/Release" "$DIST_DIR/NotificationHapticSafe_1_0_0.lplug4"

set +e
VERIFY_OUTPUT="$("$TOOL_DIR/logiplugintool" verify "$DIST_DIR/NotificationHapticSafe_1_0_0.lplug4" 2>&1)"
VERIFY_STATUS=$?
set -e
printf '%s\n' "$VERIFY_OUTPUT"

if [[ $VERIFY_STATUS -ne 0 ]] || printf '%s\n' "$VERIFY_OUTPUT" | grep -q '^ERROR:'; then
  echo "ERROR: logiplugintool verification failed." >&2
  exit 1
fi

shasum -a 256 "$DIST_DIR/NotificationHapticSafe_1_0_0.lplug4" | tee "$DIST_DIR/NotificationHapticSafe_1_0_0.lplug4.sha256"

echo "Built and verified: $DIST_DIR/NotificationHapticSafe_1_0_0.lplug4"
