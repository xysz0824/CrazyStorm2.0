# CrazyStorm 2.0 Docs

This directory contains two parts:

- `source/`: Sphinx source files (organized by language)
- `zh-cn/`, `en/`: prebuilt HTML (opened directly at runtime)

## Directory Layout

- `Docs/index.html`: documentation entry page (language navigation, generated on each build)
- `Docs/zh-cn/index.html`: Chinese tutorial homepage (current primary version)
- `Docs/en/index.html`: English placeholder page

## Build

If Sphinx is not installed yet, you can install and build automatically:

```powershell
cd Docs
.\build.ps1 -InstallSphinx
```

Manual installation:

```powershell
python -m pip install sphinx
```

```powershell
cd Docs
.\build.ps1
```

`Docs/index.html` is generated automatically at build time and should not be edited manually.

If `-Language` is not specified, all languages are built by default (equivalent to `-Language all`).

Build all languages:

```powershell
.\build.ps1 -Language all
```

Clean and rebuild:

```powershell
.\build.ps1 -Clean
```

Clean build output only (no rebuild):

```powershell
.\build.ps1 -CleanOnly
```

## Maintenance Notes

- Maintain Chinese content in `source/zh-cn/` first, then generate HTML.
- When adding a new language, create the same directory structure under `source/<lang>/`.
- Root language links in `Docs/index.html` are generated from `source/<lang>` and only include languages whose `Docs/<lang>/index.html` exists.
- The application opens the matching `index.html` based on system language first; if missing, it falls back to Chinese and then the root entry.
