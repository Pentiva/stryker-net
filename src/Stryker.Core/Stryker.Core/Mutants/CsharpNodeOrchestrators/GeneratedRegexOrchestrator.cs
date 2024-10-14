using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Stryker.Abstractions.Logging;
using Stryker.Core.Mutators;
using Stryker.RegexMutators;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Stryker.Core.Mutants.CsharpNodeOrchestrators;

internal class GeneratedRegexOrchestrator : MemberDefinitionOrchestrator<TypeDeclarationSyntax>
{
    private ILogger Logger { get; } = ApplicationLogging.LoggerFactory.CreateLogger<GeneratedRegexOrchestrator>();

    /// <inheritdoc />
    protected override bool CanHandle(TypeDeclarationSyntax t) =>
        t is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax &&
        t.Modifiers.Any(static a => a.IsKind(SyntaxKind.PartialKeyword));

    /// <inheritdoc />
    public override SyntaxNode Mutate(SyntaxNode node, SemanticModel semanticModel, MutationContext context)
    {
        var toProcess = (node switch
                            {
                                ClassDeclarationSyntax cds => cds.Members,
                                StructDeclarationSyntax sds => sds.Members,
                                _ => []
                            })
                           .Select(a=>(oldNode: a, newNodes: GenerateNewMethods(a, semanticModel)))
                           .Where(a=>a.newNodes.Count > 0)
                           .ToImmutableArray();

        var newNode = node.TrackNodes(toProcess.Select(static a => a.oldNode));

        foreach (var valueTuple in toProcess)
        {
            newNode = newNode.ReplaceNode(newNode.GetCurrentNode(valueTuple.oldNode)!, valueTuple.newNodes);
        }

        // Obtain new SemanticModel for modified syntax tree
        var correctSemanticModel = semanticModel;
        if (semanticModel is not null)
        {
            var tempAnnotation = new SyntaxAnnotation(Guid.NewGuid().ToString("N"));
            newNode = newNode.WithAdditionalAnnotations(tempAnnotation);
            var newSyntaxTree = node.SyntaxTree.GetRoot().ReplaceNode(node, newNode).SyntaxTree;
            correctSemanticModel = semanticModel.Compilation.AddSyntaxTrees(newSyntaxTree).GetSemanticModel(newSyntaxTree);
            newNode = newSyntaxTree.GetRoot().GetAnnotatedNodes(tempAnnotation).First();
        }

        return base.Mutate(newNode, correctSemanticModel, context);
    }

    private IReadOnlyCollection<SyntaxNode> GenerateNewMethods(MemberDeclarationSyntax method, SemanticModel semanticModel)
    {
        var mpds = method switch
        {
            MethodDeclarationSyntax mds => new MethodOrPropertyDeclarationSyntax(mds),
            PropertyDeclarationSyntax pds => new MethodOrPropertyDeclarationSyntax(pds),
            _ => null
        };

        var regexAttribute = mpds?.GetRegexAttribute();

        if (regexAttribute is null)
        {
            return [];
        }

        var regexMutations = GenerateNewMethods(mpds, regexAttribute, semanticModel).ToArray();

        if (regexMutations.Length == 0)
        {
            return [];
        }

        var (proxyMethod, renamedMethod) =
            MutatePartialRegexMethod(mpds, regexMutations.Select(static a => a.annotation));

        return
        [
            proxyMethod,
            renamedMethod,
            ..regexMutations.OrderBy(a=>a.name).Select(static a => a.newMethod)
        ];
    }

    private IEnumerable<(string name, SyntaxAnnotation annotation, MethodOrPropertyDeclarationSyntax newMethod)> GenerateNewMethods(
        MethodOrPropertyDeclarationSyntax method, AttributeSyntax regexAttribute, SemanticModel model)
    {
        var arguments = regexAttribute.ArgumentList?.Arguments;

        var namedArgument = arguments?.FirstOrDefault(static argument =>
                                                          argument.NameColon?.Name.Identifier.ValueText ==
                                                          "pattern");
        var patternArgument = namedArgument ?? arguments?.FirstOrDefault();
        var patternExpression = patternArgument?.Expression;

        string currentValue = null;

        if (patternExpression.IsKind(SyntaxKind.IdentifierName) && model is not null)
        {
            var constantValue = model.GetConstantValue(patternExpression);

            if (constantValue.HasValue)
            {
                currentValue = constantValue.Value as string;
            }
            else
            {
                currentValue = (model.GetSymbolInfo(patternExpression).Symbol as IFieldSymbol)?.OriginalDefinition.ConstantValue as string;
            }
        }

        if (patternExpression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
        {
            currentValue = model?.GetConstantValue(patternExpression).Value as string;
        }

        if (patternExpression.IsKind(SyntaxKind.StringLiteralExpression))
        {
            currentValue = ((LiteralExpressionSyntax)patternExpression).Token.ValueText;
        }

        if (currentValue is null)
        {
            yield break;
        }
        
        var patternExpressionLineSpan = patternExpression.GetLocation().GetLineSpan();
        var regexMutantOrchestrator = new RegexMutantOrchestrator(currentValue);
        var replacementValues = regexMutantOrchestrator.Mutate();

        foreach (var regexMutation in replacementValues)
        {
            try
            {
                _ = new Regex(regexMutation.ReplacementPattern);
            }
            catch (ArgumentException exception)
            {
                Logger.LogDebug(
                                "RegexMutator created mutation {CurrentValue} -> {ReplacementPattern} which is an invalid regular expression:\n{Message}",
                                currentValue, regexMutation.ReplacementPattern, exception.Message);
                continue;
            }

            var hashData = SHA1.HashData(Encoding.UTF8.GetBytes(regexMutation.ReplacementPattern));
            var hash = Convert.ToBase64String(hashData).Replace('+', 'A').Replace('/', 'B').Replace('=', 'C');
            var newName = $"{method.Identifier.ValueText}_{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(regexMutation.DisplayName).Replace(" ", "")}_{hash}";

            yield return
                (newName, 
                 new SyntaxAnnotation(GeneratedRegexMutator.MarkerAnnotationKind, JsonSerializer.Serialize(new GeneratedRegexMutator.GeneratedRegexMutationInfo(DisplayName: regexMutation.DisplayName, OriginalLocation: patternExpressionLineSpan, ReplacementText: $"\"{regexMutation.ReplacementPattern}\"", Description: regexMutation.Description, NewName: newName), GeneratedRegexMutatorContext.Default.GeneratedRegexMutationInfo)),
                 IfDirectiveRemover.Instance.Visit(method.ReplaceNode(patternExpression,
                                                       LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                                         Literal(regexMutation.ReplacementPattern)))
                                       .WithIdentifier(Identifier(newName))));
        }
    }

    /// <summary>
    ///     Convert partial regex marker method into a real method and a new regex marker method with a new name
    /// </summary>
    /// <param name="originalMethod"></param>
    /// <param name="annotations"></param>
    /// <returns></returns>
    private static (MethodOrPropertyDeclarationSyntax proxyMethod, MethodOrPropertyDeclarationSyntax renamedMethod)
        MutatePartialRegexMethod(MethodOrPropertyDeclarationSyntax originalMethod, IEnumerable<SyntaxAnnotation> annotations)
    {
        var newName = Identifier($"{originalMethod.Identifier.ValueText}_Original");

        var regexAttribute = originalMethod.GetRegexAttribute();

        var proxyMethod = originalMethod
                      .RemoveNode(regexAttribute, SyntaxRemoveOptions.KeepNoTrivia)!
                      .WithExpressionBody(ArrowExpressionClause(Token(TriviaList(Space), SyntaxKind.EqualsGreaterThanToken, TriviaList(Space)), originalMethod.CreateCall(newName).WithAdditionalAnnotations(annotations)))
                      .WithSemicolonToken(Token(SyntaxTriviaList.Empty, SyntaxKind.SemicolonToken, ";", ";", TriviaList(LineFeed)))
                      .WithModifiers(originalMethod.Modifiers.Remove(originalMethod.Modifiers.First(static a =>
                                                                                                        a.IsKind(SyntaxKind.PartialKeyword))));

        proxyMethod = proxyMethod.RemoveNodes(proxyMethod.AttributeLists.Where(static a => a.Attributes.Count == 0),
                                              SyntaxRemoveOptions.KeepNoTrivia)!.WithTriviaFrom(regexAttribute.Parent);
        var newMethod = IfDirectiveRemover.Instance.Visit(originalMethod.WithIdentifier(newName));

        return (proxyMethod, newMethod);
    }
}

sealed file class IfDirectiveRemover() : CSharpSyntaxRewriter(visitIntoStructuredTrivia: true)
{
    public static IfDirectiveRemover Instance { get; } = new();

    /// <inheritdoc />
    public override SyntaxNode VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node) => null;

    /// <inheritdoc />
    public override SyntaxNode VisitElseDirectiveTrivia(ElseDirectiveTriviaSyntax node) => null;

    /// <inheritdoc />
    public override SyntaxNode VisitElifDirectiveTrivia(ElifDirectiveTriviaSyntax node) => null;

    public MethodOrPropertyDeclarationSyntax Visit(MethodOrPropertyDeclarationSyntax mpds) =>
        base.Visit(mpds) switch
        {
            MethodDeclarationSyntax mds => mds,
            PropertyDeclarationSyntax pds => pds,
            _ => mpds
        };
}

internal sealed class MethodOrPropertyDeclarationSyntax
{
    private readonly MemberDeclarationSyntax _memberDeclaration;

    public MethodOrPropertyDeclarationSyntax(MethodDeclarationSyntax memberDeclaration) =>
        _memberDeclaration = memberDeclaration;

    public MethodOrPropertyDeclarationSyntax(PropertyDeclarationSyntax memberDeclaration) =>
        _memberDeclaration = memberDeclaration;

    public SyntaxToken Identifier => _memberDeclaration switch
    {
        MethodDeclarationSyntax mds => mds.Identifier,
        PropertyDeclarationSyntax pds => pds.Identifier,
        _ => throw new UnreachableException()
    };

    public SyntaxTokenList Modifiers => _memberDeclaration switch
    {
        MethodDeclarationSyntax mds => mds.Modifiers,
        PropertyDeclarationSyntax pds => pds.Modifiers,
        _ => throw new UnreachableException()
    };

    public SyntaxList<AttributeListSyntax> AttributeLists => _memberDeclaration switch
    {
        MethodDeclarationSyntax mds => mds.AttributeLists,
        PropertyDeclarationSyntax pds => pds.AttributeLists,
        _ => throw new UnreachableException()
    };

    public MethodOrPropertyDeclarationSyntax WithIdentifier(SyntaxToken identifier) => _memberDeclaration switch
    {
        MethodDeclarationSyntax mds => mds.WithIdentifier(identifier),
        PropertyDeclarationSyntax pds => pds.WithIdentifier(identifier),
        _ => throw new UnreachableException()
    };

    public MethodOrPropertyDeclarationSyntax ReplaceNode(SyntaxNode oldNode, SyntaxNode newNode) =>
        _memberDeclaration switch
        {
            MethodDeclarationSyntax mds => mds.ReplaceNode(oldNode, newNode),
            PropertyDeclarationSyntax pds => pds.ReplaceNode(oldNode, newNode),
            _ => throw new UnreachableException()
        };

    public MethodOrPropertyDeclarationSyntax RemoveNodes(IEnumerable<SyntaxNode> nodes, SyntaxRemoveOptions options) =>
        _memberDeclaration switch
        {
            MethodDeclarationSyntax mds => mds.RemoveNodes(nodes, options),
            PropertyDeclarationSyntax pds => pds.RemoveNodes(nodes, options),
            _ => throw new UnreachableException()
        };

    public MethodOrPropertyDeclarationSyntax RemoveNode(SyntaxNode node, SyntaxRemoveOptions options) =>
        _memberDeclaration switch
        {
            MethodDeclarationSyntax mds => mds.RemoveNode(node, options),
            PropertyDeclarationSyntax pds => pds.RemoveNode(node, options),
            _ => throw new UnreachableException()
        };

    public MethodOrPropertyDeclarationSyntax WithExpressionBody(ArrowExpressionClauseSyntax arrowExpressionClauseSyntax) => _memberDeclaration switch
    {
        MethodDeclarationSyntax mds => mds.WithExpressionBody(arrowExpressionClauseSyntax),
        PropertyDeclarationSyntax pds => pds.WithExpressionBody(arrowExpressionClauseSyntax).WithAccessorList(default),
        _ => throw new UnreachableException()
    };

    public MethodOrPropertyDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken) => _memberDeclaration switch
    {
        MethodDeclarationSyntax mds => mds.WithSemicolonToken(semicolonToken),
        PropertyDeclarationSyntax pds => pds.WithSemicolonToken(semicolonToken),
        _ => throw new UnreachableException()
    };

    public MethodOrPropertyDeclarationSyntax WithModifiers(SyntaxTokenList modifiers) => _memberDeclaration switch
    {
        MethodDeclarationSyntax mds => mds.WithModifiers(modifiers),
        PropertyDeclarationSyntax pds => pds.WithModifiers(modifiers),
        _ => throw new UnreachableException()
    };

    public MethodOrPropertyDeclarationSyntax WithTriviaFrom(SyntaxNode node) =>
        _memberDeclaration switch
        {
            MethodDeclarationSyntax mds => mds.WithTriviaFrom(node),
            PropertyDeclarationSyntax pds => pds.WithTriviaFrom(node),
            _ => throw new UnreachableException()
        };

    public ExpressionSyntax CreateCall(SyntaxToken newName) =>
        _memberDeclaration switch
        {
            MethodDeclarationSyntax => InvocationExpression(IdentifierName(newName), ArgumentList()),
            PropertyDeclarationSyntax => IdentifierName(newName),
            _ => throw new UnreachableException()
        };

    public AttributeSyntax GetRegexAttribute()
    {
        if (!Modifiers.Any(static a => a.IsKind(SyntaxKind.PartialKeyword)))
        {
            return null;
        }

        return AttributeLists.SelectMany(static a => a.Attributes)
                             .FirstOrDefault(static a => a.Name is IdentifierNameSyntax
                              {
                                  Identifier.Value: "GeneratedRegex" or "GeneratedRegexAttribute"
                              });
    }

    public static implicit operator MethodOrPropertyDeclarationSyntax(MethodDeclarationSyntax mds) => new(mds);

    public static implicit operator MethodOrPropertyDeclarationSyntax(PropertyDeclarationSyntax mds) => new(mds);

    public static implicit operator MemberDeclarationSyntax(MethodOrPropertyDeclarationSyntax mpds) =>
        mpds._memberDeclaration;
}
