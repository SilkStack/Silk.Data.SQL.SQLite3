﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.SQLite3.Tests
{
	[TestClass]
	public class SelectTests
	{
		static SelectTests()
		{
			TestDb.Provider.ExecuteNonQuery(QueryExpression.Transaction(
				QueryExpression.CreateTable(
					"SelectTestTable",
					QueryExpression.DefineColumn("Text", SqlDataType.Text())
					),
				QueryExpression.Insert(
					"SelectTestTable",
					new[] { "Text" },
					new object[] { "A" },
					new object[] { "A" },
					new object[] { "B" },
					new object[] { "B" },
					new object[] { "C" },
					new object[] { "C" }
					)
				));
		}

		[TestMethod]
		public void SelectValues()
		{
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.Value(1), QueryExpression.Value("Hello World") }
				)))
			{
				Assert.IsTrue(queryResult.HasRows);
				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(1, queryResult.GetInt32(0));
				Assert.AreEqual("Hello World", queryResult.GetString(1));
			}
		}

		[TestMethod]
		public void SelectAliasValues()
		{
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] {
						QueryExpression.Alias(QueryExpression.Value(1), "int"),
						QueryExpression.Alias(QueryExpression.Value("Hello World"), "str")
					}
				)))
			{
				Assert.IsTrue(queryResult.HasRows);
				Assert.AreEqual("int", queryResult.GetName(0));
				Assert.AreEqual("str", queryResult.GetName(1));
				Assert.AreEqual(0, queryResult.GetOrdinal("int"));
				Assert.AreEqual(1, queryResult.GetOrdinal("str"));
				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(1, queryResult.GetInt32(0));
				Assert.AreEqual("Hello World", queryResult.GetString(1));
			}
		}

		[TestMethod]
		public void SelectMultipleResultSets()
		{
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Transaction(
					QueryExpression.Select(new[] { QueryExpression.Value(1) }),
					QueryExpression.Select(new[] { QueryExpression.Value(2) }),
					QueryExpression.Select(new[] { QueryExpression.Value(3) })
				)))
			{
				Assert.IsTrue(queryResult.NextResult());
				Assert.IsTrue(queryResult.HasRows);
				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(1, queryResult.GetInt32(0));

				Assert.IsTrue(queryResult.NextResult());
				Assert.IsTrue(queryResult.HasRows);
				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(2, queryResult.GetInt32(0));

				Assert.IsTrue(queryResult.NextResult());
				Assert.IsTrue(queryResult.HasRows);
				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(3, queryResult.GetInt32(0));
			}
		}

		[TestMethod]
		public void SelectRandom()
		{
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(new[] { QueryExpression.Random() })
				))
			{
				Assert.IsTrue(queryResult.HasRows);
				Assert.IsTrue(queryResult.Read());
				//  this will throw an exception on failure
				queryResult.GetInt64(0);
			}
		}

		[TestMethod]
		public void SelectFromSubSelect()
		{
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Select(
						new[] { QueryExpression.Value(1) }
						)
				)))
			{
				Assert.IsTrue(queryResult.HasRows);
				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(1, queryResult.GetInt32(0));
			}
		}

		[TestMethod]
		public void SelectFromSubSelectUsingAlias()
		{
			var subSelect = QueryExpression.Select(
				new[] { QueryExpression.Value(1) }
				);
			var alias = QueryExpression.Alias(subSelect, "subSel");
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.All(alias.Identifier) },
					from: alias
				)))
			{
				Assert.IsTrue(queryResult.HasRows);
				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(1, queryResult.GetInt32(0));
			}
		}

		[TestMethod]
		public void SelectCount()
		{
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.CountFunction() },
					from: QueryExpression.Select(
						new[] { QueryExpression.Value(2) }
						)
				)))
			{
				Assert.IsTrue(queryResult.HasRows);
				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(1, queryResult.GetInt32(0));
			}
		}

		[TestMethod]
		public void SelectCountDistinct()
		{
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.CountFunction(
						QueryExpression.Distinct(QueryExpression.Column("Text"))
						) },
					from: QueryExpression.Table("SelectTestTable")
				)))
			{
				Assert.IsTrue(queryResult.HasRows);
				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual(3, queryResult.GetInt32(0));
			}
		}

		[TestMethod]
		public void SelectWithOrderBy()
		{
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table("SelectTestTable"),
					orderBy: new[] { QueryExpression.Descending(QueryExpression.Column("Text")) }
				)))
			{
				Assert.IsTrue(queryResult.HasRows);

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual("C", queryResult.GetString(0));
				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual("C", queryResult.GetString(0));

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual("B", queryResult.GetString(0));
				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual("B", queryResult.GetString(0));

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual("A", queryResult.GetString(0));
				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual("A", queryResult.GetString(0));
			}
		}

		[TestMethod]
		public void SelectWithGroupBy()
		{
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table("SelectTestTable"),
					orderBy: new[] { QueryExpression.Column("Text") },
					groupBy: new[] { QueryExpression.Column("Text") }
				)))
			{
				Assert.IsTrue(queryResult.HasRows);

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual("A", queryResult.GetString(0));

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual("B", queryResult.GetString(0));

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual("C", queryResult.GetString(0));
			}
		}

		[TestMethod]
		public void SelectWithLimit()
		{
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table("SelectTestTable"),
					orderBy: new[] { QueryExpression.Column("Text") },
					limit: QueryExpression.Value(1)
				)))
			{
				Assert.IsTrue(queryResult.HasRows);

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual("A", queryResult.GetString(0));

				Assert.IsFalse(queryResult.Read());
			}
		}

		[TestMethod]
		public void SelectWithOffset()
		{
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table("SelectTestTable"),
					orderBy: new[] { QueryExpression.Column("Text") },
					offset: QueryExpression.Value(2)
				)))
			{
				Assert.IsTrue(queryResult.HasRows);

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual("B", queryResult.GetString(0));
			}
		}

		[TestMethod]
		public void SelectWithOffsetAndLimit()
		{
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table("SelectTestTable"),
					orderBy: new[] { QueryExpression.Column("Text") },
					offset: QueryExpression.Value(2),
					limit: QueryExpression.Value(1)
				)))
			{
				Assert.IsTrue(queryResult.HasRows);

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual("B", queryResult.GetString(0));

				Assert.IsFalse(queryResult.Read());
			}
		}

		[TestMethod]
		public void SelectWithWhere()
		{
			using (var queryResult = TestDb.Provider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table("SelectTestTable"),
					where: QueryExpression.Compare(QueryExpression.Column("Text"), ComparisonOperator.AreEqual, QueryExpression.Value("C")),
					limit: QueryExpression.Value(1)
				)))
			{
				Assert.IsTrue(queryResult.HasRows);

				Assert.IsTrue(queryResult.Read());
				Assert.AreEqual("C", queryResult.GetString(0));

				Assert.IsFalse(queryResult.Read());
			}
		}
	}
}