![bumpversion](Images/GitHub-VersionBump.png)

[![Nuget](https://img.shields.io/nuget/v/bumpversion?color=B159FE&logoColor=84EBD4&style=flat-square)](https://www.nuget.org/packages/BumpVersion/) ![Checks](https://img.shields.io/github/checks-status/lockdownblog/bumpversion/main?color=84EBD4&logoColor=84EBD4&style=flat-square)


## Overview

Version-bump your software with a single command!

A small command line tool to simplify releasing software by updating all version strings in your source code by the correct increment. Also creates
commits and tags.

### Install

Install the tool via the *dotnet* cli:

```bash
dotnet tool install --global BumpVersion
```

**Attention**: you mau need to add the tools directory to your path:  

*macOS/Linux*:  
```bash
export PATH="$PATH:/root/.dotnet/tools"
```

### Usage

Everything starts with a file named `.bumpversion.cfg` containing at least one section:

```toml
[bumpversion]
current_version = "0.0.5"
commit = true
tag = true
```

#### Explanation:

 - `current_version` - *Required*: The current version of your software, a string in the form *{major}.{minor}.{patch}*.
 - `commit` - *Optional, default is false*: A boolean flag that tells the script wether to create a commit containing the changes to all the files.
 - `tag` - *Optional, default is false* (only used if `commit` is true): A boolean flag that tells the script wether to create a tag pointing to the commit containing the changes.

Then we can use the tool like this:  

```bash
bumpversion [major|minor|patch]
```

from the root of your repository to increase the part of the version you desire to change, for example, taking into account the previous file: 
 - running `bumpversion patch`, would result in the version being changed from `0.0.5` to `0.0.6`.
 - running `bumpversion minor`, would result in the version being changed from `0.0.5` to `0.1.0`.
 - running `bumpversion major`, would result in the version being changed from `0.0.5` to `1.0.0`.

#### Modifying additional files  

Obviously, changing just the configuration file is not that useful. For example, if you are developing a .NET project, you may want to change your `**proj` files, for each file you want to change you'll need to add an entry like the following to the `.bumpversion.cfg` file:  

```toml
[bumpversion.file.0]
file = "BumpVersion/BumpVersion.csproj"
search = "<AssemblyVersion>{current_version}</AssemblyVersion>"
replace = "<AssemblyVersion>{new_version}</AssemblyVersion>"
```

#### Explanation
 - `[bumpversion.file.X]`: the section header. `X` should be an increasing sequence of integers, starting at 0.
 - `file` - *Required*: Relative path (from the root of the repo) that should be modified.
 - `search` - *Optional, default is **{current_version}***: The string to search for when replacing the version values.
 - `replace` - *Optional, default is **{new_version}***: The string to replace woth when replacing the version values.

For a slightly more complex example, see the `.bumpversion.cfg` file in this repo.



