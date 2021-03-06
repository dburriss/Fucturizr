﻿namespace Tests

module A =

    open System
    open Fucturizr
    let landscape = SystemLandscapeDiagram.init "a-landscape" "A test description" Size.A5_Landscape

module SystemLandscapeTests =

    open Xunit
    open Swensen.Unquote
    open Fucturizr    
    open DSL
    open Newtonsoft.Json
    open Newtonsoft.Json.Linq

    [<Fact>]
    let ``Can create a system landscape`` () =
        let landscape = A.landscape
        test <@ landscape.Description = "A test description" @>

    [<Fact>]
    let ``Can add a software system to landscape as a view element`` () =
        let system = A.system "a-system" "A test description" (0,0)
        let landscape = 
            A.landscape
            |> SystemLandscapeDiagram.addSoftwareSystem system
        test <@ landscape.Elements = [SystemViewElement.System system] @>


    [<Fact>]
    let ``Can add a person to landscape as a view element`` () =
        let person = A.person "a-person" "A test description" (0,0)
        let landscape = 
            A.landscape
            |> SystemLandscapeDiagram.addPerson person
        test <@ landscape.Elements = [SystemViewElement.User person] @>

    [<Fact>]
    let ``Adding an element with same name just replaces it`` () =
        let person1 = A.person "a-person" "A person" (0,0)
        let person2 = A.person "a-person" "Another person" (0,0)
        let landscape = 
            A.landscape
            |> SystemLandscapeDiagram.addPerson person1
            |> SystemLandscapeDiagram.addPerson person2
        test <@ landscape.Elements = [SystemViewElement.User person2] @>

    [<Fact>]
    let ``Adding 2 elements with different name just replaces it`` () =
        let person1 = A.person "a-person" "A test description" (0,0)
        let person2 = A.person "a-person2" "A test description" (0,0)
        let landscape = 
            A.landscape
            |> SystemLandscapeDiagram.addPerson person1
            |> SystemLandscapeDiagram.addPerson person2
        test <@ landscape.Elements = [SystemViewElement.User person1;SystemViewElement.User person2] @>

    [<Fact>]
    let ``Adding a relationship to a system landscape links the elements`` () =
        let system = A.system "a-system" "A test description" (0,0)
        let person = A.person "a-person" "A test description" (0,0)
        let relationship = Relationship.between (person  |> Element.User) "Uses" (system |> Element.SoftwareSystem)
        let landscape = 
            A.landscape
            |> SystemLandscapeDiagram.addSoftwareSystem system
            |> SystemLandscapeDiagram.addPerson person
            |> SystemLandscapeDiagram.addRelationship relationship
            
        test <@ landscape.Relationships = [relationship] @>

    [<Fact>]
    let ``Adding 2 relationships to a system landscape does not result in duplicates`` () =
        let system = A.system "a-system" "A test description" (0,0)
        let person = A.person "a-person" "A test description" (0,0)
        let relationship1 = Relationship.between (person|> Element.User) "Uses" (system |> Element.SoftwareSystem)
        let relationship2 = Relationship.between (person|> Element.User) "Uses" (system |> Element.SoftwareSystem)
        let landscape = 
            A.landscape
            |> SystemLandscapeDiagram.addSoftwareSystem system
            |> SystemLandscapeDiagram.addPerson person
            |> SystemLandscapeDiagram.addRelationship relationship1
            |> SystemLandscapeDiagram.addRelationship relationship2
        test <@ List.length landscape.Relationships = List.length [relationship1] @>

    [<Fact>]
    let ``Using builer constructs diagram`` () =
        let ``person`` = A.person "a-person" "A test description" (0,0)
        let test_diagram = 
            system_landscape_diagram "a-landscape" "Just a test" Size.A5_Landscape {
                user ``person``
                system (A.system "a-system" "A test description" (0,0))
                relationship "a-person" "Uses" "a-system"
        }

        test <@ test_diagram.Elements |> List.length = 2 @>
        test <@ test_diagram.Relationships |> List.length = 1 @>

    let stringPropertyContains (property:string) (value:string) (j:JObject) = 
        let jp = j.Property(property)
        if(isNull jp) then failwithf "%s was not found in %s" property (j.ToString(Formatting.Indented))
        else jp.Value.ToString(Formatting.None).Contains(value)
    
    let test_diagram = 
            system_landscape_diagram "a-landscape" "A test description" Size.A5_Landscape {
                user (A.person "a-person" "A test description" (0,0))
                system (A.system "a-system" "A test description" (0,0))
                relationship "a-person" "Uses" "a-system"
        }

    [<Fact>]
    let ``Serialize landscape diagram serializes diagram data`` () =

        let json = Json.serialize (Diagram.SystemLandscape test_diagram)
        let j = JObject.Parse(json)
        test <@ j |> stringPropertyContains "type" "System Landscape" @> 
        test <@ j |> stringPropertyContains "scope" "a-landscape" @> 
        test <@ j |> stringPropertyContains "description" "A test description" @>
        test <@ j |> stringPropertyContains "size" "A5_Landscape" @>

    let getElements(j:JObject) = 
        let property = "elements"
        let jp = j.Property(property)
        if(isNull jp) then failwithf "%s was not found in %s" property (j.ToString(Formatting.Indented))
        else (jp.Value :?> JArray).AsJEnumerable() |> Seq.map (fun x -> x.ToObject<Json.JElement>())

    [<Fact>]
    let ``Serialize landscape diagram serializes diagram elements`` () =

        let json = Json.serialize (Diagram.SystemLandscape test_diagram)
        let j = JObject.Parse(json)
        let xs = j |> getElements
        test <@ xs |> Seq.length = 2 @>
        test <@ xs |> Seq.head |> (fun x -> x.Name = "a-person") @>

    let getRelationships(j:JObject) = 
        let property = "relationships"
        let jp = j.Property(property)
        if(isNull jp) then failwithf "%s was not found in %s" property (j.ToString(Formatting.Indented))
        else (jp.Value :?> JArray).AsJEnumerable() |> Seq.map (fun x -> x.ToObject<Json.JRelationship>())

    [<Fact>]
    let ``Serialize landscape diagram serializes diagram relationships`` () =

        let json = Json.serialize (Diagram.SystemLandscape test_diagram)
        let j = JObject.Parse(json)
        let xs = j |> getRelationships
        test <@ xs |> Seq.length = 1 @>
        test <@ xs |> Seq.head |> (fun x -> x.Source = "a-person") @>    
        test <@ xs |> Seq.head |> (fun x -> x.Destination = "a-system") @> 
        test <@ xs |> Seq.head |> (fun x -> x.Description = "Uses") @>

    let getStyles(j:JObject) = 
        let property = "styles"
        let jp = j.Property(property)
        if(isNull jp) then failwithf "%s was not found in %s" property (j.ToString(Formatting.Indented))
        else (jp.Value :?> JArray).AsJEnumerable() |> Seq.map (fun x -> x.ToObject<Json.JRelationship>())

    [<Fact>]
    let ``Serialize landscape diagram serializes diagram styles`` () =

        let json = Json.serialize (Diagram.SystemLandscape test_diagram)
        let j = JObject.Parse(json)
        let xs = j |> getStyles
        test <@ xs |> Seq.length = 2 @>   

// ===================================================    
// {
//   "type": "System Landscape",
//   "scope": "Big Bank plc",
//   "description": "The system landscape diagram for Big Bank plc.",
//   "size": "A5_Landscape",


//   "elements": [
//     {
//       "type": "Software System",
//       "name": "ATM",
//       "description": "Allows customers to withdraw cash.",
//       "tags": "Internal",
//       "position": "681,241",
//       "containers": []
//     },
//     {
//       "type": "Person",
//       "name": "Bank Staff",
//       "description": "Staff within the bank.",
//       "tags": "",
//       "position": "2015,613"
//     },
//     {
//       "type": "Person",
//       "name": "Customer",
//       "description": "A customer of the bank.",
//       "tags": "",
//       "position": "65,613"
//     },
//     {
//       "type": "Software System",
//       "name": "Internet Banking System",
//       "description": "Allows customers to view information about their bank accounts and make payments.",
//       "tags": "Internal",
//       "position": "695,1086",
//       "containers": []
//     },
//     {
//       "type": "Software System",
//       "name": "Mainframe Banking System",
//       "description": "Stores all of the core banking information about customers, accounts, transactions, etc.",
//       "tags": "Internal",
//       "position": "1347,663",
//       "containers": []
//     }
//   ],


//   "relationships": [
//     {
//       "source": "ATM",
//       "description": "Uses",
//       "technology": "",
//       "destination": "Mainframe Banking System",
//       "tags": ""
//     },
//     {
//       "source": "Bank Staff",
//       "description": "Uses",
//       "technology": "",
//       "destination": "Mainframe Banking System",
//       "tags": ""
//     },
//     {
//       "source": "Customer",
//       "description": "Uses",
//       "technology": "",
//       "destination": "Internet Banking System",
//       "tags": ""
//     },
//     {
//       "source": "Customer",
//       "description": "Withdraws cash using",
//       "technology": "",
//       "destination": "ATM",
//       "tags": ""
//     },
//     {
//       "source": "Internet Banking System",
//       "description": "Uses",
//       "technology": "",
//       "destination": "Mainframe Banking System",
//       "tags": ""
//     }
//   ],


//   "styles": [
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Person",
//       "width": "",
//       "height": "",
//       "background": "#08427b",
//       "color": "#ffffff",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "Person",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Software System",
//       "width": "",
//       "height": "",
//       "background": "#1168bd",
//       "color": "#ffffff",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "",
//       "metadata": ""
//     }
//   ]
// }

// ===================================================

// {
//   "type": "System Context",
//   "scope": "Internet Banking System",
//   "description": "The system context diagram for the Internet Banking System.",
//   "size": "A5_Landscape",


//   "elements": [
//     {
//       "type": "Person",
//       "name": "Customer",
//       "description": "A customer of the bank.",
//       "tags": "",
//       "position": "1040,97"
//     },
//     {
//       "type": "Software System",
//       "name": "Internet Banking System",
//       "description": "Allows customers to view information about their bank accounts and make payments.",
//       "tags": "Internal",
//       "position": "1015,725",
//       "containers": []
//     },
//     {
//       "type": "Software System",
//       "name": "Mainframe Banking System",
//       "description": "Stores all of the core banking information about customers, accounts, transactions, etc.",
//       "tags": "Internal",
//       "position": "1015,1230",
//       "containers": []
//     }
//   ],


//   "relationships": [
//     {
//       "source": "Customer",
//       "description": "Uses",
//       "technology": "",
//       "destination": "Internet Banking System",
//       "tags": ""
//     },
//     {
//       "source": "Internet Banking System",
//       "description": "Uses",
//       "technology": "",
//       "destination": "Mainframe Banking System",
//       "tags": ""
//     }
//   ],


//   "styles": [
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Person",
//       "width": "",
//       "height": "",
//       "background": "#08427b",
//       "color": "#ffffff",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "Person",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Software System",
//       "width": "",
//       "height": "",
//       "background": "#1168bd",
//       "color": "#ffffff",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "",
//       "metadata": ""
//     }
//   ]
// }

// ========================================================

// {
//   "type": "Container",
//   "scope": "Internet Banking System",
//   "description": "The container diagram for the Internet Banking System.",
//   "size": "A5_Landscape",


//   "elements": [
//     {
//       "type": "Person",
//       "name": "Customer",
//       "description": "A customer of the bank.",
//       "tags": "",
//       "position": "450,92"
//     },
//     {
//       "type": "Software System",
//       "name": "Internet Banking System",
//       "description": "Allows customers to view information about their bank accounts and make payments.",
//       "tags": "Internal",
//       "containers": [
//         {
//           "type": "Container",
//           "name": "Database",
//           "description": "Stores interesting data.",
//           "technology": "Relational Database Schema",
//           "tags": "Database",
//           "position": "425,1236",
//           "components": []
//         },
//         {
//           "type": "Container",
//           "name": "Web Application",
//           "description": "Provides all of the Internet banking functionality to customers.",
//           "technology": "Java and Spring MVC",
//           "tags": "",
//           "position": "425,714",
//           "components": []
//         }
//       ]
//     },
//     {
//       "type": "Software System",
//       "name": "Mainframe Banking System",
//       "description": "Stores all of the core banking information about customers, accounts, transactions, etc.",
//       "tags": "Internal",
//       "position": "1604,714",
//       "containers": []
//     }
//   ],


//   "relationships": [
//     {
//       "source": "Customer",
//       "description": "Uses",
//       "technology": "HTTPS",
//       "destination": "Web Application",
//       "tags": ""
//     },
//     {
//       "source": "Web Application",
//       "description": "Uses",
//       "technology": "XML/HTTPS",
//       "destination": "Mainframe Banking System",
//       "tags": ""
//     },
//     {
//       "source": "Web Application",
//       "description": "Reads from and writes to",
//       "technology": "JDBC",
//       "destination": "Database",
//       "tags": ""
//     }
//   ],


//   "styles": [
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Container",
//       "width": "",
//       "height": "",
//       "background": "#438dd5",
//       "color": "",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Database",
//       "width": "",
//       "height": "",
//       "background": "",
//       "color": "",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "Cylinder",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Element",
//       "width": "",
//       "height": "",
//       "background": "",
//       "color": "#ffffff",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Person",
//       "width": "",
//       "height": "",
//       "background": "#08427b",
//       "color": "",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "Person",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Software System",
//       "width": "",
//       "height": "",
//       "background": "#1168bd",
//       "color": "",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "",
//       "metadata": ""
//     }
//   ]
// }

// ========================================================

// {
//   "type": "Component",
//   "scope": "Web Application",
//   "description": "The component diagram for the Web Application.",
//   "size": "A5_Landscape",


//   "elements": [
//     {
//       "type": "Person",
//       "name": "Customer",
//       "description": "A customer of the bank.",
//       "tags": "",
//       "position": "1040,10"
//     },
//     {
//       "type": "Software System",
//       "name": "Internet Banking System",
//       "description": "Allows customers to view information about their bank accounts and make payments.",
//       "tags": "Internal",
//       "containers": [
//         {
//           "type": "Container",
//           "name": "Database",
//           "description": "Stores interesting data.",
//           "technology": "Relational Database Schema",
//           "tags": "Database",
//           "position": "1856,1318",
//           "components": []
//         },
//         {
//           "type": "Container",
//           "name": "Web Application",
//           "description": "Provides all of the Internet banking functionality to customers.",
//           "technology": "Java and Spring MVC",
//           "tags": "",
//           "components": [
//             {
//               "type": "Component",
//               "name": "Accounts Summary Controller",
//               "description": "Provides customers with an summary of their bank accounts.",
//               "technology": "Spring MVC Controller",
//               "tags": "",
//               "position": "1015,501"
//             },
//             {
//               "type": "Component",
//               "name": "Home Page Controller",
//               "description": "Serves up the home page.",
//               "technology": "Spring MVC Controller",
//               "tags": "",
//               "position": "173,501"
//             },
//             {
//               "type": "Component",
//               "name": "Mainframe Banking System Facade",
//               "description": "A facade onto the mainframe banking system.",
//               "technology": "Spring Bean",
//               "tags": "",
//               "position": "1015,892"
//             },
//             {
//               "type": "Component",
//               "name": "Security Component",
//               "description": "Provides functionality related to signing in, changing passwords, etc.",
//               "technology": "Spring Bean",
//               "tags": "",
//               "position": "1856,892"
//             },
//             {
//               "type": "Component",
//               "name": "Sign In Controller",
//               "description": "Allows users to sign in to the Internet Banking System.",
//               "technology": "Spring MVC Controller",
//               "tags": "",
//               "position": "1856,501"
//             }
//           ]
//         }
//       ]
//     },
//     {
//       "type": "Software System",
//       "name": "Mainframe Banking System",
//       "description": "Stores all of the core banking information about customers, accounts, transactions, etc.",
//       "tags": "Internal",
//       "position": "1015,1318",
//       "containers": []
//     }
//   ],


//   "relationships": [
//     {
//       "source": "Accounts Summary Controller",
//       "description": "Uses",
//       "technology": "",
//       "destination": "Mainframe Banking System Facade",
//       "tags": ""
//     },
//     {
//       "source": "Customer",
//       "description": "Uses",
//       "technology": "HTTPS",
//       "destination": "Home Page Controller",
//       "tags": ""
//     },
//     {
//       "source": "Customer",
//       "description": "Uses",
//       "technology": "HTTPS",
//       "destination": "Accounts Summary Controller",
//       "tags": ""
//     },
//     {
//       "source": "Customer",
//       "description": "Uses",
//       "technology": "HTTPS",
//       "destination": "Sign In Controller",
//       "tags": ""
//     },
//     {
//       "source": "Mainframe Banking System Facade",
//       "description": "Uses",
//       "technology": "XML/HTTPS",
//       "destination": "Mainframe Banking System",
//       "tags": ""
//     },
//     {
//       "source": "Security Component",
//       "description": "Reads from and writes to",
//       "technology": "JDBC",
//       "destination": "Database",
//       "tags": ""
//     },
//     {
//       "source": "Sign In Controller",
//       "description": "Uses",
//       "technology": "",
//       "destination": "Security Component",
//       "tags": ""
//     }
//   ],


//   "styles": [
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Component",
//       "width": "",
//       "height": "",
//       "background": "#85bbf0",
//       "color": "#000000",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Container",
//       "width": "",
//       "height": "",
//       "background": "#438dd5",
//       "color": "",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Database",
//       "width": "",
//       "height": "",
//       "background": "",
//       "color": "",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "Cylinder",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Element",
//       "width": "",
//       "height": "",
//       "background": "",
//       "color": "#ffffff",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Person",
//       "width": "",
//       "height": "",
//       "background": "#08427b",
//       "color": "",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "Person",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Software System",
//       "width": "",
//       "height": "",
//       "background": "#1168bd",
//       "color": "",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "",
//       "metadata": ""
//     }
//   ]
// }

// ============================================================

// {
//   "type": "Dynamic",
//   "scope": "Web Application",
//   "description": "Summarises how the sign in feature works.",
//   "size": "A5_Landscape",


//   "elements": [
//     {
//       "type": "Person",
//       "name": "Customer",
//       "description": "A customer of the bank.",
//       "tags": "",
//       "position": "287,64"
//     },
//     {
//       "type": "Software System",
//       "name": "Internet Banking System",
//       "description": "Allows customers to view information about their bank accounts and make payments.",
//       "tags": "Internal",
//       "containers": [
//         {
//           "type": "Container",
//           "name": "Database",
//           "description": "Stores interesting data.",
//           "technology": "Relational Database Schema",
//           "tags": "Database",
//           "position": "1742,1264",
//           "components": []
//         },
//         {
//           "type": "Container",
//           "name": "Web Application",
//           "description": "Provides all of the Internet banking functionality to customers.",
//           "technology": "Java and Spring MVC",
//           "tags": "",
//           "components": [
//             {
//               "type": "Component",
//               "name": "Security Component",
//               "description": "Provides functionality related to signing in, changing passwords, etc.",
//               "technology": "Spring Bean",
//               "tags": "",
//               "position": "1742,689"
//             },
//             {
//               "type": "Component",
//               "name": "Sign In Controller",
//               "description": "Allows users to sign in to the Internet Banking System.",
//               "technology": "Spring MVC Controller",
//               "tags": "",
//               "position": "1742,114"
//             }
//           ]
//         }
//       ]
//     }
//   ],


//   "relationships": [
//     {
//       "source": "Customer",
//       "description": "Requests /signin from",
//       "technology": "HTTPS",
//       "order": "1",
//       "destination": "Sign In Controller",
//       "tags": "",
//       "vertices": [
//         "1200,135"
//       ]
//     },
//     {
//       "source": "Customer",
//       "description": "Submits credentials to",
//       "technology": "",
//       "order": "2",
//       "destination": "Sign In Controller",
//       "tags": "",
//       "vertices": [
//         "1215,450"
//       ]
//     },
//     {
//       "source": "Security Component",
//       "description": "select * from users where username = ?",
//       "technology": "JDBC",
//       "order": "4",
//       "destination": "Database",
//       "tags": ""
//     },
//     {
//       "source": "Sign In Controller",
//       "description": "Calls isAuthenticated() on",
//       "technology": "",
//       "order": "3",
//       "destination": "Security Component",
//       "tags": ""
//     }
//   ],


//   "styles": [
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Component",
//       "width": "",
//       "height": "",
//       "background": "#85bbf0",
//       "color": "#000000",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Container",
//       "width": "",
//       "height": "",
//       "background": "#438dd5",
//       "color": "",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Database",
//       "width": "",
//       "height": "",
//       "background": "",
//       "color": "",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "Cylinder",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Element",
//       "width": "",
//       "height": "",
//       "background": "",
//       "color": "#ffffff",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Person",
//       "width": "",
//       "height": "",
//       "background": "#08427b",
//       "color": "",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "Person",
//       "metadata": ""
//     },
//     {
//       "type": "element",
//       "description": "",
//       "tag": "Software System",
//       "width": "",
//       "height": "",
//       "background": "#1168bd",
//       "color": "",
//       "fontSize": "",
//       "border": "",
//       "opacity": "",
//       "shape": "",
//       "metadata": ""
//     }
//   ]
// }