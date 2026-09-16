#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TOOL_DIR="$ROOT/.tools"
LOGI_PLUGIN_TOOL_VERSION="${LOGI_PLUGIN_TOOL_VERSION:-6.1.4.22672}"

mkdir -p "$TOOL_DIR"

if [[ ! -x "$TOOL_DIR/logiplugintool" ]]; then
  dotnet tool install LogiPluginTool --tool-path "$TOOL_DIR" --version "$LOGI_PLUGIN_TOOL_VERSION"
fi

PLUGIN_API_DLL="$(find "$TOOL_DIR/.store/logiplugintool" -path "*/tools/*/any/PluginApi.dll" -print -quit)"
if [[ -z "$PLUGIN_API_DLL" || ! -f "$PLUGIN_API_DLL" ]]; then
  echo "ERROR: PluginApi.dll was not found inside LogiPluginTool." >&2
  exit 1
fi

PLUGIN_API_DLL="$PLUGIN_API_DLL" "$ROOT/build-macos.sh"
