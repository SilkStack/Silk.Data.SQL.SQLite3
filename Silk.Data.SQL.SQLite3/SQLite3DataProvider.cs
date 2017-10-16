using System.Data.Common;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.Queries;
using Microsoft.Data.Sqlite;
using System;

namespace Silk.Data.SQL.SQLite3
{
	public class SQLite3DataProvider : DataProviderCommonBase
	{
		public const string PROVIDER_NAME = "sqlite3";

		public override string ProviderName => PROVIDER_NAME;

		protected override DbConnection DbConnection { get; }
		public bool NonBinaryGUIDs { get; }

		public SQLite3DataProvider(string file, bool nonBinaryGUIDs = false)
		{
			var connectionStringBuilder = new SqliteConnectionStringBuilder
			{
				Mode = SqliteOpenMode.ReadWriteCreate,
				DataSource = file
			};
			if (file == ":memory:")
				connectionStringBuilder.Mode = SqliteOpenMode.Memory;
			DbConnection = new SqliteConnection(connectionStringBuilder.ConnectionString);
			NonBinaryGUIDs = nonBinaryGUIDs;
		}

		protected override DbCommand CreateCommand(SqlQuery sqlQuery)
		{
			if (NonBinaryGUIDs && sqlQuery.QueryParameters != null)
			{
				foreach(var kvp in sqlQuery.QueryParameters)
				{
					if (kvp.Value.Value is Guid)
						kvp.Value.Value = kvp.Value.Value.ToString();
				}
			}

			return base.CreateCommand(sqlQuery);
		}

		protected override IQueryConverter CreateQueryConverter()
		{
			return new SQLite3QueryConverter(NonBinaryGUIDs);
		}
	}
}
