using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.ProviderTests;

namespace Silk.Data.SQL.SQLite3.Tests
{
	[TestClass]
	public class Sqlite3Tests : SqlProviderTests
	{
		public override IDataProvider DataProvider => TestDb.Provider;

		public override void Dispose()
		{
		}
	}
}
