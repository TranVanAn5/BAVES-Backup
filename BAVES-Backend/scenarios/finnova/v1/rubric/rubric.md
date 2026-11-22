---
title: Bảng tiêu chí đánh giá tài liệu BRD/SRS FinNova v1
description: Rubric chuẩn đánh giá chất lượng tài liệu Yêu cầu nghiệp vụ của học viên theo chuẩn BABOK v3 cho bài toán Digital LOS và phòng chống gian lận FinTech.
passingScore: 60.0
criteria:
  - code: COMPLETENESS
    name: Tính đầy đủ và bao quát phạm vi (Completeness)
    description: Đánh giá xem tài liệu có giải quyết trọn vẹn luồng Onboarding, Phễu thẩm định 4 tầng, eKYC Liveness, Ma trận quyết định tín dụng, ký hợp đồng điện tử và giải ngân hay không.
    weight: 1.0
    maxScore: 100.0
    isRequired: true
    babokTaskCode: RADD-7.1
  - code: FEASIBILITY
    name: Tính khả thi kỹ thuật và an toàn tài chính (Feasibility & Security)
    description: Giải pháp có tuân thủ chặt chẽ 5 Business Rules của FinNova không? Có thiết kế cơ chế Circuit Breaker và tiêu chuẩn chống Deepfake khả thi không?
    weight: 1.0
    maxScore: 100.0
    isRequired: true
    babokTaskCode: RADD-7.1
  - code: TRACEABILITY
    name: Tính truy vết và cân bằng lợi ích (Traceability & Balance)
    description: Các yêu cầu phần mềm có dẫn chứng rõ ràng từ ý kiến của 5 Stakeholders (Rủi ro, Kinh doanh, Chống gian lận, Pháp chế, Kiến trúc IT) và dung hòa được mâu thuẫn giữa tốc độ và kiểm soát không?
    weight: 1.0
    maxScore: 100.0
    isRequired: true
    babokTaskCode: RLCM-5.1
  - code: CLARITY
    name: Tính rõ ràng và tuân thủ pháp lý (Clarity & Compliance)
    description: Ngôn từ chuẩn mực tài chính, tiêu chí nghiệm thu định lượng, tuân thủ nghiêm ngặt các quy định về bảo vệ dữ liệu PDPD và chữ ký hợp đồng số.
    weight: 1.0
    maxScore: 100.0
    isRequired: true
    babokTaskCode: RADD-7.1
---
# Bảng Tiêu Chí Chấm Điểm BRD/SRS FinNova

Rubric này dùng để AI Assessor và Chuyên gia chấm điểm các tài liệu phân tích nghiệp vụ của học viên trong kịch bản FinNova (FIN-001).
