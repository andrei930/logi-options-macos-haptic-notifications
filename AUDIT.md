# Security audit notes

Upstream reviewed: `kimik-hyum/logi-notificationHapticPlugin`

Audited upstream commit:

`4e28d8b430adfb997193998f329c1ca2ade908f3`

## Upstream runtime observations

- The notification action starts the fixed executable `/usr/bin/log`.
- `UseShellExecute = false`; no shell command interpolation is used.
- The predicate restricts the source process to `NotificationCenter`.
- Matching happens locally and raises a Logi Plugin event.
- No networking code or third-party runtime package references were found.
- No custom plugin install/uninstall code was found.
- Upstream persisted the full matching Notification Center log line at verbose level; this derivative removes that behavior.
- Upstream included an unrelated `ClickNotification.app` using Accessibility/System Events; this derivative does not include it.
- Upstream retained sample counter actions; this derivative removes them.
- Upstream project file created a development `.link` file and attempted a plugin reload after builds; this derivative removes those build side effects.

## Remaining risks and limitations

- The Logi Plugin Service and `PluginApi.dll` are trusted code outside this source tree.
- `LogiPluginTool` is a build-time dependency; CI pins its version.
- macOS unified-log formats can change, which may cause missed or false triggers.
- A global 1.5 second debounce intentionally collapses notifications arriving very close together.
- A failed `/usr/bin/log` process is retried every two seconds while the plugin remains loaded.
- No per-application allow/deny list is currently implemented.
- Source review and a clean reproducible build substantially reduce supply-chain risk, but do not mathematically prove the absence of every possible vulnerability.
