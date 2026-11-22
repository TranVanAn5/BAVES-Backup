---
type: index
title: FinNova - Tự động hóa Phê duyệt Khoản vay Tiêu dùng Trực tuyến & Chống Gian lận eKYC (v1)
description: Gói kịch bản OKF tự động hóa nền tảng khởi tạo hồ sơ vay số (Digital LOS) và động cơ ra quyết định tín dụng (Decision Engine) cho công ty tài chính FinNova.
tags: [finance, fintech, digital-lending, los, ekyc, credit-scoring, babok-v3]
baves:
  bundleVersion: 1
  scenarioCode: FIN-001
  industryCode: FINANCE
  industryName: Tài chính Tiêu dùng Số & FinTech
  industryDescription: Dịch vụ tài chính tiêu dùng, cho vay trực tuyến và chấm điểm tín dụng tự động
  applicationCode: DIGITAL_LOS
  applicationName: Digital Loan Origination & Automated Underwriting System
  applicationDescription: Hệ thống khởi tạo khoản vay số tích hợp eKYC, chấm điểm tín dụng và phê duyệt tự động
---
# FinNova v1

Dữ liệu doanh nghiệp và số liệu trong gói kịch bản này là giả lập phục vụ đào tạo Business Analysis.

## Scenario
- [Bối cảnh và nhiệm vụ](scenario.md)

## Stakeholders
| Code | Vai trò | Tên | File |
| :--- | :--- | :--- | :--- |
| `risk-lead` | Credit Risk Management Lead | Nguyễn Trí Dũng | [personas/risk-lead.md](personas/risk-lead.md) |
| `biz-director` | Digital Lending Business Director | Trần Thanh Sơn | [personas/biz-director.md](personas/biz-director.md) |
| `fraud-lead` | Underwriting & Fraud Prevention Lead | Lê Kim Oanh | [personas/fraud-lead.md](personas/fraud-lead.md) |
| `legal-mgr` | Legal & Compliance Manager | Phạm Quốc Huy | [personas/legal-mgr.md](personas/legal-mgr.md) |
| `arch-lead` | Fintech Enterprise Architect & CISO | Ngô Thanh Tùng | [personas/arch-lead.md](personas/arch-lead.md) |

## Knowledge Items
| Code | Tiêu đề | Phân loại | Tầm quan trọng | File |
| :--- | :--- | :--- | :--- | :--- |
| `K-FIN-01` | Kiến trúc Phễu Thẩm định Đa tầng (Multi-Tier Underwriting Funnel) | Process | Critical | [knowledge/K-FIN-01-multi-tier-funnel.md](knowledge/K-FIN-01-multi-tier-funnel.md) |
| `K-FIN-02` | Ma trận Phân tầng Quyết định Tín dụng (Credit Decision Matrix) | BusinessRule | Critical | [knowledge/K-FIN-02-credit-decision-matrix.md](knowledge/K-FIN-02-credit-decision-matrix.md) |
| `K-FIN-03` | Tiêu chuẩn Chống Giả mạo eKYC (Anti-Spoofing & Liveness Detection) | Security | High | [knowledge/K-FIN-03-anti-spoofing-liveness.md](knowledge/K-FIN-03-anti-spoofing-liveness.md) |
| `K-FIN-04` | Quy định Bắt buộc về Chữ ký số và Hợp đồng Vay Điện tử | Policy | High | [knowledge/K-FIN-04-electronic-loan-contract.md](knowledge/K-FIN-04-electronic-loan-contract.md) |
| `K-FIN-05` | Cơ chế Cắt cầu chì API khi Đối tác Gặp sự cố (Circuit Breaker - Ẩn) | System | Medium | [knowledge/K-FIN-05-circuit-breaker-fallback.md](knowledge/K-FIN-05-circuit-breaker-fallback.md) |

## Business Rules
- `BR-FIN-01`: Độ tuổi và tỷ lệ nợ trên thu nhập (DTI) tối đa
- `BR-FIN-02`: Quy tắc loại trừ nợ xấu lịch sử CIC
- `BR-FIN-03`: Quy tắc đồng nhất thông tin định danh và tài khoản ngân hàng thụ hưởng
- `BR-FIN-04`: Hạn mức phê duyệt theo phân khúc điểm tín dụng
- `BR-FIN-05`: Khoảng thời gian giãn cách nộp lại hồ sơ sau khi bị từ chối
