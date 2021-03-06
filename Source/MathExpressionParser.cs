using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleCalculator
{
    public static class MathExpressionParser
    {
        private static IList<string> Tokenize(string input)
        {
            IList<string> tokens = new List<string>();
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].IsDigit())
                {
                    for (int j = i; j < input.Length; j++)
                    {
                        if ((!input[j].IsDigit() && input[j] != '.') || j == input.Length - 1)
                        {
                            tokens.Add(input[i..j]);
                            i = j - 1;
                            break;
                        }
                    }
                }
                else if (input[i].IsLetter() || input[i] == '_')
                {
                    for (int j = i; j < input.Length; j++)
                    {
                        if ((!input[j].IsDigit() && !input[j].IsLetter() && input[j] != '_') || j == input.Length - 1)
                        {
                            tokens.Add(input[i..j]);
                            i = j - 1;
                            break;
                        }
                    }
                }
                else if (input[i].IsWhiteSpace())
                {
                    continue;
                }
                else
                {
                    tokens.Add(input[i].ToString());
                }
            }
            return tokens;
        }

        private static string UnpackTokens(IList<string> tokens, string delimiter = "")
        {
            string result = string.Empty;
            for (int i = 0; i < tokens.Count; i++)
            {
                result += tokens[i];
                if (i != tokens.Count - 1)
                {
                    result += delimiter;
                }
            }
            return result;
        }

        /// <summary>
        /// Get the index of the <paramref name="index"/>-th <see langword="char"/> in the <paramref name="token"/>-th token as if the tokens are merged into a single string.
        /// </summary>
        /// <param name="tokens">The target tokens.</param>
        /// <param name="token">The index of the token to get the index from.</param>
        /// <param name="index">The index of the <see langword="char"/> in the token to get the index from.</param>
        /// <param name="delimiterLength">The length of the delimiter to add to the string of the unpacked<paramref name="tokens"/> between each tokens before the result is computed.</param>
        /// <returns>The index of the <paramref name="index"/>-th <see langword="char"/> in the <paramref name="token"/>-th token as if the tokens are merged into a single string.</returns>
        private static int GetIndexInStringFromToken(IList<string> tokens, int token, int index, int delimiterLength = 0)
        {
            int result = 0;
            for (int i = 0; i <= token; i++)
            {
                result += tokens[i].Length;
                if (i != token)
                {
                    result += delimiterLength;
                }
            }
            return result + index;
        }

        private static int GetCloseParenthesisIndex(IList<string> tokens, int start)
        {
            if (tokens[start] != "(")
            {
                throw new ArgumentException("Specified start index is not an open parenthesis", nameof(start));
            }
            int open = 0;
            for (int i = start; i < tokens.Count; i++)
            {
                if (tokens[i] == "(")
                {
                    open++;
                }
                else if (tokens[i] == ")")
                {
                    if (open == 0)
                    {
                        throw new ArgumentException("Too many closing parentheses", nameof(tokens));
                    }
                    else
                    {
                        open--;
                    }
                }
                if (open == 0)
                {
                    return i;
                }
                if (i == tokens.Count - 1 && open != 0)
                {
                    throw new ArgumentException("Too many opening parentheses", nameof(tokens));
                }
            }
            throw new Exception("This exception should never be thrown");
        }

        private static int GetOpenParenthesisIndex(IList<string> tokens, int end)
        {
            if (tokens[end] != ")")
            {
                throw new ArgumentException("Specified start index is not an closing parenthesis", nameof(end));
            }
            int close = 0;
            for (int i = end; i >= 0; i--)
            {
                if (tokens[i] == ")")
                {
                    close++;
                }
                else if (tokens[i] == "(")
                {
                    if (close == 0)
                    {
                        throw new ArgumentException("Too many opening parentheses", nameof(tokens));
                    }
                    else
                    {
                        close--;
                    }
                }
                if (close == 0)
                {
                    return i;
                }
                if (i == 0 && close != 0)
                {
                    throw new ArgumentException("Too many closing parentheses", nameof(tokens));
                }
            }
            throw new Exception("This exception should never be thrown");
        }

        public static string Parse(string input, IList<MathOperator> operators)
        {
            string RealParse(IList<string> tokens)
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (tokens[i] == "(")
                    {
                        int closeParenthesisIndex = GetCloseParenthesisIndex(tokens, i);
                        if
                        (
                            operators.Any
                            (
                                (MathOperator @operator) =>
                                {
                                    return @operator is FunctionMathOperator && @operator.Name == tokens[i == 0 ? 0 : i - 1]; // if the previous token before the parenthesis is a function
                                }
                            )
                        )
                        {
                            if (i == 1) // if the function is the first token
                            {
                                int totalCommas = 0, commas = 0;
                                for (int j = 2, start = 2; j < tokens.Count - 1; j++) // extract arguments
                                {
                                    if (tokens[j] == ",")
                                    {
                                        commas++;
                                        start = j + 1;
                                    }
                                    if ((tokens[j] == "," && commas > totalCommas) || j == tokens.Count - 2)
                                    {
                                        tokens = tokens.ReplaceRange
                                        (
                                            RealParse
                                            (
                                                tokens.GetRange
                                                (
                                                    start,
                                                    j == tokens.Count - 2 ? j - start + 1 : j - start
                                                )
                                            ),
                                            start,
                                            j == tokens.Count - 2 ? j - start + 1 : j - start
                                        );
                                        j = 1;
                                        start = 2; // reset to start over because the indices have now changed
                                        if (totalCommas < commas)
                                        {
                                            totalCommas = commas;
                                        }
                                        commas = 0;
                                    }
                                }
                            }
                            else
                            {
                                tokens = tokens.ReplaceRange
                                (
                                    RealParse
                                    (
                                        tokens.GetRange // starting from the function operator to the closing parenthesis
                                        (
                                            i - 1,
                                            closeParenthesisIndex - (i - 1) + 1
                                        )
                                    ),
                                    i - 1,
                                    closeParenthesisIndex - (i - 1) + 1
                                );
                            }
                        }
                        else // normal parenthesis
                        {
                            tokens = tokens.ReplaceRange
                            (
                                RealParse
                                (
                                    tokens.GetRange // starting from the opening parenthesis + 1 to the closing parenthesis - 1
                                    (
                                        i + 1,
                                        closeParenthesisIndex - (i + 1)
                                    )
                                ),
                                i,
                                closeParenthesisIndex - i + 1
                            );
                        }
                    }
                }
                /*
                 * At this point, after all the recursive algorithm above, we can be certain of a few properties of the expression we are about to parse:
                 * - If it contains a function, the function must be the first token in the expression and there are no other functions in the expression.
                 * - All parentheses(excluding the ones belonging to a function) have been resolved.
                 * - No nested functions.
                 * - All function arguments have been evaluated, meaning that each argument contains only one token and it is a number.
                 */
                MathOperatorPrecedence[] precedences = typeof(MathOperatorPrecedence).GetEnumValues().Cast<MathOperatorPrecedence>().Reverse().ToArray();
                foreach (MathOperatorPrecedence precedence in precedences)
                {

                }
                return tokens[0];
            }

            IList<string> tokens = Tokenize(input);
            for (int i = 0; i < tokens.Count; i++)
            {
                // throws an exception if a matching parenthesis isn't found, indicating that input has unmatching parentheses
                if (tokens[i] == "(")
                {
                    GetCloseParenthesisIndex(tokens, i);
                }
                else if (tokens[i] == ")")
                {
                    GetOpenParenthesisIndex(tokens, i);
                }
            }
            IList<MathOperator> invalidOperators = MathOperator.GetInvalidOperators(operators);
            if (invalidOperators.Count != 0)
            {
                string invalidNames = string.Empty;
                foreach (MathOperator @operator in invalidOperators)
                {
                    invalidNames += $"{@operator.Name} ";
                }
                throw new ArgumentException($"Invalid {nameof(MathOperator)} names: {invalidNames}", nameof(operators));
            }
            return RealParse(tokens);
        }
    }
}