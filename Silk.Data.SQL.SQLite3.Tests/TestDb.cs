namespace Silk.Data.SQL.SQLite3.Tests
{
	public static class TestDb
	{
		public static SQLite3DataProvider Provider { get; } =
			new SQLite3DataProvider(":memory:");

		static TestDb()
		{
			Provider.ExecuteNonQuery(
				SQLite3.Raw("CREATE TABLE [TableExistsTest] ([Column1] INT)")
				);
		}
	}
}
