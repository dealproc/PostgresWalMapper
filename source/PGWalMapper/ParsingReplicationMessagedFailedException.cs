namespace PGWalMapper {
    using System;

    public class ParsingReplicationMessagedFailedException : Exception {
        public ParsingReplicationMessagedFailedException() { }
        public ParsingReplicationMessagedFailedException(string? message) : base(message) { }
        public ParsingReplicationMessagedFailedException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}