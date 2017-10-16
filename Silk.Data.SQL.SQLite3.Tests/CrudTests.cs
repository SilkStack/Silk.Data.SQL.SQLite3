using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using System;

namespace Silk.Data.SQL.SQLite3.Tests
{
	[TestClass]
	public class CrudTests
	{
		static CrudTests()
		{
			TestDb.Provider.ExecuteNonQuery(
				QueryExpression.CreateTable(
					"AutoIncrementTable",
					QueryExpression.DefineColumn("Id", SqlDataType.Int(), isAutoIncrement: true),
					QueryExpression.DefineColumn("Data", SqlDataType.Text())
					)
				);

			TestDb.Provider.ExecuteNonQuery(
				QueryExpression.CreateTable(
					"GuidTable",
					QueryExpression.DefineColumn("Id", SqlDataType.Guid(), isNullable: false),
					QueryExpression.DefineColumn("Data", SqlDataType.Text(), isNullable: false)
					)
				);
		}

		[TestMethod]
		public void InsertAutoIncrementRows()
		{
			TestDb.Provider.ExecuteNonQuery(
				QueryExpression.Insert(
					"AutoIncrementTable",
					new[] { "Data" },
					new object[] { "Test 1" },
					new object[] { "Test 2" }
					)
				);
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table("AutoIncrementTable")
				)))
			{
				Assert.IsTrue(queryResult.HasRows);

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(1, queryResult.GetInt32(0));
				Assert.AreEqual("Test 1", queryResult.GetString(1));

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(2, queryResult.GetInt32(0));
				Assert.AreEqual("Test 2", queryResult.GetString(1));
			}
		}

		[TestMethod]
		public void InsertAndGetId()
		{
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Transaction(
					QueryExpression.Insert(
						"AutoIncrementTable",
						new[] { "Data" },
						new object[] { "Unique Data" }
					),
					QueryExpression.Select(
						new[] { QueryExpression.LastInsertIdFunction() }
						)
				)))
			{
				Assert.IsTrue(queryResult.NextResult());
				Assert.IsTrue(queryResult.HasRows);
				Assert.IsTrue(queryResult.Read());
				Assert.IsTrue(queryResult.GetInt32(0) > 0);
			}
		}

		[TestMethod]
		public void InsertGuidRows()
		{
			var guids = new[]
			{
				Guid.NewGuid(),
				Guid.NewGuid()
			};
			TestDb.Provider.ExecuteNonQuery(
				QueryExpression.Insert(
					"GuidTable",
					new[] { "Id", "Data" },
					new object[] { guids[0], "Test 1" },
					new object[] { guids[1], "Test 2" }
					)
				);
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table("GuidTable")
				)))
			{
				Assert.IsTrue(queryResult.HasRows);

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(guids[0], queryResult.GetGuid(0));
				Assert.AreEqual("Test 1", queryResult.GetString(1));

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(guids[1], queryResult.GetGuid(0));
				Assert.AreEqual("Test 2", queryResult.GetString(1));
			}
		}

		[TestMethod]
		public void InsertFailsWithMissingNonNullable()
		{
			var exceptionCaught = false;
			try
			{
				TestDb.Provider.ExecuteNonQuery(
					QueryExpression.Insert(
						"GuidTable",
						new[] { "Data" },
						new object[] { "Test 1" },
						new object[] { "Test 2" }
						)
					);
			} catch (Microsoft.Data.Sqlite.SqliteException sqlException)
				when (sqlException.Message == "SQLite Error 19: 'NOT NULL constraint failed: GuidTable.Id'.")
			{
				exceptionCaught = true;
			}
			Assert.IsTrue(exceptionCaught);
		}

		[TestMethod]
		public void Update()
		{
			var id = Guid.NewGuid();
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Transaction(
					QueryExpression.Insert(
						"GuidTable",
						new[] { "Id", "Data" },
						new object[] { id, "Initial data" }
						),
					QueryExpression.Update(
						QueryExpression.Table("GuidTable"),
						QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.AreEqual, QueryExpression.Value(id)),
						QueryExpression.Assign(QueryExpression.Column("Data"), QueryExpression.Value("Been changed!"))
						),
					QueryExpression.Select(
						new[] { QueryExpression.Column("Data") },
						from: QueryExpression.Table("GuidTable"),
						where: QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.AreEqual, QueryExpression.Value(id))
						)
				)))
			{
				Assert.IsTrue(queryResult.NextResult());
				Assert.IsTrue(queryResult.HasRows);
				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual("Been changed!", queryResult.GetString(0));
			}
		}

		[TestMethod]
		public void Delete()
		{
			var id = Guid.NewGuid();
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Transaction(
					QueryExpression.Insert(
						"GuidTable",
						new[] { "Id", "Data" },
						new object[] { id, "Initial data" }
						),
					QueryExpression.Delete(
						QueryExpression.Table("GuidTable"),
						QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.AreEqual, QueryExpression.Value(id))
						),
					QueryExpression.Select(
						new[] { QueryExpression.Column("Data") },
						from: QueryExpression.Table("GuidTable"),
						where: QueryExpression.Compare(QueryExpression.Column("Id"), ComparisonOperator.AreEqual, QueryExpression.Value(id))
						)
				)))
			{
				Assert.IsTrue(queryResult.NextResult());
				Assert.IsFalse(queryResult.HasRows);
			}
		}
	}
}
