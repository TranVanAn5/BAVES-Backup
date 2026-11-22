---
type: index
title: "FinNova Starter — Làm rõ bước eKYC khoản vay tiêu dùng (v1)"
description: "Beginner (trả phí): khảo sát luồng eKYC cơ bản trong Digital LOS, taxonomy lý do hold và user story màn hình Ops."
tags: [beginner, starter, los, ekyc, babok-v3]
baves:
  bundleVersion: 1
  scenarioCode: BEG-LOS-01
  industryCode: FINANCE
  industryName: "Tài chính Tiêu dùng Số & FinTech"
  applicationCode: DIGITAL_LOS
  applicationName: "Digital Loan Origination & Automated Underwriting System"
  isFreeAccess: false
---
# finnova-starter v1

Bundle Beginner OKF đầy đủ — nguồn markdown để import vào database. Dữ liệu doanh nghiệp là giả lập phục vụ đào tạo BA.

## Scenario
- [Bối cảnh và nhiệm vụ](scenario.md)

## Stakeholders
| Code | Vai trò | Tên | File |
| --- | --- | --- | --- |
| `credit-ops` | Credit Ops Officer | Ngô Minh Châu | [personas/credit-ops.md](personas/credit-ops.md) |
| `onboarding-specialist` | Onboarding Specialist | Hoàng Nhật Lê | [personas/onboarding-specialist.md](personas/onboarding-specialist.md) |
| `compliance-junior` | Compliance Junior | Đặng Thu Hà | [personas/compliance-junior.md](personas/compliance-junior.md) |

## Knowledge
| Code | Tiêu đề | Phân loại | Tầm quan trọng | File |
| --- | --- | --- | --- | --- |
| `K-BEG-LOS-01` | Top lý do hold eKYC tuần gần nhất | BusinessFact | High | [knowledge/K-BEG-LOS-01-hold-reasons.md](knowledge/K-BEG-LOS-01-hold-reasons.md) |
| `K-BEG-LOS-02` | Năm bước eKYC as-is | Process | High | [knowledge/K-BEG-LOS-02-ekyc-steps.md](knowledge/K-BEG-LOS-02-ekyc-steps.md) |
| `K-BEG-LOS-03` | Khoảng trống UI hàng đợi hold | System | Medium | [knowledge/K-BEG-LOS-03-ops-ui-gap.md](knowledge/K-BEG-LOS-03-ops-ui-gap.md) |
| `K-BEG-LOS-04` | Nhóm hold chưa phân loại và rủi ro tuân thủ (cần elicit) | Policy | Medium | [knowledge/K-BEG-LOS-04-unclassified-hold.md](knowledge/K-BEG-LOS-04-unclassified-hold.md) |

## Rules
| Code | Tiêu đề | Tầm quan trọng | File |
| --- | --- | --- | --- |
| `BR-BEG-LOS-01` | Taxonomy mã lý do hold eKYC | High | [rules/BR-BEG-LOS-01-hold-taxonomy.md](rules/BR-BEG-LOS-01-hold-taxonomy.md) |
| `BR-BEG-LOS-02` | Bắt buộc ghi chú khi chọn OTHER | High | [rules/BR-BEG-LOS-02-other-note-required.md](rules/BR-BEG-LOS-02-other-note-required.md) |
| `BR-BEG-LOS-03` | SLA xử lý hồ sơ hold | Medium | [rules/BR-BEG-LOS-03-sla-hold.md](rules/BR-BEG-LOS-03-sla-hold.md) |

## Events
| File | Tiêu đề |
| --- | --- |
| [events/E1-hold-reason-blank-spike.md](events/E1-hold-reason-blank-spike.md) | Hàng loạt hồ sơ hold thiếu lý do sau đợt cập nhật vendor |

## Rubric
- [Rubric Beginner — FinNova Starter (BEG-LOS-01)](rubric/rubric.md)

## Quy ước
- Frontmatter YAML: `type`, `title`, `description`, `tags`; trường BA-VES nằm dưới `baves:`.
- Persona: `baves.public` hiển thị cho Learner; `baves.private` chỉ gửi AI.
- Knowledge: quyền truy cập khai báo ở `baves.access[]`.
- Beginner scope: BAP ngắn, stakeholder map, interview notes, as-is/to-be, 3 user stories + AC — không yêu cầu BRD/SRS/RTM đầy đủ.
