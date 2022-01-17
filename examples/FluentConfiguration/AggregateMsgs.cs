namespace FluentConfiguration {
    using System;



    public class AggregateMsgs {
        public class Films {
            public readonly string Code;
            public readonly string Title;
            public readonly string Did;
            public readonly DateTime DateProduced;
            public readonly string Kind;

            public Films(string code, string title, string did, DateTime dateProduced, string kind) {
                Code = code;
                Title = title;
                Did = did;
                DateProduced = dateProduced;
                Kind = kind;
            }
        }

        public class Distributors {
            public readonly int Did;
            public readonly string Name;

            public Distributors(int did, string name) {
                Did = did;
                Name = name;
            }
        }
    }
}