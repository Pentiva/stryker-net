// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
// From: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/9859cc0cac7269a51afe8800458d9bf40bfdb7e4/StyleCop.Analyzers/StyleCop.Analyzers/GeneratedCodeAnalysisExtensions.cs

using System;

namespace Stryker.Core.MutantFilters
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text.RegularExpressions;

    [ExcludeFromCodeCoverage]
    public static class GeneratedCodeFilterExtension
    {
        /// <summary>
        /// Checks whether the given node or its containing document is auto generated by a tool.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="tree">The analysis context for a <see cref="SyntaxTree"/>.</param>
        /// <returns>
        /// <para><see langword="true"/> if the <see cref="SyntaxNode"/> contained in <paramref name="context"/> is
        /// located in generated code; otherwise, <see langword="false"/>.</para>
        /// </returns>
        public static bool IsGenerated(this SyntaxTree tree, IFileSystem fileSystem = default) =>
            IsGeneratedFileName(tree.FilePath, fileSystem ?? new FileSystem())
            || HasAutoGeneratedComment(tree);

        /// <summary>
        /// Checks whether the given document has an auto-generated comment as its header.
        /// </summary>
        /// <param name="tree">The syntax tree to examine.</param>
        /// <returns>
        /// <para><see langword="true"/> if <paramref name="tree"/> starts with a comment containing the text
        /// <c>&lt;auto-generated</c>; otherwise, <see langword="false"/>.</para>
        /// </returns>
        private static bool HasAutoGeneratedComment(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            var firstToken = root.GetFirstToken();
            SyntaxTriviaList trivia;
            if (firstToken == default)
            {
                var token = ((CompilationUnitSyntax)root).EndOfFileToken;
                if (!token.HasLeadingTrivia)
                {
                    return false;
                }

                trivia = token.LeadingTrivia;
            }
            else
            {
                if (!firstToken.HasLeadingTrivia)
                {
                    return false;
                }

                trivia = firstToken.LeadingTrivia;
            }

            var comments = trivia.Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) || t.IsKind(SyntaxKind.MultiLineCommentTrivia));
            return comments.Any(t =>
            {
                var s = t.ToString();
                return s.Contains("<auto-generated") || s.Contains("<autogenerated");
            });
        }

        /// <summary>
        /// Checks whether the given document has a filename that indicates it is a generated file.
        /// </summary>
        /// <param name="filePath">The source file name, without any path.</param>
        /// <returns>
        /// <para><see langword="true"/> if <paramref name="filePath"/> is the name of a generated file; otherwise,
        /// <see langword="false"/>.</para>
        /// </returns>
        /// <seealso cref="IsGenerated(SyntaxTree)"/>
        private static bool IsGeneratedFileName(string filePath, IFileSystem fileSystem) =>
            Regex.IsMatch(
                fileSystem.Path.GetFileName(filePath),
                @"\.designer\.cs$",
                RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));
    }
}
