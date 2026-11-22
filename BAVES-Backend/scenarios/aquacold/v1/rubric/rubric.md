---
title: Bảng tiêu chí đánh giá tài liệu BRD/SRS AquaCold v1
description: Rubric chuẩn đánh giá chất lượng tài liệu Yêu cầu nghiệp vụ của học viên theo chuẩn BABOK v3 cho bài toán WMS và chuỗi cung ứng lạnh 3PL.
passingScore: 60.0
criteria:
  - code: COMPLETENESS
    name: Tính đầy đủ và bao quát phạm vi (Completeness)
    description: Đánh giá xem tài liệu có giải quyết trọn vẹn luồng Inbound, Staging Dwell Time, Directed Putaway, FEFO, Cross-docking và giao vận chặng cuối có kiểm soát nhiệt độ hay không.
    weight: 1.0
    maxScore: 100.0
    isRequired: true
    babokTaskCode: RADD-7.1
  - code: FEASIBILITY
    name: Tính khả thi kỹ thuật và tiêu chuẩn chất lượng (Feasibility & Compliance)
    description: Giải pháp có tuân thủ chặt chẽ 5 Business Rules của AquaCold không? Có giải quyết được bài toán suy hao sóng trong hầm lạnh và giới hạn thời gian phơi nhiệt không?
    weight: 1.0
    maxScore: 100.0
    isRequired: true
    babokTaskCode: RADD-7.1
  - code: TRACEABILITY
    name: Tính truy vết và bằng chứng phỏng vấn (Traceability & Evidence)
    description: Các yêu cầu phần mềm có dẫn chứng rõ ràng từ ý kiến và nỗi đau của 4 Stakeholders (DC Director, QA Lead, Fleet Manager, IoT Engineer) không?
    weight: 1.0
    maxScore: 100.0
    isRequired: true
    babokTaskCode: RLCM-5.1
  - code: CLARITY
    name: Tính rõ ràng và giải quyết mâu thuẫn (Clarity & Conflict Resolution)
    description: Giải quyết thấu đáo mâu thuẫn giữa tốc độ bốc dỡ của vận hành và tiêu chuẩn khắt khe của QA, ngôn từ mạch lạc, có Acceptance Criteria định lượng rõ ràng.
    weight: 1.0
    maxScore: 100.0
    isRequired: true
    babokTaskCode: RADD-7.1
---
# Bảng Tiêu Chí Chấm Điểm BRD/SRS AquaCold

Rubric này dùng để AI Assessor và Chuyên gia chấm điểm các tài liệu phân tích nghiệp vụ của học viên trong kịch bản AquaCold (SCM-001).
