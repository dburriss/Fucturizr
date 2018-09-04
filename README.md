# Fucturizr

A DSL for f#cking around with [Structurizr](https://c4model.com).

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

```fsharp
let test_diagram = 
            system_landscape_diagram "a-landscape" "Just a test" Size.A5_Landscape {
                user (A.person "a-person")
                system (A.system "a-system")
                relationship "a-person" "Uses" "a-system"
        }
```

## Links

1. [C4 Diagrams](https://c4model.com/)
1. [Structurizr Express](https://structurizr.com/express)
1. [Structurizr.Dgml](https://github.com/merijndejonge/Structurizr.Dgml)
1. [Graphviz](https://graphviz.gitlab.io/)