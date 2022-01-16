// namespace PGWalMapper {
//     using System;
//     using System.Collections.Generic;
//     using System.Linq;
//
//     using Npgsql.Replication;
//     using Npgsql.Replication.PgOutput.Messages;
//
//     internal interface IOperationHandler {
//         bool Handle(ReplicationMessage msg);
//     }
//
//     internal abstract class OperationHandler : IOperationHandler {
//         protected Func<string> _tableName;
//         protected Func<string> _schemaName;
//         protected IEnumerable<IColumnMapping> _columnMappings;
//
//         public OperationHandler(Func<string> tableName, Func<string> schemaName, IEnumerable<IColumnMapping> columnMappings) {
//             _tableName = tableName;
//             _schemaName = schemaName;
//             _columnMappings = columnMappings;
//         }
//
//         public abstract bool Handle(ReplicationMessage msg);
//     }
//
//     internal class InsertOperationHandler<TEvent> : OperationHandler {
//         public InsertOperationHandler(Func<string> tableName, Func<string> schemaName, IEnumerable<IColumnMapping> columnMappings) : base(tableName, schemaName, columnMappings) { }
//
//         public override bool Handle(ReplicationMessage msg) {
//             var inserted = msg as InsertMessage;
//             if (inserted == null) return false;
//             if (inserted.Relation.Namespace != _schemaName() || inserted.Relation.RelationName != _tableName()) return false;
//
//             var dict = new Dictionary<string, object>();
//             var enumerator = inserted.NewRow.GetAsyncEnumerator();
//
//             foreach (var definition in inserted.Relation.Columns) {
//                 dict.Add(definition.ColumnName, enumerator.Current.Get().Result);
//                 enumerator.MoveNextAsync().GetAwaiter().GetResult();
//             }
//
//             object obj;
//
//             if (_columnMappings.Any(cm => cm.SetOnConstructor)) { } else {
//                 obj = Activator.CreateInstance<TEvent>();
//             }
//
//             // typeof(TEvent).GetConstructors().First().GetParameters().First().Name
//
//             // Activator.CreateInstance(typeof(TEvent),)
//
//             return true;
//         }
//     }
//
//     internal class UpdateOperationHandler : OperationHandler {
//         public UpdateOperationHandler(Func<string> tableName, Func<string> schemaName, IEnumerable<IColumnMapping> columnMappings) : base(tableName, schemaName, columnMappings) { }
//
//         public override bool Handle(ReplicationMessage msg) {
//             throw new System.NotImplementedException();
//         }
//     }
//
//     internal class DeleteOperationHandler : OperationHandler {
//         public DeleteOperationHandler(Func<string> tableName, Func<string> schemaName, IEnumerable<IColumnMapping> columnMappings) : base(tableName, schemaName, columnMappings) { }
//
//         public override bool Handle(ReplicationMessage msg) {
//             throw new System.NotImplementedException();
//         }
//     }
// }