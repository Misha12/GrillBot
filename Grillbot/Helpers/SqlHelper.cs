using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Grillbot.Helpers
{
    public static class SqlHelper
    {
        public static string BuildWhereInClause<T>(string partialClause, string paramPrefix, IEnumerable<T> parameters)
        {
            string[] parameterNames = parameters.Select(
                (paramText, paramNumber) => "@" + paramPrefix + paramNumber.ToString())
                .ToArray();

            string inClause = string.Join(",", parameterNames);
            return string.Format(partialClause.Trim(), inClause);
        }

        public static void AddParamsToCommand<T>(this SqlCommand cmd, string paramPrefix, IEnumerable<T> parameters)
        {
            string[] parameterValues = parameters.Select((paramText) => paramText.ToString()).ToArray();
            string[] parameterNames = parameterValues.Select((paramText, paramNumber) => "@" + paramPrefix + paramNumber.ToString()).ToArray();

            for (int i = 0; i < parameterNames.Length; i++)
            {
                cmd.Parameters.AddWithValue(parameterNames[i], parameterValues[i]);
            }
        }
    }
}
