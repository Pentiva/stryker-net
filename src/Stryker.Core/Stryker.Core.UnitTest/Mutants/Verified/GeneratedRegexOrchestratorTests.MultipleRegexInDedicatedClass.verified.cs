using System.Text.RegularExpressions;
namespace StrykerNet.UnitTest.Mutants.TestResources;
public partial class R {
    private static Regex AbcGeneratedRegex() => (StrykerNamespace.MutantControl.IsActive(1)?AbcGeneratedRegex_RegexAnchorRemovalMutation_KqIxB18xYu8ALD4NBR827tks3jYC():(StrykerNamespace.MutantControl.IsActive(0)?AbcGeneratedRegex_RegexAnchorRemovalMutation_XvqOBYS3t1u4AI1iIc6VTTMEhssC():AbcGeneratedRegex_Original()));
    [GeneratedRegex(@"^abc$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex AbcGeneratedRegex_Original();
    [GeneratedRegex("^abc", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex AbcGeneratedRegex_RegexAnchorRemovalMutation_KqIxB18xYu8ALD4NBR827tks3jYC();
    [GeneratedRegex("abc$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex AbcGeneratedRegex_RegexAnchorRemovalMutation_XvqOBYS3t1u4AI1iIc6VTTMEhssC();
    
    private static Regex AbcdGeneratedRegex() => (StrykerNamespace.MutantControl.IsActive(5)?AbcdGeneratedRegex_RegexQuantifierRemovalMutation_uJA6aLkSDZf1nNEqTTrmUwkVMpUC():(StrykerNamespace.MutantControl.IsActive(4)?AbcdGeneratedRegex_RegexCharacterClassShorthandNegationMutation_MlYZgMyXCSRTuWJOS9wBgXbB5tcC():(StrykerNamespace.MutantControl.IsActive(3)?AbcdGeneratedRegex_RegexCharacterClassNegationMutation_sUNdnI9ukFndVf8l6TkBoumJJRQC():(StrykerNamespace.MutantControl.IsActive(2)?AbcdGeneratedRegex_RegexAnchorRemovalMutation_SdtdlVHq9QAJyltvbdr0AfzHrW4C():AbcdGeneratedRegex_Original()))));
    
    [GeneratedRegex(@"^[abc]\d?", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex AbcdGeneratedRegex_Original();
    
    [GeneratedRegex("[abc]\\d?", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex AbcdGeneratedRegex_RegexAnchorRemovalMutation_SdtdlVHq9QAJyltvbdr0AfzHrW4C();
    
    [GeneratedRegex("^[^abc]\\d?", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex AbcdGeneratedRegex_RegexCharacterClassNegationMutation_sUNdnI9ukFndVf8l6TkBoumJJRQC();
    
    [GeneratedRegex("^[abc]\\D?", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex AbcdGeneratedRegex_RegexCharacterClassShorthandNegationMutation_MlYZgMyXCSRTuWJOS9wBgXbB5tcC();
    
    [GeneratedRegex("^[abc]\\d", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex AbcdGeneratedRegex_RegexQuantifierRemovalMutation_uJA6aLkSDZf1nNEqTTrmUwkVMpUC();
}