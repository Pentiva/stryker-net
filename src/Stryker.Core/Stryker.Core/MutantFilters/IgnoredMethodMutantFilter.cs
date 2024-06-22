using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Stryker.Core.Mutants;
using Stryker.Core.Mutators;
using Stryker.Core.Options;
using Stryker.Core.ProjectComponents;

namespace Stryker.Core.MutantFilters
{
    /// <summary>
    /// Checks if the mutants are part of ignored method calls.
    /// </summary>
    /// <seealso cref="IMutantFilter" />
    public sealed class IgnoredMethodMutantFilter : IMutantFilter
    {
        public MutantFilter Type => MutantFilter.IgnoreMethod;

        public string DisplayName => "method filter";

        private readonly SyntaxTriviaRemover _triviaRemover = new();

        public IEnumerable<Mutant> FilterMutants(IEnumerable<Mutant> mutants, IReadOnlyFileLeaf file, StrykerOptions options) =>
            options.IgnoredMethods.Any() ?
                    mutants.Where(m => !IsPartOfIgnoredMethodCall(m.Mutation.OriginalNode, m.Mutation.Type, options)) :
                    mutants;

        private bool IsPartOfIgnoredMethodCall(SyntaxNode syntaxNode, Mutator type, StrykerOptions options, bool canGoUp = true) =>
            syntaxNode switch
            {
                // Check if the current node is an invocation. This will also ignore invokable properties like `Func<bool> MyProp { get;}`
                // follow the invocation chain to see if it ends with a filtered one
                InvocationExpressionSyntax invocation => MatchesAnIgnoredMethod(_triviaRemover.Visit(invocation.Expression).ToString(), type, options)
                    || (invocation.Parent is MemberAccessExpressionSyntax && invocation.Parent.Parent is InvocationExpressionSyntax &&
                    IsPartOfIgnoredMethodCall(invocation.Parent.Parent, type, options, false)) || (canGoUp && IsPartOfIgnoredMethodCall(invocation.Parent, type, options)),

                // Check if the current node is an object creation syntax (constructor invocation).
                ObjectCreationExpressionSyntax creation => MatchesAnIgnoredMethod(_triviaRemover.Visit(creation.Type) + ".ctor", type, options),

                ConditionalAccessExpressionSyntax conditional => IsPartOfIgnoredMethodCall(conditional.WhenNotNull, type, options, false),

                ConditionalExpressionSyntax conditionalExpression => (IsPartOfIgnoredMethodCall(conditionalExpression.WhenTrue, type, options, false)
                                                                      && IsPartOfIgnoredMethodCall(conditionalExpression.WhenFalse, type, options, false))
                                                                     || (canGoUp && IsPartOfIgnoredMethodCall(conditionalExpression.Parent, type, options)),

                ExpressionStatementSyntax expressionStatement => IsPartOfIgnoredMethodCall(expressionStatement.Expression, type, options, false),

                AssignmentExpressionSyntax assignmentExpression =>  IsPartOfIgnoredMethodCall(assignmentExpression.Right, type, options, false),

                LocalDeclarationStatementSyntax localDeclaration => localDeclaration.Declaration.Variables.All(v => IsPartOfIgnoredMethodCall(v.Initializer?.Value, type, options, false)),
  
                BlockSyntax { Statements.Count: >0 } block => block.Statements.All(s=> IsPartOfIgnoredMethodCall(s, type, options, false)),
                
                MemberDeclarationSyntax => false,

                // Traverse the tree upwards.
                { Parent: not null }  => canGoUp && IsPartOfIgnoredMethodCall(syntaxNode.Parent, type, options),
                _ => false,
            };

        private static bool MatchesAnIgnoredMethod(string expressionString, Mutator type, StrykerOptions options) => options.IgnoredMethods.Any(r => r.IsMatch(expressionString, type));

        /// <summary>
        /// Removes comments, whitespace, and other junk from a syntax tree.
        /// </summary>
        private sealed class SyntaxTriviaRemover : CSharpSyntaxRewriter
        {
            public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia) => default;
        }
    }
}
