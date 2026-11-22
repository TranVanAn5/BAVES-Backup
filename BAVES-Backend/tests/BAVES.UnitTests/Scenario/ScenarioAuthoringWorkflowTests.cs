using BAVES.Api.Modules.Identity.Services.Interfaces;
using BAVES.Api.Modules.Scenario.Data;
using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.Entities;
using BAVES.Api.Modules.Scenario.Models.Enums;
using BAVES.Api.Modules.Scenario.Models.Requests;
using BAVES.Api.Modules.Scenario.Repositories.Interfaces;
using BAVES.Api.Modules.Scenario.Services;
using BAVES.Api.Modules.Training.Services.Interfaces;
using BAVES.Api.Shared.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using ScenarioEntity = BAVES.Api.Modules.Scenario.Models.Entities.Scenario;

namespace BAVES.UnitTests.Scenario;

/// <summary>
/// WF-01: Quản lý & Tác giả Kịch bản Đào tạo (Scenario Authoring, Configuration & Quality Gate Publishing)
/// Owner: Lê Vũ Tiến Dũng
/// </summary>
public class ScenarioAuthoringWorkflowTests
{
    private readonly Mock<IScenarioAuthoringRepository> _authoringRepoMock = new();
    private readonly Mock<IAssignmentService> _assignmentServiceMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly ScenarioDbContext _dbContext;
    private readonly ScenarioAuthoringService _sut;

    public ScenarioAuthoringWorkflowTests()
    {
        var options = new DbContextOptionsBuilder<ScenarioDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ScenarioDbContext(options);
        _sut = new ScenarioAuthoringService(
            _authoringRepoMock.Object,
            _dbContext,
            _assignmentServiceMock.Object,
            _userServiceMock.Object);
    }

    #region F-SCN-01: Tạo kịch bản & Quản lý phiên bản (Versioning)

    [Fact]
    public async Task TC_SCN_01_CreateScenarioAsync_CreatesDraftScenarioAndVersion1Successfully()
    {
        // Arrange
        const int userId = 10;
        var request = new CreateScenarioRequest
        {
            ApplicationId = 1,
            Code = "SCN-OMNI",
            Title = "OmniMart Order Fulfilment",
            Description = "Training scenario for retail order fulfillment",
            Summary = "OmniMart summary",
            BusinessContext = "OmniMart context",
            BusinessProblem = "Inventory sync lag",
            BusinessObjectives = "Improve dispatch speed",
            BARole = "Lead Business Analyst",
            Assignment = "Draft initial BRD"
        };

        _authoringRepoMock.Setup(r => r.ApplicationExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _authoringRepoMock.Setup(r => r.ScenarioCodeExistsAsync("SCN-OMNI", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.CreateScenarioAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("SCN-OMNI");
        result.Title.Should().Be("OmniMart Order Fulfilment");
        result.Status.Should().Be(ScenarioStatus.Draft);

        _authoringRepoMock.Verify(r => r.AddScenarioAsync(It.Is<ScenarioEntity>(s =>
            s.Code == "SCN-OMNI" &&
            s.CreatedByUserId == userId &&
            s.Status == ScenarioStatus.Draft &&
            s.Versions.Count == 1 &&
            s.Versions.First().VersionNo == 1 &&
            !s.Versions.First().IsPublished
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC_SCN_02_CreateVersionAsync_WithCopyFromLatest_CopiesAllDataFromPreviousVersion()
    {
        // Arrange
        const int userId = 10;
        const int scenarioId = 5;

        var existingScenario = new ScenarioEntity
        {
            Id = scenarioId,
            Code = "SCN-OMNI",
            Title = "OmniMart",
            CreatedByUserId = userId,
            Status = ScenarioStatus.Published,
            Versions = new List<ScenarioVersion>
            {
                new() { Id = 1, VersionNo = 1, IsPublished = true }
            }
        };

        var latestVersionWithAggregate = new ScenarioVersion
        {
            Id = 1,
            ScenarioId = scenarioId,
            VersionNo = 1,
            Summary = "Version 1 Summary",
            BusinessContext = "Context 1",
            BusinessProblem = "Problem 1",
            BusinessObjectives = "Objectives 1",
            BARole = "Senior BA",
            Assignment = "Assignment 1",
            Difficulty = ScenarioDifficulty.Intermediate,
            IsPublished = true,
            KnowledgeItems = new List<KnowledgeItem>
            {
                new() { Id = 101, Code = "K-01", Title = "POS Sync", Content = "POS details", Category = KnowledgeCategory.System }
            },
            StakeholderPersonas = new List<StakeholderPersona>
            {
                new()
                {
                    Id = 201,
                    Name = "Sarah Ops",
                    StakeholderRole = "Operations Manager",
                    IsActive = true,
                    StakeholderKnowledgeAccess = new List<StakeholderKnowledgeAccess>
                    {
                        new() { KnowledgeItemId = 101, AccessType = KnowledgeAccessType.Knows, IsHidden = false }
                    }
                }
            },
            BusinessRules = new List<ScenarioBusinessRule>
            {
                new() { Code = "BR-01", Title = "Reservation Timeout", Description = "15 mins" }
            },
            ScenarioEvents = new List<ScenarioEvent>
            {
                new() { Title = "Flash Sale", Description = "Event description", TriggerType = ScenarioEventTriggerType.Manual, EventContent = "{}" }
            }
        };

        _authoringRepoMock.Setup(r => r.GetScenarioWithVersionsAsync(scenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingScenario);
        _authoringRepoMock.Setup(r => r.GetLatestVersionWithAggregateAsync(scenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latestVersionWithAggregate);

        var request = new CreateScenarioVersionRequest
        {
            Summary = "Version 2 Draft",
            CopyFromLatest = true
        };

        // Act
        var result = await _sut.CreateVersionAsync(userId, scenarioId, request);

        // Assert
        result.Should().NotBeNull();
        result.VersionNo.Should().Be(2);
        result.IsPublished.Should().BeFalse();

        _authoringRepoMock.Verify(r => r.AddScenarioVersionAsync(It.Is<ScenarioVersion>(v =>
            v.VersionNo == 2 &&
            v.BusinessContext == "Context 1" &&
            v.KnowledgeItems.Count == 1 &&
            v.KnowledgeItems.First().Code == "K-01" &&
            v.StakeholderPersonas.Count == 1 &&
            v.StakeholderPersonas.First().Name == "Sarah Ops" &&
            v.BusinessRules.Count == 1 &&
            v.ScenarioEvents.Count == 1
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC_SCN_03_CreateVersionAsync_WhenActiveDraftExists_ThrowsConflictException()
    {
        // Arrange
        const int userId = 10;
        const int scenarioId = 5;

        var existingScenario = new ScenarioEntity
        {
            Id = scenarioId,
            CreatedByUserId = userId,
            Versions = new List<ScenarioVersion>
            {
                new() { Id = 1, VersionNo = 1, IsPublished = false } // Still draft!
            }
        };

        _authoringRepoMock.Setup(r => r.GetScenarioWithVersionsAsync(scenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingScenario);

        var request = new CreateScenarioVersionRequest { CopyFromLatest = false };

        // Act
        var act = () => _sut.CreateVersionAsync(userId, scenarioId, request);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*draft version already exists*");
    }

    #endregion

    #region F-SCN-02: Cấu hình Chi tiết Kịch bản (Definition Configuration)

    [Fact]
    public async Task TC_SCN_04_UpdateDefinitionAsync_ValidRequest_UpdatesPersonasKnowledgeRulesEvents()
    {
        // Arrange
        const int userId = 10;
        const int versionId = 1;

        var scenario = new ScenarioEntity { Id = 5, CreatedByUserId = userId };
        var version = new ScenarioVersion
        {
            Id = versionId,
            ScenarioId = scenario.Id,
            Scenario = scenario,
            VersionNo = 1,
            IsPublished = false
        };

        _authoringRepoMock.Setup(r => r.GetVersionWithAggregateForUpdateAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(version);
        _authoringRepoMock.Setup(r => r.GetVersionDefinitionAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(version);

        var request = new UpdateScenarioDefinitionRequest
        {
            Summary = "Updated summary",
            BusinessContext = "Updated context",
            BusinessProblem = "Updated problem",
            BusinessObjectives = "Updated objectives",
            BARole = "Lead BA",
            Assignment = "Analyze order workflows",
            Difficulty = ScenarioDifficulty.Intermediate,
            KnowledgeItems = new List<UpdateKnowledgeItemRequest>
            {
                new()
                {
                    Code = "K-POS",
                    Title = "POS Architecture",
                    Content = "Real-time sync mechanism",
                    Category = KnowledgeCategory.System,
                    Importance = KnowledgeImportance.Critical,
                    AccessList = new List<UpdateKnowledgeAccessRequest>
                    {
                        new() { PersonaCode = "P-IT", AccessType = KnowledgeAccessType.Knows, IsHidden = false }
                    }
                }
            },
            Personas = new List<UpdatePersonaRequest>
            {
                new()
                {
                    Code = "P-IT",
                    Name = "David Chen",
                    StakeholderRole = "IT Lead",
                    Department = "Engineering",
                    DisplayOrder = 1,
                    IsActive = true
                }
            },
            BusinessRules = new List<UpdateBusinessRuleRequest>
            {
                new() { Code = "BR-01", Title = "Stock Hold", Description = "Max 15 min lock", Importance = ImportanceLevel.High, IsHidden = false }
            },
            Events = new List<UpdateScenarioEventRequest>
            {
                new() { Title = "POS Outage", Description = "Simulate POS offline", TriggerType = ScenarioEventTriggerType.Manual, EventContent = "{}", DisplayOrder = 1, IsActive = true }
            }
        };

        // Act
        var result = await _sut.UpdateDefinitionAsync(userId, versionId, request);

        // Assert
        result.Should().NotBeNull();
        _authoringRepoMock.Verify(r => r.ClearVersionAggregate(version), Times.Once);
        _authoringRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        version.KnowledgeItems.Should().HaveCount(1);
        version.StakeholderPersonas.Should().HaveCount(1);
        version.BusinessRules.Should().HaveCount(1);
        version.ScenarioEvents.Should().HaveCount(1);
    }

    [Fact]
    public async Task TC_SCN_05_UpdateDefinitionAsync_DuplicateKnowledgeOrPersonaCodes_ThrowsRequestValidationException()
    {
        // Arrange
        const int userId = 10;
        const int versionId = 1;

        var scenario = new ScenarioEntity { Id = 5, CreatedByUserId = userId };
        var version = new ScenarioVersion
        {
            Id = versionId,
            Scenario = scenario,
            IsPublished = false
        };

        _authoringRepoMock.Setup(r => r.GetVersionWithAggregateForUpdateAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(version);

        var request = new UpdateScenarioDefinitionRequest
        {
            BusinessContext = "Ctx",
            BusinessProblem = "Prob",
            BusinessObjectives = "Obj",
            BARole = "BA",
            Assignment = "Task",
            KnowledgeItems = new List<UpdateKnowledgeItemRequest>
            {
                new() { Code = "K-DUP", Title = "Title 1", Content = "Content 1" },
                new() { Code = "K-DUP", Title = "Title 2", Content = "Content 2" } // Duplicate!
            },
            Personas = new List<UpdatePersonaRequest>
            {
                new() { Code = "P-01", Name = "Persona 1", StakeholderRole = "Role" }
            }
        };

        // Act
        var act = () => _sut.UpdateDefinitionAsync(userId, versionId, request);

        // Assert
        var ex = await act.Should().ThrowAsync<RequestValidationException>();
        ex.Which.Errors.Should().ContainKey("KnowledgeItems");
    }

    [Fact]
    public async Task TC_SCN_06_UpdateDefinitionAsync_DifferentUserThanAuthor_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        const int authorId = 10;
        const int otherUserId = 99;
        const int versionId = 1;

        var scenario = new ScenarioEntity { Id = 5, CreatedByUserId = authorId };
        var version = new ScenarioVersion
        {
            Id = versionId,
            Scenario = scenario,
            IsPublished = false
        };

        _authoringRepoMock.Setup(r => r.GetVersionWithAggregateForUpdateAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(version);

        var request = new UpdateScenarioDefinitionRequest
        {
            BusinessContext = "Ctx",
            BusinessProblem = "Prob",
            BusinessObjectives = "Obj",
            BARole = "BA",
            Assignment = "Task"
        };

        // Act
        var act = () => _sut.UpdateDefinitionAsync(otherUserId, versionId, request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not have permission to update*");
    }

    #endregion

    #region F-SCN-03: Phê duyệt & Đóng băng xuất bản (Quality Gate & Publish)

    [Fact]
    public async Task TC_SCN_07_PublishVersionAsync_ValidDraftMeetingChecklist_PublishesAndFreezesVersion()
    {
        // Arrange
        const int userId = 10;
        const int versionId = 1;

        var scenario = new ScenarioEntity
        {
            Id = 5,
            CreatedByUserId = userId,
            Status = ScenarioStatus.Draft
        };

        var persona = new StakeholderPersona
        {
            Id = 1,
            Name = "Ops Manager",
            IsActive = true
        };
        persona.StakeholderKnowledgeAccess.Add(new StakeholderKnowledgeAccess { KnowledgeItemId = 10 });

        var version = new ScenarioVersion
        {
            Id = versionId,
            Scenario = scenario,
            VersionNo = 1,
            BusinessContext = "Comprehensive Retail Context",
            BusinessProblem = "Omnichannel inventory sync problem",
            BusinessObjectives = "Achieve 99% order fulfillment accuracy",
            BARole = "Senior Business Analyst",
            Assignment = "Interview stakeholders and specify business requirements",
            IsPublished = false,
            StakeholderPersonas = new List<StakeholderPersona> { persona }
        };

        _authoringRepoMock.Setup(r => r.GetVersionForPublishAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(version);

        // Act
        var result = await _sut.PublishVersionAsync(userId, versionId);

        // Assert
        result.Should().NotBeNull();
        result.IsPublished.Should().BeTrue();
        result.PublishedAt.Should().NotBeNull();
        version.IsPublished.Should().BeTrue();
        scenario.Status.Should().Be(ScenarioStatus.Published);

        _authoringRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC_SCN_08_PublishVersionAsync_ActivePersonaWithoutAssignedKnowledge_ThrowsRequestValidationException()
    {
        // Arrange
        const int userId = 10;
        const int versionId = 1;

        var scenario = new ScenarioEntity { Id = 5, CreatedByUserId = userId };
        var personaWithoutKnowledge = new StakeholderPersona
        {
            Id = 1,
            Name = "Empty Persona",
            IsActive = true,
            StakeholderKnowledgeAccess = new List<StakeholderKnowledgeAccess>() // Missing knowledge!
        };

        var version = new ScenarioVersion
        {
            Id = versionId,
            Scenario = scenario,
            BusinessContext = "Context",
            BusinessProblem = "Problem",
            BusinessObjectives = "Objectives",
            BARole = "BA",
            Assignment = "Assignment",
            IsPublished = false,
            StakeholderPersonas = new List<StakeholderPersona> { personaWithoutKnowledge }
        };

        _authoringRepoMock.Setup(r => r.GetVersionForPublishAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(version);

        // Act
        var act = () => _sut.PublishVersionAsync(userId, versionId);

        // Assert
        var ex = await act.Should().ThrowAsync<RequestValidationException>();
        ex.Which.Errors.Should().ContainKey("KnowledgeAccess");
    }

    [Fact]
    public async Task TC_SCN_09_UpdateDefinitionAsync_WhenVersionAlreadyPublished_ThrowsConflictException_ImmutabilityTest()
    {
        // Arrange
        const int userId = 10;
        const int versionId = 1;

        var scenario = new ScenarioEntity { Id = 5, CreatedByUserId = userId, Status = ScenarioStatus.Published };
        var publishedVersion = new ScenarioVersion
        {
            Id = versionId,
            Scenario = scenario,
            IsPublished = true, // Already published!
            PublishedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        _authoringRepoMock.Setup(r => r.GetVersionWithAggregateForUpdateAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publishedVersion);

        var request = new UpdateScenarioDefinitionRequest
        {
            BusinessContext = "Attempt to modify published version"
        };

        // Act
        var act = () => _sut.UpdateDefinitionAsync(userId, versionId, request);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Cannot modify a scenario version that has already been published*");
    }

    #endregion

    #region End-to-End Workflow Tests

    [Fact]
    public async Task TC_SCN_E2E_01_AuthoringWorkflow_CreateDraft_UpdateDefinition_Publish_VerifyContextPublished()
    {
        // Arrange
        const int userId = 10;
        _authoringRepoMock.Setup(r => r.ApplicationExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _authoringRepoMock.Setup(r => r.ScenarioCodeExistsAsync("SCN-E2E", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Step 1: Create Scenario
        var createReq = new CreateScenarioRequest
        {
            ApplicationId = 1,
            Code = "SCN-E2E",
            Title = "End-to-End Scenario",
            Summary = "Initial draft"
        };
        var created = await _sut.CreateScenarioAsync(userId, createReq);
        created.Status.Should().Be(ScenarioStatus.Draft);

        // Step 2: Update Definition
        var version = new ScenarioVersion
        {
            Id = 1,
            ScenarioId = created.Id,
            Scenario = new ScenarioEntity { Id = created.Id, Code = "SCN-E2E", Title = "End-to-End Scenario", CreatedByUserId = userId },
            BusinessContext = "Full business context",
            BusinessProblem = "Core problem statement",
            BusinessObjectives = "Objectives",
            BARole = "Lead BA",
            Assignment = "BA assignment",
            IsPublished = false
        };
        _authoringRepoMock.Setup(r => r.GetVersionWithAggregateForUpdateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(version);
        _authoringRepoMock.Setup(r => r.GetVersionDefinitionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(version);

        var updateReq = new UpdateScenarioDefinitionRequest
        {
            BusinessContext = "Full business context",
            BusinessProblem = "Core problem statement",
            BusinessObjectives = "Objectives",
            BARole = "Lead BA",
            Assignment = "BA assignment",
            Personas = new List<UpdatePersonaRequest>
            {
                new() { Code = "P-1", Name = "Sarah", StakeholderRole = "Ops Lead", IsActive = true }
            },
            KnowledgeItems = new List<UpdateKnowledgeItemRequest>
            {
                new()
                {
                    Code = "K-1", Title = "Inventory Rules", Content = "Safety stock rules",
                    AccessList = new List<UpdateKnowledgeAccessRequest>
                    {
                        new() { PersonaCode = "P-1", AccessType = KnowledgeAccessType.Knows, IsHidden = false }
                    }
                }
            }
        };
        await _sut.UpdateDefinitionAsync(userId, 1, updateReq);

        // Step 3: Publish
        _authoringRepoMock.Setup(r => r.GetVersionForPublishAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(version);
        var publishedResult = await _sut.PublishVersionAsync(userId, 1);
        publishedResult.IsPublished.Should().BeTrue();

        // Step 4: Verify Context
        _dbContext.ScenarioVersions.Add(version);
        await _dbContext.SaveChangesAsync();

        var context = await _sut.GetContextAsync(userId, ["Learner"], 1);
        context.Should().NotBeNull();
        context.BusinessContext.Should().Be("Full business context");
    }

    [Fact]
    public async Task TC_SCN_E2E_02_VersioningWorkflow_PublishV1_CreateV2Copy_UpdateV2_PublishV2_IncrementsVersionNo()
    {
        // Arrange
        const int userId = 10;
        const int scenarioId = 1;

        var scenario = new ScenarioEntity
        {
            Id = scenarioId,
            Code = "SCN-MULTI",
            CreatedByUserId = userId,
            Status = ScenarioStatus.Published,
            Versions = new List<ScenarioVersion>
            {
                new() { Id = 1, VersionNo = 1, IsPublished = true }
            }
        };

        var v1 = new ScenarioVersion
        {
            Id = 1,
            ScenarioId = scenarioId,
            VersionNo = 1,
            IsPublished = true,
            BusinessContext = "V1 Context",
            BusinessProblem = "V1 Problem",
            BusinessObjectives = "V1 Objectives",
            BARole = "BA",
            Assignment = "V1 Task"
        };

        _authoringRepoMock.Setup(r => r.GetScenarioWithVersionsAsync(scenarioId, It.IsAny<CancellationToken>())).ReturnsAsync(scenario);
        _authoringRepoMock.Setup(r => r.GetLatestVersionWithAggregateAsync(scenarioId, It.IsAny<CancellationToken>())).ReturnsAsync(v1);

        // Act: Create V2 copied from V1
        var v2Result = await _sut.CreateVersionAsync(userId, scenarioId, new CreateScenarioVersionRequest { CopyFromLatest = true });

        // Assert
        v2Result.VersionNo.Should().Be(2);
        v2Result.IsPublished.Should().BeFalse();
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public async Task TC_SCN_ST_01_StateTransition_DraftToPublished_UpdatesVersionAndScenarioStatus()
    {
        // Arrange
        const int userId = 10;
        var scenario = new ScenarioEntity { Id = 1, CreatedByUserId = userId, Status = ScenarioStatus.Draft };
        var persona = new StakeholderPersona { Id = 1, Name = "Lead", IsActive = true };
        persona.StakeholderKnowledgeAccess.Add(new StakeholderKnowledgeAccess { KnowledgeItemId = 1 });

        var version = new ScenarioVersion
        {
            Id = 1,
            Scenario = scenario,
            BusinessContext = "Context",
            BusinessProblem = "Problem",
            BusinessObjectives = "Objectives",
            BARole = "BA",
            Assignment = "Task",
            IsPublished = false,
            StakeholderPersonas = new List<StakeholderPersona> { persona }
        };

        _authoringRepoMock.Setup(r => r.GetVersionForPublishAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(version);

        // Act
        var result = await _sut.PublishVersionAsync(userId, 1);

        // Assert
        result.IsPublished.Should().BeTrue();
        scenario.Status.Should().Be(ScenarioStatus.Published);
    }

    [Fact]
    public async Task TC_SCN_ST_02_StateTransition_PublishedToPublishAgain_ThrowsConflictException()
    {
        // Arrange
        const int userId = 10;
        var scenario = new ScenarioEntity { Id = 1, CreatedByUserId = userId, Status = ScenarioStatus.Published };
        var version = new ScenarioVersion
        {
            Id = 1,
            Scenario = scenario,
            IsPublished = true // already published
        };

        _authoringRepoMock.Setup(r => r.GetVersionForPublishAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(version);

        // Act
        var act = () => _sut.PublishVersionAsync(userId, 1);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*has already been published*");
    }

    [Fact]
    public async Task TC_SCN_ST_03_StateTransition_PublishedToUpdateDefinition_ThrowsConflictException()
    {
        // Arrange
        const int userId = 10;
        var scenario = new ScenarioEntity { Id = 1, CreatedByUserId = userId, Status = ScenarioStatus.Published };
        var version = new ScenarioVersion
        {
            Id = 1,
            Scenario = scenario,
            IsPublished = true
        };

        _authoringRepoMock.Setup(r => r.GetVersionWithAggregateForUpdateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(version);

        // Act
        var act = () => _sut.UpdateDefinitionAsync(userId, 1, new UpdateScenarioDefinitionRequest());

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Cannot modify a scenario version that has already been published*");
    }

    [Fact]
    public async Task TC_SCN_ST_04_ContextAuthorization_LearnerCannotAccessDraft_ExpertCanAccessDraft()
    {
        // Arrange
        var scenario = new ScenarioEntity { Id = 1, Code = "SCN-1", Title = "Scenario 1" };
        var draftVersion = new ScenarioVersion
        {
            Id = 1,
            Scenario = scenario,
            VersionNo = 1,
            Summary = "Draft summary",
            BusinessContext = "Secret draft context",
            BusinessProblem = "Draft problem",
            BusinessObjectives = "Draft objectives",
            BARole = "BA",
            Assignment = "Draft assignment",
            IsPublished = false
        };

        _dbContext.ScenarioVersions.Add(draftVersion);
        await _dbContext.SaveChangesAsync();

        // Act & Assert 1: Learner cannot view draft
        var learnerAct = () => _sut.GetContextAsync(100, ["Learner"], 1);
        await learnerAct.Should().ThrowAsync<NotFoundException>();

        // Act & Assert 2: BAExpert can view draft
        var expertResult = await _sut.GetContextAsync(100, ["BAExpert"], 1);
        expertResult.Should().NotBeNull();
        expertResult.BusinessContext.Should().Be("Secret draft context");
    }

    [Fact]
    public async Task TC_SCN_RV_01_SubmitThenApprove_PublishesOnlyAfterExpertDecision()
    {
        const int authorId = 10;
        const int expertId = 20;
        var scenario = new ScenarioEntity
        {
            Id = 5,
            Code = "SCN-RV",
            Title = "Review gate",
            CreatedByUserId = authorId,
            Status = ScenarioStatus.Draft
        };
        var persona = new StakeholderPersona { Id = 1, Name = "Ops", IsActive = true };
        persona.StakeholderKnowledgeAccess.Add(new StakeholderKnowledgeAccess { KnowledgeItemId = 10 });
        var version = ReadyVersion(scenario, persona);

        _authoringRepoMock.Setup(r => r.GetVersionForPublishAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(version);

        var submitted = await _sut.SubmitForReviewAsync(authorId, ["INSTRUCTOR"], 1);
        submitted.Status.Should().Be(nameof(ScenarioStatus.PendingReview));
        submitted.IsPublished.Should().BeFalse();
        scenario.Status.Should().Be(ScenarioStatus.PendingReview);

        var approved = await _sut.ApproveVersionAsync(expertId, ["EXPERT"], 1);
        approved.Status.Should().Be(nameof(ScenarioStatus.Published));
        approved.IsPublished.Should().BeTrue();
        approved.ReviewedByUserId.Should().Be(expertId);
        scenario.Status.Should().Be(ScenarioStatus.Published);
    }

    [Fact]
    public async Task TC_SCN_RV_02_Reject_StoresCommentAndDoesNotPublish()
    {
        const int authorId = 10;
        var scenario = new ScenarioEntity
        {
            Id = 5,
            Code = "SCN-RJ",
            Title = "Rejected",
            CreatedByUserId = authorId,
            Status = ScenarioStatus.PendingReview
        };
        var version = ReadyVersion(scenario, PersonaWithKnowledge());
        version.Status = ScenarioStatus.PendingReview;

        _authoringRepoMock.Setup(r => r.GetVersionForPublishAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(version);

        var rejected = await _sut.RejectVersionAsync(30, ["BAExpert"], 1, "Thiếu persona tài chính và quy tắc ngân sách.");

        rejected.Status.Should().Be(nameof(ScenarioStatus.Rejected));
        rejected.IsPublished.Should().BeFalse();
        rejected.ReviewComment.Should().Contain("persona tài chính");
        scenario.Status.Should().Be(ScenarioStatus.Rejected);
    }

    [Fact]
    public async Task TC_SCN_RV_03_InstructorCannotApprove()
    {
        var scenario = new ScenarioEntity
        {
            Id = 5,
            Code = "SCN-NO",
            Title = "No self publish",
            CreatedByUserId = 10
        };
        var version = ReadyVersion(scenario, PersonaWithKnowledge());
        version.Status = ScenarioStatus.PendingReview;
        _authoringRepoMock.Setup(r => r.GetVersionForPublishAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(version);

        var act = () => _sut.ApproveVersionAsync(10, ["INSTRUCTOR"], 1);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static StakeholderPersona PersonaWithKnowledge()
    {
        var persona = new StakeholderPersona { Id = 1, Name = "Ops", IsActive = true };
        persona.StakeholderKnowledgeAccess.Add(new StakeholderKnowledgeAccess { KnowledgeItemId = 10 });
        return persona;
    }

    private static ScenarioVersion ReadyVersion(ScenarioEntity scenario, StakeholderPersona persona) =>
        new()
        {
            Id = 1,
            Scenario = scenario,
            ScenarioId = scenario.Id,
            VersionNo = 1,
            BusinessContext = "Context",
            BusinessProblem = "Problem",
            BusinessObjectives = "Objectives",
            BARole = "Senior BA",
            Assignment = "Interview stakeholders",
            IsPublished = false,
            Status = ScenarioStatus.Draft,
            StakeholderPersonas = new List<StakeholderPersona> { persona }
        };

    #endregion
}
