using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Stryker.Core.Mutators;
using Xunit;

namespace Stryker.Core.UnitTest.Mutators;

public class IntegerMutatorTests : TestBase
{
    [Theory]
    [InlineData(SyntaxKind.StringLiteralExpression)]
    [InlineData(SyntaxKind.CharacterLiteralExpression)]
    [InlineData(SyntaxKind.NullLiteralExpression)]
    [InlineData(SyntaxKind.DefaultLiteralExpression)]
    [InlineData(SyntaxKind.TrueLiteralExpression)]
    [InlineData(SyntaxKind.FalseLiteralExpression)]
    public void ShouldNotMutate(SyntaxKind original)
    {
        var target = new IntegerMutator();

        var result = target.ApplyMutations(SyntaxFactory.LiteralExpression(original), null).ToList();

        Assert.Empty(result);
    }

    [Theory]
    [InlineData("array[1]")]
    [InlineData("array[1..2]")]
    [InlineData("array[1..^1]")]
    [InlineData("(variable & Flag) == 0")]
    [InlineData("0 == (variable & Flag)")]
    public void ShouldNotMutate2(string expression)
    {
        var target = new IntegerMutator();

        var parent = SyntaxFactory.ParseExpression(expression);
        var child = parent.DescendantNodes(_ => true).OfType<LiteralExpressionSyntax>().FirstOrDefault();

        var result = target.ApplyMutations(child, null).ToList();

        Assert.Empty(result);
    }

    [Theory]
    [InlineData("private const int Constant = 55;")]
    [InlineData("""
                [Flags]
                public enum FlagEnum {
                    None = 0,
                    A = 1 << 0,
                    B = 1 << 1,
                    C = 1 << 2,
                    D = 1 << 3
                }
                """)]
    [InlineData("""
                [Flags]
                public enum FlagEnum {
                    A = 1 << 0,
                    B = 1 << 1,
                    C = 1 << 2,
                    D = 1 << 3
                }
                """)]
    public void ShouldNotMutate3(string expression)
    {
        var target = new IntegerMutator();

        var parent = SyntaxFactory.ParseMemberDeclaration(expression);
        var child = parent?.DescendantNodes(_ => true).OfType<LiteralExpressionSyntax>().FirstOrDefault();

        var result = target.ApplyMutations(child, null).ToList();

        Assert.Empty(result);
    }

    [Theory]
    [InlineData(10, new object[] { 0, -10 })]
    [InlineData(-10, new object[] { 0, 10 })]
    public void ShouldMutate(int original, object[] expected)
    {
        var target = new IntegerMutator();

        var parent =
            SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                                            SyntaxFactory.Literal(original)));

        var result = target.ApplyMutations(parent.DescendantNodes().First() as LiteralExpressionSyntax, null).ToList();

        foreach (var mutation in result)
        {
            mutation.ReplacementNode.ShouldBeOfType<LiteralExpressionSyntax>().Token.Value.ShouldBeOneOf(expected);
            mutation.DisplayName.ShouldMatch("Integer .+ mutation");
        }
    }

    [Theory]
    [InlineData("(variable + Flag) != 0", 1)]
    [InlineData("2 + 5", 2)]
    [InlineData("variable - 5", 2)]
    [InlineData("Method(3)", 2)]
    [InlineData("Method(0)", 1)]
    public void ShouldMutate2(string expression, int expectedMutations)
    {
        var target = new IntegerMutator();

        var parent = SyntaxFactory.ParseExpression(expression);
        var child = parent.DescendantNodes(_ => true).OfType<LiteralExpressionSyntax>().FirstOrDefault();

        var result = target.ApplyMutations(child, null).ToList();

        result.Count.ShouldBe(expectedMutations);
    }

    [Theory]
    [InlineData("public int Abc = 23;", 2)]
    [InlineData("public int Abc { get; } = 23;", 2)]
    [InlineData("private int Abc { get; } = 0;", 1)]
    [InlineData("""
                public struct Structure {
                    public int A = 1;
                }
                """, 2)]
    public void ShouldMutate3(string expression, int expectedMutations)
    {
        var target = new IntegerMutator();

        var parent = SyntaxFactory.ParseMemberDeclaration(expression);
        var child = parent?.DescendantNodes(_ => true).OfType<LiteralExpressionSyntax>().FirstOrDefault();

        var result = target.ApplyMutations(child, null).ToList();

        result.Count.ShouldBe(expectedMutations);
    }

    [Fact]
    public void ShouldBeMutationLevelAdvanced()
    {
        var target = new IntegerMutator();
        target.MutationLevel.ShouldBe(MutationLevel.Advanced);
    }
}
