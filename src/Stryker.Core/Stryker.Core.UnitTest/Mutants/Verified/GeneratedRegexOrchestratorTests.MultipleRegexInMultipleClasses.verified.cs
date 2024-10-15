using System.Text.RegularExpressions;
namespace StrykerNet.UnitTest.Mutants.TestResources;
public partial class R1 {
    private static Regex Abc1GeneratedRegex() => (StrykerNamespace.MutantControl.IsActive(1)?Abc1GeneratedRegex_RegexAnchorRemovalMutation_XvqOBYS3t1u4AI1iIc6VTTMEhssC():(StrykerNamespace.MutantControl.IsActive(0)?Abc1GeneratedRegex_RegexAnchorRemovalMutation_KqIxB18xYu8ALD4NBR827tks3jYC():Abc1GeneratedRegex_Original()));
    [GeneratedRegex(@"^abc$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abc1GeneratedRegex_Original();
    [GeneratedRegex("^abc", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abc1GeneratedRegex_RegexAnchorRemovalMutation_KqIxB18xYu8ALD4NBR827tks3jYC();
    [GeneratedRegex("abc$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abc1GeneratedRegex_RegexAnchorRemovalMutation_XvqOBYS3t1u4AI1iIc6VTTMEhssC();
    
    private static Regex Abcd1GeneratedRegex() => (StrykerNamespace.MutantControl.IsActive(5)?Abcd1GeneratedRegex_RegexQuantifierRemovalMutation_uJA6aLkSDZf1nNEqTTrmUwkVMpUC():(StrykerNamespace.MutantControl.IsActive(4)?Abcd1GeneratedRegex_RegexCharacterClassShorthandNegationMutation_MlYZgMyXCSRTuWJOS9wBgXbB5tcC():(StrykerNamespace.MutantControl.IsActive(3)?Abcd1GeneratedRegex_RegexCharacterClassNegationMutation_sUNdnI9ukFndVf8l6TkBoumJJRQC():(StrykerNamespace.MutantControl.IsActive(2)?Abcd1GeneratedRegex_RegexAnchorRemovalMutation_SdtdlVHq9QAJyltvbdr0AfzHrW4C():Abcd1GeneratedRegex_Original()))));
    
    [GeneratedRegex(@"^[abc]\d?", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abcd1GeneratedRegex_Original();
    
    [GeneratedRegex("[abc]\\d?", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abcd1GeneratedRegex_RegexAnchorRemovalMutation_SdtdlVHq9QAJyltvbdr0AfzHrW4C();
    
    [GeneratedRegex("^[^abc]\\d?", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abcd1GeneratedRegex_RegexCharacterClassNegationMutation_sUNdnI9ukFndVf8l6TkBoumJJRQC();
    
    [GeneratedRegex("^[abc]\\D?", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abcd1GeneratedRegex_RegexCharacterClassShorthandNegationMutation_MlYZgMyXCSRTuWJOS9wBgXbB5tcC();
    
    [GeneratedRegex("^[abc]\\d", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abcd1GeneratedRegex_RegexQuantifierRemovalMutation_uJA6aLkSDZf1nNEqTTrmUwkVMpUC();
}
public partial class R2 {
    private static Regex Abc2GeneratedRegex() => (StrykerNamespace.MutantControl.IsActive(8)?Abc2GeneratedRegex_RegexAnchorRemovalMutation_zGamrKwW6ANBC778udllQZ1MR7QC():(StrykerNamespace.MutantControl.IsActive(7)?Abc2GeneratedRegex_RegexAnchorRemovalMutation_SWsP0ttxcvtw60j1d0lOMxgnqR8C():(StrykerNamespace.MutantControl.IsActive(6)?Abc2GeneratedRegex_RegexAnchorRemovalMutation_oUokMRf9irbrUFWAuW1GF0UYUmEC():Abc2GeneratedRegex_Original())));
    [GeneratedRegex(@"^abc\b$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abc2GeneratedRegex_Original();
    [GeneratedRegex("abc\\b$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abc2GeneratedRegex_RegexAnchorRemovalMutation_oUokMRf9irbrUFWAuW1GF0UYUmEC();
    [GeneratedRegex("^abc\\b", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abc2GeneratedRegex_RegexAnchorRemovalMutation_SWsP0ttxcvtw60j1d0lOMxgnqR8C();
    [GeneratedRegex("^abc$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abc2GeneratedRegex_RegexAnchorRemovalMutation_zGamrKwW6ANBC778udllQZ1MR7QC();
    
    private static Regex Abcd2GeneratedRegex() => (StrykerNamespace.MutantControl.IsActive(13)?Abcd2GeneratedRegex_RegexQuantifierRemovalMutation_KBl0fhby530iRtCWz8NjzdsGEVkC():(StrykerNamespace.MutantControl.IsActive(12)?Abcd2GeneratedRegex_RegexCharacterClassShorthandNegationMutation_JT1uBo8WX3bw1WvAlBzY08lwAdAC():(StrykerNamespace.MutantControl.IsActive(11)?Abcd2GeneratedRegex_RegexCharacterClassShorthandNegationMutation_cy4mCbUGK1XhyZGp20URzeZ16WwC():(StrykerNamespace.MutantControl.IsActive(10)?Abcd2GeneratedRegex_RegexCharacterClassNegationMutation_R1njA5T5mqbQX4QdgTtA84aAUf4C():(StrykerNamespace.MutantControl.IsActive(9)?Abcd2GeneratedRegex_RegexAnchorRemovalMutation_nrttgUdiLHGmkMufZczT1IN3JbcC():Abcd2GeneratedRegex_Original())))));
    
    [GeneratedRegex(@"^\d[abc]\d?", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abcd2GeneratedRegex_Original();
    
    [GeneratedRegex("\\d[abc]\\d?", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abcd2GeneratedRegex_RegexAnchorRemovalMutation_nrttgUdiLHGmkMufZczT1IN3JbcC();
    
    [GeneratedRegex("^\\d[^abc]\\d?", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abcd2GeneratedRegex_RegexCharacterClassNegationMutation_R1njA5T5mqbQX4QdgTtA84aAUf4C();
    
    [GeneratedRegex("^\\D[abc]\\d?", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abcd2GeneratedRegex_RegexCharacterClassShorthandNegationMutation_cy4mCbUGK1XhyZGp20URzeZ16WwC();
    
    [GeneratedRegex("^\\d[abc]\\D?", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abcd2GeneratedRegex_RegexCharacterClassShorthandNegationMutation_JT1uBo8WX3bw1WvAlBzY08lwAdAC();
    
    [GeneratedRegex("^\\d[abc]\\d", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Abcd2GeneratedRegex_RegexQuantifierRemovalMutation_KBl0fhby530iRtCWz8NjzdsGEVkC();
}