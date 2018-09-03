namespace Fucturizr

open Helper
type Tag = string

type Position = int * int

type Size =
| A6Landscape
| A6Portrait
| A5Landscape
| A5Portrait
| A4Landscape
| A4Portrait
| A3Landscape
| A3Portrait
| A2Landscape
| A2Portrait
| LetterLandscape
| LetterPortrait
| LegalLandscape
| LegalPortrait
| Slide43Landscape
| Slide169Landscape

type ColorHex = string

type Shape =
| Default
| Box
| RoundedBox
| Circle
| Ellipse
| Folder
| Hexagon
| Person
| Cylinder
| Pipe
| WebBrowser
| MobileApp
| DesktopApp
| Robot

type ElementStyle = {
    Tag:Tag
    Width:int
    Height:int
    Background:ColorHex
    Color:ColorHex
    FontSize:int
    Opacity:int
    Shape:Shape    
}

type Routing = Dashed | Orthogonal

type RelationshipStyle = {
    Tag:Tag
    Position:Position
    Thickness:int
    Width:int
    Height:int
    Color:ColorHex
    FontSize:int
    Dashed:bool
    Routing:Routing
    Opacity:int    
}

type Style = 
| Element of ElementStyle
| Relationship of RelationshipStyle

type UserElement = {
    Name:string
    Description:string
    Tags:Tag list
    Position:Position
}

type User =
| Person of UserElement
| Mobile of UserElement
| WebBrowser of UserElement
| DesktopApp of UserElement

type SoftwareSystemElement = {
    Name:string
    Description:string
    Tags:Tag list
    Position:Position
    Containers: ContainerElement list
}

and ContainerElement = {
    Name:string
    Description:string
    Technology:string
    Tags:Tag list
    Position:Position
    Components: ComponentElement list
}

and ComponentElement = {
    Name:string
    Description:string
    Technology:string
    Tags:Tag list
    Position:Position
}

type Element =
| User of UserElement
| SoftwareSystem of SoftwareSystemElement
| Container of ContainerElement
| Component of ComponentElement

and SystemViewElement =
| User of User
| System of SoftwareSystemElement

and ContainerViewElement =
| User of User
| System of SoftwareSystemElement
| Container of ContainerElement

type Relationship = {
    Source:Element
    Destination:Element
    Description:string
    Technology:string
    Tags:Tag list
    Vertices:Position list
    Order:int
}

type SystemLandscapeDiagram = {
    Scope:string
    Description:string
    Size:Size
    Elements: SystemViewElement list
    Relationships: Relationship list
    Styles: Style list
}

type SystemContextDiagram = {
    Scope:string
    Description:string
    Size:Size
    Elements: SystemViewElement list
    Relationships: Relationship list
    Styles: Style list
}

type ContainerDiagram = {
    Scope:string
    Description:string
    Size:Size
    Elements: ContainerViewElement list
    Relationships: Relationship list
    Styles: Style list
}

type ElementDiagram = {
    Scope:string
    Description:string
    Size:Size
    Elements: Element list
    Relationships: Relationship list
    Styles: Style list
}

type Diagram = 
| SystemLandscape of SystemLandscapeDiagram
| SystemContext of SystemContextDiagram
| Container of ContainerDiagram
| Component of ElementDiagram
| Dynamic of ElementDiagram

[<AutoOpen>]
module Core =
    let i = ()

module Style =
    let element = {
            Tag = ""
            Width = 0
            Height = 0
            Background = "#000000"
            Color = "#ffffff"
            FontSize = 0
            Opacity = 0
            Shape = Shape.Default  
        }
    let withColor color (style:ElementStyle) = { style with Color = color }
    let withBackground color (style:ElementStyle) = { style with Background = color }
    let asPerson style = { style with Tag = "Person"; Shape = Shape.Person }
    let defaultUser = element |> asPerson |> withBackground "#08427b" |> Style.Element
    let defaultSoftwareSystem = element |> withBackground "#1168bd" |> Style.Element
    let softwareSystemDefaults:Style list = [defaultUser;defaultSoftwareSystem]

[<RequireQualifiedAccess>]
module User =
    let person name desc tags pos = 
        User.Person {
            Name = name
            Description = desc
            Tags = tags
            Position = pos
        }

    let mobile name desc tags pos = 
        User.Mobile {
            Name = name
            Description = desc
            Tags = tags
            Position = pos
        }

    let webBrowser name desc tags pos = 
        User.WebBrowser {
            Name = name
            Description = desc
            Tags = tags
            Position = pos
        }

    let desktopApp name desc tags pos = 
        User.DesktopApp {
            Name = name
            Description = desc
            Tags = tags
            Position = pos
        }

    let name person = 
        match person with
        | Person x -> x.Name    
        | WebBrowser x -> x.Name    
        | Mobile x -> x.Name    
        | DesktopApp x -> x.Name    

[<RequireQualifiedAccess>]
module SoftwareSystem =
    let init name desc tags pos : SoftwareSystemElement = {
        Name = name
        Description = desc
        Tags = tags
        Position = pos
        Containers = []
    }

    let name system =
        match system with
        | SystemViewElement.System x -> x.Name
        | SystemViewElement.User x -> User.name x

[<RequireQualifiedAccess>]
module Relationship =
    let between src desc dest = {
        Source = src
        Destination = dest
        Description = desc
        Technology = ""
        Tags = []
        Vertices = []
        Order = 0
    }
    // api |> Relationship.``with`` "calls" service
    let ``with`` desc dest src = between src desc dest

[<RequireQualifiedAccess>]
module SystemLandscapeDiagram = 
    let init scope desc size : SystemLandscapeDiagram = 
        {
            Scope = scope
            Description = desc
            Size = size
            Elements = []
            Relationships = []
            Styles = Style.softwareSystemDefaults
        }
    let private systemNamesMatch a b =
        (SoftwareSystem.name a) = (SoftwareSystem.name b)

    let addElement (element:SystemViewElement) (diagram:SystemLandscapeDiagram) =
        let update = fun (x) -> if(systemNamesMatch x element) then element else x
        let updated = Update.collectioni update (diagram.Elements |> List.toSeq)|> List.ofSeq |> List.map snd
        if(Update.hasReplaced updated) then
            let elements = updated |> List.map Update.get
            {diagram with Elements = elements}
        else
            let elements = List.append diagram.Elements [element]
            {diagram with Elements = elements}
        
    let addSoftwareSystem (softwareSystem:SoftwareSystemElement) (diagram:SystemLandscapeDiagram) =
        addElement (SystemViewElement.System softwareSystem) diagram

    let addPerson (user:User) (diagram:SystemLandscapeDiagram) =
        addElement (SystemViewElement.User user) diagram