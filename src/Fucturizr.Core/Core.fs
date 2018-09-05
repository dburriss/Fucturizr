namespace Fucturizr

open Helper
open System
open System
open Newtonsoft.Json.Linq
open Newtonsoft.Json
type Tag = string

type Position = int * int

type Size =
| A6_Landscape
| A6_Portrait
| A5_Landscape
| A5_Portrait
| A4_Landscape
| A4_Portrait
| A3_Landscape
| A3_Portrait
| A2_Landscape
| A2_Portrait
| Letter_Landscape
| Letter_Portrait
| Legal_Landscape
| Legal_Portrait
| Slide43_Landscape
| Slide169_Landscape

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
| User of User
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

[<RequireQualifiedAccess>]
module Element =
    let name (e:Element) =
        match e with
        | Element.User x -> User.name x
        | Element.SoftwareSystem x -> x.Name
        | Element.Container x -> x.Name
        | Element.Component x -> x.Name

    let equal (e1:Element) (e2:Element) = (e1 |> name) = (e2 |> name)

    let ofSystemViewElement (src:SystemViewElement) : Element =
        match src with
        | SystemViewElement.System el -> Element.SoftwareSystem el
        | SystemViewElement.User el -> Element.User el

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

    let equal (r1:Relationship) (r2:Relationship) : bool =
        (Element.equal r1.Source r2.Source) && (Element.equal r1.Destination r2.Destination) && (r1.Description = r2.Description)

[<RequireQualifiedAccess>]
module SystemViewElement =
    let name system =
        match system with
        | SystemViewElement.System x -> x.Name
        | SystemViewElement.User x -> User.name x

    let equal s1 s2 = (s1 |> name) = (s2 |> name)    

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

    let addElement (element:SystemViewElement) (diagram:SystemLandscapeDiagram) =
        let update = fun (x) -> if(SystemViewElement.equal element x) then element else x
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

    let addRelationship (relationship:Relationship) (diagram:SystemLandscapeDiagram) =
        if(diagram.Relationships |> List.exists (fun x -> Relationship.equal x relationship) |> not) then 
            let rels = List.append diagram.Relationships [relationship]
            {diagram with Relationships = rels}
        else diagram

    let findElementByName name (diagram:SystemLandscapeDiagram) = 
        diagram.Elements |> List.tryFind (fun x -> (SystemViewElement.name x) = name)
  
module DSL =
    type SystemLandscapeDiagramBuilder internal (scope, desc, size) =
        member __.Yield(_) : SystemLandscapeDiagram = 
            SystemLandscapeDiagram.init scope desc size

        [<CustomOperation("user")>]
        member __.Person(diagram, user) : SystemLandscapeDiagram =
            match user with
            | User.DesktopApp _ -> diagram |> SystemLandscapeDiagram.addElement (SystemViewElement.User user)
            | User.Mobile _ -> diagram |> SystemLandscapeDiagram.addElement (SystemViewElement.User user)
            | User.Person _ -> diagram |> SystemLandscapeDiagram.addPerson (user)
            | User.WebBrowser _ -> diagram |> SystemLandscapeDiagram.addElement (SystemViewElement.User user)
            
        [<CustomOperation("system")>]
        member __.System(diagram, system) : SystemLandscapeDiagram =
            diagram |> SystemLandscapeDiagram.addSoftwareSystem system

        [<CustomOperation("relationship")>]
        member __.Relationship(diagram, source, description, destination) : SystemLandscapeDiagram =
            let srcElOpt = diagram |> SystemLandscapeDiagram.findElementByName source
            let destElOpt = diagram |> SystemLandscapeDiagram.findElementByName destination
            match srcElOpt,destElOpt with
            | _,None -> diagram
            | None,_ -> diagram
            | Some srcEl, Some destEl ->
                let relationship = Relationship.between (srcEl |> Element.ofSystemViewElement) description (destEl |> Element.ofSystemViewElement)
                diagram |> SystemLandscapeDiagram.addRelationship relationship        

    let system_landscape_diagram scope desc size = SystemLandscapeDiagramBuilder(scope,desc,size)

[<RequireQualifiedAccess>]
module Json =
    
    open Newtonsoft.Json
    open Newtonsoft.Json.Linq
    open Newtonsoft.Json.Serialization
    open Microsoft.FSharp.Reflection

    type JComponent = {
        Type:string
        Name:string
        Description:string
        Technology:string
        Tags:string
        Position:string
    }

    type JContainer = {
        Type:string
        Name:string
        Description:string
        Technology:string
        Tags:string
        Position:string
        Components: JComponent[]
    }

    type JElement = {
        Type:string
        Name:string
        Description:string
        Tags:string
        Position:string
        Containers: JContainer[]
    }

    type JSystemLandscape = {
        Type:string
        Scope:string
        Description:string
        Size:string
        Elements:JElement[]
    }

    let private toString (x:'a) = 
        match FSharpValue.GetUnionFields(x, typeof<'a>) with
        | case, _ -> case.Name

    let private fromString<'a> (s:string) =
        match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name = s) with
        |[|case|] -> Some(FSharpValue.MakeUnion(case,[||]) :?> 'a)
        |_ -> None
    
    let private positionToString ((x,y):Position) =
        sprintf "%i,%i" x y
    
    let private tagsToString (tags:Tag list) = 
        tags |> List.fold (+) "," |> (fun s -> s.Substring(0, s.Length - 1))
    let private userElToJElement (userEl:UserElement) : JElement =
        {
            Type = "Person"
            Name = userEl.Name
            Description = userEl.Description
            Tags = userEl.Tags |> tagsToString
            Position = userEl.Position |> positionToString
            Containers = [||]
        }
    let private serializerSettings = 
        let s = JsonSerializerSettings()
        s.ContractResolver <- new CamelCasePropertyNamesContractResolver()
        s
    let private serializeSystemLandscapeDiagram (diagram:SystemLandscapeDiagram) =
        
        let toJElement (el:SystemViewElement) : JElement =
            match el with
            | SystemViewElement.System x -> 
                {
                    Type = "Software System"
                    Name = x.Name
                    Description = x.Description
                    Tags = x.Tags |> tagsToString
                    Position = x.Position |> positionToString
                    Containers = [||]
                }
            | SystemViewElement.User x -> 
                match x with
                | User.DesktopApp y -> userElToJElement y
                | User.Mobile y -> userElToJElement y
                | User.Person y -> userElToJElement y
                | User.WebBrowser y -> userElToJElement y

        let data : JSystemLandscape = {
            Type = "System Landscape"
            Scope = diagram.Scope
            Description = diagram.Description
            Size = diagram.Size |> toString
            Elements = diagram.Elements |> List.map toJElement |> List.toArray
        }
        JsonConvert.SerializeObject(data, serializerSettings)
    let serialize (diagram:Diagram) =
        match diagram with
        | Diagram.SystemLandscape d -> serializeSystemLandscapeDiagram d
        | _ -> failwith "NotImplemented"