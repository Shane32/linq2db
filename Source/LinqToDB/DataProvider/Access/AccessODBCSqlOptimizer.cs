﻿namespace LinqToDB.DataProvider.Access
{
	using SqlProvider;
	using SqlQuery;

	class AccessODBCSqlOptimizer : AccessSqlOptimizer
	{
		public AccessODBCSqlOptimizer(SqlProviderFlags sqlProviderFlags) : base(sqlProviderFlags)
		{
		}

		public override SqlStatement Finalize(SqlStatement statement, bool inlineParameters)
		{
			statement = base.Finalize(statement, inlineParameters);

			statement = WrapParameters(statement);

			return statement;
		}

		private SqlStatement WrapParameters(SqlStatement statement)
		{
			// ODBC cannot properly type result column, if it produced by parameter
			// To fix it we will wrap all parameters, used as select columns into type-cast
			// we use CVar, as other cast functions generate error for null-values parameters and CVar works
			
			// we are interested only in selects, because error generated by data reader, not by database itself
			if (statement.QueryType != QueryType.Select)
				return statement;

			statement = ConvertVisitor.Convert(statement, (visitor, e) =>
			{
				if (e is SqlParameter p && p.IsQueryParameter && visitor.ParentElement is SqlColumn)
					return new SqlExpression(p.Type.SystemType, "CVar({0})", Precedence.Primary, p);

				return e;
			});

			return statement;
		}
	}
}
