# Dokumentation RAT

## Projekttagebuch

Projekttagebuch wer wann was gemacht hat (ohne DBI)

### 21.5.2026

- Christof
    - Initial Commit
    - Basis .NET Struktur
    - Basis mancher Logik Layer Klassen - noch nicht Komplett implementiert

- Beide
    - Projektplanung und UML

### 27.05.2026

- Tobias
    - Basisstruktur von Logik Layer Klassen

### 28.05

- Tobias
    - Basisstruktur von Account und User

- Christof
    - Basisstruktur von NetworkObjectInterface
    - Beginn implementierung von IDatabaseConnection (also einfach die Funktionen vorgeben welche vorhanden sind)

### 30.05

- Christof
    - Implementierung von NetworkObject
    - SNMP-Implementierung bei NetworkObject

### 31.05

- Christof
    - Beginn von DatabaseMock implementierung
    - Validationen von Port und IP
- Tobias
    - MVVM Basisstruktur
    - Erstellen von LoginView und LoginViewModel - nur noch nicht mit API verbunden
    - Erstellen der Grundstruktur von TopologyView und TopologyViewModel (Hauptfenster mit Canvas für NetworkObjects)

### 2.06.2026

- Christof
    - Erstellen von NetworkObjectSettingsWindow
    - Infos vom eigenem Gerät (GPU, RAM, CPU, Name)
    - Vebesserung vom Code-Layout

### 3.06.2026

- Christof
    - Erstellen von UpdateInterfaceWindow für das Hinzufügen/Ändern von Interfaces beim NetworkObject
    - Änderungen in NetworkObjectSettingsWindow
    - Bug Fixes bei SSH
    - Erstellen von SSH Shell Streams

- Tobias
    - Erstellen von Drag and Drop in TopologyView
        - Man kann NetworkObjects von einer Listview in den Canvas ziehen
    - Bug Fixes
        - NetworkObject war gleichzeitig Klasse und Namespace, Namespace wurde umbenannt
    - Verknüpfung NetworkObject und SettingsWindow

### 10.06.2026

- Christof mit "Unterstützung" von Claude
    - Erstellen von Debug Mode
        - Überspringt LoginView
    - Bugfixes

- Tobias
    - Restrukturierung von TopologyView/ViewModel
        - Canvas Usercontroll ist jetzt einfach nur ein Canvas Element, weniger Komplex
        - NetworkObjects werden jetzt automatisch aus einer Liste per Binding auf dem Canvas gezeichnet
        - Beim Drag and Drop werden sie in die Liste hinzugefügt mit der Position wo sie gedropt werden
    - NetworkObjects speichern jetzt ihre Position (X, Y)

### 11.06.2026

- Christof mit "Unterstützung" von Claude
    - Erstellen von Light/Darkmode
    - Erstellen von RAT icons
    - Erstellen von SettingsWindow
    - Erstellen von MIB Browser
    - Erstellen von Permissions
    - Hinzufügen von löschen von NetworkObjects
    - Erstellen von SelectInterfaceWindow
    - Redesign von Interface/Login
    - Bearbeitung von Access Controll
    - Bearbeitung von SSH Terminal, jetzt mit Farben


- Tobias
    - Erstellen von "Tools" in TopologyView
        - Werden an enum in TopologyViewModel gebinded
    - Verbesserung von löschen von NetworkObjects
        - Jetzt über Tool
    - Erstellen von NetworkConnection
        - Logik ist da, werden aber noch nicht gezeichnet

### 13.06.2026

- Tobias
    - Erstellen von NetworkConnectionViewModel
    - Erstellen von Zeichnen von NetworkConnectionViewModel
        - Werden geupdated wenn sich Source oder Target NetworkObject bewegen
        - Funktioniert per Binding an ObservableCollection in TopologyViewModel

### 14.06.2026

- Tobias
    - Bugfixes
        - Löschen von NetworkObjectVMs vom Canvas löscht nicht NetworkConnection in dem Interface des anderen NetworkObjects

### 16.06.2026

- Christof mit "Unterstützung" von Claude
    - Erstellen von Verbindung mit Datenbank
    - TODO

- Tobias
    - Fehler bei Binding mit Namen von NetworkObjectVM/View+

### 17.06.2026

- Christof mit "Unterstützung" von Claude
    - TODO

## Projektplanung (Lastenheft)

## Umsetzungsdetails (Pflichtenheft)

### Softwarevoraussetzungen

### Funktionsblöcke bzw. Architektur

### Detailliert Beschreibung der Umsetzung

### Mögliche Probleme und Lösung

## Quellen

- RAT-Icons: Claude
- RAT-Logo: Tobias

## KI Teil

### Vorgehensweise der KI

- Was habt ihr ändern lassen?
- Was waren die Ziele?
- Welche AI Tools wurden eingesetzt?
    - Claude (Agent)
    - Gemini
- Welche Kosten sind dabei entstanden?

### Reflektion KI

- Wo war AI hilfreich?
- Was würdet ihr nächstes mal anders machen?
- Was hat gut/schlecht funktioniert?