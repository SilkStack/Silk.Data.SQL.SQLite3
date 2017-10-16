using Silk.Data.SQL.SQLite3.Expressions;

namespace Silk.Data.SQL.SQLite3
{
	public static class SQLite3
	{
		public static SQLite3RawQueryExpression Raw(string sql)
		{
			return new SQLite3RawQueryExpression(sql);
		}
	}
}
