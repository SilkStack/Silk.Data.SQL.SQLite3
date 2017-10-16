namespace Silk.Data.SQL.SQLite3.Tests
{
	public static class TestDb
	{
		public static SQLite3DataProvider Provider { get; } =
			new SQLite3DataProvider(":memory:");

		static TestDb()
		{
			Provider.ExecuteNonQuery(
				SQLite3.Raw("CREATE TABLE [Persistent] ([Column1] TEXT)")
				);
			Provider.ExecuteNonQuery(
				SQLite3.Raw("CREATE TABLE [ForDrop] ([Column1] TEXT)")
				);
		}
	}
}
