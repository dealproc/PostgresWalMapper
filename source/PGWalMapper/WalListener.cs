namespace PGWalMapper {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Npgsql.Replication;
    using Npgsql.Replication.PgOutput;

    public class WalListener : IDisposable {
        private readonly LogicalReplicationConnection _connection;
        private readonly IEnumerable<InstanceConstructor> _instanceConstructors;
        private CancellationTokenSource _tokenSource;
        private readonly string _publicationName;
        private readonly string _slotName;
        private readonly IEnumerable<ActionHandler> _actions;

        internal WalListener(LogicalReplicationConnection connection,
            string publicationName,
            string slotName,
            IEnumerable<InstanceConstructor> instanceConstructors,
            IEnumerable<ActionHandler> actions) {
            _connection = connection;
            _instanceConstructors = instanceConstructors;
            _publicationName = publicationName;
            _slotName = slotName;
            _actions = actions;
        }

        public void Connect() {
            _tokenSource = new CancellationTokenSource();
            //TBD: Do we need to hold a reference to the task?
            Task.Run(async () => {
                await _connection.Open();
                Console.WriteLine("Connection has been opened.");

                await foreach (var msg in _connection.StartReplication(new PgOutputReplicationSlot(_slotName),
                                   new PgOutputReplicationOptions(_publicationName, 1),
                                   _tokenSource.Token)) {
                    Console.WriteLine("Got data...");
                    foreach (var x in _instanceConstructors) {
                        try {
                            Console.WriteLine("Using constructor...");
                            var ic = await x.CreateInstance(msg, _tokenSource.Token);
                            Console.WriteLine("Possibly resolved an instance...");
                            if (ic != null) {
                                _actions.Where(a => a.CanHandle(ic)).Apply(a => a.Handle(ic));
                            } else {
                                Console.WriteLine("Nothing to parse?");
                            }
                        }
                        catch (Exception exc) {
                            Console.WriteLine(exc.Message);
                            Console.WriteLine(exc.GetType().Name);
                        }
                    }


                    // to mark the WAL as completed.
                    _connection.SetReplicationStatus(msg.WalEnd);
                }
            }, _tokenSource.Token);
        }

        public void Disconnect() {
            _tokenSource.Cancel();
            _tokenSource = null;
        }

        public void Dispose() {
            Disconnect();
        }
    }
}