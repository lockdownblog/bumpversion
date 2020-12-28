bumpversion
===========

## Overview  

Version-bump your software with a single command!

A small command line tool to simplify releasing software by updating all
version strings in your source code by the correct increment. Also creates
commits and tags.

### Install  

Install the tool via the *dotnet* cli:  

```bash
dotnet tool install --global BumpVersion
```

### Usage  

Everything starts with a file named `.bumpversion.cfg` containing at least one section:

```toml
[bumpversion]
current_version = "0.0.5"
tag = true
commit = true
```

This section tells `bumpversion` what the current version of your software is, it also specifies wether the tool is going to create a commit or not, also wether a tag is going to be created once the commit is done.

```toml
[bumpversion.file.0]
file = "BumpVersion/BumpVersion.csproj"
search = "<AssemblyVersion>{current_version}</AssemblyVersion>"
replace = "<AssemblyVersion>{new_version}</AssemblyVersion>"

[bumpversion.file.1]
file = "BumpVersion/BumpVersion.csproj"
search = "<FileVersion>{current_version}</FileVersion>"
replace = "<FileVersion>{new_version}</FileVersion>"

[bumpversion.file.2]
file = "BumpVersion/BumpVersion.csproj"
search = "<Version>{current_version}</Version>"
replace = "<Version>{new_version}</Version>"
``` 
