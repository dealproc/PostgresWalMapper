namespace FluentConfiguration {
    using PGWalMapper;

    class Program {
        static void Main(string[] args) {
            var listener = new WalConfigurationBuilder()
                .WithConnectionString("")
                
                .Map<AggregateMsgs.Insert>()
                .ToTable("some_table_name")
                .InSchema("some_schema")
                .OnInsert()
                .Column("").ToProperty(prop => prop.Id).AsConstructorArgument()
                .Column("").ToProperty(prop => prop.Col1).AsConstructorArgument()
                .Column("").ToProperty(prop => prop.Col2).AsConstructorArgument()
                .On(i => { })
                
                .Map<AggregateMsgs.Update>()
                .ToTable("some_table_name")
                .InSchema("some_schema")
                .OnUpdate()
                .Column("").ToProperty(prop => prop.Id).AsConstructorArgument()
                .Column("").ToProperty(prop => prop.Col1).AsConstructorArgument()
                .Column("").ToProperty(prop => prop.Col2).AsConstructorArgument()
                .On(u => { })
                
                .Map<AggregateMsgs.Delete>()
                .ToTable("some_table_name")
                .InSchema("some_schema")
                .OnDelete()
                .Column("").ToProperty(prop => prop.Id).AsConstructorArgument()
                .Column("").ToProperty(prop => prop.Col1).AsConstructorArgument()
                .Column("").ToProperty(prop => prop.Col2).AsConstructorArgument()
                .On(o => { })
                
                .Build();

            listener.Connect();
            listener.Disconnect();
            listener.Dispose();


            new WalConfigurationBuilder()
                .On(_ => { })
                .On<AggregateMsgs.Insert>(_ => { })
                .Map<AggregateMsgs.Delete>()
                .ToTable("some_table_name")
                .InSchema("some_schema")
                .OnDelete()
                .Column("").ToProperty(p => p.Col1)
                .On(_ => { })
                .Build();
        }
    }
}