# Notification Haptic Safe for Logitech MX Master 4

A security-minimized macOS Logi Options+ plugin that triggers MX Master 4 haptic feedback when macOS Notification Center presents a notification.

This project is derived from `kimik-hyum/logi-notificationHapticPlugin`, audited at upstream commit `4e28d8b430adfb997193998f329c1ca2ade908f3`.

## Security design

The runtime is intentionally small:

- launches only Apple's fixed `/usr/bin/log` executable;
- uses `UseShellExecute = false` and fixed `ArgumentList` arguments (no shell command construction);
- listens only to unified-log messages from `NotificationCenter`;
- reacts only to `com.apple.unc:application` + `Queuing action present` presentation events;
- raises a local Logitech haptic event;
- has no HTTP client, sockets, telemetry, updater, browser extension, or remote endpoints;
- does not persist raw Notification Center log lines;
- does not include the upstream `ClickNotification.app` Accessibility helper;
- does not request Accessibility, Automation, Full Disk Access, Contacts, Photos, or Keychain permissions;
- does not implement custom plugin `Install()` or `Uninstall()` behavior.

See [AUDIT.md](AUDIT.md) for the audit notes and remaining limitations.

## Build

### GitHub Actions

The workflow in `.github/workflows/build.yml` performs a clean macOS build with .NET 8, obtains Logitech's `PluginApi.dll` from a pinned `LogiPluginTool` package, builds the plugin, runs `logiplugintool pack`, then `logiplugintool verify`, and uploads the resulting `.lplug4` plus its SHA-256 checksum as an artifact.

### Local macOS build

With Logi Options+ and .NET 8 installed:

```bash
chmod +x build-macos.sh
./build-macos.sh
```

The script only builds and verifies the package. It does **not** automatically install it.

## Compatibility note

The upstream project is no longer maintained and had an unresolved Logi Options+ installation report in 2026. This derivative reduces security exposure, but compatibility with a particular Logi Options+ / macOS version still needs to be validated on real MX Master 4 hardware.

## License

MIT. The upstream copyright and attribution are preserved in `LICENSE`.
