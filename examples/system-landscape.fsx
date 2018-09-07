//#r "../packages/NETStandard.Library/build/netstandard2.0/ref/netstandard.dll"
#r "../src/Fucturizr.Core/bin/Debug/netstandard2.0/Fucturizr.Core.dll"

open Fucturizr
open Fucturizr.DSL
open System.IO

// example defining before use
let ``Supplier API`` = A.external_system "Supplier API" "Contains product information and order management" (500,830)

// define diagram
let contract_management_landscape_diagram =
    system_landscape_diagram "Contract Management" "A test description" Size.A5_Landscape {
        system ``Supplier API``
        user (A.person "Buyer" "Negotiates policies with suppliers and orders..." (730,230))
        system (A.system "Acme Contract Management System" "Manages contracts negotiated with suppliers" (705,830))

        relationship "Buyer" "Captures contracts" "Acme Contract Management System"
        relationship "Acme Contract Management System" "Gets product data" ``Supplier API``.Name
    }

// Wrap as a Diagram then serialze serialize to json
let json = contract_management_landscape_diagram 
            |> Diagram.SystemLandscape
            |> Json.serialize

// write to json file
let filename = __SOURCE_FILE__ |> Path.GetFileNameWithoutExtension |> fun p -> p + ".json"
json |> save filename

// {
//   "type": "System Landscape",
//   "scope": "a-landscape",
//   "description": "A test description",
//   "size": "A5_Landscape",


//   "elements": [
//     {
//       "type": "Software System",
//       "name": "Acme Contract Management System",
//       "description": "Manages contracts negotiated with suppliers",
//       "tags": "Internal,",
//       "position": "705,830",
//       "containers": []
//     },
//     {
//       "type": "Person",
//       "name": "Buyer",
//       "description": "Negotiates policies with suppliers and orders...",
//       "tags": "",
//       "position": "730,230"
//     }
//   ],


//   "relationships": [
//     {
//       "source": "Buyer",
//       "description": "Uses",
//       "technology": "",
//       "destination": "Acme Contract Management System",
//       "tags": ""
//     }
//   ],


//   "styles": [
//     {
//       "type": "element",
//       "description": "true",
//       "tag": "Element",
//       "width": "",
//       "height": "",
//       "background": "#1168bd",
//       "color": "#FFFFFF",
//       "fontSize": "",
//       "border": "Solid",
//       "opacity": "",
//       "shape": "Box",
//       "metadata": "true"
//     },
//     {
//       "type": "element",
//       "description": "true",
//       "tag": "Person",
//       "width": "",
//       "height": "",
//       "background": "#08427b",
//       "color": "",
//       "fontSize": "",
//       "border": "Solid",
//       "opacity": "",
//       "shape": "Person",
//       "metadata": "true"
//     },
//     {
//       "type": "relationship",
//       "tag": "Relationship",
//       "position": "",
//       "thickness": "",
//       "width": "",
//       "color": "",
//       "fontSize": "",
//       "dashed": "false",
//       "routing": "Direct",
//       "opacity": ""
//     }
//   ]
// }