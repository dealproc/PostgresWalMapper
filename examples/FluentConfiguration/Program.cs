namespace FluentConfiguration {
    using System;
    using System.Text.Json;

    using PGWalMapper;

    class Program {
        static void Main(string[] args) {
            var listener = new WalConfigurationBuilder("Host=127.0.0.1;Port=5432;Database=postgres;username=postgres;password=mysecretpassword;")
                .ForPublication("films_pub").UsingSlot("films_slot")
                .OnInsert(o => Console.WriteLine("'Object' OnInsert called."))
                .OnUpdate(o => Console.WriteLine("'Object' OnUpdate called."))
                .OnDelete(o => Console.WriteLine("'Object' OnDelete called."))
                
                .Map<AggregateMsgs.Films>().ToTable("films").InSchema("public")
                .Column("code").ToProperty(prop => prop.Code)
                .Column("title").ToProperty(prop => prop.Title)
                .Column("did").ToProperty(prop => prop.Did)
                .Column("date_prod").ToProperty(prop => prop.DateProduced)
                .Column("kind").ToProperty(prop => prop.Kind)
                .OnInsert(o => Console.WriteLine($"Insert: {JsonSerializer.Serialize(o)}"))
                .OnUpdate(o => Console.WriteLine($"Update: {JsonSerializer.Serialize(o)}"))
                .OnDelete(o => Console.WriteLine($"Delete: {JsonSerializer.Serialize(o)}"))
                
                .Map<AggregateMsgs.Distributors>().ToTable("distributors").InSchema("public")
                .Column("did").ToProperty(prop => prop.Did)
                .Column("name").ToProperty(prop=> prop.Name)
                .OnInsert(d=> Console.WriteLine($"Insert: {JsonSerializer.Serialize(d)}"))
                .OnUpdate(o => Console.WriteLine($"Update: {JsonSerializer.Serialize(o)}"))
                .OnDelete(o => Console.WriteLine($"Delete: {JsonSerializer.Serialize(o)}"))
             
                .Build();

            listener.Connect();
            Console.WriteLine("Waiting for actions...");
            Console.ReadLine();
            listener.Disconnect();
            listener.Dispose();
        }
    }
}