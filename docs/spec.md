var connector = WalMapper.Configure()
    .WithConnectionString("...")

    .Map<{event}>()
        .ToTable("")
        .InSchema("") // without specifying this, we assume "public"
        .OnInsert().OnUpdate().OnDelete() // rather than having an enumeration, this is clearer to read.
        .Column("").ToProperty(o => o.{PropertyName})
        .Column("").ToProperty(o => o.{PropertyName})

    .AddMappingsFromAssemblyOf<{a class within the assembly}>()

    .On(e => ...) // $all stream in EventStore
    .On<{event}>(e => ...) // a particular event

    .BuildConnector();

connector.Connect(); // start listening
connector.Disconnect(); // stop listening.
connector.Dispose(); // will call Disconnect, then cleanup anything else that may be hanging around.