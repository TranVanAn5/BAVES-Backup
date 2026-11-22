---
type: index
title: OmniMart - Cải tiến xử lý đơn hàng đa kênh (v1)
description: Bundle OKF của scenario demo BA-VES. Là file nguồn để import vào database, runtime không đọc trực tiếp.
tags: [retail, order-management, omnimart]
baves:
  bundleVersion: 1
  scenarioCode: OMNI-001
  industry: Retail
  application: Order Management
---
# OmniMart v1

Dữ liệu doanh nghiệp và số liệu trong bundle này là giả lập.

## Scenario
- [Bối cảnh và nhiệm vụ](scenario.md)

## Stakeholder
| Code | Vai trò | Tên | File |
| --- | --- | --- | --- |
| `ops-manager` | Operations Manager | Trần Minh Quân | [personas/ops-manager.md](personas/ops-manager.md) |
| `sales-lead` | Sales / Customer Service Lead | Lê Hoàng Yến | [personas/sales-lead.md](personas/sales-lead.md) |
| `it-lead` | IT Lead | Nguyễn Quốc Tuấn | [personas/it-lead.md](personas/it-lead.md) |

## Knowledge
| Code | Tiêu đề | Phân loại | Tầm quan trọng | File |
| --- | --- | --- | --- | --- |
| `K-OPS-01` | Thời điểm giữ tồn kho cho đơn online | Process | High | [knowledge/K-OPS-01-inventory-hold.md](knowledge/K-OPS-01-inventory-hold.md) |
| `K-OPS-02` | Hạn mức phê duyệt hoàn tiền (Ẩn) | Policy | Critical | [knowledge/K-OPS-02-refund-approval.md](knowledge/K-OPS-02-refund-approval.md) |
| `K-SAL-01` | Kênh và quy tắc thông báo trạng thái đơn hàng | Process | High | [knowledge/K-SAL-01-order-notification.md](knowledge/K-SAL-01-order-notification.md) |
| `K-IT-01` | Cơ chế đồng bộ tồn kho và giới hạn kỹ thuật POS | System | Critical | [knowledge/K-IT-01-pos-sync-limit.md](knowledge/K-IT-01-pos-sync-limit.md) |

## Quy ước
- Mỗi file có frontmatter YAML. Trường chuẩn OKF: `type`, `title`, `description`, `tags`. Trường riêng của BA-VES nằm dưới `baves:`.
- `type` dùng trong bundle: `index`, `scenario`, `persona`, `knowledge`, `business-rule`, `event`.
- Tham chiếu chéo giữa các file bằng **code** (ví dụ `ops-manager`, `K-OPS-01`), không dùng id database.
- Persona: trường dưới `baves.public` được hiển thị cho Learner; trường dưới `baves.private` chỉ gửi cho AI.
- Knowledge: quyền truy cập khai báo ở `baves.access[]`, ánh xạ sang bảng `StakeholderKnowledgeAccess`.
- Phần thân markdown của file knowledge là `KnowledgeItem.Content`.
