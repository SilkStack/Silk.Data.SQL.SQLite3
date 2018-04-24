using System.Text;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Queries;
using Silk.Data.SQL.SQLite3.Expressions;
using System.Linq;

namespace Silk.Data.SQL.SQLite3
{
	public class SQLite3QueryConverter : QueryConverterCommonBase
	{
		protected override string ProviderName => SQLite3DataProvider.PROVIDER_NAME;

		protected override string AutoIncrementSql => "";

		public bool NonBinaryGuids { get; }

		public SQLite3QueryConverter(bool nonBinaryGuids)
		{
			ExpressionWriter = new SQLite3QueryWriter(Sql, this);
			NonBinaryGuids = nonBinaryGuids;
		}

		protected override string GetDbDatatype(SqlDataType sqlDataType)
		{
			switch (sqlDataType.BaseType)
			{
				case SqlBaseType.Guid:
					return NonBinaryGuids ? "TEXT" : "BLOB";
				case SqlBaseType.TinyInt:
				case SqlBaseType.SmallInt:
				case SqlBaseType.Int:
				case SqlBaseType.BigInt:
					return "INTEGER";
				case SqlBaseType.Float:
					return "REAL";
				case SqlBaseType.Bit:
				case SqlBaseType.Decimal:
				case SqlBaseType.Date:
				case SqlBaseType.Time:
				case SqlBaseType.DateTime:
					return "NUMERIC";
				case SqlBaseType.Text:
					return "TEXT";
				case SqlBaseType.Binary:
					return "BLOB";
			}
			throw new System.NotSupportedException($"SQL data type not supported: {sqlDataType.BaseType}.");
		}

		protected override void WriteFunctionToSql(QueryExpression queryExpression)
		{
			switch (queryExpression)
			{
				case LastInsertIdFunctionExpression lastInsertIdExpression:
					Sql.Append("last_insert_rowid()");
					return;
				case TableExistsVirtualFunctionExpression tableExistsExpression:
					ExpressionWriter.Visit(
						QueryExpression.Select(
							new[] { QueryExpression.Value(1) },
							from: QueryExpression.Table("sqlite_master"),
							where: QueryExpression.AndAlso(
								QueryExpression.Compare(QueryExpression.Column("type"), ComparisonOperator.AreEqual, QueryExpression.Value("table")),
								QueryExpression.Compare(QueryExpression.Column("name"), ComparisonOperator.AreEqual, QueryExpression.Value(tableExistsExpression.Table.TableName))
								)
							)
						);
					return;
			}
			base.WriteFunctionToSql(queryExpression);
		}

		protected override string QuoteIdentifier(string schemaComponent)
		{
			if (schemaComponent == "*")
				return "*";
			return $"[{schemaComponent}]";
		}

		private class SQLite3QueryWriter : QueryWriter
		{
			public new SQLite3QueryConverter Converter { get; }

			public SQLite3QueryWriter(StringBuilder sql,
				SQLite3QueryConverter converter) : base(sql, converter)
			{
				Converter = converter;
			}

			protected override void VisitQuery(QueryExpression queryExpression)
			{
				switch (queryExpression)
				{
					case SQLite3RawQueryExpression rawExpression:
						Sql.Append(rawExpression.SqlText);
						break;
					case CreateTableExpression create:
						Sql.Append($"CREATE TABLE {Converter.QuoteIdentifier(create.TableName)} (");
						VisitExpressionGroup(create.ColumnDefinitions, ExpressionGroupType.ColumnDefinitions);
						var primaryKeyColumnNames = create.ColumnDefinitions
								.Where(q => q.IsPrimaryKey || q.IsAutoIncrement)
								.Select(q => Converter.QuoteIdentifier(q.ColumnName))
								.ToArray();
						if (primaryKeyColumnNames.Length > 0)
						{
							Sql.Append($", CONSTRAINT {Converter.QuoteIdentifier("PK")} PRIMARY KEY ({string.Join(",", primaryKeyColumnNames)})");
						}
						Sql.Append("); ");
						break;
					default:
						base.VisitQuery(queryExpression);
						break;
				}
			}
		}
	}
}
