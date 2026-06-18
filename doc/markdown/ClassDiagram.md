<div align="center">

# 🐀 RAT — UML-Klassendiagramm (gesamtes Projekt)

UML-Klassendiagramme aller Projekte der Solution, gezeichnet mit **Mermaid**.
Spiegelt den **aktuellen** Code-Stand wider (ersetzt die Vorab-Plan-UML aus der Planungsphase).

</div>

---

## Inhalt

1. [Schichten & Projektabhängigkeiten](#1-schichten--projektabhängigkeiten)
2. [RAT_Logic — Domänen- & Logikschicht](#2-rat_logic--domänen--logikschicht)
3. [RAT_Data — Datenschicht](#3-rat_data--datenschicht)
4. [RAT_WPF — Präsentationsschicht (MVVM)](#4-rat_wpf--präsentationsschicht-mvvm)
5. [RAT_Tests — Testschicht](#5-rat_tests--testschicht)
6. [Legende](#6-legende)

---

## 1. Schichten & Projektabhängigkeiten

```mermaid
flowchart TB
    WPF["RAT_WPF<br/>(WPF / MVVM, net9.0-windows)"]
    DATA["RAT_Data<br/>(net9.0)"]
    LOGIC["RAT_Logic<br/>(net9.0)"]
    TESTS["RAT_Tests<br/>(xUnit, net9.0)"]
    BACKEND["RAT-Backend<br/>(FastAPI, separat)"]

    WPF --> DATA
    WPF --> LOGIC
    DATA --> LOGIC
    TESTS --> LOGIC
    DATA -. HTTP/JSON + JWT .-> BACKEND
```

---

## 2. RAT_Logic — Domänen- & Logikschicht

### 2.1 Topologie-Modell

```mermaid
classDiagram
    direction LR

    class NetworkObjectGraph {
        +List~NetworkObject~ networkObjects
    }
    class NetworkObject {
        +int ID
        +string Name
        +NetworkObjectType Type
        +int X
        +int Y
        +string Os
        +string Cpu
        +string Gpu
        +string Ram
        +string Specs
        +List~NetworkObjectInterface~ NetworkInterfaces
        +NetworkObjectSettings Settings
        +List~AccessRight~ AccessRights
        +DeviceDescriptor Descriptor
        +GetRight(NetworkUser) AccesRights
        +CanChangeRight(actor, target, AccesRights) bool
        +SetRight(actor, target, AccesRights) bool
        +CanBeDeletedBy(NetworkUser) bool
        +PopulateOwnDeviceInterfaces() void
        +OpenSSH(Login) void
        +OpenSFTP(Login) void
        +OpenSCP(Login) void
        +OpenTelnet(Login) void
        +EnsureConnected(LoginType) bool
        +IsConnected(LoginType) bool
        +GetSnmp(SnmpSettings, oid) IList~Variable~
        +SetSnmp(SnmpSettings, oid, value) void
        +WalkSnmp(SnmpSettings, oid) IList~Variable~
    }
    class NetworkObjectInterface {
        +int ID
        +int NetworkObjectId
        +string Name
        +int MaxSpeed
        +bool IsUp
        +List~int~ OpenPorts
        +IP IP
        +NetworkConnection Connection
    }
    class NetworkConnection {
        +int ID
        +string Name
        +int Speed
        +NetworkConnectionType Type
        +string Note
        +NetworkObjectInterface[2] networkObectInterfaces
    }
    class IP {
        +string IPv4
        +string IPv6
        +string IPv4SubnetMask
        +int IPv6PrefixLength
        +string IPv4Gateway
        +IsIpv4Valid(string)$ bool
        +IsPortVlaid(int)$ bool
    }
    class NetworkObjectSettings {
        +List~Login~ Logins
        +SnmpSettings Snmp
        +AddLogin(Login) void
        +RemoveLogin(Login) void
        +FindLoginFor(LoginType) Login
        +GetAllLoginsByType(LoginType) List~Login~
    }
    class Login {
        +int ID
        +int Port
        +LoginType Type
        +string Username
        +string Password
        +Covers(LoginType) bool
    }
    class SnmpSettings {
        +int ID
        +string ReadCommunity
        +string WriteCommunity
        +int Port
    }
    class ConnectionSpeed {
        +string Label
        +long Bps
        +Ethernet$ IReadOnlyList~ConnectionSpeed~
        +Wifi$ IReadOnlyList~ConnectionSpeed~
        +For(NetworkConnectionType)$ IReadOnlyList~ConnectionSpeed~
    }

    NetworkObjectGraph "1" o-- "*" NetworkObject
    NetworkObject "1" *-- "*" NetworkObjectInterface
    NetworkObject "1" *-- "1" NetworkObjectSettings
    NetworkObject "1" *-- "*" AccessRight
    NetworkObjectInterface "1" o-- "0..1" IP
    NetworkObjectInterface "2" -- "1" NetworkConnection
    NetworkObjectSettings "1" *-- "*" Login
    NetworkObjectSettings "1" o-- "0..1" SnmpSettings
    NetworkObject ..> NetworkObjectType
    NetworkConnection ..> NetworkConnectionType
    Login ..> LoginType
    NetworkConnection ..> ConnectionSpeed
```

### 2.2 Gerätetypen, Klassifizierung & Benutzer/Rechte

```mermaid
classDiagram
    direction LR

    class DeviceDescriptor {
        <<abstract>>
        +NetworkObjectType Type*
        +string IconKey*
        +string DisplayLabel*
        +bool CanUseHostSpecs
        +For(NetworkObjectType)$ DeviceDescriptor
    }
    class PcDescriptor
    class RouterDescriptor
    class SwitchDescriptor
    class ServerDescriptor
    class ClientDescriptor
    class HubDescriptor
    class CloudDescriptor
    class AccessPointDescriptor

    DeviceDescriptor <|-- PcDescriptor
    DeviceDescriptor <|-- RouterDescriptor
    DeviceDescriptor <|-- SwitchDescriptor
    DeviceDescriptor <|-- ServerDescriptor
    DeviceDescriptor <|-- ClientDescriptor
    DeviceDescriptor <|-- HubDescriptor
    DeviceDescriptor <|-- CloudDescriptor
    DeviceDescriptor <|-- AccessPointDescriptor
    DeviceDescriptor ..> NetworkObjectType

    class DeviceClassifier {
        <<static>>
        +CiscoInterfaceName(int)$ string
        +ClassifyByPorts(IEnumerable~int~)$ NetworkObjectType
    }
    class PortNames {
        <<static>>
        +ServiceName(int)$ string
        +Describe(int)$ string
    }
    class PasswordPolicy {
        <<static>>
        +Validate(string)$ bool
        +Describe()$ string
    }
    DeviceClassifier ..> NetworkObjectType

    class NetworkUser {
        +int ID
        +string UserName
        +bool CanCreate
        +int Privileges
    }
    class AccessRight {
        +int ID
        +NetworkUser User
        +AccesRights Rights
    }
    class Session {
        <<static>>
        +NetworkUser CurrentUser
    }
    AccessRight "1" --> "1" NetworkUser
    AccessRight ..> AccesRights
    Session ..> NetworkUser

    class HostInterfaceInfo {
        +string Name
        +string Description
        +HostInterfaceKind Kind
        +bool IsUp
        +string Mac
        +List~string~ IPv4
        +List~string~ IPv6
    }
    class MibNode {
        +string Name
        +string Oid
        +string Description
    }
    class MibCatalog {
        <<static>>
        +CommonNodes$ IReadOnlyList~MibNode~
    }
    class MainController
    HostInterfaceInfo ..> HostInterfaceKind
    MibCatalog "1" o-- "*" MibNode
```

### 2.3 Enums

```mermaid
classDiagram
    class NetworkObjectType {
        <<enumeration>>
        PC
        Router
        Switch
        Server
        Client
        Hub
        Cloud
        AccessPoint
    }
    class NetworkConnectionType {
        <<enumeration>>
        Wireless
        Wired
    }
    class LoginType {
        <<enumeration>>
        SSH
        Telnet
        SFTP
        SCP
    }
    class AccesRights {
        <<enumeration>>
        Hidden
        See
        Edit
        Admin
        Owner
    }
    class HostInterfaceKind {
        <<enumeration>>
        Ethernet
        Wifi
        Loopback
        Other
    }
```

---

## 3. RAT_Data — Datenschicht

```mermaid
classDiagram
    direction LR

    class IDatabaseConnection {
        <<interface>>
        +User User
        +GetNetworkGraph() Task~NetworkObjectGraph~
        +AddNetworkObject(NetworkObject) Task~NetworkObject~
        +EditNetworkObject(NetworkObject) Task
        +DeleteNetworkObject(NetworkObject) Task
        +AddInterface(NetworkObjectInterface, NetworkObject) Task~NetworkObjectInterface~
        +EditInterface(NetworkObjectInterface) Task
        +DeleteInterface(NetworkObjectInterface) Task
        +AddConnection(NetworkConnection, i1, i2) Task~NetworkConnection~
        +EditConnection(NetworkConnection) Task
        +DeleteConnection(NetworkConnection) Task
        +GetNetworkObjectPermissions(NetworkObject) Task~List~AccessRight~~
        +SetPermission(NetworkObject, NetworkUser, AccesRights) Task
        +DeletePermission(NetworkObject, AccessRight) Task
        +GetUserDeviceLogin(NetworkObject) Task~List~Login~~
        +AddUserDeviceLogin(Login, NetworkObject) Task~Login~
        +EditUserDeviceLogin(Login) Task
        +DeletetUserDeviceLogin(Login) Task
        +GetAllUsers() Task~List~User~~
        +Login() Task~User~
        +AddUser(User) Task~User~
        +EditUser(User) Task
        +DeleteUser(User) Task
        +GetUserSettings() Task~UserSettings~
        +EditUserSettings(UserSettings) Task
    }
    class DatabaseConnection {
        +User User
        -string serverIp
        -int port
        -string token
        +Login() Task~User~
        -EnsureToken() Task
    }
    class DatabaseConnectionMock {
        +User User
    }
    class Account {
        +User User
    }
    class User {
        +int ID
        +string UserName
        +string Password
        +int Privileges
        +bool CanCreate
    }
    class UserSettings {
        +int Zoom
        +bool ShowPorts
        +bool ShowInterfaces
    }

    IDatabaseConnection <|.. DatabaseConnection
    IDatabaseConnection <|.. DatabaseConnectionMock
    IDatabaseConnection --> User
    Account --> User
    DatabaseConnection ..> NetworkObjectGraph
    DatabaseConnection ..> UserSettings
    DatabaseConnection ..> NetworkObject
```

---

## 4. RAT_WPF — Präsentationsschicht (MVVM)

### 4.1 ViewModels, Commands & Stores

```mermaid
classDiagram
    direction LR

    class ViewModelBase {
        <<abstract>>
        +PropertyChanged : event
        #OnPropertyChanged(name) void
    }
    class CommandBase {
        <<abstract>>
        +CanExecute(object) bool
        +Execute(object) void
        +CanExecuteChanged : event
    }

    class MainViewModel {
        +ViewModelBase CurrentViewModel
    }
    class LoginViewModel {
        +string Username
        +string Password
        +string ServerIp
        +int ServerPort
        +ICommand LoginCommand
        +ICommand LocalOnlyCommand
        +ShowStatus(bool, string) void
    }
    class TopologyViewModel {
        +NetworkObjectListingViewModel defaultItems
        +IEnumerable~NetworkObjectViewModel~ NetworkObjects
        +IEnumerable~NetworkConnectionViewModel~ NetworkConnectionViewModels
        +EnumTool ToolEnum
        +ICommand NetworkObjectAddConnectionCommand
        +ICommand NetworkObjectDeleteCommand
        +ICommand LogoutCommand
        +DiscoverDevicesAsync() Task
        +EnsurePortsScannedAsync() void
        +Logout() void
    }
    class NetworkObjectViewModel {
        +NetworkObject Model
        +string Type
        +string Name
        +int X
        +int Y
        +bool ShowPorts
        +IEnumerable~PortEntry~ Ports
        +ICommand NetworkObjectOpenSettings
        +RefreshPorts() void
    }
    class PortEntry {
        +string Text
        +bool IsUnreachableLogin
    }
    class NetworkConnectionViewModel {
        +NetworkObjectViewModel Source
        +NetworkObjectViewModel Target
        +int xSource
        +int ySource
        +int xTarget
        +int yTarget
        +bool IsWireless
        +DoubleCollection StrokeDashArray
        +bool ShowInterfaceLabels
        +RefreshAfterEdit() void
    }
    class NetworkObjectListingViewModel {
        +ObservableCollection~NetworkObjectViewModel~ NetworkObjects
        +AddNetworkObject(NetworkObject) void
    }
    class SettingsViewModel {
        +IEnumerable~AppTheme~ Themes
        +AppTheme SelectedTheme
        +IEnumerable~int~ ZoomLevels
        +int SelectedZoom
        +ICommand ApplyThemeCommand
    }
    class CanvasViewModel

    ViewModelBase <|-- MainViewModel
    ViewModelBase <|-- LoginViewModel
    ViewModelBase <|-- TopologyViewModel
    ViewModelBase <|-- NetworkObjectViewModel
    ViewModelBase <|-- NetworkConnectionViewModel
    ViewModelBase <|-- SettingsViewModel
    ViewModelBase <|-- CanvasViewModel
    NetworkObjectViewModel +-- PortEntry

    class LoginCommand
    class LocalOnlyCommand
    class LogoutCommand
    class ChangeThemeCommand
    class NetworkObjectAddConnectionCommand
    class NetworkObjectDeleteCommand
    class NetworkObjectAddedCommand
    class NetworkObjectOpenSettingsCommand
    CommandBase <|-- LoginCommand
    CommandBase <|-- LocalOnlyCommand
    CommandBase <|-- LogoutCommand
    CommandBase <|-- ChangeThemeCommand
    CommandBase <|-- NetworkObjectAddConnectionCommand
    CommandBase <|-- NetworkObjectDeleteCommand
    CommandBase <|-- NetworkObjectAddedCommand
    CommandBase <|-- NetworkObjectOpenSettingsCommand

    class NavigationStore {
        +ViewModelBase CurrentViewModel
        +CurrentViewModelChanged : event
    }
    class DatabaseConnectionStore {
        <<static>>
        +IDatabaseConnection Current
        +string LastServerIp
        +int LastServerPort
    }

    MainViewModel --> NavigationStore
    TopologyViewModel --> NavigationStore
    TopologyViewModel "1" o-- "*" NetworkObjectViewModel
    TopologyViewModel "1" o-- "*" NetworkConnectionViewModel
    TopologyViewModel --> NetworkObjectListingViewModel
    NetworkObjectViewModel --> NetworkObject
    NetworkConnectionViewModel --> NetworkConnection
    LoginCommand ..> DatabaseConnectionStore
    TopologyViewModel ..> DatabaseConnectionStore
```

### 4.2 Querschnitt: Themes, Discovery, Setup, Logging, Converter

```mermaid
classDiagram
    direction LR

    class ThemeManager {
        <<static>>
        +AppTheme Current
        +ThemeChanged : event
        +Apply(AppTheme) void
    }
    class ZoomManager {
        <<static>>
        +int Min
        +int Default
        +int Max
        +IReadOnlyList~int~ Levels
        +int Current
        +double Scale
        +Apply(int) void
    }
    class DisplaySettings {
        <<static>>
        +bool ShowInterfaces
        +bool ShowPorts
        +ShowInterfacesChanged : event
        +ShowPortsChanged : event
    }
    class IconProvider {
        <<static>>
        +Get(string) ImageSource
    }
    class CanvasLayout {
        <<static>>
        +double Width
        +double Height
        +ClampX(double)$ int
        +ClampY(double)$ int
    }
    class AppLogger {
        <<static>>
        +string LogFilePath
        +string LogDirectory
        +Start() void
        +Debug(string) void
        +Info(string) void
        +Warn(string) void
        +Error(string) void
        +OpenLogFolder()$ bool
    }
    class NmapService {
        <<static>>
        +IsInstalled()$ bool
        +InstallAsync()$ Task~bool~
        +GetLocalSubnetInfo()$ tuple
        +IsActiveConnectionWireless()$ bool
        +ScanLocalSubnetAsync(bool)$ Task~List~DiscoveredHost~~
        +ScanHostPortsFastAsync(ip)$ Task~List~int~~
        +ScanHostOsAsync(ip)$ Task~string~
        +FindRouterAsync()$ Task~tuple~
    }
    class DiscoveredHost {
        +string Ip
        +string Hostname
        +List~int~ OpenPorts
        +string Os
        +string SubnetMask
    }
    class SetupService {
        <<static>>
        +bool HasRunSetup
        +bool NmapDeclined
        +MarkSetupDone()$ void
        +CreateDesktopShortcut()$ bool
    }
    class IconKeyConverter {
        <<IValueConverter>>
    }
    class ComparisonConverter {
        <<IValueConverter>>
    }
    class PortColorConverter {
        <<IValueConverter>>
    }

    NmapService ..> DiscoveredHost
    IconKeyConverter ..> IconProvider
```

### 4.3 Enums (WPF)

```mermaid
classDiagram
    class AppTheme {
        <<enumeration>>
        Light
        Dark
    }
    class EnumTool {
        <<enumeration>>
        Cursor
        Connector
        Delete
    }
```

---

## 5. RAT_Tests — Testschicht

```mermaid
classDiagram
    class RatLogicTests {
        <<xUnit>>
        +IsIpv4Valid_*()
        +DeviceDescriptor_*()
        +GetRight_*()
        +CanChangeRight_*()
        +SetRight_*()
    }
    class DeviceClassifierTests {
        <<xUnit>>
        +CiscoInterfaceName_*()
        +ClassifyByPorts_*()
        +DeviceDescriptor_CloudAndAccessPoint_*()
    }
    RatLogicTests ..> NetworkObject
    RatLogicTests ..> DeviceDescriptor
    RatLogicTests ..> IP
    DeviceClassifierTests ..> DeviceClassifier
```

---

## 6. Legende

| Symbol | Bedeutung |
|--------|-----------|
| `*--` | Komposition (Teil-Ganzes, Lebensdauer gebunden) |
| `o--` | Aggregation (lose Zugehörigkeit) |
| `-->` | gerichtete Assoziation |
| `..>` | Abhängigkeit / Nutzung |
| `<|--` | Vererbung (Klasse erbt von Klasse) |
| `<|..` | Implementierung (Klasse implementiert Interface) |
| `+-- ` | verschachtelte (nested) Klasse |
| `$` | statisches Member · `*` (an Member) | abstraktes Member |
| `<<...>>` | Stereotyp (interface, abstract, enumeration, static, IValueConverter, xUnit) |

> Hinweis: Aus Lesbarkeitsgründen sind die Diagramme nach Projekt/Thema aufgeteilt; Typen wie
> `NetworkObject`, `User` oder `IDatabaseConnection` tauchen in mehreren Diagrammen als
> Beziehungsziel auf. Eine Architektur-Übersicht steht in der
> [Dokumentation](Dokumentation.md#5-funktionsblöcke-bzw-architektur).
