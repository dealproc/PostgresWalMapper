namespace FluentConfiguration {
    using System;



    public class AggregateMsgs {
        public class Films {
            public string Code;
            public string Title;
            public string Did;
            public DateTime DateProduced;
            public string Kind;
        }

        public class Distributors {
            public int Did;
            public string Name;
        }
    }
}