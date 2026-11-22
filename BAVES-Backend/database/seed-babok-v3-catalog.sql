-- ==============================================================================================
-- BA-VES · BABOK® v3 Knowledge Area catalog (idempotent by Code)
-- Đủ 6 vùng tri thức + task đại diện để hồ sơ năng lực / rubric gắn đúng chuẩn IIBA.
-- ==============================================================================================
SET NOCOUNT ON;

IF OBJECT_ID(N'[assessment].[BabokTasks]', N'U') IS NULL
BEGIN
    RAISERROR(N'[assessment].[BabokTasks] chưa tồn tại — chạy schema trước.', 16, 1);
    RETURN;
END
GO

DECLARE @Tasks TABLE (
    KnowledgeAreaCode nvarchar(80)  NOT NULL,
    KnowledgeAreaName nvarchar(200) NOT NULL,
    Code              nvarchar(80)  NOT NULL,
    Name              nvarchar(200) NOT NULL,
    Purpose           nvarchar(500) NULL,
    Description       nvarchar(max) NULL,
    DisplayOrder      int           NOT NULL
);

INSERT INTO @Tasks (KnowledgeAreaCode, KnowledgeAreaName, Code, Name, Purpose, Description, DisplayOrder) VALUES
-- BAPM · Business Analysis Planning and Monitoring
(N'BAPM', N'Business Analysis Planning and Monitoring',
 N'BAPM-3.1', N'Plan Business Analysis Approach',
 N'Lập kế hoạch tiếp cận phân tích nghiệp vụ',
 N'Xác định phương pháp, mức độ chi tiết, và cách tổ chức nỗ lực BA phù hợp ngữ cảnh dự án.', 10),
(N'BAPM', N'Business Analysis Planning and Monitoring',
 N'BAPM-3.2', N'Plan Stakeholder Engagement',
 N'Lập kế hoạch gắn kết bên liên quan',
 N'Phân tích stakeholder, mức ảnh hưởng/quan tâm, và chiến lược cộng tác (RACI).', 11),
(N'BAPM', N'Business Analysis Planning and Monitoring',
 N'BAPM-3.4', N'Plan Business Analysis Information Management',
 N'Lập kế hoạch quản trị thông tin BA',
 N'Quy định cách lưu trữ, truy vết và chia sẻ artifact phân tích trong suốt vòng đời.', 12),

-- EC · Elicitation and Collaboration
(N'EC', N'Elicitation and Collaboration',
 N'EC-4.1', N'Prepare for Elicitation',
 N'Chuẩn bị khơi gợi yêu cầu',
 N'Xác định mục tiêu buổi khơi gợi, nguồn thông tin, kỹ thuật và câu hỏi then chốt.', 20),
(N'EC', N'Elicitation and Collaboration',
 N'EC-4.2', N'Conduct Elicitation',
 N'Tiến hành khơi gợi',
 N'Thực hiện phỏng vấn/workshop, ghi nhận facts, assumptions và thông tin ẩn từ stakeholder.', 21),
(N'EC', N'Elicitation and Collaboration',
 N'EC-4.3', N'Confirm Elicitation Results',
 N'Xác nhận kết quả khơi gợi',
 N'Đối chiếu biên bản với stakeholder, làm rõ mâu thuẫn và xác thực bằng chứng.', 22),

-- RLCM · Requirements Life Cycle Management
(N'RLCM', N'Requirements Life Cycle Management',
 N'RLCM-5.1', N'Trace Requirements',
 N'Thiết lập truy vết yêu cầu',
 N'Liên kết nhu cầu → yêu cầu → thiết kế/kiểm thử; duy trì ma trận truy vết (RTM).', 30),
(N'RLCM', N'Requirements Life Cycle Management',
 N'RLCM-5.3', N'Prioritize Requirements',
 N'Ưu tiên hóa yêu cầu',
 N'Đánh giá giá trị, rủi ro, phụ thuộc để xếp ưu tiên backlog yêu cầu.', 31),
(N'RLCM', N'Requirements Life Cycle Management',
 N'RLCM-5.4', N'Assess Requirements Changes',
 N'Đánh giá thay đổi yêu cầu',
 N'Phân tích tác động khi phạm vi/biến cố thay đổi và đề xuất xử lý.', 32),

-- SA · Strategy Analysis
(N'SA', N'Strategy Analysis',
 N'SA-6.1', N'Analyze Current State',
 N'Phân tích hiện trạng (AS-IS)',
 N'Khảo sát bối cảnh, điểm nghẽn, quy trình và hạn chế của trạng thái hiện tại.', 40),
(N'SA', N'Strategy Analysis',
 N'SA-6.2', N'Define Future State',
 N'Định nghĩa trạng thái tương lai (TO-BE)',
 N'Xác định mục tiêu nghiệp vụ, phạm vi và tiêu chí thành công của trạng thái mong muốn.', 41),
(N'SA', N'Strategy Analysis',
 N'SA-6.3', N'Assess Risks',
 N'Đánh giá rủi ro chuyển đổi',
 N'Nhận diện rủi ro, giả định và ràng buộc ảnh hưởng chiến lược giải pháp.', 42),

-- RADD · Requirements Analysis and Design Definition
(N'RADD', N'Requirements Analysis and Design Definition',
 N'RADD-7.1', N'Specify and Model Requirements',
 N'Đặc tả và mô hình hóa yêu cầu',
 N'Phân tích, tổng hợp và đặc tả yêu cầu từ kết quả khảo sát stakeholder (BRD, model, story).', 50),
(N'RADD', N'Requirements Analysis and Design Definition',
 N'RADD-7.2', N'Verify Requirements',
 N'Xác minh yêu cầu',
 N'Đảm bảo yêu cầu đủ chất lượng: rõ, nhất quán, khả thi và có thể kiểm chứng.', 51),
(N'RADD', N'Requirements Analysis and Design Definition',
 N'RADD-7.5', N'Define Design Options',
 N'Định nghĩa phương án thiết kế',
 N'Đề xuất các phương án thiết kế khả thi đáp ứng nhu cầu nghiệp vụ đã phân tích.', 52),

-- SE · Solution Evaluation
(N'SE', N'Solution Evaluation',
 N'SE-8.1', N'Measure Solution Performance',
 N'Đo lường hiệu quả giải pháp',
 N'Thiết lập/đánh giá KPI, ROI và mức đáp ứng mục tiêu nghiệp vụ của giải pháp.', 60),
(N'SE', N'Solution Evaluation',
 N'SE-8.2', N'Analyze Performance Measures',
 N'Phân tích chỉ số hiệu quả',
 N'Giải thích khoảng cách giữa kỳ vọng và thực tế, nhận diện nguyên nhân gốc.', 61),
(N'SE', N'Solution Evaluation',
 N'SE-8.3', N'Assess Solution Limitations',
 N'Đánh giá hạn chế giải pháp',
 N'Xác định rào cản tổ chức/công nghệ và đề xuất hành động tối ưu giá trị.', 62);

MERGE [assessment].[BabokTasks] AS target
USING @Tasks AS src
    ON target.[BabokVersion] = N'v3' AND target.[Code] = src.[Code]
WHEN MATCHED THEN
    UPDATE SET
        target.[KnowledgeAreaCode] = src.[KnowledgeAreaCode],
        target.[KnowledgeAreaName] = src.[KnowledgeAreaName],
        target.[Name] = src.[Name],
        target.[Purpose] = src.[Purpose],
        target.[Description] = src.[Description],
        target.[DisplayOrder] = src.[DisplayOrder]
WHEN NOT MATCHED THEN
    INSERT ([KnowledgeAreaCode], [KnowledgeAreaName], [Code], [Name], [Purpose], [Description], [BabokVersion], [DisplayOrder])
    VALUES (src.[KnowledgeAreaCode], src.[KnowledgeAreaName], src.[Code], src.[Name], src.[Purpose], src.[Description], N'v3', src.[DisplayOrder]);
GO

-- Remap tiêu chí rubric hiện có sang đúng Knowledge Area (qua BabokTask)
DECLARE @Map TABLE (CriterionCode nvarchar(40) NOT NULL, TaskCode nvarchar(40) NOT NULL);
INSERT INTO @Map VALUES
(N'COMPLETENESS', N'RADD-7.1'),
(N'FEASIBILITY',  N'SA-6.3'),
(N'TRACEABILITY', N'RLCM-5.1'),
(N'CLARITY',      N'EC-4.3'),
(N'PLANNING',     N'BAPM-3.1'),
(N'VALUE',        N'SE-8.1');

UPDATE rc
SET rc.[BabokTaskId] = bt.[Id]
FROM [assessment].[RubricCriteria] rc
INNER JOIN @Map m ON m.[CriterionCode] = rc.[Code]
INNER JOIN [assessment].[BabokTasks] bt ON bt.[Code] = m.[TaskCode] AND bt.[BabokVersion] = N'v3';
GO

-- Bổ sung PLANNING (BAPM) + VALUE (SE) cho mọi rubric còn thiếu
DECLARE @BapmId int = (SELECT TOP 1 [Id] FROM [assessment].[BabokTasks] WHERE [Code] = N'BAPM-3.1');
DECLARE @SeId   int = (SELECT TOP 1 [Id] FROM [assessment].[BabokTasks] WHERE [Code] = N'SE-8.1');

IF @BapmId IS NOT NULL AND @SeId IS NOT NULL
BEGIN
    INSERT INTO [assessment].[RubricCriteria]
        ([RubricId], [BabokTaskId], [Code], [Name], [Description], [Weight], [MaxScore], [EvidenceGuidance], [DisplayOrder], [IsRequired])
    SELECT
        r.[Id], @BapmId, N'PLANNING',
        N'Lập kế hoạch BA & gắn kết stakeholder (Planning)',
        N'Đánh giá BAP, stakeholder map/RACI, phạm vi và cách tiếp cận phân tích.',
        0.15, 100,
        N'Tìm BAP ngắn, ma trận stakeholder, kế hoạch phỏng vấn trong artifact.',
        5, 1
    FROM [assessment].[Rubrics] r
    WHERE r.[Status] IN (N'Published', N'Draft')
      AND NOT EXISTS (
          SELECT 1 FROM [assessment].[RubricCriteria] rc
          WHERE rc.[RubricId] = r.[Id] AND rc.[Code] = N'PLANNING'
      );

    INSERT INTO [assessment].[RubricCriteria]
        ([RubricId], [BabokTaskId], [Code], [Name], [Description], [Weight], [MaxScore], [EvidenceGuidance], [DisplayOrder], [IsRequired])
    SELECT
        r.[Id], @SeId, N'VALUE',
        N'Giá trị giải pháp & tiêu chí thành công (Solution Value)',
        N'Đánh giá KPI/ROI, tiêu chí chấp nhận và mức đáp ứng mục tiêu nghiệp vụ.',
        0.15, 100,
        N'Tìm mục tiêu đo được, AC/KPI, đánh giá hạn chế hoặc khuyến nghị giá trị.',
        6, 1
    FROM [assessment].[Rubrics] r
    WHERE r.[Status] IN (N'Published', N'Draft')
      AND NOT EXISTS (
          SELECT 1 FROM [assessment].[RubricCriteria] rc
          WHERE rc.[RubricId] = r.[Id] AND rc.[Code] = N'VALUE'
      );
END
GO

PRINT N'>> BABOK v3 catalog seeded (6 Knowledge Areas) + rubric criteria remapped/extended.';
GO
