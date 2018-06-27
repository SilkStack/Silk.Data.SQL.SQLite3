using System.Data.Common;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.Queries;
using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;

namespace Silk.Data.SQL.SQLite3
{
	public class SQLite3DataProvider : DataProviderCommonBase
	{
		public const string PROVIDER_NAME = "sqlite3";

		public override string ProviderName => PROVIDER_NAME;

		public bool NonBinaryGUIDs { get; }

		private string _connectionString;

		private InMemorySqlConnection _inMemoryConnection;

		public SQLite3DataProvider(SqliteConnectionStringBuilder connectionStringBuilder, bool nonBinaryGUIDs = false)
		{
			NonBinaryGUIDs = nonBinaryGUIDs;
			_connectionString = connectionStringBuilder.ConnectionString;
			if (connectionStringBuilder.Mode == SqliteOpenMode.Memory)
			{
				_inMemoryConnection = new InMemorySqlConnection(_connectionString);
				_inMemoryConnection.Open();
			}
		}

		public SQLite3DataProvider(string connectionString, bool nonBinaryGUIDs = false) :
			this(new SqliteConnectionStringBuilder(connectionString), nonBinaryGUIDs) { }

		public SQLite3DataProvider(Uri file, bool nonBinaryGUIDs = false) :
			this(new SqliteConnectionStringBuilder { Mode = SqliteOpenMode.ReadWriteCreate, DataSource = file.OriginalString }, nonBinaryGUIDs) { }

		public override DbCommand CreateCommand(DbConnection connection, SqlQuery sqlQuery)
		{
			if (NonBinaryGUIDs && sqlQuery.QueryParameters != null)
			{
				foreach (var kvp in sqlQuery.QueryParameters)
				{
					if (kvp.Value.Value is Guid)
						kvp.Value.Value = kvp.Value.Value.ToString();
				}
			}

			return base.CreateCommand(connection, sqlQuery);
		}

		protected override IQueryConverter CreateQueryConverter()
		{
			return new SQLite3QueryConverter(NonBinaryGUIDs);
		}

		protected override DbConnection Connect()
		{
			if (_inMemoryConnection != null)
				return _inMemoryConnection;

			var connection = new SqliteConnection(_connectionString);
			connection.Open();
			return connection;
		}

		protected override async Task<DbConnection> ConnectAsync()
		{
			if (_inMemoryConnection != null)
				return _inMemoryConnection;

			var connection = new SqliteConnection(_connectionString);
			await connection.OpenAsync();
			return connection;
		}

		public override void Dispose()
		{
			if (_inMemoryConnection != null)
				_inMemoryConnection.InternalDispose();
		}

		private class InMemorySqlConnection : SqliteConnection
		{
			public InMemorySqlConnection() : base() { }
			public InMemorySqlConnection(string connectionString) : base(connectionString) { }

			protected override void Dispose(bool disposing)
			{
			}

			public void InternalDispose()
			{
				Dispose();
			}
		}
	}
}
