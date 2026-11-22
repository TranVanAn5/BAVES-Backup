---
title: Bảng tiêu chí đánh giá tài liệu BRD OmniMart v1
description: Rubric chuẩn đánh giá chất lượng tài liệu Yêu cầu nghiệp vụ của học viên theo chuẩn BABOK v3.
passingScore: 60.0
criteria:
  - code: COMPLETENESS
    name: Tính đầy đủ và bao quát phạm vi (Completeness)
    description: Đánh giá xem tài liệu có giải quyết đầy đủ bài toán tồn kho, đồng bộ POS, và quy trình xử lý đơn hàng hay không.
    weight: 1.0
    maxScore: 100.0
  - code: FEASIBILITY
    name: Tính khả thi và đúng quy tắc nghiệp vụ (Feasibility & Correctness)
    description: Giải pháp có tuân thủ 8 Business Rules của OmniMart không? Có phù hợp với giới hạn kỹ thuật của hệ thống POS/ERP không?
    weight: 1.0
    maxScore: 100.0
  - code: TRACEABILITY
    name: Tính truy vết và bằng chứng phỏng vấn (Traceability & Evidence)
    description: Các yêu cầu (Requirements) có được dẫn nguồn từ biên bản phỏng vấn 3 Stakeholders (IT Lead, Ops Manager, Sales Lead) không?
    weight: 1.0
    maxScore: 100.0
  - code: CLARITY
    name: Tính rõ ràng, mạch lạc và phi mâu thuẫn (Clarity & Consistency)
    description: Ngôn từ rõ ràng, có tiêu chí nghiệm thu định lượng, không chứa thuật ngữ mập mờ hoặc câu từ mâu thuẫn lẫn nhau.
    weight: 1.0
    maxScore: 100.0
---
# Bảng Tiêu Chí Chấm Điểm BRD (Rubric)

Rubric này được thiết kế để AI Assessor và Chuyên gia (Expert) sử dụng chung nhằm đánh giá bài nộp của học viên trong kịch bản OmniMart.

## 1. Thang điểm đánh giá từng tiêu chí:
- **Xuất sắc (85 - 100 điểm)**: Đáp ứng toàn diện, phân tích sâu sắc, có dẫn chứng vững chắc từ lời thoại của Stakeholders.
- **Đạt yêu cầu (60 - 84 điểm)**: Đầy đủ các phần cơ bản, còn một vài thiếu sót nhỏ ở các trường hợp ngoại lệ.
- **Chưa đạt (< 60 điểm)**: Thiếu phần quan trọng, mâu thuẫn với quy tắc kinh doanh, hoặc bịa đặt yêu cầu không có căn cứ.
