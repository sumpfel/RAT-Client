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

---------

## Prompt 4 — Claude (model: Claude Opus 4.8, via Claude Code)

**Request:** Make the canvas dark themed and give devices a light background; default the PC name to the host's own
machine name (still changeable; other PC specs stay read-only); add a Delete tool to remove objects from the canvas.

**Changes made (AI regions wrapped in `KI start (Claude Opus 4.8, prompt 4)` … `KI end`):**

- `RAT_WPF/Themes/LightTheme.xaml`, `DarkTheme.xaml` — added `Brush.Canvas` (dark in both themes), `Brush.CanvasNode`
  (light) and `Brush.CanvasNodeText`.
- `RAT_WPF/Views/TopologyView.xaml(.cs)` — canvas `Background` now `Brush.Canvas`; added a "🗑 Delete" tool
  RadioButton; `IsDeleteToolActive` + `DeleteNode(...)` helpers.
- `RAT_WPF/Views/NetworkObjectView.xaml(.cs)` — device node now uses the light `Brush.CanvasNode` card (with border)
  and shows the device **Name**; `MouseLeftButtonDown` removes the node when the Delete tool is active, and dragging is
  suppressed while that tool is active.
- `RAT_WPF/ViewModels/TopologyViewModel.cs` — default PC name set to `Environment.MachineName`; added
  `RemoveNetworkObjectViewModelFromCanvas(...)`.

(Own-PC specs remaining read-only with only the name editable was already handled in prompt 1's `LoadOverview`, which
keys on `NetworkObjectType.PC`.)

**Verified:** solution builds (0 errors); drove the app via UI Automation + screenshots — confirmed the canvas is dark
brown, device nodes sit on light cards, and selecting the Delete tool + clicking a node removes it from the canvas.

---------

## Prompt 5 — Claude (model: Claude Opus 4.8, via Claude Code)

**Request:** Don't force a dark canvas / white device nodes regardless of theme — the canvas and device nodes should
follow the active theme (dark in dark mode, light in light mode).

**Changes made (AI region marked `prompt 4/5`):**

- `RAT_WPF/Themes/LightTheme.xaml` — `Brush.Canvas` is now light (paper-deep), `Brush.CanvasNode` light, node text dark.
- `RAT_WPF/Themes/DarkTheme.xaml` — `Brush.Canvas` stays dark, `Brush.CanvasNode` is now a dark fur tone (not white),
  node text light.

**Verified:** built (0 errors); screenshotted both themes — light mode shows a paper canvas with light device cards,
dark mode shows a dark canvas with dark device cards; labels readable in both.

---------

## Prompt 6 — Claude (model: Claude Opus 4.8, via Claude Code)

**Request:** Make the canvas clearly distinct (a slightly different background and/or a border).

**Changes:** added `Brush.CanvasBorder` + brighter `Brush.Canvas`, and wrapped the canvas `ItemsControl` in a bordered
`Border` in `TopologyView.xaml`. (Superseded the next turn — see prompt 8.)

---------

## Prompt 7 — Claude (model: Claude Opus 4.8, via Claude Code)

**Request:** Improve the interfaces and logins inside the device settings window — colour the Edit button (not just
Connect/Delete), take the overall design "to the next level" (clean, useful). When adding an interface only the name
should be required, and interfaces should be editable, deletable, and show up/down via a dedicated control.

**Changes made (AI regions marked `prompt 7`):**

- `RAT_Logic/NetworkObjectInterface.cs` — added `IsUp` (software up/down state); made `IP` nullable/optional and gave
  `Name` a default.
- `RAT_WPF/Themes/Shared.xaml` — new `AccentOutlineButton` style so "Edit" reads as a real (accent-outlined) action.
- `RAT_WPF/NetworkObject_UI/InterfaceControl.xaml(.cs)` — reworked into a two-mode control: read-only for host PC NICs
  (icon + status pill + (i) details), and **editable** for modelled device interfaces (Toggle up/down, Edit, Delete,
  (i)); raises `EditRequested` / `DeleteRequested`.
- `RAT_WPF/NetworkObject_UI/UpdateInterfaceWindow.xaml(.cs)` — only the **name** is required (IP fields optional and
  only built when filled); supports **editing** an existing interface (ctor takes an optional interface, prefills).
- `RAT_WPF/NetworkObject_UI/LoginControl.xaml` — restyled into a card with a status pill and an accent-outlined Edit
  button to match.
- `RAT_WPF/NetworkObject_UI/NetworkObjectSettingsWindow.xaml(.cs)` — Logins + Interfaces tabs given hints, scroll and
  empty-states; software interfaces now render as editable `InterfaceControl`s wired to add/edit/delete.

**Verified:** built (0 errors); via UI Automation opened a seeded device's settings and screenshotted both tabs —
Interfaces show eth0 (UP, IP) and eth1 (DOWN, no IP) each with Toggle/Edit/Delete/(i); Logins show the SSH credential
with status pill and Connect/Edit/Delete. Edit is clearly coloured in both.

---------

## Prompt 8 — Claude (model: Claude Opus 4.8, via Claude Code)

**Request:** Remove the border around the canvas.

**Changes:** removed the bordered `Border` wrapper from `TopologyView.xaml` (back to a plain `ItemsControl`/`Canvas`)
and dropped the now-unused `Brush.CanvasBorder` from both themes. The canvas keeps its slightly-distinct background tone.

---------

## Prompt 9 — Claude (model: Claude Opus 4.8, via Claude Code)

**Request:** Create a window to select an interface of a device — for now it just returns the chosen interface, nothing
is done with it.

**Changes made (AI region marked `prompt 9`):**

- `RAT_WPF/NetworkObject_UI/SelectInterfaceWindow.xaml(.cs)` (new) — themed dialog that lists a device's
  `NetworkInterfaces` (name + IPv4), supports double-click, and returns the picked `NetworkObjectInterface` via the
  public `SelectedInterface` property (null on cancel). Not wired to anything else yet, by request.

**Verified:** solution builds (0 errors).

---------

## Prompt 10 — Claude (model: Claude Opus 4.8, via Claude Code)

**Request:** Reworked the access-rights model (user edited `AccessRight.cs` + `User.cs`). A NetworkObject has multiple
AccessRights (one per user; a user with no entry is assumed `Hidden`/0 — the DB already drops objects a user can't
see). `User` now has `CanCreate` (needed to create NetworkObjects) and a `Privileges` field. Privilege rules: a user
may change only Hidden/See/Edit of others if they are an **Admin** of that object; cannot take rights from another
**Admin** or the **Owner**; only an **Owner** can change Admins/Owners, grant/remove Owner, and delete the object.

**Note on the circular reference:** `AccessRight` as written referenced `User`, but RAT_Logic can't reference RAT_Data
(RAT_Data already depends on RAT_Logic). As written, `User` actually bound to `Lextm.SharpSnmpLib.Security.User`. Kept
the model in RAT_Logic referencing the logic-layer `NetworkUser` (extended with `CanCreate`/`Privileges`); the WPF/data
layer maps `RAT_Data.User` -> `NetworkUser`.

**Changes made (AI regions marked `prompt 11`/`prompt 1/11`):**

- `RAT_Logic/AccessRight.cs` — `AccesRights` enum (Hidden/See/Edit/Admin/Owner) + `AccessRight { NetworkUser; AccesRights }`.
- `RAT_Logic/Permission.cs` — dropped the old `[Flags]` `Permission`; `NetworkUser` now carries `CanCreate` + `Privileges`.
- `RAT_Logic/NetworkObject.cs` — replaced `Owner`/`Permissions`/`SetPermission`/`UserHasRight` with
  `List<AccessRight> AccessRights` + `GetRight` (Hidden default), `HasAtLeast`, `CanChangeRight` (enforces the
  Admin/Owner rules), `SetRight`/`ApplyRight`, and `CanBeDeletedBy` (Owner only).
- `RAT_WPF/Commands/LoginCommand.cs` + `App.xaml.cs` — current user carries `CanCreate` (defaults true in dev until the
  DB login is wired); debug-skip path seeds a dev user.
- `RAT_WPF/ViewModels/NetworkObjectViewModel.cs` — exposes `Model` so create/delete can check rights.
- `RAT_WPF/Views/TopologyView.xaml.cs` — creating (drop) requires `CanCreate` and makes the creator the Owner; the
  Delete tool requires Owner (`CanBeDeletedBy`).
- `RAT_WPF/NetworkObject_UI/NetworkObjectSettingsWindow.xaml(.cs)` — Access Control tab rebuilt: shows the current
  user's role, a level dropdown limited to what they may assign (Admin -> up to Edit, Owner -> any), a grid of
  per-user rights, and routes changes through `SetRight` so the rules are enforced (with a clear "Not allowed" popup).

**Verified:** builds (0 errors); via UI Automation opened a seeded device's Access Control tab — "Your access: Owner",
grant panel enabled, grid lists debug=Owner / alice=Admin / bob=See.

---------

## Prompt 11 — Claude (model: Claude Opus 4.8, via Claude Code)

**Request:** In the add-interface window, mark every IP field "(optional)" (not just IPv4) and make sure they really
are optional. Make the PC's interfaces real `NetworkObjectInterface`s on the NetworkObject so they can be picked in the
SelectInterfaceWindow. The SelectInterfaceWindow was buggy/weird — rebuild it nicely, e.g. reuse the InterfaceControl
without the edit/delete buttons.

**Changes made (AI regions marked `prompt 12` / `prompt 9/12` / `prompt 2/12` / `prompt 4/12`):**

- `RAT_WPF/NetworkObject_UI/UpdateInterfaceWindow.xaml(.cs)` — every IP label now says "(optional)"; IPv6 prefix
  defaults to empty; cleaned the "any IP filled?" check; only builds an `IP` when a field is filled (truly optional).
- `RAT_Logic/NetworkObject.cs` — `GetOwnDeviceInterfacesAsModel()` (real host NICs as `NetworkObjectInterface`s) and
  `PopulateOwnDeviceInterfaces()` to fill the object's interface list.
- `RAT_WPF/ViewModels/TopologyViewModel.cs` — the PC palette item is populated with the host's real interfaces.
- `RAT_WPF/NetworkObject_UI/NetworkObjectSettingsWindow.xaml.cs` — own-PC tab also populates the model list if empty.
- `RAT_WPF/NetworkObject_UI/InterfaceControl.xaml.cs` — modelled-interface ctor takes `readOnly` (hides
  Toggle/Edit/Delete, keeps (i)).
- `RAT_WPF/NetworkObject_UI/SelectInterfaceWindow.xaml(.cs)` — rebuilt: a styled ListBox whose rows are read-only
  `InterfaceControl`s with a clear selection highlight; double-click or Select returns the chosen interface.

**Verified:** builds (0 errors); launched the SelectInterfaceWindow against the host PC and screenshotted — rows show
the real NICs (Ethernet/Wi-Fi/Local Area Connections) with status pills, the selected row is highlighted.

---------

## Prompt 12 — Claude (model: Claude Opus 4.8, via Claude Code)

**Request:** Make the sshTerminalControl not a flat black background but match the app styling, and improve the shell
styling and syntax highlighting — not just colors.

**Changes made (AI regions marked `prompt 13`):**

- `RAT_WPF/Themes/DarkTheme.xaml` + `LightTheme.xaml` — added a `Term.*` palette (warm dark console that stays
  on-brand with the brown theme in both modes): background/input/header, text/muted, prompt, user/host/path,
  command/flag/string/number, file-type colors (dir/exec/archive/image/code), and error/warn/success/border.
- `RAT_WPF/NetworkObject_UI/sshTermainalControl.xaml` — restyled into a rounded console card: header bar with
  traffic-light dots + "ssh — shell #N" title, transparent themed RichTextBox with line-height, and a themed input row
  with a `❯` prompt glyph (no more `#0c0c0c`/`#181818`).
- `RAT_WPF/NetworkObject_UI/sshTermainalControl.xaml.cs` — replaced the ad-hoc highlighter with a tokenizing one that
  resolves theme brushes and classifies: bash prompts (user/host/path/`$`|`#`), command vs flags (`-x`/`--long`),
  quoted strings, env vars (`$VAR`), unix permission strings (`drwxr-xr-x`), IPv4(:port), numbers/sizes, and files by
  extension category (dir/exec/archive/image/code), plus error/warning/success log lines. Whitespace is preserved.

**Verified:** builds (0 errors); previewed the control with sample output and screenshotted — prompts, commands, flags,
perms, IPs, numbers, per-type file colors, and error/warn/success lines all render correctly on the themed console.
