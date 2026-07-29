# VersePulse Architecture

**Version:** 1.0

---

# Overview

VersePulse is built using a layered architecture.

Each layer has a single responsibility and communicates only with the layer directly above or below it.

```
Star Citizen
      │
      ▼
   Game.log
      │
      ▼
  Installation Manager
      │
      ▼
    Log Reader
      │
      ▼
  Parser Pipeline
      │
      ▼
 Telemetry Engine
      │
      ▼
 TelemetryData Model
      │
 ┌────┼──────────────┐
 ▼    ▼              ▼
UI  Overlay      Modules
```

---

# Layers

## Installation Manager

Responsibilities:

- Locate Star Citizen
- Detect installed channels
- Detect active channel
- Locate Game.log
- Validate installation

---

## Log Reader

Responsibilities:

- Open Game.log
- Monitor file changes
- Read appended lines
- Handle reconnects
- Never parse data

---

## Parser Pipeline

Responsibilities:

- Parse raw log lines
- Convert raw text into strongly typed data
- Notify Telemetry Engine of changes

Each parser has a single responsibility.

Examples:

- GameStateParser
- SessionParser
- EnvironmentParser
- LocationParser
- ShipParser
- PerformanceParser

---

## Telemetry Engine

Responsibilities:

- Own application state
- Merge parser output
- Raise events
- Publish TelemetryData

The Telemetry Engine is the single source of truth.

---

## TelemetryData

TelemetryData contains all game state.

Example:

- Installation
- Session
- Environment
- Location
- Ship
- Performance
- Missions
- Player

---

## UI Layer

Responsibilities:

- Display information
- Never parse logs
- Never access Game.log directly

---

## Modules

Modules consume TelemetryData.

Examples:

- Trading
- Mining
- Cargo
- Navigation
- Medical
- Fleet

Modules never communicate directly with Game.log.

---

# Threading

Background Thread

- Installation detection
- Log reading
- Parsing

UI Thread

- ViewModels
- Windows
- Controls

Heavy work must never run on the UI thread.

---

# Design Principles

- Single Responsibility
- Separation of Concerns
- Dependency Injection
- Event-driven updates
- Immutable telemetry where practical
- Modular expansion

---

# Folder Layout

```
src/

Models/
Services/
Telemetry/
Parsers/
UI/
ViewModels/
Views/
```

---

# Data Flow

```
Game.log

↓

LogReader

↓

Parser Pipeline

↓

Telemetry Engine

↓

TelemetryData

↓

UI / Overlay / Modules
```

---

# Future Expansion

The architecture supports future additions without changing existing parsers.

Examples:

- New telemetry parsers
- New UI panels
- New companion modules
- Session recording
- Statistics
- Public API