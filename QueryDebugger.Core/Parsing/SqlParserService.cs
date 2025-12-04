using Microsoft.SqlServer.TransactSql.ScriptDom;
using QueryDebugger.Core.Models;
using System.Collections.Generic;
using System.IO;

namespace QueryDebugger.Core.Parsing
{
    public class SqlParserService
    {
        public ParsedQuery Parse(string sql)
        {
            var parser = new TSql160Parser(true);
            IList<ParseError> errors;
            var fragment = parser.Parse(new StringReader(sql), out errors);

            if (errors.Count > 0)
            {
                // In a real app, throw or return errors
                throw new System.Exception($"SQL Parse Error: {errors[0].Message}");
            }

            var visitor = new QueryVisitor();
            fragment.Accept(visitor);

            return visitor.Result;
        }
    }
}

