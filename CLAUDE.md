# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

PTConsole is a .NET 9 console application for project/task time tracking. It combines a CLI command interface (Spectre.Console.Cli) with a live TUI stopwatch/dashboard (Spectre.Console Live rendering).

## Build & Run

```bash
# Build
dotnet build PTConsole.sln

# Run (default launches GUI mode per launchSettings.json)
dotnet run --project PTConsole/PTConsole.csproj -- gui

# Run a CLI command
dotnet run --project PTConsole/PTConsole.csproj -- client list
dotnet run --project PTConsole/PTConsole.csproj -- client create <name> <alias> [--color #hex]
dotnet run --project PTConsole/PTConsole.csproj -- client delete <id>

# EF Core migrations
dotnet ef migrations add <Name> --project PTConsole/PTConsole.csproj
dotnet ef database update --project PTConsole/PTConsole.csproj
```

There are no tests in this project.

## Architecture

**Entry flow:** `Program.cs` → `Startup` (configures DI services and CLI command tree) → Spectre.Console.Cli `CommandApp` dispatches to command classes.

**Key patterns:**
- **Spectre.Console.Cli commands:** Each entity (Client, Project, User, Task) has a group of command classes (Create/List/Delete) in `Commands/`. Commands inherit `AsyncCommand<TSettings>` with nested `Settings` classes for argument binding.
- **Generic repository:** `IRepository<TEntity>` / `Repository<TEntity>` wraps EF Core DbSet. Repositories save immediately (no unit-of-work).
- **DI bridge:** `TypeRegistrar` / `TypeResolver` in `Infrastructure/` bridge `Microsoft.Extensions.DependencyInjection` with Spectre.Console.Cli's DI system.
- **Entity base class:** All models inherit `AbstractEntity` (in `Models/Base/`) providing `Id`, `CreatedAt`, `ChangedAt`.

**Data layer:** EF Core with SQLite (`Db.sqlite` copied to output dir). Connection string is hardcoded in `Program.cs`. Database schema includes: Client, User, Project (FK to Client), Note (FK to User, optional FK to Project), Session (FKs to User and Project), WorkTask (optional FK to Project), and a ProjectUser many-to-many join table.

**Live TUI:** `GuiCommand` launches `GuiContext.Draw()` which uses `AnsiConsole.Live()` with a split layout — FIGlet clock in content area, styled input panel at bottom. This feature is work-in-progress.

## Current State

- Only `gui` and `client` command branches are registered in `Startup.ConfigureCommands()`. The `ProjectCommands`, `UserCommands`, and `TaskCommands` classes exist but are not yet wired into the CLI.
- `ConfigCommand` is a stub (returns 0, does nothing).
- The GUI has no graceful exit mechanism yet.
