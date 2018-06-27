using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.ProviderTests;

namespace Silk.Data.SQL.SQLite3.Tests
{
	[TestClass]
	public class Sqlite3Tests : SqlProviderTests
	{
		public override IDataProvider CreateDataProvider(string connectionString)
		{
			var provider = new SQLite3DataProvider(connectionString);
			provider.ExecuteNonQuery(
				SQLite3.Raw("CREATE TABLE [TableExistsTest] ([Column1] INT)")
				);
			return provider;
		}

		public override Task Data_StoreWideDecimal()
		{
			/*
			 * Hidden Test
			 * Justification:
			 *    Sqlite3 doesn't support more than 15 digit storage for a decimal, this is documented as a limitation of the provider.
			 */
			return Task.CompletedTask;
		}

		public override void Dispose()
		{
		}
	}
}
