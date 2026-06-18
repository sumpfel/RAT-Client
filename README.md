<div align="center">

# 🐀 RAT — Remote Access Topologie (Client)

**Eine WPF-Anwendung, mit der man sein reales Netzwerk zentral visualisieren, erreichen und auslesen kann — angelehnt an Cisco Packet Tracer.**

Backend: <https://github.com/sumpfel/RAT-Backend>

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D6)](#voraussetzungen)

</div>

---

## Was kann RAT?

Geräte werden als Knoten auf einer Topologie-Leinwand dargestellt und mit Kabeln verbunden.
Per Mausklick öffnet man Shells (SSH/Telnet), überträgt Dateien (SFTP/SCP) oder liest/setzt
SNMP-Werte. Das lokale Netzwerk lässt sich automatisch erkennen. Alles wird zentral über ein
Backend gespeichert; ein Offline-Modus ist ebenfalls vorhanden.

---

## ⚡ Schnellstart (Quickstart Guide)

Mit diesem Guide kannst du RAT und das dazugehörige Backend in wenigen Minuten lokal starten.

### 1. Backend klonen und starten (erforderlich für alle Features)
Das Backend persistiert die Topologie und Benutzerdaten. Ohne Backend kannst du RAT zwar im Offline-Modus starten, es stehen dir aber weniger Features zur Verfügung.

Öffne eine **Eingabeaufforderung (CMD)** unter Windows und führe diesen Befehl aus, um das Backend zu klonen, einzurichten und zu starten:

```cmd
git clone https://github.com/sumpfel/RAT-Backend.git && cd RAT-Backend && set HOST=0.0.0.0 && set PORT=8080 && run.bat
```

> [!TIP]
> **Zukünftige Starts:** Nach der Erstinstallation über `run.bat` musst du zukünftig nur noch die Datei `Main.py` starten (z. B. via `python Main.py` oder `python src/Main.py` im Backend-Ordner).

Das Backend läuft nun unter `http://127.0.0.1:8080` (oder über die IP deines Rechners).

### 2. Frontend (Client) herunterladen und ausführen
Für den WPF-Client wird Windows 10/11 benötigt.

1. Lade das fertige Paket der Version **[v1.0.0](https://github.com/sumpfel/RAT-Client/releases/tag/v1.0.0)** herunter (oder nutze den Ordner `bin/` direkt aus diesem Repository).
2. Starte die **`RAT_WPF.exe`**.
3. **Erststart-Einrichtung:**
   * Beim ersten Start fragt dich RAT, ob eine **Desktop-Verknüpfung** angelegt werden soll.
   * Zudem wirst du gefragt, ob **NMAP** installiert werden soll (wird beim ersten Start von RAT installiert, falls nicht vorhanden). Bestätige dies, da NMAP zwingend für die automatische Netzwerk-Erkennung (Discovery) und Port-Scans benötigt wird.

### 3. Verbinden und Einloggen

Wenn das Backend und der Client laufen, melde dich im Client-Login-Screen mit folgenden Daten an:

* **Server-Adresse:** `http://127.0.0.1:8080`
* **Automatischer Admin-Benutzer (wird automatisch angelegt):**
  * **Username:** `admin`
  * **Password:** `admin`

> [!NOTE]
> **Offline-Modus:** Alternativ kannst du auf dem Login-Screen auf **„Use locally only“** (Ohne Backend ausführen) klicken. Beachte jedoch, dass im Offline-Modus deine Topologien und Einstellungen nach dem Schließen der App nicht gespeichert werden.

---

## ✨ Funktionen

### Für alle Benutzer

| Bereich | Funktion |
|---------|----------|
| **Topologie** | Geräte (PC, Router, Switch, Server, Client, Hub, Access Point, Internet-Cloud) per **Drag & Drop** aus der Seitenleiste auf die Leinwand ziehen und frei platzieren. |
| **Verbindungen** | Geräte mit **Kabeln** verbinden (Connection-Tool). Kabel **doppelklicken** zum Bearbeiten (Name, Typ kabelgebunden/WLAN, Geschwindigkeit, Notiz), **Rechtsklick** ebenfalls. WLAN wird gestrichelt gezeichnet. Mit dem **Delete-Tool** löschen. |
| **Navigation** | Leinwand mit der Maus **verschieben** (Drag-to-Pan) und über Scrollbalken bewegen; app-weiter **Zoom 50–300 %** (Einstellungen). |
| **Geräte-Shell** | Auf Knopfdruck **SSH-** oder **Telnet-Shell** öffnen (mit Syntaxhervorhebung); automatischer Verbindungsaufbau, wenn ein Kabel vom eigenen PC und ein passender Login vorhanden ist. |
| **Datei-Transfer** | Dateien per **SFTP/SCP** hoch-/herunterladen, Verzeichnisse auflisten. |
| **SNMP** | **Get / Set / Walk** auf OIDs; **MIB-Browser** mit gängigen Knoten. |
| **Discovery** | **„Discover devices“** scannt das lokale Netz mit **nmap** (+ **tracert** für den Router), erzeugt eine Topologie (Switch in der Mitte, Geräte im Kreis), erkennt den Gerätetyp aus offenen Ports und ergänzt eine **Internet-Cloud**. |
| **Ports** | **„Show ports“** zeigt offene Ports pro Gerät (mit Klarnamen, z. B. „22 - SSH“); ein konfigurierter Login, dessen Port nmap nicht offen sieht, wird **rot** dargestellt. |
| **Interfaces** | Schnittstellen anlegen/bearbeiten/löschen (Name, IP, Subnetzmaske, Speed); **„Show interfaces“** blendet Interface-Namen/IP an den Kabeln ein. |
| **Logins** | Pro Gerät Zugangsdaten (SSH/SFTP/SCP/Telnet) verwalten. |
| **Theme** | **Dark/Light-Mode** mit eigenen Ratten-Vektor-Icons. |
| **Konto** | Eigenen **Benutzernamen + Passwort** in den Einstellungen ändern (Passwortrichtlinie: ≥ 8 Zeichen, Buchstabe + Ziffer). |
| **Diagnose** | **„Open log folder“** öffnet die Logdateien (pro Start eine neue, die neuesten 3 bleiben). |
| **Offline** | **„Use locally only“** auf dem Login-Screen — ohne Backend, nichts wird gespeichert. |

### Zusätzlich für Administratoren

| Funktion | Beschreibung |
|----------|--------------|
| **Benutzerverwaltung** | **„👤 Users“** in der Topbar (nur für Admins sichtbar): Benutzer **anlegen, bearbeiten** (Name, Passwort, Admin-Flag, „CanCreate“) und **löschen**. |
| **Rechte vergeben** | Im **Access-Control-Tab** eines Geräts pro Benutzer eine Stufe vergeben: **Hidden / See / Edit / Admin / Owner**. |
| **Globaler Admin** | Besitzt implizit **Owner-Rechte auf jedem Gerät** und darf alles löschen. |

> **Rechte-Stufen:** `Hidden` (nicht sichtbar) · `See` (ansehen) · `Edit` (ändern, nicht löschen) ·
> `Admin` (niedrigeren Nutzern Rechte vergeben) · `Owner` (löschen, Owner vergeben).

---

## 🚀 Installation & Start

### Variante A — fertige Anwendung (`bin/`)

Im Ordner **`bin/`** liegt eine **eigenständige** (self-contained) Windows-x64-Anwendung —
es muss **kein .NET installiert** sein.

1. Ordner `bin/` herunterladen/kopieren.
2. **`RAT_WPF.exe`** starten.
3. Beim **Erststart** fragt RAT, ob eine **Desktop-Verknüpfung** (mit RAT-Icon) angelegt und
   **nmap** installiert werden soll (für die Discovery; kann abgelehnt werden — dann ist nur der
   „Discover“-Button deaktiviert).
4. **Anmelden** (Server-IP/Port + Benutzer) **oder** **„Use locally only“** für den Offline-Modus.

### Variante B — selbst bauen

```powershell
# Voraussetzung: .NET 9 SDK
git clone https://github.com/sumpfel/RAT-Client.git
cd RAT-Client

# starten
dotnet run --project src/RATClient/RAT_WPF/RAT_WPF.csproj

# ODER: eigenständiges bin/ erzeugen (exe + alle DLLs + Laufzeit)
dotnet publish src/RATClient/RAT_WPF/RAT_WPF.csproj -c Release -r win-x64 --self-contained true -o bin
```

### Backend

Für den Mehrbenutzerbetrieb wird das **RAT-Backend** (FastAPI) benötigt
(<https://github.com/sumpfel/RAT-Backend>). Standard-Adresse im Client: `http://127.0.0.1:8000` (bzw. `http://127.0.0.1:8080` falls über den CMD-Schnellstart ausgeführt).
Ohne Backend funktioniert der **Offline-Modus** („Use locally only“).

---

## 🧩 Voraussetzungen

- **Windows 10/11** (WPF + WMI-Geräteinfos sind Windows-spezifisch).
- Für **Variante B**: **.NET 9 SDK**.
- Optional zur Laufzeit: **nmap** (Discovery) und **tracert** (Windows-Bordmittel).
- Details/Versionen siehe [`doc/markdown/Dokumentation.md`](doc/markdown/Dokumentation.md).

---

## 📚 Dokumentation

- **Projektdokumentation:** [`doc/markdown/Dokumentation.md`](doc/markdown/Dokumentation.md) (+ PDF)
- **API des Backends:** [`doc/markdown/API_documentation.md`](doc/markdown/API_documentation.md)
- **Klassendiagramm:** [`doc/markdown/ClassDiagram.md`](doc/markdown/ClassDiagram.md)
- **KI-Einsatz (Prompt-Log):** [`doc/markdown/AI_usage.md`](doc/markdown/AI_usage.md)

---

## 📈 Entwicklungsstand

- [x] Planung
- [x] Entwicklung
- [x] Launch (eigenständiges `bin/`)
- [ ] Verbesserungen
- [ ] Updates
