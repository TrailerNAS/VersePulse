# Contributing to VersePulse

Thank you for your interest in contributing to VersePulse.

---

# Development Workflow

1. Create a feature branch.
2. Make your changes.
3. Build the solution.
4. Test your changes.
5. Update documentation if needed.
6. Commit using a descriptive message.
7. Submit a Pull Request.

---

# Branch Naming

```
feature/<feature-name>

bugfix/<issue>

hotfix/<issue>

release/<version>
```

Examples:

```
feature/telemetry-engine

feature/location-parser

bugfix/game-log-reader

release/v0.4.0
```

---

# Commit Messages

Examples:

```
Added Installation Manager

Implemented Telemetry Engine

Added Location Parser

Fixed Game.log reconnect

Updated Documentation
```

---

# Coding Standards

- .NET 8
- C#
- Async where appropriate
- One responsibility per class
- Avoid duplicate code
- No magic strings
- Meaningful method names
- XML documentation on public APIs where practical

---

# Pull Requests

Every Pull Request should:

- Build successfully
- Contain one logical feature
- Include documentation updates
- Keep existing functionality working

---

# Documentation

Documentation is maintained alongside the code.

Update documentation whenever architecture, behavior, or public APIs change.

---

# Testing

Before submitting changes:

- Build successfully
- Verify existing functionality
- Verify new functionality
- Check for compiler warnings

---

# Project Philosophy

VersePulse values:

- Simplicity
- Performance
- Reliability
- Maintainability
- Respect for Star Citizen's Terms of Service

Every contribution should support these goals.