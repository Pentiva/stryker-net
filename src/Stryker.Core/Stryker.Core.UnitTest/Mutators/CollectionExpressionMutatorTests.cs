using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Stryker.Core.Mutators;
using Xunit;

namespace Stryker.Core.UnitTest.Mutators;

public class CollectionExpressionMutatorTests : TestBase
{
    [Fact]
    public void ShouldBeMutationLevelAdvanced()
    {
        var target = new CollectionExpressionMutator();
        target.MutationLevel.ShouldBe(MutationLevel.Advanced);
    }
    
    [Theory]
    [InlineData("[]")]
    [InlineData("[ ]")]
    [InlineData("[           ]")]
    [InlineData("[ /* Comment */ ]")]
    public void ShouldAddValueToEmptyCollectionExpression(string expression)
    {
        var expressionSyntax = SyntaxFactory.ParseExpression(expression) as CollectionExpressionSyntax;

        var target = new CollectionExpressionMutator();

        var result = target.ApplyMutations(expressionSyntax, null);

        result.Count().ShouldBe(2);
        foreach (var mutation in result)
        {
            mutation.DisplayName.ShouldBe("Collection expression mutation");

            var replacement = mutation.ReplacementNode.ShouldBeOfType<CollectionExpressionSyntax>();
            replacement.Elements.ShouldNotBeEmpty();
        }
    }

    [Theory]
    [InlineData("[1, 2, 3]")]
    [InlineData("[-1, 3]")]
    [InlineData("[1, .. abc, 3]")]
    [InlineData("[..abc]")]
    public void ShouldRemoveValuesFromCollectionExpression(string expression)
    {
        var expressionSyntax = SyntaxFactory.ParseExpression(expression) as CollectionExpressionSyntax;

        var target = new CollectionExpressionMutator();

        var result = target.ApplyMutations(expressionSyntax, null);

        var mutation = result.ShouldHaveSingleItem();
        mutation.DisplayName.ShouldBe("Collection expression mutation");

        var replacement = mutation.ReplacementNode.ShouldBeOfType<CollectionExpressionSyntax>();
        replacement.Elements.ShouldBeEmpty();
    }
}
