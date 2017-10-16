using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using System.Threading.Tasks;

namespace Silk.Data.SQL.SQLite3.Tests
{
	[TestClass]
	public class TableTests
	{
		[TestMethod]
		public async Task TableExists()
		{
			Assert.IsTrue(await TableExistsAsync("Persistent").ConfigureAwait(false));
		}

		[TestMethod]
		public async Task CreateTable()
		{
			await TestDb.Provider.ExecuteNonQueryAsync(
				QueryExpression.CreateTable(
					"CreateTest",
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("Text", SqlDataType.Text(), isNullable: false)
					)
				).ConfigureAwait(false);

			Assert.IsTrue(await TableExistsAsync("CreateTest").ConfigureAwait(false));
		}

		[TestMethod]
		public void CreateTableWithCompositePrimaryKey()
		{
			TestDb.Provider.ExecuteNonQuery(
				QueryExpression.CreateTable(
					"CreateCompositeTest",
					QueryExpression.DefineColumn("AutoId", SqlDataType.Int(), isAutoIncrement: true, isPrimaryKey: true),
					QueryExpression.DefineColumn("RefId", SqlDataType.Guid(), isNullable: false, isPrimaryKey: true)
					)
				);

			Assert.IsTrue(TableExistsAsync("CreateCompositeTest").GetAwaiter().GetResult());
		}

		[TestMethod]
		public async Task DropTable()
		{
			await TestDb.Provider.ExecuteNonQueryAsync(
				QueryExpression.DropTable("ForDrop")
				).ConfigureAwait(false);

			Assert.IsFalse(await TableExistsAsync("ForDrop").ConfigureAwait(false));
		}

		private static async Task<bool> TableExistsAsync(string tableName)
		{
			using (var queryResult = await
				TestDb.Provider.ExecuteReaderAsync(
					QueryExpression.TableExists(tableName)
					)
				.ConfigureAwait(false))
			{
				if (!queryResult.HasRows)
					return false;
				if (!await queryResult.ReadAsync().ConfigureAwait(false))
					return false;
				return queryResult.GetInt32(0) == 1;
			}
		}
	}
}
