# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this project is

`Fin` is a .NET Framework 4.7.2 console app that drives ABB Control Builder M Professional via its COM API (`CBOpenIFHelper` / `CONTROLBUILDERLib`) to dump engineering data — the project tree, diagram type definitions, hardware/controller info, and application names — to XML files on disk.

It is COM-driven and only runs on a Windows machine where Control Builder M Professional is installed and registered, with a project loaded in Control Builder at runtime. There is no test suite, no CI, and no headless/mock mode.

## Building and running

Classic (non-SDK) `.csproj` with `packages.config` — use MSBuild from a Visual Studio install, not `dotnet build`.

- Build: open `Fin.sln` in Visual Studio 2022, or from a Developer Command Prompt:
  - `msbuild Fin.sln /p:Configuration=Debug` (or `Release`)
- Run: launch `bin\Debug\Fin.exe` on a workstation with Control Builder installed and a project open.

CLI:

```
Fin [extractor ...]

  tree           project tree dump
  diagrams       diagram instances + type definitions + FDCodeBlocks
  hardware       per-controller hardware unit XML
  applications   applications tree + names list
  all            (default with no args) runs every extractor
```

Output is written under `bin\Debug\output\{project,diagrams,hardware,applications}\`. A run log is written next to the exe as `Fin_yyyyMMdd_HHmmss.log`.

## Architecture

The app is a thin orchestrator over a set of independent extractors:

- `Program.Main` parses args, opens a single `ControlBuilderSession`, then calls each requested extractor's `Run(session)`. A failure in one extractor is logged and execution continues to the next.
- `ControlBuilderSession` (`IDisposable`) wraps the lifetime of `CBOpenIF` and guarantees `Marshal.ReleaseComObject` runs in `Dispose`. **All COM access goes through `session.Client`** — never new up `CBOpenIF` directly elsewhere.
- `Log` writes to console and to a per-run log file. `Log.Error(message, ex)` adds the HRESULT for `COMException` automatically.
- `Paths` centralizes the output layout under `<exe>\output\` and creates each subfolder on demand.
- `IExtractor` (`Name`, `Run(session)`) is implemented by each class under `Extractors/`. Adding a new extractor is: create a class implementing `IExtractor`, add a `<Compile Include>` to `Fin.csproj`, register it in the `Available` dictionary in `Program.cs`.

The Control Builder XML uses namespace `"CBOpenIFSchema3_0"` for LINQ-to-XML queries. `XmlDocument.GetElementsByTagName` ignores namespaces — `DiagramExtractor` uses the latter (matches the upstream node names), `HardwareExtractor`/`ApplicationExtractor` use the former (need namespaced descendants). Match the existing style in the file you're editing.

## Things to know before changing things

- COM references are embedded interop (`EmbedInteropTypes=True`), so the exe doesn't ship interop assemblies. Both `CBOpenIFHelper` and `CONTROLBUILDERLib` must be registered on the build machine — they ship with Control Builder M Professional.
- `packages.config` is intentionally empty. The project previously pulled in EPPlus / Office interop for Excel output that was never written; if you add Excel output, re-add the package and Office COM ref.
- Adding a `.cs` file requires both creating the file and adding a `<Compile Include>` line to `Fin.csproj`. Visual Studio does this automatically when you add via the IDE.
- Extractor failures must not bubble out of `Run` — catch within the per-item loop and log via `Log.Error(msg, ex)` so other items continue. The top-level `Program.Main` also catches per-extractor exceptions as a safety net.
