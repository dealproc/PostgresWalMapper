namespace FluentConfiguration {
    using System;
    using System.Text.Json;

    using PGWalMapper;

    class Program {
        static void Main(string[] args) {
            var listener = new WalConfigurationBuilder("Host=127.0.0.1;Port=5432;Database=postgres;username=postgres;password=mysecretpassword;")
                .ForPublication("films_pub").UsingSlot("films_slot")
                
                .Map<AggregateMsgs.Films>().ToTable("films").InSchema("public")
                .Column("code").ToProperty(prop => prop.Code)
                .Column("title").ToProperty(prop => prop.Title)
                .Column("did").ToProperty(prop => prop.Did)
                .Column("date_prod").ToProperty(prop => prop.DateProduced)
                .Column("kind").ToProperty(prop => prop.Kind)
                .On(i => { Console.WriteLine($"Insert: {JsonSerializer.Serialize(i)}"); })
                
                .Map<AggregateMsgs.Distributors>().ToTable("distributors").InSchema("public")
                .Column("did").ToProperty(prop => prop.Did)
                .Column("name").ToProperty(prop=> prop.Name)
                .On(d=> Console.WriteLine($"Insert: {JsonSerializer.Serialize(d)}"))
                
                .Build();

            listener.Connect();
            Console.WriteLine("Waiting for actions...");
            Console.ReadLine();
            listener.Disconnect();
            listener.Dispose();
        }
    }
}