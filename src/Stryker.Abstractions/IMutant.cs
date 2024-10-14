using Microsoft.CodeAnalysis;
using Stryker.Abstractions.TestRunners;

namespace Stryker.Abstractions.Mutants;

public interface IMutant : IReadOnlyMutant
{
    new int Id { get; set; }
    new Mutation Mutation { get; set; }
    new MutantStatus ResultStatus { get; set; }
    new string ResultStatusReason { get; set; }
    new ITestGuids CoveringTests { get; set; }
    new ITestGuids KillingTests { get; set; }
    new ITestGuids AssessingTests { get; set; }
    new bool CountForStats { get; }
    new bool IsStaticValue { get; set; }

    string DisplayName { get; }
    bool MustBeTestedInIsolation { get; set; }

    new string ReplacementText => Mutation.ReplacementNode.ToString();
    new FileLinePositionSpan OriginalLocation => Mutation.OriginalNode.GetLocation().GetMappedLineSpan();

    void AnalyzeTestRun(ITestGuids failedTests, ITestGuids resultRanTests, ITestGuids timedOutTests, bool sessionTimedOut);
}
