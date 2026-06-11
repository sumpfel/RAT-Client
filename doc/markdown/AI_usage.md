# AI useage
This file is for documenting all ai usage :D

---------

## Prompt 1 — Claude (model: Claude Opus 4.8, via Claude Code)

**Request:** Add a working SNMP MIB browser to the NetworkObjectSettingsWindow; make name/specs editable in the
settings menu (PC = name only, live specs; other devices = name + specs edited in software only, not pushed to the
device); improve the UI as much as possible; store the logged-in user on PC network objects; add a permission object
(user + rights) to network objects. Also mark all AI-written code with start/end comments.

**Changes made (all AI-written regions are wrapped in `//KI start (Claude Opus 4.8, prompt 1)` ... `//KI end`
or `<!--KI start ...-->` ... `<!--KI end-->`):**

- `RAT_Logic/AccessRight.cs` (new) — `[Flags] AccessRight` enum (View/Edit/Connect/Snmp/Manage/Full).
- `RAT_Logic/Permission.cs` (new) — `NetworkUser` (logic-layer user identity) + `Permission` (user + rights).
- `RAT_Logic/Session.cs` (new) — app-wide `Session.CurrentUser` so PC objects can be owned by the logged-in user.
- `RAT_Logic/MibCatalog.cs` (new) — built-in catalog of common SNMP OIDs (`MibNode` + `MibCatalog.CommonNodes`).
- `RAT_Logic/NetworkObject.cs` — added stored spec fields (Os/Cpu/Gpu/Ram/Specs), `Owner`, `Permissions` list with
  `SetPermission`/`RemovePermission`/`UserHasRight`, and `WalkSnmp(...)` for the MIB browser.
- `RAT_Logic/SnmpSettings.cs` — added a convenience constructor that also takes a port.
- `RAT_WPF/App.xaml` — shared dark theme brushes + styles (ModernTextBox/ModernButton/SubtleButton/DangerButton/FieldLabel).
- `RAT_WPF/Commands/LoginCommand.cs` — sets `Session.CurrentUser` on login.
- `RAT_WPF/NetworkObject_UI/NetworkObjectSettingsWindow.xaml(.cs)` — full restyle; editable Overview (PC: name only,
  read-only live specs; others: name + specs in software); new **MIB Browser** tab (common-OID list + Get/Walk/Set);
  working **Access Control** tab (owner + grant per-user rights).
- `RAT_WPF/NetworkObject_UI/UpdateLoginWindow.xaml`, `UpdateInterfaceWindow.xaml(.cs)`, `LoginControl.xaml(.cs)` —
  restyled to match; fixed the previously dead Cancel button on the interface dialog; LoginControl now shows the real
  protocol/user/port.

**Notes / scope:** Permissions, owner, and non-PC specs are in-memory only (no real database exists yet — DB
persistence is left as TODO). SNMP name resolution uses the built-in common-OID catalog plus raw numeric Get/Walk/Set.

---------

## Prompt 2 — Claude (model: Claude Opus 4.8, via Claude Code)

**Request:** Style every WPF window in our CSS brown palette (doc/markdown/style/rats.css); add a light/dark dropdown in
a NEW settings window built with MVVM + command binding; put the logo in the app; add anime-style transparent icons
(rat using a PC = PC icon, rat biting a cable getting electrocuted = no connection, connection-lost, device icons,
etc.); add error-handling popups (e.g. "no connection" when sending an SSH command / opening an SSH stream with no
connection); give logins a connected / not-connected status; make the interfaces list scrollable; show PC interfaces
with an interface control + an (i) info button; and filter the PC interface list down to real NICs (Wi-Fi / Ethernet /
USB-Ethernet) instead of all the garbage.

**Theme / icons decision:** the user asked for both vector-now and PNG-swappable-later, plus a light/dark dropdown in a
new MVVM settings window. I can't generate raster anime art, so icons are hand-built **vector** art that can be
**overridden** by dropping a PNG into `Assets/Icons/<key>.png` (see `IconProvider`).

**Changes made (all AI regions wrapped in `KI start (Claude Opus 4.8, prompt 2)` … `KI end`):**

- `RAT_WPF/Themes/LightTheme.xaml`, `DarkTheme.xaml` (new) — brown palettes (light = paper+brown from rats.css; dark =
  same fur tones on a dark base).
- `RAT_WPF/Themes/Shared.xaml` (new) — control styles (ModernButton/SubtleButton/DangerButton/IconButton/ModernTextBox/
  Card/Header*) that bind brushes via **DynamicResource** so theme switches recolor live; also `AppLogo` + the icon converter.
- `RAT_WPF/Themes/Icons.xaml` (new) — vector `DrawingImage` icons: PC (rat at a computer), NoConnection (rat biting a
  cable + spark), ConnectionLost, Connected, Router/Switch/Server/Client, Ethernet/Wifi/Usb.
- `RAT_WPF/Themes/ThemeManager.cs` (new) — swaps the active palette dictionary at runtime (light/dark).
- `RAT_WPF/Themes/IconProvider.cs` (new) — returns a PNG override from `Assets/Icons/<key>.png` if present, else the vector.
- `RAT_WPF/Converters/IconKeyConverter.cs` (new) — binds an icon key / device-type string to an ImageSource.
- `RAT_WPF/App.xaml` — composes the theme from merged dictionaries (palette swappable in slot 0).
- `RAT_WPF/SettingsWindow.xaml(.cs)` + `ViewModels/SettingsViewModel.cs` + `Commands/ChangeThemeCommand.cs` (new) —
  MVVM settings window with the light/dark dropdown (bound + ApplyThemeCommand).
- `RAT_WPF/RAT_WPF.csproj` — bundles `Assets/logo.png` as a resource and copies `Assets/Icons/*.png` to output; added a
  RAT_Data project reference.
- Restyled to the theme + logo: `MainWindow`, `Views/TopologyView` (logo + ⚙ Settings button), `Views/LoginView`,
  `Views/NetworkObjectListingView` + `Views/NetworkObjectView` (device icons), `EnterPasswordWindow`, and the
  prompt-1 windows converted to DynamicResource brushes.
- `RAT_Logic/NetworkObject.cs` — `IsSshConnected`/`IsSftpConnected`/`IsScpConnected`/`IsConnected(LoginType)` status; new
  `GetOwnDeviceInterfacesDetailed()` returning real NICs only (Ethernet/Wi-Fi/USB-Ethernet), filtering loopback/tunnel/virtual.
- `RAT_Logic/HostInterfaceInfo.cs` (new) — structured host-interface model (kind/status/mac/speed/IPs).
- `RAT_WPF/NetworkObject_UI/LoginControl.xaml(.cs)` — connected/not-connected dot + label, connect with no-connection
  popup, edit refresh, delete.
- `RAT_WPF/NetworkObject_UI/InterfaceControl.xaml(.cs)` (new) — host interface row with type icon, status dot, and an
  (i) details popup.
- `RAT_WPF/NetworkObject_UI/NetworkObjectSettingsWindow.xaml(.cs)` — scrollable Interfaces tab; PC interfaces shown via
  InterfaceControl (add-button hidden for the host); loads existing logins + delete; no-connection popups for SSH
  send / SSH stream open / SFTP / SCP; shared `ShowNoConnection`/`ShowActionFailed` helpers.

**Verified:** full solution builds (0 errors); app launches and renders the light brown theme, the logo in the topbar,
and per-device vector icons in the sidebar.

**Notes / scope:** Theme switches live across all open windows because styles use DynamicResource. Anime PNGs are not
shipped — drop them into `Assets/Icons/<Icon.Key>.png` to override the vectors. Connection status reflects live SSH.NET
client state. The "no connection" check uses the device's open-session state, not a reachability ping.

---------

## Prompt 3 — Claude (model: Claude Opus 4.8, via Claude Code)

**Request:** In dark mode the theme dropdown (and similar controls) stayed white / unreadable — fix it. Also make
windows with Apply/Close/Save buttons size to fit (or scroll) so the buttons are always reachable. Commit as the
Visual Studio user (sumpfel), no co-author, then pull prioritising remote changes for merging — but do not push.

**Changes made (AI regions wrapped in `KI start (Claude Opus 4.8, prompt 3)` … `KI end`):**

- `RAT_WPF/Themes/Shared.xaml` — fully templated, theme-aware `ComboBox` + `ComboBoxItem` (readable dropdown popup in
  both themes), plus themed `ListBoxItem`, `CheckBox`, `DataGrid`/`DataGridColumnHeader`/`DataGridCell` so nothing
  stays white in dark mode.
- `RAT_WPF/SettingsWindow.xaml`, `NetworkObject_UI/UpdateLoginWindow.xaml`, `NetworkObject_UI/UpdateInterfaceWindow.xaml`
  — `SizeToContent="Height"` with a `MaxHeight`, content in a `ScrollViewer`, and the button bar moved to
  `DockPanel.Dock="Bottom"` so Apply/Close/Save/Cancel never scroll out of reach. `EnterPasswordWindow` set to
  `SizeToContent="Height"`. `NetworkObjectSettingsWindow` given MinHeight/MinWidth + window icon.

**Verified:** solution builds (0 errors); drove the app via UI Automation to open Settings, switch to Dark, and
confirmed by screenshot that the dropdown text ("Light"/"Dark") is legible and the whole UI recolors to dark brown.
