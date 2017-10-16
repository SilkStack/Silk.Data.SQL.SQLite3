using Silk.Data.SQL.Expressions;

namespace Silk.Data.SQL.SQLite3.Expressions
{
	public class SQLite3RawQueryExpression : QueryExpression
	{
		public string SqlText { get; }

		public override ExpressionNodeType NodeType => ExpressionNodeType.Query;

		public SQLite3RawQueryExpression(string sqlText)
		{
			SqlText = sqlText;
		}
	}
}
