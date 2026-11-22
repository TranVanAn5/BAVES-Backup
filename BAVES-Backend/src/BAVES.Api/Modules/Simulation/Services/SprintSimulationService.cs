using System.Text.Json;
using System.Text.RegularExpressions;
using BAVES.Api.Modules.Artifact.Data;
using BAVES.Api.Modules.Artifact.Models.Enums;
using BAVES.Api.Modules.Scenario.Data;
using BAVES.Api.Modules.Scenario.Models.Enums;
using BAVES.Api.Modules.Scenario.Services;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Modules.Simulation.Data;
using BAVES.Api.Modules.Simulation.Models.DTOs;
using BAVES.Api.Modules.Simulation.Models.Entities;
using BAVES.Api.Modules.Simulation.Models.Enums;
using BAVES.Api.Modules.Simulation.Services.Interfaces;
using BAVES.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Simulation.Services;

public sealed class SprintSimulationService(
    SimulationDbContext simulationDb,
    ScenarioDbContext scenarioDb,
    ArtifactDbContext artifactDb,
    IAiRuntimeClient aiRuntimeClient,
    IScenarioCatalogService scenarioCatalogService
) : ISprintSimulationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const int SprintDurationMinutes = 45;

    public async Task<SprintBoardDto> GetBoardAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await RequireSessionAsync(learnerId, sessionId, cancellationToken);
        var tasks = await LoadSprintTasksAsync(session.ScenarioVersionId, cancellationToken);
        var reviews = await simulationDb.SessionSprintReviews
            .AsNoTracking()
            .Where(review => review.SessionId == sessionId)
            .ToListAsync(cancellationToken);
        var submittedCodes = await LoadSubmittedTemplateCodesAsync(sessionId, cancellationToken);

        return BuildBoard(session, tasks, reviews, submittedCodes);
    }

    public async Task<SprintReviewTurnDto> ConductReviewAsync(
        int learnerId,
        int sessionId,
        int sprintNo,
        string? learnerReply,
        bool finalizeSprint,
        AgentAiConnection aiConnection,
        CancellationToken cancellationToken = default)
    {
        var session = await RequireSessionAsync(learnerId, sessionId, cancellationToken);
        var tasks = await LoadSprintTasksAsync(session.ScenarioVersionId, cancellationToken);
        var totalSprints = Math.Max(tasks.Count, 1);
        if (sprintNo < 1 || sprintNo > totalSprints)
        {
            throw new ConflictException($"Sprint {sprintNo} is outside the 1..{totalSprints} range.");
        }

        var reviews = await simulationDb.SessionSprintReviews
            .Where(review => review.SessionId == sessionId)
            .ToListAsync(cancellationToken);

        if (sprintNo > 1 && reviews.All(review => review.SprintNo != sprintNo - 1 || review.Status != "Finalized"))
        {
            throw new ConflictException($"Sprint {sprintNo - 1} must be finalized before reviewing sprint {sprintNo}.");
        }

        var current = reviews.FirstOrDefault(review => review.SprintNo == sprintNo);
        if (current?.Status == "Finalized")
        {
            throw new ConflictException($"Sprint {sprintNo} has already been finalized.");
        }

        var history = ReadMessages(current?.DiscussionJson);
        if (!string.IsNullOrWhiteSpace(learnerReply))
        {
            history.Add(new DiscussionMessage("Learner", learnerReply.Trim()));
        }

        var personas = await scenarioDb.StakeholderPersonas
            .AsNoTracking()
            .Where(persona => persona.ScenarioVersionId == session.ScenarioVersionId && persona.IsActive)
            .OrderBy(persona => persona.DisplayOrder)
            .Take(4)
            .ToListAsync(cancellationToken);

        var submittedArtifacts = await LoadSubmittedArtifactsAsync(sessionId, cancellationToken);
        var sprint = tasks.First(task => task.SprintNo == sprintNo);
        var requiredCodes = sprint.TemplateCodes;
        var priorCodes = tasks
            .Where(task => task.SprintNo < sprintNo)
            .SelectMany(task => task.TemplateCodes)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var submittedCodes = submittedArtifacts
            .Select(item => item.TemplateCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var missingCodes = requiredCodes
            .Where(code => submittedCodes.All(submitted => !string.Equals(submitted, code, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        var newCodes = requiredCodes
            .Where(code => priorCodes.All(prior => !string.Equals(prior, code, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var interviewExcerpts = await simulationDb.Messages
            .AsNoTracking()
            .Where(message => message.SessionId == sessionId)
            .OrderByDescending(message => message.CreatedAt)
            .Take(24)
            .Select(message => new
            {
                speaker = message.SenderType == MessageSenderType.Learner ? "Learner" : "Stakeholder",
                content = message.Content
            })
            .ToListAsync(cancellationToken);
        interviewExcerpts.Reverse();

        var priorReviews = reviews
            .Where(review => review.SprintNo < sprintNo && review.Status == "Finalized")
            .OrderBy(review => review.SprintNo)
            .Select(review => new
            {
                sprintNo = review.SprintNo,
                score = review.SprintScore,
                decision = review.Decision,
                digest = review.CompressedDigest,
                actionItems = ReadStrings(review.ActionItemsJson)
            })
            .ToList();

        var previousFinalized = reviews
            .Where(review => review.SprintNo < sprintNo && review.Status == "Finalized")
            .OrderByDescending(review => review.SprintNo)
            .FirstOrDefault();
        var priorSnapshots = ReadArtifactSnapshots(previousFinalized?.ArtifactSnapshotJson);
        var compareCodes = priorCodes
            .Where(code => submittedCodes.Any(submitted => string.Equals(submitted, code, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var isFinalSprint = sprintNo >= totalSprints;
        var reviewPhase = !finalizeSprint
            ? "Discuss"
            : isFinalSprint
                ? "FinalizeFinal"
                : "FinalizeMid";
        var revealNumericScore = isFinalSprint && finalizeSprint;

        var gradeArtifacts = submittedArtifacts
            .Where(item =>
                requiredCodes.Count == 0
                || requiredCodes.Any(code => string.Equals(code, item.TemplateCode, StringComparison.OrdinalIgnoreCase))
                || priorCodes.Any(code => string.Equals(code, item.TemplateCode, StringComparison.OrdinalIgnoreCase)))
            .GroupBy(item => item.TemplateCode, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(item => item.SubmittedAt).First())
            .ToList();

        var artifactPayload = gradeArtifacts.Count == 0
            ? new object[]
            {
                new
                {
                    artifactType = "Generic",
                    templateCode = (string?)null,
                    title = sprint.Name,
                    versionNo = 1,
                    sections = new[]
                    {
                        new
                        {
                            title = "Thiếu deliverable",
                            sectionKey = "missing",
                            content = missingCodes.Count > 0
                                ? $"Learner chưa nộp các artifact bắt buộc: {string.Join(", ", missingCodes)}."
                                : "Chưa có artifact nào được nộp cho sprint này."
                        }
                    }
                }
            }
            : gradeArtifacts.Select(artifact => new
            {
                artifactId = artifact.ArtifactId,
                artifactType = MapArtifactType(artifact.ArtifactType),
                templateCode = artifact.TemplateCode,
                title = artifact.Title,
                versionNo = artifact.VersionNo,
                sections = new[]
                {
                    new
                    {
                        title = "Nội dung đã nộp",
                        sectionKey = "body",
                        content = Trim(artifact.PlainTextContent, 1800)
                    }
                }
            }).ToArray<object>();

        var priorArtifactPayload = compareCodes
            .Select(code =>
            {
                var current = gradeArtifacts.FirstOrDefault(item =>
                    string.Equals(item.TemplateCode, code, StringComparison.OrdinalIgnoreCase));
                var prior = priorSnapshots.FirstOrDefault(item =>
                    string.Equals(item.TemplateCode, code, StringComparison.OrdinalIgnoreCase));
                if (current is null && prior is null)
                {
                    return null;
                }

                return new
                {
                    artifactId = current?.ArtifactId ?? prior?.ArtifactId,
                    artifactType = current is not null
                        ? MapArtifactType(current.ArtifactType)
                        : (string.IsNullOrWhiteSpace(prior?.ArtifactType) ? "Generic" : prior!.ArtifactType),
                    templateCode = code,
                    title = $"{current?.Title ?? prior?.Title ?? code} (so sánh Sprint {previousFinalized?.SprintNo} → {sprintNo})",
                    versionNo = current?.VersionNo ?? prior?.VersionNo ?? 1,
                    sections = new[]
                    {
                        new
                        {
                            title = $"Bản Sprint {previousFinalized?.SprintNo ?? sprintNo - 1}",
                            sectionKey = "prior",
                            content = Trim(prior?.PlainTextContent, 1400)
                        },
                        new
                        {
                            title = $"Bản Sprint {sprintNo}",
                            sectionKey = "current",
                            content = Trim(current?.PlainTextContent, 1400)
                        }
                    }
                };
            })
            .Where(item => item is not null)
            .ToArray();

        var runtime = await aiRuntimeClient.ReviewSprintAsync(new
        {
            requestId = $"sprint-{sessionId}-{sprintNo}",
            sessionId,
            sprintNo,
            totalSprints,
            reviewPhase,
            revealNumericScoreToLearner = revealNumericScore,
            chairPersonas = personas.Select(persona => new
            {
                personaId = persona.Id,
                name = persona.Name,
                stakeholderRole = persona.StakeholderRole,
                department = persona.Department,
                goals = persona.Goals,
                painPoints = persona.PainPoints,
                businessConcerns = persona.BusinessConcerns,
                communicationStyle = persona.CommunicationStyle
            }),
            artifacts = artifactPayload,
            requiredDeliverableCodes = requiredCodes,
            missingDeliverableCodes = missingCodes,
            newDeliverableCodes = newCodes.Count > 0 ? newCodes : requiredCodes,
            priorArtifactsForComparison = priorArtifactPayload,
            interviewExcerpts = interviewExcerpts.Select(item => new
            {
                speaker = item.speaker,
                content = Trim(item.content, 500)
            }),
            priorSprintDigests = priorReviews,
            discussionHistory = history.Select(message => new { speaker = message.Speaker, content = message.Content }),
            learnerReply,
            finalizeSprint,
            aiConnection = new
            {
                provider = aiConnection.Provider,
                model = aiConnection.Model,
                apiKey = aiConnection.ApiKey
            }
        }, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        current ??= new SessionSprintReview
        {
            SessionId = sessionId,
            SprintNo = sprintNo,
            CreatedAt = now
        };
        if (current.Id == 0)
        {
            simulationDb.SessionSprintReviews.Add(current);
        }

        if (!string.IsNullOrWhiteSpace(runtime.ChairReply))
        {
            history.Add(new DiscussionMessage(runtime.ChairSpeakerName, runtime.ChairReply));
        }

        current.DiscussionJson = JsonSerializer.Serialize(history, JsonOptions);
        current.ChairSpeakerName = runtime.ChairSpeakerName;
        current.ChairReply = runtime.ChairReply;
        current.ChallengeQuestionsJson = JsonSerializer.Serialize(runtime.ChallengeQuestions ?? [], JsonOptions);
        current.PromptVersion = runtime.PromptVersion;

        PriorSprintReassessmentDto? priorReassessment = null;
        var improvementNotes = Array.Empty<string>();

        if (finalizeSprint && runtime.SprintSummary is not null)
        {
            current.Status = "Finalized";
            current.Decision = runtime.SprintSummary.Decision;
            current.SprintScore = runtime.SprintSummary.SprintScore;
            current.CompressedDigest = runtime.SprintSummary.CompressedDigest;
            current.ApprovedPointsJson = JsonSerializer.Serialize(runtime.SprintSummary.ApprovedPoints ?? [], JsonOptions);
            var missingNotes = missingCodes.Count > 0
                ? new[] { $"Thiếu deliverable bắt buộc: {string.Join(", ", missingCodes)}." }
                : Array.Empty<string>();
            improvementNotes = missingNotes
                .Concat(runtime.SprintSummary.ActionItemsForNextSprint ?? [])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            current.ActionItemsJson = JsonSerializer.Serialize(improvementNotes, JsonOptions);
            current.ArtifactSnapshotJson = JsonSerializer.Serialize(
                gradeArtifacts.Select(item => new ArtifactSnapshot(
                    item.ArtifactId,
                    item.TemplateCode,
                    item.Title,
                    MapArtifactType(item.ArtifactType),
                    item.VersionNo,
                    Trim(item.PlainTextContent, 4000),
                    item.SubmittedAt)),
                JsonOptions);
            current.FinalizedAt = now;

            if (sprintNo >= 2 && previousFinalized is not null)
            {
                var previousScore = previousFinalized.SprintScore;
                var reassessed = runtime.SprintSummary.SprintScore;
                var improved = previousScore is null || reassessed >= previousScore;
                var comparisonHint = priorArtifactPayload.Length > 0
                    ? $"Đã so sánh {priorArtifactPayload.Length} artifact mang sang từ Sprint {previousFinalized.SprintNo}."
                    : "Chưa có artifact mang sang để so sánh với sprint trước.";
                var notes = improvementNotes.Length > 0
                    ? $"{comparisonHint} {string.Join(" ", improvementNotes.Take(2))}"
                    : (runtime.SprintSummary.CompressedDigest ?? comparisonHint);
                priorReassessment = new PriorSprintReassessmentDto(
                    previousFinalized.SprintNo,
                    previousScore,
                    reassessed,
                    improved,
                    notes);
            }

            // Auto-trigger biến cố cấu hình "sau khi kết thúc Sprint N".
            await AutoTriggerAfterSprintEndsEventsAsync(
                sessionId,
                session.ScenarioVersionId,
                sprintNo,
                now,
                cancellationToken);
        }
        else
        {
            current.Status = "Open";
            improvementNotes = ReadStrings(current.ActionItemsJson).ToArray();
        }

        await simulationDb.SaveChangesAsync(cancellationToken);
        reviews = await simulationDb.SessionSprintReviews
            .AsNoTracking()
            .Where(review => review.SessionId == sessionId)
            .ToListAsync(cancellationToken);
        var submittedCodesForBoard = submittedCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var board = BuildBoard(session, tasks, reviews, submittedCodesForBoard);

        var scoreForLearner = revealNumericScore ? current.SprintScore : null;
        var decisionForLearner = revealNumericScore ? current.Decision : null;
        var finalEvaluation = revealNumericScore ? board.FinalEvaluation : null;

        return new SprintReviewTurnDto(
            sprintNo,
            current.Status,
            current.ChairSpeakerName ?? "Hội đồng",
            SanitizeChairReply(current.ChairReply ?? "", revealNumericScore, finalizeSprint, isFinalSprint),
            ReadStrings(current.ChallengeQuestionsJson),
            scoreForLearner,
            decisionForLearner,
            revealNumericScore ? current.CompressedDigest : null,
            board.ProjectFinalized && revealNumericScore,
            finalEvaluation,
            reviewPhase,
            revealNumericScore,
            finalizeSprint
                ? ReadStrings(current.ActionItemsJson)
                : ReadStrings(current.ChallengeQuestionsJson),
            priorReassessment);
    }

    private async Task AutoTriggerAfterSprintEndsEventsAsync(
        int sessionId,
        int scenarioVersionId,
        int finalizedSprintNo,
        DateTimeOffset triggeredAt,
        CancellationToken cancellationToken)
    {
        var catalog = await scenarioCatalogService.GetActiveEventsAsync(scenarioVersionId, cancellationToken);
        var due = catalog
            .Where(e => e.TriggerType == ScenarioEventTriggerType.AfterSprintEnds)
            .Where(e => EventTriggerConditionCodec.TryGetAfterSprintNo(e.TriggerConditionJson) == finalizedSprintNo)
            .ToList();
        if (due.Count == 0)
        {
            return;
        }

        var already = await simulationDb.SessionEvents
            .AsNoTracking()
            .Where(e => e.SessionId == sessionId && due.Select(d => d.Id).Contains(e.ScenarioEventId))
            .Select(e => e.ScenarioEventId)
            .ToListAsync(cancellationToken);
        var alreadySet = already.ToHashSet();

        var nextSeq = await simulationDb.SessionEvents
            .Where(e => e.SessionId == sessionId)
            .Select(e => (int?)e.SequenceNo)
            .MaxAsync(cancellationToken) ?? 0;

        foreach (var evt in due.Where(e => !alreadySet.Contains(e.Id)))
        {
            nextSeq++;
            simulationDb.SessionEvents.Add(new SessionEvent
            {
                SessionId = sessionId,
                ScenarioEventId = evt.Id,
                TriggerSource = EventTriggerSource.Automatic,
                TriggeredAt = triggeredAt,
                SequenceNo = nextSeq,
                EventContentSnapshot = evt.EventContent
            });
        }
    }

    private async Task<SimulationSession> RequireSessionAsync(int learnerId, int sessionId, CancellationToken cancellationToken)
    {
        var session = await simulationDb.SimulationSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == sessionId && item.LearnerId == learnerId, cancellationToken);
        if (session is null)
        {
            throw new NotFoundException($"Simulation session {sessionId} was not found.");
        }

        return session;
    }

    private async Task<IReadOnlyList<SprintDefinition>> LoadSprintTasksAsync(int scenarioVersionId, CancellationToken cancellationToken)
    {
        var version = await scenarioDb.ScenarioVersions
            .AsNoTracking()
            .Include(v => v.Sprints)
            .FirstOrDefaultAsync(v => v.Id == scenarioVersionId, cancellationToken);

        if (version is null)
        {
            return NormalizeSprints([]);
        }

        var fromTable = version.Sprints
            .OrderBy(s => s.SprintNo)
            .Select(s => new SprintDefinition(
                s.SprintNo,
                $"Sprint {s.SprintNo}",
                s.Focus,
                s.DurationValue,
                s.DurationUnit,
                ScenarioSprintMapper.ParseTemplateCodes(s.TemplateCodes).ToList()))
            .ToList();

        if (fromTable.Count > 0)
        {
            return NormalizeSprints(fromTable);
        }

        // Fallback: Assignment JSON/legacy (dữ liệu chưa migrate).
        return NormalizeSprints(ParseSprints(version.Assignment));
    }

    private static IReadOnlyList<SprintDefinition> NormalizeSprints(List<SprintDefinition> parsed)
    {
        if (parsed.Count == 0)
        {
            return DefaultSprints();
        }

        return parsed
            .OrderBy(item => item.SprintNo)
            .Select((item, index) => item with
            {
                SprintNo = index + 1,
                Name = string.IsNullOrWhiteSpace(item.Name) ? $"Sprint {index + 1}" : item.Name,
                TemplateCodes = NormalizeCodes(item.TemplateCodes)
            })
            .ToList();
    }

    private static List<SprintDefinition> ParseSprints(string? assignment)
    {
        if (string.IsNullOrWhiteSpace(assignment))
        {
            return [];
        }

        return AssignmentSprintCodec.Parse(assignment)
            .Select(item => new SprintDefinition(
                item.SprintNo,
                $"Sprint {item.SprintNo}",
                item.Focus,
                item.DurationValue,
                item.DurationUnit,
                NormalizeCodes(item.TemplateCodes)))
            .ToList();
    }

    private static IReadOnlyList<SprintDefinition> DefaultSprints() =>
    [
        new(1, "Sprint 1: Khởi động", "Lập kế hoạch BA, ma trận stakeholder và phạm vi.", 1, "weeks", ["DEL-02", "DEL-03"]),
        new(2, "Sprint 2: Khơi gợi", "Phỏng vấn stakeholder, ghi yêu cầu và quy tắc nghiệp vụ.", 1, "weeks", ["DEL-04", "DEL-05"]),
        new(3, "Sprint 3: Ứng biến", "Xử lý biến cố và cập nhật đặc tả.", 1, "weeks", ["DEL-07"]),
        new(4, "Sprint 4: Nghiệm thu", "Hoàn thiện hồ sơ và ma trận truy vết.", 1, "weeks", ["DEL-08"])
    ];

    private static SprintBoardDto BuildBoard(
        SimulationSession session,
        IReadOnlyList<SprintDefinition> tasks,
        List<SessionSprintReview> reviews,
        IReadOnlySet<string> submittedCodes)
    {
        var definitions = tasks.Count > 0 ? tasks : DefaultSprints();
        var sprintDtos = definitions.Select(task =>
        {
            var review = reviews.FirstOrDefault(item => item.SprintNo == task.SprintNo);
            var status = review?.Status ?? "Pending";
            IReadOnlyList<SprintDiscussionMessageDto> discussion;
            try
            {
                discussion = ReadMessages(review?.DiscussionJson)
                    .Select(message => new SprintDiscussionMessageDto(message.Speaker, message.Content))
                    .ToList();
            }
            catch
            {
                discussion = [];
            }

            var required = NormalizeCodes(task.TemplateCodes);
            var submitted = required
                .Where(code => submittedCodes.Contains(code))
                .ToList();
            var missing = required
                .Where(code => !submittedCodes.Contains(code))
                .ToList();

            return new SprintTaskDto(
                task.SprintNo,
                task.Name,
                task.Focus,
                status,
                review?.SprintScore,
                review?.Decision,
                review?.CompressedDigest,
                ReadStrings(review?.ChallengeQuestionsJson),
                ReadStrings(review?.ActionItemsJson),
                review?.ChairSpeakerName,
                review?.ChairReply,
                discussion,
                task.DurationValue,
                task.DurationUnit,
                required,
                submitted,
                missing);
        }).ToList();

        var finalized = sprintDtos.Count(item => item.Status == "Finalized");
        var current = sprintDtos.FirstOrDefault(item => item.Status != "Finalized")?.SprintNo ?? definitions.Count;
        var projectFinalized = finalized == definitions.Count && definitions.Count > 0;
        SprintFinalEvaluationDto? finalEvaluation = null;
        if (projectFinalized)
        {
            var lines = sprintDtos.Select(item => new SprintScoreLineDto(item.SprintNo, item.Score, item.Decision, item.Digest)).ToList();
            var scored = lines.Where(line => line.Score.HasValue).Select(line => line.Score!.Value).ToList();
            var average = scored.Count == 0 ? (decimal?)null : Math.Round(scored.Average(), 2);
            var summary = string.Join(" ", lines.Select(line => $"Sprint {line.SprintNo}: {line.Score?.ToString("0.##") ?? "n/a"} ({line.Decision ?? "n/a"}). {line.Digest}"));
            finalEvaluation = new SprintFinalEvaluationDto(average, summary.Trim(), lines);
        }

        var currentReview = reviews.FirstOrDefault(item => item.SprintNo == current);
        var previousFinalized = reviews
            .Where(item => item.SprintNo < current && item.Status == "Finalized" && item.FinalizedAt.HasValue)
            .OrderByDescending(item => item.SprintNo)
            .FirstOrDefault();
        // Sprint 1: từ lúc bắt đầu mô phỏng. Sprint sau: từ lúc sprint trước kết thúc (finalize).
        var startedAt = previousFinalized?.FinalizedAt ?? session.StartedAt;
        var currentDefinition = definitions.FirstOrDefault(item => item.SprintNo == current);
        var durationDays = AssignmentSprintCodec.ToDays(
            currentDefinition?.DurationValue ?? 1,
            currentDefinition?.DurationUnit);
        var dueAt = startedAt.AddDays(durationDays);
        var now = DateTimeOffset.UtcNow;
        var currentFinalized = currentReview?.Status == "Finalized" || projectFinalized;
        var isOverdue = !currentFinalized && now > dueAt;
        var minutesRemaining = currentFinalized
            ? null
            : (int?)Math.Max(0, (int)Math.Ceiling((dueAt - now).TotalMinutes));
        var sprintDurationMinutes = Math.Max(SprintDurationMinutes, durationDays * 24 * 60);

        return new SprintBoardDto(
            session.Id,
            current,
            definitions.Count,
            projectFinalized,
            sprintDtos,
            finalEvaluation,
            startedAt,
            dueAt,
            sprintDurationMinutes,
            isOverdue,
            minutesRemaining,
            ReviewRequired: false);
    }

    private static string SanitizeChairReply(string reply, bool revealNumericScore, bool finalizeSprint, bool isFinalSprint)
    {
        if (!finalizeSprint || revealNumericScore || isFinalSprint)
        {
            return reply;
        }

        // Mid-sprint finalize: strip score-like phrases so learner only sees improvement guidance.
        var cleaned = Regex.Replace(reply, @"\(?\s*\d{1,3}(?:[.,]\d+)?\s*/\s*100\s*(điểm)?\s*\)?", "", RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @"\d{1,3}(?:[.,]\d+)?\s*điểm", "các điểm cần cải thiện", RegexOptions.IgnoreCase);
        return cleaned.Trim();
    }

    private static List<DiscussionMessage> ReadMessages(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<DiscussionMessage>>(json, JsonOptions) ?? [];
    }

    private static IReadOnlyList<string> ReadStrings(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
    }

    private static string Trim(string? value, int max)
    {
        var text = string.IsNullOrWhiteSpace(value) ? "Chưa có nội dung." : value.Trim();
        return text.Length <= max ? text : text[..max];
    }

    private static List<string> NormalizeCodes(IEnumerable<string>? codes) =>
        (codes ?? [])
            .Select(code => code.Trim().ToUpperInvariant())
            .Where(code => code.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static string MapArtifactType(ArtifactType? artifactType) =>
        artifactType switch
        {
            null => "Generic",
            ArtifactType.ProblemStatement => "Generic",
            ArtifactType.ElicitationNotes => "Generic",
            ArtifactType.ProcessModel => "Generic",
            _ => artifactType.Value.ToString()
        };

    private async Task<HashSet<string>> LoadSubmittedTemplateCodesAsync(
        int sessionId,
        CancellationToken cancellationToken)
    {
        var codes = await artifactDb.Submissions
            .AsNoTracking()
            .Where(submission => submission.SessionId == sessionId)
            .Select(submission => submission.ArtifactVersion.Artifact.ArtifactTemplate.Code)
            .ToListAsync(cancellationToken);

        return codes
            .Select(code => code.Trim().ToUpperInvariant())
            .Where(code => code.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<List<SubmittedArtifactInfo>> LoadSubmittedArtifactsAsync(
        int sessionId,
        CancellationToken cancellationToken)
    {
        var rows = await artifactDb.Submissions
            .AsNoTracking()
            .Where(submission => submission.SessionId == sessionId)
            .OrderByDescending(submission => submission.SubmittedAt)
            .Select(submission => new
            {
                ArtifactId = submission.ArtifactVersion.ArtifactId,
                TemplateCode = submission.ArtifactVersion.Artifact.ArtifactTemplate.Code,
                Title = submission.ArtifactVersion.Artifact.Title,
                ArtifactType = submission.ArtifactVersion.Artifact.ArtifactTemplate.ArtifactType,
                VersionNo = submission.ArtifactVersion.VersionNo,
                PlainTextContent = submission.ArtifactVersion.PlainTextContent,
                submission.SubmittedAt
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new SubmittedArtifactInfo(
                row.ArtifactId,
                row.TemplateCode.Trim().ToUpperInvariant(),
                row.Title,
                row.ArtifactType,
                row.VersionNo,
                row.PlainTextContent,
                row.SubmittedAt))
            .ToList();
    }

    private static List<ArtifactSnapshot> ReadArtifactSnapshots(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<ArtifactSnapshot>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private sealed record SprintDefinition(
        int SprintNo,
        string Name,
        string Focus,
        int DurationValue,
        string DurationUnit,
        IReadOnlyList<string> TemplateCodes);

    private sealed record DiscussionMessage(string Speaker, string Content);

    private sealed record SubmittedArtifactInfo(
        int ArtifactId,
        string TemplateCode,
        string Title,
        ArtifactType ArtifactType,
        int VersionNo,
        string? PlainTextContent,
        DateTimeOffset SubmittedAt);

    private sealed record ArtifactSnapshot(
        int ArtifactId,
        string TemplateCode,
        string Title,
        string ArtifactType,
        int VersionNo,
        string? PlainTextContent,
        DateTimeOffset SubmittedAt);
}
