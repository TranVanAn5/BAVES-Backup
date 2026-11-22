---
type: index
title: "OmniMart Starter — Theo dõi trạng thái đơn hàng cơ bản (v1)"
description: "Beginner: học viên làm quen BA qua bài toán chuẩn hóa trạng thái đơn hàng giữa website và cửa hàng, luyện phỏng vấn stakeholder và viết user story cơ bản."
tags: [beginner, starter, oms, order-status, babok-v3]
baves:
  bundleVersion: 1
  scenarioCode: BEG-OMS-01
  industryCode: SUPPLY_CHAIN
  industryName: "Chuỗi cung ứng & Bán lẻ"
  applicationCode: OMS
  applicationName: "Order Management System"
  isFreeAccess: true
---
# omnimart-starter v1

Bundle Beginner OKF đầy đủ — nguồn markdown để import vào database. Dữ liệu doanh nghiệp là giả lập phục vụ đào tạo BA.

## Scenario
- [Bối cảnh và nhiệm vụ](scenario.md)

## Stakeholders
| Code | Vai trò | Tên | File |
| --- | --- | --- | --- |
| `ops-coordinator` | Operations Coordinator | Phạm Thu Hà | [personas/ops-coordinator.md](personas/ops-coordinator.md) |
| `cskh-lead` | CSKH Lead | Nguyễn Minh Anh | [personas/cskh-lead.md](personas/cskh-lead.md) |
| `store-staff` | Store Staff | Trần Quốc Bảo | [personas/store-staff.md](personas/store-staff.md) |

## Knowledge
| Code | Tiêu đề | Phân loại | Tầm quan trọng | File |
| --- | --- | --- | --- | --- |
| `K-BEG-OMS-01` | Danh mục trạng thái đơn hiện tại (as-is) | Process | High | [knowledge/K-BEG-OMS-01-order-status.md](knowledge/K-BEG-OMS-01-order-status.md) |
| `K-BEG-OMS-02` | Khối lượng ticket hỏi trạng thái đơn | BusinessFact | High | [knowledge/K-BEG-OMS-02-ticket-volume.md](knowledge/K-BEG-OMS-02-ticket-volume.md) |
| `K-BEG-OMS-03` | Quy ước SLA cập nhật trạng thái nội bộ | Policy | Medium | [knowledge/K-BEG-OMS-03-update-sla.md](knowledge/K-BEG-OMS-03-update-sla.md) |
| `K-BEG-OMS-04` | Thói quen cập nhật trễ tại cửa hàng (cần elicit) | BusinessFact | Medium | [knowledge/K-BEG-OMS-04-late-update-habit.md](knowledge/K-BEG-OMS-04-late-update-habit.md) |

## Rules
| Code | Tiêu đề | Tầm quan trọng | File |
| --- | --- | --- | --- |
| `BR-BEG-OMS-01` | Danh mục trạng thái đơn chuẩn dùng chung | High | [rules/BR-BEG-OMS-01-status-catalog.md](rules/BR-BEG-OMS-01-status-catalog.md) |
| `BR-BEG-OMS-02` | Trách nhiệm cập nhật theo trạng thái | Medium | [rules/BR-BEG-OMS-02-update-owner.md](rules/BR-BEG-OMS-02-update-owner.md) |
| `BR-BEG-OMS-03` | Trạng thái hiển thị cho khách trên app | Medium | [rules/BR-BEG-OMS-03-customer-visible.md](rules/BR-BEG-OMS-03-customer-visible.md) |

## Events
| File | Tiêu đề |
| --- | --- |
| [events/E1-status-mismatch-spike.md](events/E1-status-mismatch-spike.md) | Tăng đột biến ticket hỏi trạng thái sau đợt đồng bộ tên trạng thái thử |

## Rubric
- [Rubric Beginner — OmniMart Starter (BEG-OMS-01)](rubric/rubric.md)

## Quy ước
- Frontmatter YAML: `type`, `title`, `description`, `tags`; trường BA-VES nằm dưới `baves:`.
- Persona: `baves.public` hiển thị cho Learner; `baves.private` chỉ gửi AI.
- Knowledge: quyền truy cập khai báo ở `baves.access[]`.
- Beginner scope: BAP ngắn, stakeholder map, interview notes, as-is/to-be, 3 user stories + AC — không yêu cầu BRD/SRS/RTM đầy đủ.
