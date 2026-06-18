<div align="center">

# 🐀 RAT — Klassendiagramm (Logikschicht)

UML-Klassendiagramm der `RAT_Logic`-Schicht (Domänenmodell), gezeichnet mit **Mermaid**.
Spiegelt den **aktuellen** Code-Stand wider (ersetzt die Vorab-Plan-UML aus der Planungsphase).

</div>

---

## Überblick

`NetworkObjectGraph` hält die gesamte Topologie. Ein `NetworkObject` (Gerät) besitzt
mehrere `NetworkObjectInterface` (Schnittstellen); je zwei Interfaces werden durch eine
`NetworkConnection` (Kabel/Funkstrecke) verbunden. Pro Gerät gibt es `NetworkObjectSettings`
mit `Login`s und `SnmpSettings` sowie `AccessRight`s, die einem `NetworkUser` eine Rechtestufe
zuordnen. Der Gerätetyp wird über die abstrakte `DeviceDescriptor`-Hierarchie aufgelöst.

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
        +OpenSSH(Login) void
        +OpenTelnet(Login) void
        +EnsureConnected(LoginType) bool
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

    class AccessRight {
        +int ID
        +NetworkUser User
        +AccesRights Rights
    }

    class NetworkUser {
        +int ID
        +string UserName
        +bool CanCreate
        +int Privileges
    }

    class Session {
        +NetworkUser CurrentUser$
    }

    %% --- abstract device-type hierarchy ---
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

    class DeviceClassifier {
        <<static>>
        +CiscoInterfaceName(int)$ string
        +ClassifyByPorts(IEnumerable~int~)$ NetworkObjectType
    }

    %% --- enums ---
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

    %% --- relationships ---
    NetworkObjectGraph "1" o-- "*" NetworkObject : enthält
    NetworkObject "1" *-- "*" NetworkObjectInterface : hat
    NetworkObject "1" *-- "1" NetworkObjectSettings : hat
    NetworkObject "1" *-- "*" AccessRight : vergibt
    NetworkObjectInterface "1" o-- "0..1" IP : hat
    NetworkObjectInterface "2" -- "1" NetworkConnection : verbunden über
    NetworkObjectSettings "1" *-- "*" Login : speichert
    NetworkObjectSettings "1" o-- "0..1" SnmpSettings : hat
    AccessRight "1" --> "1" NetworkUser : für
    Session ..> NetworkUser : referenziert

    NetworkObject ..> DeviceDescriptor : Descriptor
    DeviceDescriptor <|-- PcDescriptor
    DeviceDescriptor <|-- RouterDescriptor
    DeviceDescriptor <|-- SwitchDescriptor
    DeviceDescriptor <|-- ServerDescriptor
    DeviceDescriptor <|-- ClientDescriptor
    DeviceDescriptor <|-- HubDescriptor
    DeviceDescriptor <|-- CloudDescriptor
    DeviceDescriptor <|-- AccessPointDescriptor

    NetworkObject ..> NetworkObjectType
    NetworkConnection ..> NetworkConnectionType
    Login ..> LoginType
    AccessRight ..> AccesRights
    DeviceDescriptor ..> NetworkObjectType
    DeviceClassifier ..> NetworkObjectType
```

---

## Legende

| Symbol | Bedeutung |
|--------|-----------|
| `*--` | Komposition (Teil-Ganzes, Lebensdauer gebunden) |
| `o--` | Aggregation (lose Zugehörigkeit) |
| `-->` | gerichtete Assoziation |
| `..>` | Abhängigkeit / Nutzung |
| `<|--` | Vererbung (Sub- erbt von Superklasse) |
| `$` | statisches Member · `*` | abstraktes Member |

> Hinweis: Querschnittsklassen der Daten- und UI-Schicht (`IDatabaseConnection`,
> `DatabaseConnection`, ViewModels usw.) sind hier bewusst weggelassen — siehe
> [Architektur in der Dokumentation](Dokumentation.md#5-funktionsblöcke-bzw-architektur).
