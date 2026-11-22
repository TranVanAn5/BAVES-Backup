---
type: index
title: AquaCold - Tối ưu hóa Vận hành Kho Lạnh & Giám sát Chuỗi Cung ứng (v1)
description: Gói kịch bản OKF tối ưu hóa WMS kho lạnh và giám sát chuỗi cung ứng nhạy cảm nhiệt độ cho dịch vụ hậu cần 3PL AquaCold.
tags: [supply-chain, cold-chain, logistics, wms, iot, babok-v3]
baves:
  bundleVersion: 1
  scenarioCode: SCM-001
  industryCode: SUPPLY_CHAIN
  industryName: Chuỗi cung ứng & Logistics Lạnh
  industryDescription: Vận hành kho lạnh 3PL, chuỗi cung ứng lạnh thực phẩm & dược phẩm
  applicationCode: WMS_COLD
  applicationName: Cold Chain Warehouse Management System
  applicationDescription: Hệ thống quản lý kho thông minh tích hợp giám sát nhiệt độ IoT và Cross-docking
---
# AquaCold v1

Dữ liệu doanh nghiệp và số liệu trong gói kịch bản này là giả lập phục vụ đào tạo Business Analysis.

## Scenario
- [Bối cảnh và nhiệm vụ](scenario.md)

## Stakeholders
| Code | Vai trò | Tên | File |
| :--- | :--- | :--- | :--- |
| `dc-director` | DC Operations Director | Hoàng Minh Đức | [personas/dc-director.md](personas/dc-director.md) |
| `qa-lead` | QA/QC & Compliance Lead | Trần Thị Mai Lan | [personas/qa-lead.md](personas/qa-lead.md) |
| `fleet-manager` | Fleet & Transport Manager | Nguyễn Văn Hùng | [personas/fleet-manager.md](personas/fleet-manager.md) |
| `iot-lead` | WMS & IoT Systems Engineer | Lê Đăng Khoa | [personas/iot-lead.md](personas/iot-lead.md) |

## Knowledge Items
| Code | Tiêu đề | Phân loại | Tầm quan trọng | File |
| :--- | :--- | :--- | :--- | :--- |
| `K-SCM-01` | Tiêu chuẩn Thời gian Phơi nhiệt tại Sàn đệm (Staging Dwell Time) | Process | Critical | [knowledge/K-SCM-01-staging-dwell-time.md](knowledge/K-SCM-01-staging-dwell-time.md) |
| `K-SCM-02` | Quy tắc Cách ly Lô hàng Vi phạm Nhiệt độ (Automated QC Lock) | Policy | Critical | [knowledge/K-SCM-02-qc-temperature-lock.md](knowledge/K-SCM-02-qc-temperature-lock.md) |
| `K-SCM-03` | Cơ chế Trừ hao Thời gian Mở cửa Thùng xe (Door Open Offset Logic) | System | High | [knowledge/K-SCM-03-door-open-offset.md](knowledge/K-SCM-03-door-open-offset.md) |
| `K-SCM-04` | Hao hụt Cân nặng do Thoát ẩm Lạnh lâu ngày (Cold Dehydration - Ẩn) | BusinessRule | High | [knowledge/K-SCM-04-cold-dehydration-weight-loss.md](knowledge/K-SCM-04-cold-dehydration-weight-loss.md) |
| `K-SCM-05` | Giới hạn Suy hao Tín hiệu Mạng Sóng Radio trong Panel Kho Lạnh | System | Medium | [knowledge/K-SCM-05-rfid-repeater-panel-attenuation.md](knowledge/K-SCM-05-rfid-repeater-panel-attenuation.md) |

## Business Rules
- `BR-SCM-01`: Thời hạn tối đa lưu sàn đệm Inbound
- `BR-SCM-02`: Quy tắc ưu tiên xuất hàng theo hạn sử dụng FEFO
- `BR-SCM-03`: Điều kiện kích hoạt luồng bốc dỡ thẳng Cross-Docking
- `BR-SCM-04`: Hạn mức dung sai vi phạm nhiệt độ buồng lạnh trên đường
- `BR-SCM-05`: Quy tắc cách ly pallet mồ côi và mã vạch không xác định
