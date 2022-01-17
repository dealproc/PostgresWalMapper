namespace PGWalMapper {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Npgsql.Replication;
    using Npgsql.Replication.PgOutput;
    using Npgsql.Replication.PgOutput.Messages;

    public class WalListener : IDisposable {
        private readonly LogicalReplicationConnection _connection;
        private readonly IEnumerable<InstanceConstructor> _instanceConstructors;
        private CancellationTokenSource _tokenSource;
        private readonly string _publicationName;
        private readonly string _slotName;
        private readonly IEnumerable<ActionHandler> _insertActions;
        private readonly IEnumerable<ActionHandler> _updateActions;
        private readonly IEnumerable<ActionHandler> _deleteActions;

        internal WalListener(LogicalReplicationConnection connection,
            string publicationName,
            string slotName,
            IEnumerable<InstanceConstructor> instanceConstructors,
            IEnumerable<ActionHandler> insertActions,
            IEnumerable<ActionHandler> updateActions,
            IEnumerable<ActionHandler> deleteActions) {
            _connection = connection;
            _instanceConstructors = instanceConstructors;
            _publicationName = publicationName;
            _slotName = slotName;
            _insertActions = insertActions.ToArray();
            _updateActions = updateActions.ToArray();
            _deleteActions = deleteActions.ToArray();
        }

        public void Connect() {
            _tokenSource = new CancellationTokenSource();
            //TBD: Do we need to hold a reference to the task?
            Task.Run(async () => {
                await _connection.Open();
                await foreach (var msg in _connection.StartReplication(new PgOutputReplicationSlot(_slotName),
                                   new PgOutputReplicationOptions(_publicationName, 1),
                                   _tokenSource.Token)) {
                    var constructor = _instanceConstructors.FirstOrDefault(ic => ic.CanCreateInstance(msg));
                    if (constructor != null) {
                        try {
                            var ic = await constructor.CreateInstance(msg, _tokenSource.Token);
                            if (ic != null) {
                                switch (msg) {
                                    case InsertMessage:
                                        _insertActions.Where(a => a.CanHandle(ic)).Apply(a => a.Handle(ic));
                                        break;
                                    case UpdateMessage:
                                        _updateActions.Where(a => a.CanHandle(ic)).Apply(a => a.Handle(ic));
                                        break;
                                    case DeleteMessage:
                                        _deleteActions.Where(a => a.CanHandle(ic)).Apply(a => a.Handle(ic));
                                        break;
                                    default:
                                        Console.WriteLine("Unknown Wal message type.  No callbacks exist.");
                                        break;
                                }
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