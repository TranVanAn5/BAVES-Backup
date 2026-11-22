---
title: Bảng tiêu chí đánh giá tài liệu BRD/SRS MegaFashion v1
description: Rubric chuẩn đánh giá chất lượng tài liệu Yêu cầu nghiệp vụ của học viên theo chuẩn BABOK v3 cho bài toán Promotion Engine & Loyalty đa kênh.
passingScore: 60.0
criteria:
  - code: COMPLETENESS
    name: Tính đầy đủ và bao quát phạm vi (Completeness)
    description: Đánh giá xem tài liệu có giải quyết trọn vẹn vòng đời voucher (tạo, hold, commit, expire, clawback), tích hợp POS đa kênh và luồng đổi trả hàng hay không.
    weight: 1.0
    maxScore: 100.0
    isRequired: true
    babokTaskCode: RADD-7.1
  - code: FEASIBILITY
    name: Tính khả thi và đúng quy tắc nghiệp vụ (Feasibility & Correctness)
    description: Giải pháp có tuân thủ chặt chẽ 5 Business Rules của MegaFashion không? Có mô hình hóa chính xác công thức phân bổ chiết khấu theo dòng hàng (Line-item allocation) không?
    weight: 1.0
    maxScore: 100.0
    isRequired: true
    babokTaskCode: RADD-7.1
  - code: TRACEABILITY
    name: Tính truy vết và bằng chứng phỏng vấn (Traceability & Evidence)
    description: Các yêu cầu phần mềm có dẫn chứng rõ ràng từ ý kiến của 4 Stakeholders (Marketing, Tài chính, Vận hành cửa hàng, CNTT) không?
    weight: 1.0
    maxScore: 100.0
    isRequired: true
    babokTaskCode: RLCM-5.1
  - code: CLARITY
    name: Tính rõ ràng, mạch lạc và phi mâu thuẫn (Clarity & Consistency)
    description: Ngôn từ rõ ràng, bảng biểu logic, có Acceptance Criteria định lượng rõ ràng, không chứa mâu thuẫn giữa nghiệp vụ kế toán và công nghệ.
    weight: 1.0
    maxScore: 100.0
    isRequired: true
    babokTaskCode: RADD-7.1
---
# Bảng Tiêu Chí Chấm Điểm BRD/SRS MegaFashion

Rubric này dùng để AI Assessor và Chuyên gia chấm điểm các tài liệu phân tích nghiệp vụ của học viên trong kịch bản MegaFashion (RET-001).
