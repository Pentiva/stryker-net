using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using Stryker.Abstractions.Mutators;
using Stryker.Core.Mutators;

namespace Stryker.Core.UnitTest.Mutators;

[TestClass]
public sealed class IntegerMutatorTests : TestBase
{
    [TestMethod]
    [DataRow(SyntaxKind.StringLiteralExpression)]
    [DataRow(SyntaxKind.CharacterLiteralExpression)]
    [DataRow(SyntaxKind.NullLiteralExpression)]
    [DataRow(SyntaxKind.DefaultLiteralExpression)]
    [DataRow(SyntaxKind.TrueLiteralExpression)]
    [DataRow(SyntaxKind.FalseLiteralExpression)]
    public void ShouldNotMutate(SyntaxKind original)
    {
        var target = new IntegerMutator();

        var result = target.ApplyMutations(SyntaxFactory.LiteralExpression(original), null);

        result.ShouldBeEmpty();
    }

    [TestMethod]
    [DataRow("array[1]")]
    [DataRow("array[1..2]")]
    [DataRow("array[1..^1]")]
    [DataRow("(variable & Flag) == 0")]
    [DataRow("0 == (variable & Flag)")]
    public void ShouldNotMutate2(string expression)
    {
        var target = new IntegerMutator();

        var parent = SyntaxFactory.ParseExpression(expression);
        var child = parent.DescendantNodes(_ => true).OfType<LiteralExpressionSyntax>().FirstOrDefault();

        var result = target.ApplyMutations(child, null).ToList();

        result.ShouldBeEmpty();
    }

    [TestMethod]
    [DataRow("private const int Constant = 55;")]
    [DataRow("""
                [Flags]
                public enum FlagEnum {
                    None = 0,
                    A = 1 << 0,
                    B = 1 << 1,
                    C = 1 << 2,
                    D = 1 << 3
                }
                """)]
    [DataRow("""
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

        var result = target.ApplyMutations(child, null);

        result.ShouldBeEmpty();
    }
    
    [TestMethod]
    public void ShouldNotMutateZeroWithImplicitEnumConversion()
    {
        var target = new IntegerMutator();

        var parent = SyntaxFactory.ParseSyntaxTree("""
                                                   public class Abc {
                                                     public void Method(AEnum e) { }
                                                     public void Method() {
                                                         Method(0);
                                                     }
                                                   }
                                                   public enum AEnum {
                                                     Member1,
                                                     Member2
                                                   }
                                                   """);
        var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        Compilation compilation = CSharpCompilation.Create("MyCompilation",
        [parent], [mscorlib], new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var child = parent?.GetRoot().DescendantNodes(_ => true).OfType<LiteralExpressionSyntax>().FirstOrDefault();
        
        var result = target.ApplyMutations(child, compilation.GetSemanticModel(parent));

        result.ShouldBeEmpty();
    }

    [TestMethod]
    [DataRow(10, new object[] { 0, -10 })]
    [DataRow(-10, new object[] { 0, 10 })]
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

    [TestMethod]
    [DataRow("(variable + Flag) != 0", 1)]
    [DataRow("2 + 5", 2)]
    [DataRow("variable - 5", 2)]
    [DataRow("Method(3)", 2)]
    [DataRow("Method(0)", 1)]
    public void ShouldMutate2(string expression, int expectedMutations)
    {
        var target = new IntegerMutator();

        var parent = SyntaxFactory.ParseExpression(expression);
        var child = parent.DescendantNodes(_ => true).OfType<LiteralExpressionSyntax>().FirstOrDefault();

        var result = target.ApplyMutations(child, null).ToList();

        result.Count.ShouldBe(expectedMutations);
    }

    [TestMethod]
    [DataRow("public int Abc = 23;", 2)]
    [DataRow("public int Abc { get; } = 23;", 2)]
    [DataRow("private int Abc { get; } = 0;", 1)]
    [DataRow("""
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

    [TestMethod]
    public void ShouldBeMutationLevelAdvanced()
    {
        var target = new IntegerMutator();
        target.MutationLevel.ShouldBe(MutationLevel.Advanced);
    }
}
