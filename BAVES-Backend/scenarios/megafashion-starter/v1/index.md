---
type: index
title: "MegaFashion Starter — Làm rõ điều kiện voucher cơ bản (v1)"
description: "Beginner (trả phí): khảo sát điều kiện voucher GIẢM50K, chỗ lệch banner vs hệ thống, và viết AC cơ bản."
tags: [beginner, starter, promo, voucher, babok-v3]
baves:
  bundleVersion: 1
  scenarioCode: BEG-PROMO-01
  industryCode: RETAIL
  industryName: "Bán lẻ & Thời trang"
  applicationCode: LMS_PROMO
  applicationName: "Customer Loyalty & Promotion Engine"
  isFreeAccess: false
---
# megafashion-starter v1

Bundle Beginner OKF đầy đủ — nguồn markdown để import vào database. Dữ liệu doanh nghiệp là giả lập phục vụ đào tạo BA.

## Scenario
- [Bối cảnh và nhiệm vụ](scenario.md)

## Stakeholders
| Code | Vai trò | Tên | File |
| --- | --- | --- | --- |
| `marketing-ops` | Marketing Ops | Vũ Thanh Tâm | [personas/marketing-ops.md](personas/marketing-ops.md) |
| `pos-supervisor` | POS Supervisor | Lý Quang Dũng | [personas/pos-supervisor.md](personas/pos-supervisor.md) |
| `cx-specialist` | CX Specialist | Phan Ngọc Mai | [personas/cx-specialist.md](personas/cx-specialist.md) |

## Knowledge
| Code | Tiêu đề | Phân loại | Tầm quan trọng | File |
| --- | --- | --- | --- | --- |
| `K-BEG-PROMO-01` | Cấu hình voucher GIẢM50K (as-is) | Policy | High | [knowledge/K-BEG-PROMO-01-voucher-rules.md](knowledge/K-BEG-PROMO-01-voucher-rules.md) |
| `K-BEG-PROMO-02` | Cơ cấu khiếu nại mã GIẢM50K | BusinessFact | High | [knowledge/K-BEG-PROMO-02-complaint-mix.md](knowledge/K-BEG-PROMO-02-complaint-mix.md) |
| `K-BEG-PROMO-03` | Quy ước không cộng dồn với FREESHIP | Process | Medium | [knowledge/K-BEG-PROMO-03-stacking.md](knowledge/K-BEG-PROMO-03-stacking.md) |
| `K-BEG-PROMO-04` | Độ trễ đồng bộ rule POS (cần elicit) | System | Medium | [knowledge/K-BEG-PROMO-04-pos-cache.md](knowledge/K-BEG-PROMO-04-pos-cache.md) |

## Rules
| Code | Tiêu đề | Tầm quan trọng | File |
| --- | --- | --- | --- |
| `BR-BEG-PROMO-01` | Đơn tối thiểu áp dụng GIẢM50K | High | [rules/BR-BEG-PROMO-01-min-order.md](rules/BR-BEG-PROMO-01-min-order.md) |
| `BR-BEG-PROMO-02` | Không cộng dồn với FREESHIP | High | [rules/BR-BEG-PROMO-02-no-stack.md](rules/BR-BEG-PROMO-02-no-stack.md) |
| `BR-BEG-PROMO-03` | Giới hạn 1 mã mỗi tài khoản mỗi ngày | Medium | [rules/BR-BEG-PROMO-03-daily-cap.md](rules/BR-BEG-PROMO-03-daily-cap.md) |

## Events
| File | Tiêu đề |
| --- | --- |
| [events/E1-banner-vs-pos-mismatch.md](events/E1-banner-vs-pos-mismatch.md) | Lệch banner và POS trong 30 phút sau khi siết danh mục Áo |

## Rubric
- [Rubric Beginner — MegaFashion Starter (BEG-PROMO-01)](rubric/rubric.md)

## Quy ước
- Frontmatter YAML: `type`, `title`, `description`, `tags`; trường BA-VES nằm dưới `baves:`.
- Persona: `baves.public` hiển thị cho Learner; `baves.private` chỉ gửi AI.
- Knowledge: quyền truy cập khai báo ở `baves.access[]`.
- Beginner scope: BAP ngắn, stakeholder map, interview notes, as-is/to-be, 3 user stories + AC — không yêu cầu BRD/SRS/RTM đầy đủ.
