---
type: index
title: MegaFashion - Chuẩn hóa Động cơ Khuyến mãi & Điểm thưởng Đa kênh (v1)
description: Gói kịch bản OKF chuẩn hóa Promo Engine và hệ thống Loyalty cho chuỗi bán lẻ thời trang MegaFashion.
tags: [retail, loyalty, promotion-engine, omni-channel, babok-v3]
baves:
  bundleVersion: 1
  scenarioCode: RET-001
  industryCode: RETAIL
  industryName: Bán lẻ & Thời trang
  industryDescription: Bán lẻ thời trang đa kênh, chuỗi cửa hàng và thương mại điện tử
  applicationCode: LMS_PROMO
  applicationName: Customer Loyalty & Promotion Engine
  applicationDescription: Hệ thống quản lý khách hàng thân thiết và động cơ khuyến mãi đa kênh
---
# MegaFashion v1

Dữ liệu doanh nghiệp và số liệu trong gói kịch bản này là giả lập phục vụ đào tạo Business Analysis.

## Scenario
- [Bối cảnh và nhiệm vụ](scenario.md)

## Stakeholders
| Code | Vai trò | Tên | File |
| :--- | :--- | :--- | :--- |
| `mkt-lead` | Head of Marketing & Loyalty | Vũ Bích Ngọc | [personas/mkt-lead.md](personas/mkt-lead.md) |
| `fin-manager` | Chief Accountant / Finance Manager | Đặng Văn Bình | [personas/fin-manager.md](personas/fin-manager.md) |
| `store-ops` | Retail Store Operations Director | Lê Trọng Nghĩa | [personas/store-ops.md](personas/store-ops.md) |
| `it-lead` | IT Integration Lead | Phạm Thế Anh | [personas/it-lead.md](personas/it-lead.md) |

## Knowledge Items
| Code | Tiêu đề | Phân loại | Tầm quan trọng | File |
| :--- | :--- | :--- | :--- | :--- |
| `K-RET-01` | Cơ chế Khóa tạm Voucher (Two-Phase Voucher Hold) | System | Critical | [knowledge/K-RET-01-voucher-hold.md](knowledge/K-RET-01-voucher-hold.md) |
| `K-RET-02` | Quy tắc Phân bổ Chiết khấu theo Dòng hàng (Line-Item Allocation) | Policy | High | [knowledge/K-RET-02-line-item-allocation.md](knowledge/K-RET-02-line-item-allocation.md) |
| `K-RET-03` | Quy định Thu hồi Điểm và Giá trị Tích lũy khi Trả hàng | Policy | High | [knowledge/K-RET-03-point-clawback.md](knowledge/K-RET-03-point-clawback.md) |
| `K-RET-04` | Giới hạn Cấp phép Ngoại tuyến POS (Offline Fallback Limit - Ẩn) | System | Critical | [knowledge/K-RET-04-offline-fallback-limit.md](knowledge/K-RET-04-offline-fallback-limit.md) |
| `K-RET-05` | Cơ chế Chống tấn công Brute-force mã Voucher | Security | Medium | [knowledge/K-RET-05-brute-force-protection.md](knowledge/K-RET-05-brute-force-protection.md) |

## Business Rules
- `BR-RET-01`: Thời gian giữ voucher giỏ hàng
- `BR-RET-02`: Nguyên tắc cấm cộng dồn voucher
- `BR-RET-03`: Tỷ lệ quy đổi và trần thanh toán điểm thưởng
- `BR-RET-04`: Hạn mức phê duyệt chiết khấu tại chỗ
- `BR-RET-05`: Quy tắc làm tròn hoàn tiền sau phân bổ chiết khấu
