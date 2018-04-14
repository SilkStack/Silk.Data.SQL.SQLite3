using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.SQLite3.Tests
{
	[TestClass]
	public class TransactionTests
	{
		[TestMethod]
		public void InsertUsingTransaction()
		{
			using (var provider = new SQLite3DataProvider(":memory:"))
			{
				using (var transaction = provider.CreateTransaction())
				{
					transaction.ExecuteNonQuery(
						QueryExpression.CreateTable("Test", QueryExpression.DefineColumn("Data", SqlDataType.Text()))
						);
					transaction.ExecuteNonQuery(
						QueryExpression.Insert("Test", new[] { "Data" }, new[] { "Hello" }, new[] { "World" })
						);

					using (var queryResult = transaction.ExecuteReader(QueryExpression.Select(
						new[] { QueryExpression.All() },
						from: QueryExpression.Table("Test")
						)))
					{
						Assert.IsTrue(queryResult.HasRows);
					}

					transaction.Rollback();
				}

				try
				{
					using (var queryResult = provider.ExecuteReader(QueryExpression.Select(
							new[] { QueryExpression.All() },
							from: QueryExpression.Table("Test")
							)))
					{
					}

					Assert.Fail("An exception should have been thrown.");
				}
				catch (SqliteException ex) when (ex.Message == "SQLite Error 1: 'no such table: Test'.")
				{
				}
			}
		}
	}
}
