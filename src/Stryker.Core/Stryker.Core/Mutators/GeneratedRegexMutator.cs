using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Stryker.Abstractions.Mutants;
using Stryker.Abstractions.Mutators;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Stryker.Core.Mutators;

public class GeneratedRegexMutator : MutatorBase<ExpressionSyntax>
{
    public static string MarkerAnnotationKind => nameof(GeneratedRegexMutator);

    public override MutationLevel MutationLevel => MutationLevel.Complete;

    public override IEnumerable<Mutation> ApplyMutations(ExpressionSyntax node,
                                                         SemanticModel    semanticModel)
    {
        if (node is not (InvocationExpressionSyntax or IdentifierNameSyntax))
        {
            yield break;
        }

        foreach (var annotation in node.GetAnnotations(MarkerAnnotationKind))
        {
            if (annotation.Data is null)
            {
                continue;
            }

            var (newName, displayName, description, replacementText, originalLocation) =
                JsonSerializer.Deserialize(annotation.Data!, GeneratedRegexMutatorContext.Default.GeneratedRegexMutationInfo);

            yield return new GeneratedRegexMutation
            {
                OriginalNode = node,
                ReplacementNode =
                    node is InvocationExpressionSyntax
                        ? InvocationExpression(IdentifierName(newName), ArgumentList())
                        : IdentifierName(newName),
                DisplayName = displayName,
                Type        = Mutator.Regex,
                Description = description,
                ReplacementText = replacementText,
                OriginalLocation = originalLocation
            };
        }
    }

    internal sealed record GeneratedRegexMutationInfo(
        string                                                      NewName,
        string                                                      DisplayName,
        string                                                      Description,
        string                                                      ReplacementText,
        GeneratedRegexMutationInfo.SerializableFileLinePositionSpan OriginalLocation)
    {
        internal sealed record SerializableFileLinePositionSpan(
            string                                                    Path,
            SerializableFileLinePositionSpan.SerializableLinePosition Start,
            SerializableFileLinePositionSpan.SerializableLinePosition End)
        {
            public static implicit operator SerializableFileLinePositionSpan(FileLinePositionSpan a) =>
                new(a.Path, a.StartLinePosition, a.EndLinePosition);

            public static implicit operator FileLinePositionSpan(SerializableFileLinePositionSpan a) =>
                new(a.Path, a.Start, a.End);

            internal sealed record SerializableLinePosition(int Line, int Character)
            {
                public static implicit operator SerializableLinePosition(LinePosition a) =>
                    new(a.Line, a.Character);

                public static implicit operator LinePosition(SerializableLinePosition a) =>
                    new(a.Line, a.Character);
            }
        }
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.General, WriteIndented = false,
                                GenerationMode = JsonSourceGenerationMode.Metadata |
                                                 JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(GeneratedRegexMutator.GeneratedRegexMutationInfo))]
internal sealed partial class GeneratedRegexMutatorContext : JsonSerializerContext;
