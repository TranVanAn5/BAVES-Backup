using BAVES.Api.Modules.Artifact.Repositories.Interfaces;
using BAVES.Api.Modules.Assessment.Repositories.Interfaces;
using BAVES.Api.Modules.Identity.Models.DTOs;
using BAVES.Api.Modules.Identity.Services.Interfaces;
using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.Enums;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Modules.Simulation.Services.Interfaces;
using BAVES.Api.Modules.Training.Models.DTOs;
using BAVES.Api.Modules.Training.Models.Entities;
using BAVES.Api.Modules.Training.Models.Enums;
using BAVES.Api.Modules.Training.Models.Requests;
using BAVES.Api.Modules.Training.Repositories.Interfaces;
using BAVES.Api.Modules.Training.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace BAVES.UnitTests.Training;

public class AssignmentServiceTests
{
    private readonly Mock<IAssignmentRepository> _assignmentRepoMock = new();
    private readonly Mock<ICohortRepository> _cohortRepoMock = new();
    private readonly Mock<IScenarioCatalogService> _scenarioCatalogServiceMock = new();
    private readonly Mock<ISimulationSessionService> _simulationSessionServiceMock = new();
    private readonly Mock<IArtifactRepository> _artifactRepoMock = new();
    private readonly Mock<IAssessmentRepository> _assessmentRepoMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();

    private readonly AssignmentService _sut;

    public AssignmentServiceTests()
    {
        _sut = new AssignmentService(
            _assignmentRepoMock.Object,
            _cohortRepoMock.Object,
            _scenarioCatalogServiceMock.Object,
            _simulationSessionServiceMock.Object,
            _artifactRepoMock.Object,
            _assessmentRepoMock.Object,
            _userServiceMock.Object
        );
    }

    [Fact]
    public async Task CreateAssignmentAsync_BothCohortIdAndLearnerIdProvided_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateAssignmentRequest
        {
            Title = "Assignment 1",
            ScenarioVersionId = 1,
            CohortId = 10,
            LearnerId = 20
        };

        // Act
        var act = () => _sut.CreateAssignmentAsync(1, "INSTRUCTOR", request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*đúng 1 đối tượng*");
    }

    [Fact]
    public async Task CreateAssignmentAsync_NeitherCohortIdNorLearnerIdProvided_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateAssignmentRequest
        {
            Title = "Assignment 1",
            ScenarioVersionId = 1,
            CohortId = null,
            LearnerId = null
        };

        // Act
        var act = () => _sut.CreateAssignmentAsync(1, "INSTRUCTOR", request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*đúng 1 đối tượng*");
    }

    [Fact]
    public async Task CreateAssignmentAsync_StartAtAfterDueAt_ThrowsArgumentException()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var request = new CreateAssignmentRequest
        {
            Title = "Assignment 1",
            ScenarioVersionId = 1,
            CohortId = 10,
            StartAt = now.AddDays(5),
            DueAt = now.AddDays(1)
        };

        // Act
        var act = () => _sut.CreateAssignmentAsync(1, "INSTRUCTOR", request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Thời hạn nộp (DueAt) phải lớn hơn hoặc bằng thời gian mở đề (StartAt).*");
    }

    [Fact]
    public async Task CreateAssignmentAsync_MaxAttemptsLessThanOrEqualToZero_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateAssignmentRequest
        {
            Title = "Assignment 1",
            ScenarioVersionId = 1,
            CohortId = 10,
            MaxAttempts = 0
        };

        // Act
        var act = () => _sut.CreateAssignmentAsync(1, "INSTRUCTOR", request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Số lần làm tối đa (MaxAttempts) phải lớn hơn 0.*");
    }

    [Fact]
    public async Task CreateAssignmentAsync_ScenarioVersionDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new CreateAssignmentRequest
        {
            Title = "Assignment 1",
            ScenarioVersionId = 999,
            CohortId = 10
        };

        _scenarioCatalogServiceMock
            .Setup(s => s.GetVersionInfoAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScenarioVersionInfoDto?)null);

        // Act
        var act = () => _sut.CreateAssignmentAsync(1, "INSTRUCTOR", request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*không tồn tại hoặc chưa được xuất bản*");
    }

    [Fact]
    public async Task CreateAssignmentAsync_ScenarioVersionNotPublished_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new CreateAssignmentRequest
        {
            Title = "Assignment 1",
            ScenarioVersionId = 10,
            CohortId = 10
        };

        _scenarioCatalogServiceMock
            .Setup(s => s.GetVersionInfoAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScenarioVersionInfoDto(10, 1, 1, ScenarioDifficulty.Intermediate, IsPublished: false));

        // Act
        var act = () => _sut.CreateAssignmentAsync(1, "INSTRUCTOR", request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*không tồn tại hoặc chưa được xuất bản*");
    }

    [Fact]
    public async Task CreateAssignmentAsync_CohortNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new CreateAssignmentRequest
        {
            Title = "Assignment 1",
            ScenarioVersionId = 10,
            CohortId = 99
        };

        _scenarioCatalogServiceMock
            .Setup(s => s.GetVersionInfoAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScenarioVersionInfoDto(10, 1, 1, ScenarioDifficulty.Intermediate, IsPublished: true));

        _cohortRepoMock
            .Setup(c => c.GetEntityByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cohort?)null);

        // Act
        var act = () => _sut.CreateAssignmentAsync(1, "INSTRUCTOR", request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Không tìm thấy lớp học ID 99.*");
    }

    [Fact]
    public async Task CreateAssignmentAsync_InstructorDoesNotOwnCohort_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new CreateAssignmentRequest
        {
            Title = "Assignment 1",
            ScenarioVersionId = 10,
            CohortId = 5
        };

        _scenarioCatalogServiceMock
            .Setup(s => s.GetVersionInfoAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScenarioVersionInfoDto(10, 1, 1, ScenarioDifficulty.Intermediate, IsPublished: true));

        _cohortRepoMock
            .Setup(c => c.GetEntityByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Cohort
            {
                Id = 5,
                InstructorId = 2 // Different instructor
            });

        // Act
        var act = () => _sut.CreateAssignmentAsync(1, "INSTRUCTOR", request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Bạn không có quyền giao bài tập cho lớp học của giảng viên khác.*");
    }

    [Fact]
    public async Task CreateAssignmentAsync_ValidCohortAssignment_AddsEntityAndReturnsDetail()
    {
        // Arrange
        var request = new CreateAssignmentRequest
        {
            Title = "Valid Cohort Assignment",
            ScenarioVersionId = 10,
            CohortId = 5,
            MaxAttempts = 3,
            Status = AssignmentStatus.Active
        };

        _scenarioCatalogServiceMock
            .Setup(s => s.GetVersionInfoAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScenarioVersionInfoDto(10, 1, 1, ScenarioDifficulty.Intermediate, IsPublished: true));

        _cohortRepoMock
            .Setup(c => c.GetEntityByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Cohort
            {
                Id = 5,
                InstructorId = 1
            });

        var expectedDto = new AssignmentDetailDto(
            Id: 101,
            Title: "Valid Cohort Assignment",
            Instructions: null,
            ScenarioVersionId: 10,
            CohortId: 5,
            CohortCode: "SE101",
            CohortName: "SE Class 101",
            LearnerId: null,
            RubricId: null,
            AssignedByUserId: 1,
            StartAt: null,
            DueAt: null,
            MaxAttempts: 3,
            Status: AssignmentStatus.Active,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: null
        );

        _assignmentRepoMock
            .Setup(a => a.GetDetailByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _sut.CreateAssignmentAsync(1, "INSTRUCTOR", request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Valid Cohort Assignment");
        result.CohortId.Should().Be(5);
        _assignmentRepoMock.Verify(a => a.AddAssignment(It.Is<Assignment>(entity =>
            entity.Title == "Valid Cohort Assignment" &&
            entity.CohortId == 5 &&
            entity.AssignedByUserId == 1
        )), Times.Once);
        _assignmentRepoMock.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAssignmentAsync_LearnerTargetDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new CreateAssignmentRequest
        {
            Title = "Direct Learner Assignment",
            ScenarioVersionId = 10,
            LearnerId = 88
        };

        _scenarioCatalogServiceMock
            .Setup(s => s.GetVersionInfoAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScenarioVersionInfoDto(10, 1, 1, ScenarioDifficulty.Intermediate, IsPublished: true));

        _userServiceMock
            .Setup(u => u.GetUserSummaryByIdAsync(88, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSummaryDto?)null);

        // Act
        var act = () => _sut.CreateAssignmentAsync(1, "INSTRUCTOR", request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Không tìm thấy tài khoản học viên ID 88.*");
    }
}
