module Step1.Program

open Step1.Domain
open Step1.Infrastructure


type Msg =
  | AppendIcecreamSoldVanilla
  | AppendIcecreamSoldStrawberry
  | AppendIcecreamSoldStrawberryFlavourEmptyStrawberry
  | GetEvents of AsyncReplyChannel<Event list>

let mailbox () =
  let eventStore : EventStore<Event> = EventStore.initialize()

  MailboxProcessor.Start(fun inbox ->
    let rec loop eventStore =
      async {
        let! msg = inbox.Receive()

        match msg with
        | AppendIcecreamSoldVanilla ->
            eventStore.Append [IcecreamSold Vanilla]
            return! loop eventStore

        | AppendIcecreamSoldStrawberry ->
            eventStore.Append [IcecreamSold Strawberry ]
            return! loop eventStore

        | AppendIcecreamSoldStrawberryFlavourEmptyStrawberry ->
            eventStore.Append [IcecreamSold Strawberry ; Flavour_empty Strawberry]
            return! loop eventStore

        | GetEvents reply ->
            reply.Reply (eventStore.Get())
            return! loop eventStore
      }

    loop eventStore
  )


let appendIcecreamSoldVanilla (mailbox : MailboxProcessor<Msg>) =
  mailbox.Post Msg.AppendIcecreamSoldVanilla

let appendIcecreamSoldStrawberry (mailbox : MailboxProcessor<Msg>) =
  mailbox.Post Msg.AppendIcecreamSoldStrawberry

let appendIcecreamSoldStrawberryFlavourEmptyStrawberry (mailbox : MailboxProcessor<Msg>) =
  mailbox.Post Msg.AppendIcecreamSoldStrawberryFlavourEmptyStrawberry

let getEvents (mailbox : MailboxProcessor<Msg>) =
  mailbox.PostAndReply Msg.GetEvents