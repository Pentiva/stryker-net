using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Stryker.Core.Mutators;

namespace Stryker.Core.Options.Inputs
{
    public partial class IgnoreMethodsInput : Input<IEnumerable<string>>
    {
        public override IEnumerable<string> Default => [];

        protected override string Description => "Ignore mutations on method parameters.";

        public IEnumerable<IgnoredMethod> Validate() => SuppliedInput is not null ? ParseInput(SuppliedInput) : ParseInput(Default);

        private static List<IgnoredMethod> ParseInput(IEnumerable<string> methodPatterns) =>
            methodPatterns
                .Where(static x => !string.IsNullOrEmpty(x))
                .Select(ParseIgnoredMethod)
                .ToList();

        private static IgnoredMethod ParseIgnoredMethod(string input)
        {
            var match = InputParser().Match(input);
            if (match.Groups["Mutants"] is not { Success: true } mutants)
            {
                return new IgnoredMethod(ParseRegex(match.Groups["MethodPattern"].Value), FrozenSet<Mutator>.Empty);
            }

            Span<Range> d = stackalloc Range[24];
            var count = mutants.ValueSpan.Split(d, [','], StringSplitOptions.TrimEntries);
            var set = new HashSet<Mutator>();
            for (var i = 0; i < count; i++)
            {
                if (Enum.TryParse(mutants.ValueSpan[d[i]], true, out Mutator m))
                {
                    set.Add(m);
                }
            }

            return new IgnoredMethod(ParseRegex(match.Groups["MethodPattern"].Value), set.ToFrozenSet());
        }

        private static Regex ParseRegex(string methodPattern) =>
            new($"^(?:[^.]*\\.)*{Regex.Escape(methodPattern).Replace("\\*", "[^.]*")}(<[^>]*>)?$", RegexOptions.IgnoreCase);

        [GeneratedRegex("^(?<MethodPattern>.+?)(?::(?<Mutants>.+))?$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
        private static partial Regex InputParser();
    }

    public class IgnoredMethod(Regex regex, FrozenSet<Mutator> mutator = null)
    {
        public bool IsMatch(string input, Mutator type) => regex.IsMatch(input) && (mutator is null || mutator.Count == 0 || mutator.Contains(type));

        public override string ToString() => regex.ToString();
    }
}
