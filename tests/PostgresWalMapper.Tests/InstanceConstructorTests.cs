namespace PostgresWalMapper.Tests {
    using System;

    public class InstanceConstructorTests {


        public class NonConstructorTestClassWithProperties {
            public Guid Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public class NonConstructorTestClassWithFields {
            public Guid Id;
            public string FirstName;
            public string LastName;
        }

        public class ConstructorTestClassWithProperties {
            public Guid Id { get; }
            public string FirstName { get; }
            public string LastName { get; }

            public ConstructorTestClassWithProperties(Guid id, string firstName, string lastName) {
                Id = id;
                FirstName = firstName;
                LastName = lastName;
            }
        }

        public class ConstructorTestClassWithFields {
            public readonly Guid Id;
            public readonly string FirstName;
            public readonly string LastName;

            public ConstructorTestClassWithFields(Guid id, string firstName, string lastName) {
                Id = id;
                FirstName = firstName;
                LastName = lastName;
            }
        }
    }
}