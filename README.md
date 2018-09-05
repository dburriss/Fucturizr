# Fucturizr

A DSL for f#cking around with [Structurizr](https://c4model.com).

[![Build status](https://ci.appveyor.com/api/projects/status/05drr0dq7omoru07?svg=true)](https://ci.appveyor.com/project/dburriss/Fucturizr)
[![MyGet CI](https://img.shields.io/myget/dburriss-ci/vpre/Fucturizr.Core.svg)](http://myget.org/gallery/dburriss-ci)
[![NuGet CI](https://img.shields.io/nuget/v/Fucturizr.Core.svg)](https://www.nuget.org/packages/Fucturizr.Core/)

## Features

- [ ] Types to describe diagrams
- [ ] Serialize to JSON
- [ ] Deserialize from JSON
- [ ] High-level strongly typed DSL for defining diagrams
- [ ] Low level data structure that interops with [Structurizr .NET](https://github.com/structurizr/dotnet)
- [ ] Styles
- [ ] Export to Graphviz
- [ ] Export to Structurizr.Dgml
- [ ] Other export formats...

## Sample diagram definition

Below is the definition of a landscape diagram. See the [script file example](/examples/system-landscape.fsx) of using this to generate the Structurizr json.

```fsharp
let contract_management_landscape_diagram =
    system_landscape_diagram "Contract Management" "A test description" Size.A5_Landscape {
        user (A.person "Buyer" "Negotiates policies with suppliers and orders..." (730,230))
        system (A.system "Acme Contract Management System" "Manages contracts negotiated with suppliers" (705,830))
        relationship "Buyer" "Captures contracts" "Acme Contract Management System"
    }
```

This results in [the following json](/examples/system-landscape.json) that can then be [used to generate](https://structurizr.com/express?src=https://raw.githubusercontent.com/dburriss/Fucturizr/master/examples/system-landscape.json) the landscape diagram.

![Contract Management System Landscape Diagram](/assets/contract-management-landscape.png)

## Links

1. [C4 Diagrams](https://c4model.com/)
1. [Structurizr Express](https://structurizr.com/express)
1. [Structurizr.Dgml](https://github.com/merijndejonge/Structurizr.Dgml)
1. [Graphviz](https://graphviz.gitlab.io/)