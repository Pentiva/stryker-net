using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Stryker.Core.Mutants;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Stryker.Core.Mutators;

public class IntegerMutator : MutatorBase<LiteralExpressionSyntax> {
    public override MutationLevel MutationLevel => MutationLevel.Advanced;

    public override IEnumerable<Mutation> ApplyMutations(LiteralExpressionSyntax node, SemanticModel semanticModel) {
        if (!ShouldMutate(node))
        {
            yield break;
        }

        if (node.Token.Value is not int currentValue)
        {
            yield break;
        }

        if (currentValue is not 0) {
            yield return new Mutation {
                OriginalNode = node,
                ReplacementNode = LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(-currentValue)),
                DisplayName = "Integer negation mutation",
                Type = Mutator.Number
            };

            yield return new Mutation {
                OriginalNode = node,
                ReplacementNode = LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)),
                DisplayName = "Integer nullification mutation",
                Type = Mutator.Number
            };
        }
        else
        {
            yield return new Mutation {
                OriginalNode = node,
                ReplacementNode = LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(42)),
                DisplayName = "Integer un-nullification mutation",
                Type = Mutator.Number
            };
        }
    }

    private static bool ShouldMutate(LiteralExpressionSyntax node) =>
        node.Kind() == SyntaxKind.NumericLiteralExpression &&
        !IsElementAccessExpression(node)                   &&
        !IsBitwiseComparison(node)                         &&
        !IsConstant(node)                                  &&
        !IsPartOfEnumDeclaration(node.Parent);

    private static bool IsElementAccessExpression(LiteralExpressionSyntax node) =>
        node.Parent is
            ArgumentSyntax { Parent: BracketedArgumentListSyntax { Parent: ElementAccessExpressionSyntax } } or
            RangeExpressionSyntax { Parent: ArgumentSyntax { Parent: BracketedArgumentListSyntax { Parent: ElementAccessExpressionSyntax } } } or
            PrefixUnaryExpressionSyntax { Parent: RangeExpressionSyntax { Parent: ArgumentSyntax { Parent: BracketedArgumentListSyntax { Parent: ElementAccessExpressionSyntax } } } };

    private static bool IsConstant(LiteralExpressionSyntax node) =>
        node.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax { Modifiers: var m } } } } &&
        m.Any(SyntaxKind.ConstKeyword);

    private static bool IsPartOfEnumDeclaration(SyntaxNode node) => node is not null && (node is EnumDeclarationSyntax || IsPartOfEnumDeclaration(node.Parent));

    private static bool IsBitwiseComparison(LiteralExpressionSyntax node) =>
        node.Parent is BinaryExpressionSyntax
        {
            RawKind:
            (int)SyntaxKind.EqualsExpression or
            (int)SyntaxKind.NotEqualsExpression,
            Left: ParenthesizedExpressionSyntax
            {
                Expression: BinaryExpressionSyntax
                {
                    RawKind:
                    (int)SyntaxKind.BitwiseAndExpression or
                    (int)SyntaxKind.BitwiseOrExpression or
                    (int)SyntaxKind.BitwiseNotExpression
                }
            } or BinaryExpressionSyntax
            {
                RawKind:
                (int)SyntaxKind.BitwiseAndExpression or
                (int)SyntaxKind.BitwiseOrExpression or
                (int)SyntaxKind.BitwiseNotExpression
            }
        } or BinaryExpressionSyntax
        {
            RawKind:
            (int)SyntaxKind.EqualsExpression or
            (int)SyntaxKind.NotEqualsExpression,
            Right: ParenthesizedExpressionSyntax
            {
                Expression: BinaryExpressionSyntax
                {
                    RawKind:
                    (int)SyntaxKind.BitwiseAndExpression or
                    (int)SyntaxKind.BitwiseOrExpression or
                    (int)SyntaxKind.BitwiseNotExpression
                }
            } or BinaryExpressionSyntax
            {
                RawKind:
                (int)SyntaxKind.BitwiseAndExpression or
                (int)SyntaxKind.BitwiseOrExpression or
                (int)SyntaxKind.BitwiseNotExpression
            }
        };
}
