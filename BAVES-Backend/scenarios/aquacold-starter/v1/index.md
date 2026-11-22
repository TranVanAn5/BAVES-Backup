---
type: index
title: "AquaCold Starter — Theo dõi nhiệt độ kho lạnh cơ bản (v1)"
description: "Beginner: làm quen BA trong cold-chain WMS — khảo sát cảnh báo nhiệt độ, phân mức nghiêm trọng và nhật ký sự cố tối thiểu."
tags: [beginner, starter, wms, cold-chain, babok-v3]
baves:
  bundleVersion: 1
  scenarioCode: BEG-WMS-01
  industryCode: SUPPLY_CHAIN
  industryName: "Chuỗi cung ứng & Logistics Lạnh"
  applicationCode: WMS_COLD
  applicationName: "Cold Chain Warehouse Management System"
  isFreeAccess: true
---
# aquacold-starter v1

Bundle Beginner OKF đầy đủ — nguồn markdown để import vào database. Dữ liệu doanh nghiệp là giả lập phục vụ đào tạo BA.

## Scenario
- [Bối cảnh và nhiệm vụ](scenario.md)

## Stakeholders
| Code | Vai trò | Tên | File |
| --- | --- | --- | --- |
| `warehouse-supervisor` | Warehouse Supervisor | Đỗ Văn Khoa | [personas/warehouse-supervisor.md](personas/warehouse-supervisor.md) |
| `qa-assistant` | QA Assistant | Lê Thu Trang | [personas/qa-assistant.md](personas/qa-assistant.md) |
| `night-shift-lead` | Night Shift Lead | Phạm Hoàng Nam | [personas/night-shift-lead.md](personas/night-shift-lead.md) |

## Knowledge
| Code | Tiêu đề | Phân loại | Tầm quan trọng | File |
| --- | --- | --- | --- | --- |
| `K-BEG-WMS-01` | Ngưỡng nhiệt độ kho đông hiện hành | Policy | High | [knowledge/K-BEG-WMS-01-temp-thresholds.md](knowledge/K-BEG-WMS-01-temp-thresholds.md) |
| `K-BEG-WMS-02` | Luồng nhận cảnh báo và phản ứng as-is | Process | High | [knowledge/K-BEG-WMS-02-alert-flow.md](knowledge/K-BEG-WMS-02-alert-flow.md) |
| `K-BEG-WMS-03` | Trường nhật ký sự cố QA mong muốn | System | Medium | [knowledge/K-BEG-WMS-03-incident-fields.md](knowledge/K-BEG-WMS-03-incident-fields.md) |
| `K-BEG-WMS-04` | Thói quen im lặng thông báo ca đêm (cần elicit) | BusinessFact | Medium | [knowledge/K-BEG-WMS-04-night-mute.md](knowledge/K-BEG-WMS-04-night-mute.md) |

## Rules
| Code | Tiêu đề | Tầm quan trọng | File |
| --- | --- | --- | --- |
| `BR-BEG-WMS-01` | Phân mức cảnh báo nhiệt độ kho đông | High | [rules/BR-BEG-WMS-01-alert-levels.md](rules/BR-BEG-WMS-01-alert-levels.md) |
| `BR-BEG-WMS-02` | SLA acknowledge cảnh báo | High | [rules/BR-BEG-WMS-02-acknowledge-sla.md](rules/BR-BEG-WMS-02-acknowledge-sla.md) |
| `BR-BEG-WMS-03` | Bắt buộc nhật ký sau Warning/Critical | Medium | [rules/BR-BEG-WMS-03-incident-log-required.md](rules/BR-BEG-WMS-03-incident-log-required.md) |

## Events
| File | Tiêu đề |
| --- | --- |
| [events/E1-warning-missed-night.md](events/E1-warning-missed-night.md) | Bỏ lỡ một cảnh báo Warning lúc 02:10 ca đêm |

## Rubric
- [Rubric Beginner — AquaCold Starter (BEG-WMS-01)](rubric/rubric.md)

## Quy ước
- Frontmatter YAML: `type`, `title`, `description`, `tags`; trường BA-VES nằm dưới `baves:`.
- Persona: `baves.public` hiển thị cho Learner; `baves.private` chỉ gửi AI.
- Knowledge: quyền truy cập khai báo ở `baves.access[]`.
- Beginner scope: BAP ngắn, stakeholder map, interview notes, as-is/to-be, 3 user stories + AC — không yêu cầu BRD/SRS/RTM đầy đủ.
