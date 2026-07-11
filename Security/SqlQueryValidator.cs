using System.Text.RegularExpressions;

namespace pos_service.Security
{
    public static class SqlQueryValidator
    {
        private static readonly string[] BlacklistedKeywords = new[]
        {
            "insert", "update", "delete", "truncate", "drop", "alter", 
            "execute", "merge", "call", "exec", "create", "grant", "revoke"
        };

        public static bool ValidateSelectOnly(string sqlQuery, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(sqlQuery))
            {
                errorMessage = "SQL query cannot be empty.";
                return false;
            }

            // Clean query by removing comments first to prevent bypasses inside comments
            var cleanedQuery = RemoveSqlComments(sqlQuery).Trim();

            if (string.IsNullOrWhiteSpace(cleanedQuery))
            {
                errorMessage = "SQL query cannot be empty after removing comments.";
                return false;
            }

            if (!cleanedQuery.StartsWith("select", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Only SELECT queries are permitted.";
                return false;
            }

            // Tokenize by word boundaries and check for blacklisted keywords
            foreach (var keyword in BlacklistedKeywords)
            {
                var pattern = $@"\b{keyword}\b";
                if (Regex.IsMatch(cleanedQuery, pattern, RegexOptions.IgnoreCase))
                {
                    errorMessage = $"Query contains restricted instruction: '{keyword.ToUpper()}' commands are not allowed.";
                    return false;
                }
            }

            return true;
        }

        private static string RemoveSqlComments(string sql)
        {
            // Remove block comments /* ... */
            var blockCommentsPattern = @"/\*[\s\S]*?\*/";
            var result = Regex.Replace(sql, blockCommentsPattern, "");

            // Remove line comments -- ...
            var lineCommentsPattern = @"--.*?\r?\n";
            result = Regex.Replace(result, lineCommentsPattern, "");
            
            // Also handle single-line comments that might be at the end of the query without trailing newline
            var endLineCommentPattern = @"--.*$";
            result = Regex.Replace(result, endLineCommentPattern, "");

            return result;
        }
    }
}
