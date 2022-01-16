namespace FluentConfiguration {
    using System;



    public class AggregateMsgs {
        public class Films {
            public string Code { get; set; }
            public string Title { get; set; }
            public string Did { get; set; }
            public DateTime DateProduced { get; set; }
            public string Kind { get; set; }
        }

        public class Distributors {
            public int Did { get; set; }
            public string Name { get; set; }
        }
    }
}