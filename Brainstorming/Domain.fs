module Brainstorm.Domain

open Brainstorm.Infrastructure


  // 2 Beispiele in der Domäne

  // wie werden aus Events State

  // einfach Funktionen, kein Subscriber. Wir müssen also die Events im State halten und abfragen

  // Aktion holt sich auch den Stream und lässt die Projektion durchlaufen. Aber welchen Stream?

// brauchen möglichkeit State zu speichern, vllt ein Objekt?
// Aktor erklären
// Mailbox Prozessor erklären
// brauchen eine Antwort

// zeige Type Driven: erst später den Event Store implementieren


// mehrere Trucks

// Domain

type Truck = System.Guid

type Flavour =
  | Vanilla
  | Strawberry

type Event =
  | Flavour_sold of Flavour
  | Flavour_restocked of Flavour * int
  | Flavour_went_out_of_stock of Flavour
  | Flavour_was_not_in_stock of Flavour


let updateSoldFlavours state event =
  match event with
  | Flavour_sold flavour ->
      flavour :: state

  | _ ->
      state

let soldFlavours : Projection<Flavour list, Event> =
  {
    Init = []
    Update = updateSoldFlavours
  }

let restock flavour number  stock =
  stock
  |> Map.tryFind flavour
  |> Option.map (fun portions -> stock |> Map.add flavour (portions + number))
  |> Option.defaultValue stock

let updateFlavoursInStock stock event =
  match event with
  // | Flavour_sold flavour ->
  //     stock
  //     |> Map.tryFind flavour
  //     |> Option.map (fun portions -> stock |> Map.add flavour (portions - 1))
  //     |> Option.defaultValue stock

  // | Flavour_restocked (flavour, portions) ->
  //     stock
  //     |> Map.tryFind flavour
  //     |> Option.map (fun portions -> stock |> Map.add flavour (portions + portions))
  //     |> Option.defaultValue stock
  | Flavour_sold flavour ->
      stock |> restock flavour -1

  | Flavour_restocked (flavour, portions) ->
      stock |> restock flavour portions

  | _ ->
      stock


let flavoursInStock : Projection<Map<Flavour, int>, Event> =
  {
    Init = Map.empty
    Update = updateFlavoursInStock
  }

let project projection events =
  events |> List.fold projection.Update projection.Init

// brauche irgendwie mehr events und Projektionen

let soldIceCreams getEvents =
  getEvents()
  |> List.fold soldFlavours.Update soldFlavours.Init
  // erstmal so, dann beim 2. zeigen wieder das gleich
  // eine Project Funktion bauen (baue was du brauchst)



// hier dann Program
let eventStore : EventStore<Event> = initializeEventStore()



// let sold = soldIceCreams eventStoreReal.Get


// // ab hier dann Aktionen
// let sellIceCream flavour events =
//   [Flavour_sold flavour]


let numberOfFlavourInStock flavour stock =
  stock
  |> Map.tryFind flavour
  |> Option.defaultValue 0

let sellIceCream flavour events =
  let stock =
    events
    |> project flavoursInStock
    |> numberOfFlavourInStock flavour

  match stock with
  | 0 -> [Flavour_was_not_in_stock flavour]
  | 1 -> [Flavour_sold flavour ; Flavour_went_out_of_stock flavour]
  | _ -> [Flavour_sold flavour]


// wenn empty muss neu bestellt werden

let truck = System.Guid.NewGuid()

eventStore.Evolve truck (sellIceCream Vanilla)
eventStore.Evolve truck (sellIceCream Strawberry)
eventStore.Evolve truck (sellIceCream Vanilla)
eventStore.Evolve truck (sellIceCream Vanilla)

// Frage: wie mit Problemen umgehen

// auch Events

// Erweiterung der Domäne

// wie viel in Stock
// Available, nicht available




// erst rein mit append zeigen, dann sagen, hmm das bringt nichts, keine Regeln
// dann mit Run

// erst mal nur für eins dann mit aggregaten
// nicht alle events anpassen, deswegen nehmen wir das Aggregate auf

// wir haben keine Commands in denen die Aggregate stehen, nehmen die



(*
was mache ich wenn ich eine Liste aller Trucks haben will
* normale Projection: benutzt EventStore.Get und macht damit was es will
* für Constraints:
* wenn das häufig vorkommt, sollte man sich eventuell überlegen, ob man seine Aggregate richtig geschnitten hat (after all we are doing DDD)
    Beispiel: überlegen
* wenn nicht, kann man in der Funktion trotzdem eine Projection machen die sich über EventStore.Get alles holt

*)
