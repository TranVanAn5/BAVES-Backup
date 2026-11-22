namespace BAVES.Api.Modules.Scenario.Models.Enums;

public enum KnowledgeCategory
{
    BusinessFact = 0,
    Process = 1,
    Data = 2,
    Policy = 3,
    System = 4,
    Constraint = 5,
    Stakeholder = 6,
    /// <summary>Seed/legacy value used in older scenario packs.</summary>
    BusinessRule = 7,
    /// <summary>Seed/legacy value used in older scenario packs.</summary>
    Security = 8
}
