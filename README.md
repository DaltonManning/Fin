# Fin

Console app that drives ABB Control Builder M Professional through its COM API (`CONTROLBUILDERLib`) and dumps engineering data — the project tree, diagram type definitions, hardware/controller info, and application names — to XML files on disk.

## Requirements

- Windows
- .NET Framework 4.7.2
- ABB Control Builder M Professional installed and registered (provides the `CONTROLBUILDERLib` COM server)
- A project **loaded in the Control Builder GUI** at runtime — Fin attaches to the running instance, it doesn't open project files itself

The build itself does **not** require Control Builder to be installed — `lib/Interop.CONTROLBUILDERLib.dll` is checked in, so any Windows machine with Visual Studio / MSBuild can compile the project. The exe will only do useful work on a machine where Control Builder is registered and running.

## Build

Open `Fin.sln` in Visual Studio 2022 and build, or from a Developer Command Prompt:

```
msbuild Fin.sln /p:Configuration=Debug
```

Output: `bin\Debug\Fin.exe`.

## Usage

```
Fin [extractor ...]

  tree           full project tree dump
  diagrams       diagram instances + per-type definitions + FDCodeBlock collection
  hardware       per-controller hardware unit XML
  applications   applications tree + names list
  all            (default with no args) runs every extractor in order
```

Examples:

```
Fin.exe                      # run everything
Fin.exe diagrams             # diagrams only
Fin.exe hardware tree        # multiple, in the order given
```

## Output

Everything is written under `bin\Debug\output\`:

```
output/
  project/        ProjectTree.xml
  diagrams/       <Type>.xml (one per unique diagram type)
                  Diagrams.xml          all <DiagramInstance> nodes
                  Types.xml             list of types referenced by instances
                  DiagramTypeTotal.xml  every fetched type, concatenated
                  FDCodeBlock.xml       every <FDCodeBlock> across all types
  hardware/       HardwareTree.xml + <ControllerName>.xml per controller
  applications/   Applications.xml + application_names.txt
```

A run log (`Fin_yyyyMMdd_HHmmss.log`) is written next to `Fin.exe`. Per-extractor failures are logged and the run continues; per-item failures inside an extractor (e.g. one diagram type fails to fetch) are also logged and skipped.

Re-runs overwrite files at the same paths in place. They do **not** delete files from prior runs whose source no longer exists in Control Builder — wipe `output\` between runs if you need a guaranteed-clean state.

## What it parses vs. dumps raw

Fin is mostly a structured XML dumper. It walks the project tree to drive subsequent COM calls (finding diagram types, controller names, application names) and produces a couple of derived aggregates (`FDCodeBlock.xml`, `application_names.txt`). It does **not** crack open the contents of FDCodeBlocks, hardware unit modules, or application-internal alarm/variable definitions — those land on disk as XML and are left for a downstream consumer to parse.

## Architecture

- `Program.Main` — parses args, opens one `ControlBuilderSession`, dispatches each requested extractor.
- `ControlBuilderSession` — `IDisposable` wrapper around `CBOpenIF`. Guarantees `Marshal.ReleaseComObject` runs in `Dispose`. All COM access goes through `session.Client`.
- `Log` — console + per-run logfile. `Log.Error(msg, ex)` adds the HRESULT for `COMException`.
- `Paths` — centralizes the `output\{...}` layout.
- `IExtractor` — `Name` + `Run(session)`. One implementation per concern under `Extractors/`.

Adding a new extractor: write a class implementing `IExtractor`, register it in the `Available` dictionary in `Program.cs`, add a `<Compile Include>` line to `Fin.csproj`. Done.
