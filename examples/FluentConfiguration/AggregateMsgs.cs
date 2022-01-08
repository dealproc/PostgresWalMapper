namespace FluentConfiguration {
    using System;

    public class AggregateMsgs {
        public class Insert {
            public Guid Id { get; set; }
            public string Col1 { get; set; }
            public string Col2 { get; set; }
        }

        public class Update {
            public Guid Id { get; set; }
            public string Col1 { get; set; }
            public string Col2 { get; set; }
        }

        public class Delete {
            public Guid Id { get; set; }
            public string Col1 { get; set; }
            public string Col2 { get; set; }
        }
    }
}