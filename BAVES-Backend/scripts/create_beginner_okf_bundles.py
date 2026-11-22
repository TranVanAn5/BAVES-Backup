#!/usr/bin/env python3
"""Create 4 polished Beginner OKF scenario bundles (source of truth) and import SQL."""
from __future__ import annotations

import shutil
import subprocess
import sys
from pathlib import Path
from typing import Any

ROOT = Path(__file__).resolve().parents[1]
SCENARIOS = ROOT / "scenarios"


def write(path: Path, text: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text.lstrip("\n"), encoding="utf-8")


def q(s: str) -> str:
    return '"' + s.replace("\\", "\\\\").replace('"', '\\"') + '"'


def fold(s: str) -> str:
    """Collapse multiline prose into one YAML-safe quoted string."""
    return " ".join(line.strip() for line in s.replace("\r", "").split("\n") if line.strip())


def clear_bundle_dirs(base: Path) -> None:
    for name in ("personas", "knowledge", "rules", "events", "rubric"):
        d = base / name
        if d.exists():
            shutil.rmtree(d)
        d.mkdir(parents=True, exist_ok=True)


# ---------------------------------------------------------------------------
# Bundle definitions — rich Beginner content (Vietnamese, BA training tone)
# ---------------------------------------------------------------------------

BUNDLES: list[dict[str, Any]] = [
    # =====================================================================
    # 1) OmniMart Starter — OMS — free
    # =====================================================================
    {
        "folder": "omnimart-starter",
        "code": "BEG-OMS-01",
        "free": True,
        "industryCode": "SUPPLY_CHAIN",
        "industryName": "Chuỗi cung ứng & Bán lẻ",
        "applicationCode": "OMS",
        "applicationName": "Order Management System",
        "title": "OmniMart Starter — Theo dõi trạng thái đơn hàng cơ bản",
        "description": "Beginner: học viên làm quen BA qua bài toán chuẩn hóa trạng thái đơn hàng giữa website và cửa hàng, luyện phỏng vấn stakeholder và viết user story cơ bản.",
        "tags": "[beginner, starter, oms, order-status, babok-v3]",
        "context": {
            "summary": "Chuẩn hóa danh mục trạng thái đơn hàng giữa website OmniMart Starter và 3 cửa hàng, giảm ticket CSKH hỏi “đơn đang ở đâu?”.",
            "companyName": "OmniMart Starter",
            "businessContext": (
                "OmniMart Starter là chuỗi bán lẻ đồ gia dụng quy mô nhỏ dùng cho đào tạo BA: 1 website bán hàng, "
                "1 app di động đơn giản và 3 cửa hàng tại TP.HCM. Mỗi ngày xử lý khoảng 180–250 đơn (khoảng 60% online, 40% tại quầy). "
                "Đội vận hành điều phối bằng bảng Excel + chat nội bộ; CSKH trả lời khách trên hotline và Facebook. "
                "Hiện website dùng tên trạng thái tiếng Anh (New/Paid/Shipping/Done) trong khi cửa hàng ghi tiếng Việt "
                "(Chờ lấy / Đang gói / Đã giao), khiến khách và CSKH hiểu lệch nhau."
            ),
            "businessProblem": (
                "1) Khách hàng không hiểu trạng thái đơn trên app/website vì tên trạng thái khác với lời giải thích của CSKH. "
                "2) Khoảng 35% ticket tuần là hỏi “đơn của tôi đang ở đâu?”, CSKH phải gọi cửa hàng thủ công. "
                "3) Chưa có danh mục trạng thái chuẩn dùng chung giữa Ops, CSKH và cửa hàng; ghi chú ticket còn tự do."
            ),
            "businessObjectives": (
                "1) Liệt kê và chuẩn hóa danh mục trạng thái đơn (as-is → to-be) dùng chung cho website và cửa hàng. "
                "2) Xác định stakeholder chịu trách nhiệm cập nhật từng trạng thái. "
                "3) Giảm tỷ lệ ticket hỏi trạng thái đơn xuống dưới 20% trong phạm vi đề xuất. "
                "4) Hoàn thành bộ deliverable Beginner: BAP ngắn, stakeholder map, interview notes, as-is/to-be đơn giản, 3 user stories + AC cơ bản."
            ),
            "baRole": (
                "Bạn là Junior Business Analyst mới vào dự án OmniMart Starter, phụ trách khảo sát hiện trạng trạng thái đơn hàng "
                "và hỗ trợ đội vận hành/CSKH thống nhất ngôn ngữ trạng thái — không yêu cầu viết BRD/SRS đầy đủ."
            ),
            "assignment": (
                "Sprint 1: Đọc context, lập BAP ngắn (mục tiêu, phạm vi, kế hoạch phỏng vấn) và stakeholder map 3 bên. "
                "Sprint 2: Phỏng vấn Operations Coordinator, CSKH Lead và Store Staff; ghi elicitation notes (facts + assumptions). "
                "Sprint 3: Chuẩn hóa danh mục trạng thái đơn as-is / to-be và ánh xạ trách nhiệm cập nhật. "
                "Sprint 4: Viết Problem Statement ngắn + 3 user stories cơ bản kèm Acceptance Criteria; phản ánh nhẹ sự kiện E1 nếu được kích hoạt."
            ),
            "inScope": (
                "- Vòng đời trạng thái đơn online cơ bản: đặt → xác nhận/thanh toán → đóng gói → giao → hoàn tất hoặc hủy. "
                "- Danh mục trạng thái dùng chung và trách nhiệm cập nhật (Ops / CSKH / cửa hàng). "
                "- Deliverable Beginner: BAP ngắn, stakeholder map, interview notes, as-is/to-be, 3 user stories + AC."
            ),
            "outOfScope": (
                "- Thanh toán cổng, đối soát COD nâng cao, loyalty/điểm thưởng. "
                "- Thay thế POS cửa hàng hoặc thiết kế engine định tuyến kho phức tạp. "
                "- BRD/SRS/RTM đầy đủ, crisis management, ma trận RACI chi tiết cấp Intermediate."
            ),
            "knownConstraints": (
                "- Thời gian khảo sát học tập ≤ 1 tuần (4 sprint ngắn). "
                "- Không yêu cầu thay hệ thống lõi; ưu tiên chuẩn hóa quy ước và màn hình trạng thái tối thiểu. "
                "- Số liệu vận hành là giả lập phục vụ đào tạo BA."
            ),
        },
        "personas": [
            {
                "file": "ops-coordinator.md",
                "code": "ops-coordinator",
                "name": "Phạm Thu Hà",
                "role": "Operations Coordinator",
                "department": "Vận hành",
                "order": 1,
                "responsibilities": "Điều phối đơn giữa website và 3 cửa hàng; cập nhật trạng thái thủ công khi hệ thống lệch; theo dõi đơn treo quá SLA nội bộ 4 giờ.",
                "goals": "Muốn danh mục trạng thái đơn thống nhất để CSKH đỡ hỏi lại và cửa hàng cập nhật đúng một lần.",
                "pain": "Phải trả lời chat nội bộ mỗi khi khách hỏi đơn đang ở đâu; cuối ngày còn đối soát Excel với website.",
                "concerns": "Khách đánh giá thấp vì thiếu minh bạch trạng thái; sợ thêm cột trạng thái làm cửa hàng quên cập nhật.",
                "style": "Ngắn gọn, thích ví dụ ca thực tế, hỏi lại khi câu hỏi mơ hồ. Sẵn sàng chia sẻ số liệu ticket nếu learner hỏi cụ thể.",
                "personality": "Thực tế, hơi vội nhưng hợp tác khi thấy learner nắm đúng pain của vận hành.",
                "trust": 48,
                "patience": 52,
                "coop": 55,
                "note": "Mâu thuẫn nhẹ: Ops muốn trạng thái chi tiết (Đang lấy hàng / Đang gói), CSKH muốn ít trạng thái dễ giải thích cho khách.",
            },
            {
                "file": "cskh-lead.md",
                "code": "cskh-lead",
                "name": "Nguyễn Minh Anh",
                "role": "CSKH Lead",
                "department": "Chăm sóc khách hàng",
                "order": 2,
                "responsibilities": "Quản lý hotline/Facebook inbox; đào tạo agent trả lời trạng thái đơn; tổng hợp ticket tuần về khiếu nại giao hàng.",
                "goals": "Muốn script trả lời trạng thái đơn thống nhất và thấy cùng một tên trạng thái với app khách.",
                "pain": "Agent phải đoán nghĩa New/Paid/Shipping; gọi cửa hàng mất 5–10 phút/ticket trong giờ cao điểm.",
                "concerns": "NPS bị kéo xuống vì khách thấy trạng thái trên app khác lời agent; sợ thêm trạng thái làm script dài hơn.",
                "style": "Thân thiện, kể case khách thật, thích bảng mapping ngắn. Ít kiên nhẫn với thuật ngữ kỹ thuật.",
                "personality": "Hướng khách hàng, hợp tác, đôi khi bảo vệ đội agent trước đề xuất phức tạp.",
                "trust": 50,
                "patience": 58,
                "coop": 60,
                "note": "CSKH Lead muốn tối đa 5–6 trạng thái hiển thị cho khách; Ops muốn chi tiết hơn cho nội bộ.",
            },
            {
                "file": "store-staff.md",
                "code": "store-staff",
                "name": "Trần Quốc Bảo",
                "role": "Store Staff",
                "department": "Cửa hàng Quận 7",
                "order": 3,
                "responsibilities": "Nhận đơn online gán về cửa hàng, đóng gói, bàn giao shipper; ghi trạng thái trên sổ/chat nhóm cửa hàng.",
                "goals": "Muốn thao tác cập nhật trạng thái tối giản trên điện thoại, không phải nhập hai nơi (chat + website).",
                "pain": "Hay quên đổi trạng thái khi giờ cao điểm; bị CSKH gọi lại hỏi dù đã giao xong.",
                "concerns": "Sợ bị đổ lỗi khi trạng thái trễ dù hàng đã ra khỏi cửa hàng; lo thêm bước xác nhận làm chậm ca.",
                "style": "Nói thẳng, dùng tiếng cửa hàng, trả lời ngắn. Hợp tác nếu câu hỏi gắn với ca làm việc cụ thể.",
                "personality": "Thực dụng, hơi phòng thủ khi bị hỏi về sai sót cập nhật, nhưng sẵn sàng mô tả quy trình thật.",
                "trust": 42,
                "patience": 48,
                "coop": 50,
                "note": "Store Staff biết thực tế hay cập nhật trễ 15–30 phút; đây là fact learner cần elicit.",
            },
        ],
        "knowledge": [
            {
                "file": "K-BEG-OMS-01-order-status.md",
                "code": "K-BEG-OMS-01",
                "title": "Danh mục trạng thái đơn hiện tại (as-is)",
                "description": "Website và cửa hàng đang dùng bộ tên trạng thái khác nhau.",
                "category": "Process",
                "importance": "High",
                "access": [
                    {"persona": "ops-coordinator", "accessType": "Knows", "isHidden": False},
                    {"persona": "cskh-lead", "accessType": "Partial", "isHidden": False},
                    {"persona": "store-staff", "accessType": "Partial", "isHidden": False},
                ],
                "body": """Website/app hiện dùng: **New → Paid → Shipping → Done** (và Cancelled khi hủy).

Cửa hàng ghi trên sổ/chat: **Chờ lấy → Đang gói → Đã giao** (đôi khi ghi “Shipper lấy” thay vì Shipping).

CSKH trên ticket dùng ghi chú tự do, ví dụ “đang ở kho Q7”, “shipper đang tới”, không map chuẩn với website.

Ví dụ lệch: khách thấy **Shipping** trên app, agent nói “đang gói tại cửa hàng” vì cửa hàng chưa đổi chat — cùng một đơn nhưng hai cách gọi.""",
            },
            {
                "file": "K-BEG-OMS-02-ticket-volume.md",
                "code": "K-BEG-OMS-02",
                "title": "Khối lượng ticket hỏi trạng thái đơn",
                "description": "Số liệu CSKH về ticket liên quan trạng thái đơn trong 4 tuần gần nhất.",
                "category": "BusinessFact",
                "importance": "High",
                "access": [
                    {"persona": "cskh-lead", "accessType": "Knows", "isHidden": False},
                    {"persona": "ops-coordinator", "accessType": "Partial", "isHidden": False},
                ],
                "body": """Trong 4 tuần gần nhất (giả lập đào tạo):

| Tuần | Tổng ticket | Ticket hỏi trạng thái đơn | Tỷ lệ |
| --- | ---: | ---: | ---: |
| T1 | 420 | 148 | 35% |
| T2 | 390 | 140 | 36% |
| T3 | 455 | 155 | 34% |
| T4 | 410 | 139 | 34% |

Thời gian xử lý trung bình mỗi ticket hỏi trạng thái: **6–8 phút** (gồm gọi cửa hàng). Mục tiêu nội bộ CSKH: đưa tỷ lệ xuống **dưới 20%** sau khi chuẩn hóa trạng thái.""",
            },
            {
                "file": "K-BEG-OMS-03-update-sla.md",
                "code": "K-BEG-OMS-03",
                "title": "Quy ước SLA cập nhật trạng thái nội bộ",
                "description": "Ops kỳ vọng cửa hàng cập nhật trong 15 phút; thực tế thường trễ.",
                "category": "Policy",
                "importance": "Medium",
                "access": [
                    {"persona": "ops-coordinator", "accessType": "Knows", "isHidden": False},
                    {"persona": "store-staff", "accessType": "Partial", "isHidden": False},
                ],
                "body": """Quy ước nội bộ Ops (chưa thành policy chính thức trên hệ thống):

- Sau khi nhận đơn gán về cửa hàng: cập nhật **Chờ lấy / Confirmed** trong **15 phút**.
- Sau khi đóng gói xong: cập nhật **Packed / Đang gói xong** trước khi gọi shipper.
- Sau bàn giao shipper: cập nhật **Shipping** ngay.

CSKH được phép hẹn khách “kiểm tra lại trong 30 phút” nếu cửa hàng chưa phản hồi chat.""",
            },
            {
                "file": "K-BEG-OMS-04-late-update-habit.md",
                "code": "K-BEG-OMS-04",
                "title": "Thói quen cập nhật trễ tại cửa hàng (cần elicit)",
                "description": "Fact bán ẩn: cửa hàng hay cập nhật trạng thái trễ 15–30 phút giờ cao điểm.",
                "category": "BusinessFact",
                "importance": "Medium",
                "access": [
                    {"persona": "store-staff", "accessType": "Knows", "isHidden": False},
                    {"persona": "ops-coordinator", "accessType": "Partial", "isHidden": True},
                ],
                "body": """Fact cần khơi gợi qua phỏng vấn Store Staff:

Vào khung **11:30–13:00** và **17:30–20:00**, nhân viên ưu tiên đóng gói và tiếp khách tại quầy; cập nhật trạng thái trên website thường **trễ 15–30 phút** so với thực tế.

Khoảng **1/5 đơn** trong giờ cao điểm bị CSKH gọi hỏi trong lúc hàng đã đóng gói xong nhưng trạng thái app vẫn là Paid/Confirmed.

Store Staff đề xuất: cho phép cập nhật nhanh bằng 2 nút trên điện thoại (“Đã gói” / “Đã giao shipper”) thay vì mở full OMS trên máy tính.""",
            },
        ],
        "rules": [
            {
                "file": "BR-BEG-OMS-01-status-catalog.md",
                "code": "BR-BEG-OMS-01",
                "title": "Danh mục trạng thái đơn chuẩn dùng chung",
                "description": "Mọi kênh (website, cửa hàng, CSKH) phải dùng cùng bộ mã trạng thái chuẩn; không đặt tên tự do trên ticket khi đã có mã tương ứng.",
                "importance": "High",
                "isHidden": False,
                "body": "Hệ thống và quy trình nội bộ chỉ cho phép các trạng thái: New, Confirmed, Packed, Shipping, Completed, Cancelled. Tên hiển thị tiếng Việt cho khách phải map 1-1 với mã này (ví dụ Confirmed = Đã xác nhận).",
            },
            {
                "file": "BR-BEG-OMS-02-update-owner.md",
                "code": "BR-BEG-OMS-02",
                "title": "Trách nhiệm cập nhật theo trạng thái",
                "description": "Mỗi trạng thái có đúng một nhóm chịu trách nhiệm cập nhật chính.",
                "importance": "Medium",
                "isHidden": False,
                "body": "New do hệ thống tạo khi đặt hàng. Confirmed do Ops/CSKH xác nhận thanh toán hoặc COD. Packed và Shipping do cửa hàng cập nhật. Completed do hệ thống hoặc Ops khi giao thành công. Cancelled do Ops/CSKH theo quy trình hủy.",
            },
            {
                "file": "BR-BEG-OMS-03-customer-visible.md",
                "code": "BR-BEG-OMS-03",
                "title": "Trạng thái hiển thị cho khách trên app",
                "description": "Khách chỉ thấy tập trạng thái đã được CSKH duyệt wording; trạng thái nội bộ chi tiết không lộ ra app.",
                "importance": "Medium",
                "isHidden": False,
                "body": "App khách chỉ hiển thị: Đã tiếp nhận, Đã xác nhận, Đang chuẩn bị, Đang giao, Hoàn tất, Đã hủy. Các ghi chú nội bộ (ví dụ “đợi shipper Q7”) không được đưa lên giao diện khách.",
            },
        ],
        "event": {
            "file": "E1-status-mismatch-spike.md",
            "title": "Tăng đột biến ticket hỏi trạng thái sau đợt đồng bộ tên trạng thái thử",
            "description": "Ops thử đổi nhãn Shipping thành “Đang vận chuyển” trên website nhưng cửa hàng vẫn ghi “Đang gói”, khiến ticket hỏi trạng thái tăng nhẹ trong một buổi sáng.",
            "triggerType": "Manual",
            "displayOrder": 1,
            "isActive": True,
            "body": """Buổi sáng thứ Ba, Ops đổi nhãn hiển thị trạng thái **Shipping → Đang vận chuyển** trên website mà chưa thông báo CSKH và cửa hàng.

Trong 3 giờ đầu, ticket hỏi trạng thái tăng khoảng **+25%** so với cùng khung giờ tuần trước. Agent vẫn nói “đang gói” theo thói quen cửa hàng, trong khi khách đọc “Đang vận chuyển” trên app.

Yêu cầu learner (mức Beginner): nêu **tác động** tới CSKH và khách, liệt kê **2–3 câu hỏi** cần hỏi thêm stakeholder, và đề xuất **1 chỉnh sửa ngắn** cho bảng trạng thái to-be / communication note — không yêu cầu crisis plan đầy đủ.""",
        },
        "rubric": {
            "title": "Rubric Beginner — OmniMart Starter (BEG-OMS-01)",
            "description": "Tiêu chí đánh giá deliverable Beginner: BAP ngắn, stakeholder map, interview notes, as-is/to-be trạng thái đơn, 3 user stories + AC cơ bản.",
            "passingScore": 60.0,
            "body": """# Rubric Beginner OmniMart Starter

Dùng cho AI Assessor / Mentor khi chấm bài học viên mức Beginner (không yêu cầu BRD/SRS/RTM đầy đủ).

## Thang điểm từng tiêu chí
- **Xuất sắc (85–100)**: Đủ phạm vi Beginner, có số liệu/dẫn chứng phỏng vấn, AC rõ.
- **Đạt (60–84)**: Đủ các phần bắt buộc, còn thiếu chi tiết nhỏ.
- **Chưa đạt (<60)**: Thiếu phần chính, bịa fact, hoặc nâng scope lên Intermediate không cần thiết.""",
            "criteria": [
                {
                    "code": "COMPLETENESS",
                    "name": "Tính đầy đủ deliverable Beginner (Completeness)",
                    "description": "Có BAP ngắn, stakeholder map 3 bên, interview notes, as-is/to-be trạng thái đơn, và 3 user stories + AC cơ bản.",
                },
                {
                    "code": "FEASIBILITY",
                    "name": "Tính khả thi trong phạm vi Beginner (Feasibility)",
                    "description": "Đề xuất khớp BR-BEG-OMS-*, không đòi thay POS/ERP, phù hợp ràng buộc 1 tuần khảo sát học tập.",
                },
                {
                    "code": "TRACEABILITY",
                    "name": "Truy vết từ phỏng vấn (Traceability)",
                    "description": "Yêu cầu/user story dẫn được về ghi chú phỏng vấn Ops / CSKH / Store Staff (kể cả fact cập nhật trễ nếu đã elicit).",
                },
                {
                    "code": "CLARITY",
                    "name": "Rõ ràng và nhất quán (Clarity)",
                    "description": "Tên trạng thái thống nhất, AC kiểm thử được, không mâu thuẫn wording giữa app và CSKH.",
                },
            ],
        },
    },
    # =====================================================================
    # 2) AquaCold Starter — WMS_COLD — free
    # =====================================================================
    {
        "folder": "aquacold-starter",
        "code": "BEG-WMS-01",
        "free": True,
        "industryCode": "SUPPLY_CHAIN",
        "industryName": "Chuỗi cung ứng & Logistics Lạnh",
        "applicationCode": "WMS_COLD",
        "applicationName": "Cold Chain Warehouse Management System",
        "title": "AquaCold Starter — Theo dõi nhiệt độ kho lạnh cơ bản",
        "description": "Beginner: làm quen BA trong cold-chain WMS — khảo sát cảnh báo nhiệt độ, phân mức nghiêm trọng và nhật ký sự cố tối thiểu.",
        "tags": "[beginner, starter, wms, cold-chain, babok-v3]",
        "context": {
            "summary": "Làm rõ cảnh báo nhiệt độ kho đông AquaCold Starter và thiết kế nhật ký sự cố tối thiểu cho ca ngày/đêm.",
            "companyName": "AquaCold Starter",
            "businessContext": (
                "AquaCold Starter vận hành 1 kho lạnh nhỏ (khoảng 800 pallet) cho thực phẩm đông lạnh, phục vụ 8 khách hàng 3PL. "
                "Cảm biến IoT ghi nhiệt độ mỗi 5 phút; cảnh báo hiện gửi chung một nhóm Zalo “Kho đông”, khoảng 12 người "
                "(giám sát, QA, ca đêm). Mục tiêu kho đông: -18°C ± 2°C. Ca đêm (22:00–06:00) thường phản ứng chậm hơn "
                "vì tin nhắn bị lẫn với thông báo vận hành khác."
            ),
            "businessProblem": (
                "1) Cảnh báo nhiệt độ không phân mức Info / Warning / Critical — mọi tin đều giống nhau. "
                "2) Không có form nhật ký sự cố chuẩn; QA phải hỏi lại bằng chat. "
                "3) Ca đêm đôi khi bỏ lỡ tin Zalo; chưa rõ ai acknowledge trong bao lâu."
            ),
            "businessObjectives": (
                "1) Mô tả quy trình nhận và phản ứng cảnh báo nhiệt độ as-is. "
                "2) Phân loại mức cảnh báo cơ bản kèm ngưỡng số. "
                "3) Đề xuất trường dữ liệu tối thiểu cho nhật ký sự cố nhiệt độ. "
                "4) Hoàn thành deliverable Beginner: BAP ngắn, stakeholder map, interview notes, as-is/to-be, 3 user stories + AC."
            ),
            "baRole": (
                "Bạn là Junior BA hỗ trợ khảo sát vận hành kho lạnh AquaCold Starter, tập trung làm rõ cảnh báo và nhật ký sự cố — "
                "không thiết kế lại toàn bộ WMS hay routing xe lạnh."
            ),
            "assignment": (
                "Sprint 1: Đọc context, lập BAP ngắn và stakeholder map (Warehouse Supervisor, QA Assistant, Night Shift Lead). "
                "Sprint 2: Phỏng vấn 3 stakeholder; ghi elicitation notes về ngưỡng nhiệt và phản ứng ca đêm. "
                "Sprint 3: Lập bảng mức cảnh báo Info / Warning / Critical và luồng acknowledge as-is / to-be. "
                "Sprint 4: Viết 3 user stories cho nhật ký sự cố nhiệt độ + AC cơ bản; phản ánh nhẹ sự kiện E1 nếu có."
            ),
            "inScope": (
                "- Cảnh báo nhiệt độ 1 kho đông, phân mức nghiêm trọng, acknowledge và nhật ký sự cố thủ công/bán tự động. "
                "- Deliverable Beginner: BAP ngắn, stakeholder map, interview notes, as-is/to-be, 3 user stories + AC."
            ),
            "outOfScope": (
                "- Tối ưu routing xe lạnh, thay toàn bộ WMS, RFID nâng cao, crisis mất điện kéo dài cấp Intermediate. "
                "- Thiết kế lại hợp đồng SLA 3PL chi tiết pháp lý."
            ),
            "knownConstraints": (
                "- Ngân sách giai đoạn học tập; không mua cảm biến mới. "
                "- Giữ kênh Zalo tạm thời nếu chưa có app alert; ưu tiên quy ước và form tối thiểu. "
                "- Số liệu nhiệt độ/sự cố là giả lập đào tạo."
            ),
        },
        "personas": [
            {
                "file": "warehouse-supervisor.md",
                "code": "warehouse-supervisor",
                "name": "Đỗ Văn Khoa",
                "role": "Warehouse Supervisor",
                "department": "Kho lạnh",
                "order": 1,
                "responsibilities": "Giám sát ca ngày kho đông, phản ứng khi nhiệt độ vượt ngưỡng, phân công xử lý và báo cáo sự cố cho quản lý.",
                "goals": "Muốn cảnh báo rõ mức độ và template ghi sự cố hoàn tất trong 2 phút.",
                "pain": "Zalo nhóm quá ồn; khó phân biệt tin nhiệt độ với tin xe vào/ra.",
                "concerns": "Hư hàng nếu phản ứng chậm — mất hàng và uy tín khách 3PL.",
                "style": "Thẳng, dùng số liệu nhiệt độ, thích checklist. Hỏi lại nếu đề xuất quá lý thuyết.",
                "personality": "Trách nhiệm cao, hơi nóng khi bị hỏi chung chung, hợp tác khi learner nắm đúng ca vận hành.",
                "trust": 45,
                "patience": 50,
                "coop": 55,
                "note": "Muốn Critical gọi điện thoại; QA muốn mọi mức đều ghi form đủ trường — căng nhẹ về tốc độ vs đầy đủ.",
            },
            {
                "file": "qa-assistant.md",
                "code": "qa-assistant",
                "name": "Lê Thu Trang",
                "role": "QA Assistant",
                "department": "Đảm bảo chất lượng",
                "order": 2,
                "responsibilities": "Đối chiếu nhiệt độ với biên bản QA, yêu cầu giữ hàng khi nghi ngờ lệch nhiệt, tổng hợp sự cố tuần cho khách 3PL.",
                "goals": "Muốn mỗi sự cố nhiệt độ có mã, thời điểm, mức, hành động và chữ ký ca — đủ để trả lời audit nhẹ.",
                "pain": "Phải lục chat Zalo để ghép lại timeline; thiếu trường “nhiệt độ cao nhất ghi nhận”.",
                "concerns": "Báo cáo QA thiếu chứng cứ khi khách hỏi; sợ form quá ngắn bị audit nội bộ từ chối.",
                "style": "Cẩn thận, thích bảng trường dữ liệu, nói chậm. Sẵn sàng chia sẻ mẫu biên bản nếu được hỏi.",
                "personality": "Kỷ luật, hợp tác, đôi khi bảo thủ với thay đổi làm mất dấu vết QA.",
                "trust": 52,
                "patience": 60,
                "coop": 58,
                "note": "Partial know về thói quen ca đêm tắt thông báo Zalo — cần elicit từ Night Shift Lead.",
            },
            {
                "file": "night-shift-lead.md",
                "code": "night-shift-lead",
                "name": "Phạm Hoàng Nam",
                "role": "Night Shift Lead",
                "department": "Kho lạnh — Ca đêm",
                "order": 3,
                "responsibilities": "Trực ca 22:00–06:00, nhận cảnh báo, quyết định gọi bảo trì hoặc đóng cụm kệ tạm thời, bàn giao ca sáng.",
                "goals": "Muốn Critical đủ “ồn” để không bỏ lỡ khi đang trong phòng máy lạnh ồn; Warning không làm phiền liên tục.",
                "pain": "Zalo rung liên tục vì tin Info; đôi khi để chế độ im lặng sau 01:00.",
                "concerns": "Sợ bị đổ lỗi bỏ lỡ cảnh báo trong khi tin bị lẫn; lo thêm bước ghi form làm chậm phản ứng cấp cứu nhiệt.",
                "style": "Nói nhanh, kể ca đêm thật, ít kiên nhẫn với câu hỏi dài. Hợp tác nếu learner tôn trọng ràng buộc ca đêm.",
                "personality": "Thực dụng, hơi phòng thủ về việc tắt thông báo, nhưng trung thực nếu được hỏi đúng cách.",
                "trust": 40,
                "patience": 45,
                "coop": 48,
                "note": "Fact ẩn: sau 01:00 thường để Zalo im lặng trừ cuộc gọi — learner cần elicit.",
            },
        ],
        "knowledge": [
            {
                "file": "K-BEG-WMS-01-temp-thresholds.md",
                "code": "K-BEG-WMS-01",
                "title": "Ngưỡng nhiệt độ kho đông hiện hành",
                "description": "Mục tiêu và ngưỡng Warning/Critical đang dùng tại kho đông.",
                "category": "Policy",
                "importance": "High",
                "access": [
                    {"persona": "warehouse-supervisor", "accessType": "Knows", "isHidden": False},
                    {"persona": "qa-assistant", "accessType": "Knows", "isHidden": False},
                    {"persona": "night-shift-lead", "accessType": "Partial", "isHidden": False},
                ],
                "body": """Kho đông mục tiêu: **-18°C ± 2°C** (dải chấp nhận -20°C đến -16°C).

| Mức | Điều kiện (as-is đang hiểu nội bộ) | Kênh cảnh báo hiện tại |
| --- | --- | --- |
| Info | Dao động trong ±2°C nhưng gần biên | Hiếm khi gửi |
| Warning | Vượt **-15°C** quá **10 phút** | Zalo nhóm — cùng template “Nhiệt độ bất thường” |
| Critical | Vượt **-12°C** hoặc mất điện cụm máy lạnh | Zalo nhóm — cùng template, không phân biệt |

Hiện **mọi mức** (khi gửi) đều dùng một tin Zalo giống nhau, không gắn mức nghiêm trọng.""",
            },
            {
                "file": "K-BEG-WMS-02-alert-flow.md",
                "code": "K-BEG-WMS-02",
                "title": "Luồng nhận cảnh báo và phản ứng as-is",
                "description": "Cách đội kho nhận tin và xử lý khi nhiệt độ lệch.",
                "category": "Process",
                "importance": "High",
                "access": [
                    {"persona": "warehouse-supervisor", "accessType": "Knows", "isHidden": False},
                    {"persona": "night-shift-lead", "accessType": "Knows", "isHidden": False},
                    {"persona": "qa-assistant", "accessType": "Partial", "isHidden": False},
                ],
                "body": """As-is (rút gọn):

1. Cảm biến ghi nhiệt mỗi **5 phút** → gateway đẩy về dashboard nội bộ.
2. Rule đơn giản gửi Zalo khi vượt -15°C.
3. Người trực đọc tin → vào phòng máy / kiểm tra cửa kho → điều chỉnh hoặc gọi bảo trì.
4. QA hỏi lại sau để ghi biên bản (thường **sau 30–90 phút**).

Thời gian phản ứng trung bình ca ngày: **8–12 phút**. Ca đêm: **15–25 phút** (ước lượng Supervisor).""",
            },
            {
                "file": "K-BEG-WMS-03-incident-fields.md",
                "code": "K-BEG-WMS-03",
                "title": "Trường nhật ký sự cố QA mong muốn",
                "description": "Danh sách trường tối thiểu QA Assistant muốn có trên form sự cố nhiệt độ.",
                "category": "System",
                "importance": "Medium",
                "access": [
                    {"persona": "qa-assistant", "accessType": "Knows", "isHidden": False},
                    {"persona": "warehouse-supervisor", "accessType": "Partial", "isHidden": False},
                ],
                "body": """QA đề xuất form tối thiểu (Beginner — chưa phải hệ thống QMS đầy đủ):

1. Mã sự cố (auto)
2. Thời điểm bắt đầu lệch nhiệt
3. Nhiệt độ cao nhất ghi nhận (°C)
4. Mức cảnh báo (Warning / Critical)
5. Người acknowledge
6. Hành động đã làm (chọn nhanh + ghi chú)
7. Thời điểm ổn định lại
8. Ảnh/chụp màn hình dashboard (tuỳ chọn)

Supervisor đồng ý các trường 1–7 nhưng muốn thao tác **≤ 2 phút** trên điện thoại.""",
            },
            {
                "file": "K-BEG-WMS-04-night-mute.md",
                "code": "K-BEG-WMS-04",
                "title": "Thói quen im lặng thông báo ca đêm (cần elicit)",
                "description": "Fact bán ẩn: sau 01:00 ca đêm thường để Zalo im lặng trừ cuộc gọi.",
                "category": "BusinessFact",
                "importance": "Medium",
                "access": [
                    {"persona": "night-shift-lead", "accessType": "Knows", "isHidden": False},
                    {"persona": "warehouse-supervisor", "accessType": "Partial", "isHidden": True},
                    {"persona": "qa-assistant", "accessType": "Partial", "isHidden": True},
                ],
                "body": """Fact cần khơi gợi từ Night Shift Lead:

Sau **01:00**, nhóm ca đêm thường để Zalo ở chế độ **im lặng** vì tin Info/xe vào ra làm mất ngủ giữa ca. Họ vẫn nhận **cuộc gọi điện thoại**.

Trong 2 tháng gần đây có **3 lần** Warning gửi lúc 01:30–03:00 chỉ được xử lý khi người trực tình cờ mở nhóm lúc đi tuần.

Đề xuất từ ca đêm: Critical phải **gọi voice/SMS**; Warning giữ Zalo nhưng gắn nhãn rõ; Info không gửi Zalo ban đêm.""",
            },
        ],
        "rules": [
            {
                "file": "BR-BEG-WMS-01-alert-levels.md",
                "code": "BR-BEG-WMS-01",
                "title": "Phân mức cảnh báo nhiệt độ kho đông",
                "description": "Mọi cảnh báo nhiệt độ phải gắn mức Info, Warning hoặc Critical theo ngưỡng quy định.",
                "importance": "High",
                "isHidden": False,
                "body": "Warning khi nhiệt độ > -15°C liên tục quá 10 phút. Critical khi > -12°C hoặc mất tín hiệu cụm máy lạnh > 5 phút. Info chỉ ghi log, không đẩy Zalo ca đêm.",
            },
            {
                "file": "BR-BEG-WMS-02-acknowledge-sla.md",
                "code": "BR-BEG-WMS-02",
                "title": "SLA acknowledge cảnh báo",
                "description": "Thời gian tối đa để người trực xác nhận đã nhận cảnh báo theo từng mức.",
                "importance": "High",
                "isHidden": False,
                "body": "Warning phải được acknowledge trong 15 phút. Critical phải được acknowledge trong 5 phút; nếu không, hệ thống/quy trình phải leo thang gọi điện cho Night Shift Lead hoặc Supervisor on-call.",
            },
            {
                "file": "BR-BEG-WMS-03-incident-log-required.md",
                "code": "BR-BEG-WMS-03",
                "title": "Bắt buộc nhật ký sau Warning/Critical",
                "description": "Mọi Warning và Critical đã xử lý phải có bản ghi sự cố trước khi kết thúc ca.",
                "importance": "Medium",
                "isHidden": False,
                "body": "Trước khi bàn giao ca, người trực phải hoàn tất nhật ký sự cố (tối thiểu: thời điểm, mức, nhiệt độ cao nhất, hành động, người xử lý). Thiếu nhật ký thì ca sau phải đánh dấu “pending QA”.",
            },
        ],
        "event": {
            "file": "E1-warning-missed-night.md",
            "title": "Bỏ lỡ một cảnh báo Warning lúc 02:10 ca đêm",
            "description": "Một Warning nhiệt độ -14,5°C kéo dài 18 phút lúc 02:10 chỉ được phát hiện khi đi tuần lúc 02:40; chưa thành Critical.",
            "triggerType": "Manual",
            "displayOrder": 1,
            "isActive": True,
            "body": """Lúc **02:10**, cảm biến ghi **-14,5°C** và giữ trên -15°C hơn 10 phút → Warning gửi Zalo. Ca đêm đang để nhóm im lặng nên không thấy tin.

Lúc **02:40** người trực đi tuần, mở cửa kho và thấy dashboard vàng. Nhiệt về **-16,8°C** sau khi đóng kín cửa và giảm tần suất mở cổng. Không có hư hàng ghi nhận, nhưng QA yêu cầu giải trình.

Yêu cầu learner (Beginner): phân tích tác động nhẹ tới quy trình acknowledge, nêu **gap** as-is, và đề xuất **1–2** chỉnh sửa cho user story cảnh báo/nhật ký — không viết crisis playbook đầy đủ.""",
        },
        "rubric": {
            "title": "Rubric Beginner — AquaCold Starter (BEG-WMS-01)",
            "description": "Tiêu chí đánh giá deliverable Beginner về cảnh báo nhiệt độ và nhật ký sự cố kho lạnh.",
            "passingScore": 60.0,
            "body": """# Rubric Beginner AquaCold Starter

Chấm mức Beginner: BAP ngắn, stakeholder map, interview notes, bảng mức cảnh báo, 3 user stories + AC. Không yêu cầu SRS cold-chain đầy đủ.

## Thang điểm
- **85–100**: Ngưỡng số rõ, có phân ca ngày/đêm, AC kiểm thử được.
- **60–84**: Đủ phần bắt buộc, còn thiếu chi tiết nhỏ.
- **<60**: Thiếu phân mức cảnh báo hoặc không dựa trên phỏng vấn.""",
            "criteria": [
                {
                    "code": "COMPLETENESS",
                    "name": "Tính đầy đủ deliverable Beginner (Completeness)",
                    "description": "Có BAP ngắn, stakeholder map 3 bên, interview notes, bảng Info/Warning/Critical, 3 user stories + AC cho nhật ký/cảnh báo.",
                },
                {
                    "code": "FEASIBILITY",
                    "name": "Tính khả thi trong phạm vi Beginner (Feasibility)",
                    "description": "Tuân thủ BR-BEG-WMS-*, không đòi thay WMS/cảm biến hàng loạt, phù hợp kênh Zalo tạm thời.",
                },
                {
                    "code": "TRACEABILITY",
                    "name": "Truy vết từ phỏng vấn (Traceability)",
                    "description": "Yêu cầu gắn với ghi chú Supervisor / QA / Night Shift (kể cả fact im lặng Zalo nếu đã elicit).",
                },
                {
                    "code": "CLARITY",
                    "name": "Rõ ràng và nhất quán (Clarity)",
                    "description": "Ngưỡng nhiệt và SLA acknowledge viết rõ số; không mâu thuẫn giữa các mức cảnh báo.",
                },
            ],
        },
    },
    # =====================================================================
    # 3) FinNova Starter — DIGITAL_LOS — paid
    # =====================================================================
    {
        "folder": "finnova-starter",
        "code": "BEG-LOS-01",
        "free": False,
        "industryCode": "FINANCE",
        "industryName": "Tài chính Tiêu dùng Số & FinTech",
        "applicationCode": "DIGITAL_LOS",
        "applicationName": "Digital Loan Origination & Automated Underwriting System",
        "title": "FinNova Starter — Làm rõ bước eKYC khoản vay tiêu dùng",
        "description": "Beginner (trả phí): khảo sát luồng eKYC cơ bản trong Digital LOS, taxonomy lý do hold và user story màn hình Ops.",
        "tags": "[beginner, starter, los, ekyc, babok-v3]",
        "context": {
            "summary": "Làm rõ 5 bước eKYC và lý do hồ sơ bị hold trên Digital LOS FinNova Starter để Ops xử lý đúng SLA.",
            "companyName": "FinNova Starter",
            "businessContext": (
                "FinNova Starter cung cấp khoản vay tiêu dùng online hạn mức 5–50 triệu đồng. Khách hàng chụp CCCD và selfie "
                "trên app; hệ thống eKYC tự động duyệt khoảng 70% hồ sơ. Phần còn lại rơi vào hàng đợi “chờ đối soát thủ công”. "
                "Credit Ops xử lý khoảng 120–150 hồ sơ hold/ngày với SLA nội bộ **4 giờ**. Onboarding Specialist hỗ trợ khách "
                "bổ sung giấy tờ; Compliance Junior rà soát định kỳ mẫu reject/hold."
            ),
            "businessProblem": (
                "1) Màn hình Ops không hiển thị lý do hold rõ — phải mở log thô. "
                "2) Khách bị gọi đi gọi lại vì Onboarding không biết chính xác thiếu gì. "
                "3) Chưa có taxonomy hold/reject eKYC thống nhất cho BA trainee luyện tập."
            ),
            "businessObjectives": (
                "1) Vẽ as-is 5 bước eKYC (từ mở hồ sơ đến pass/hold/reject). "
                "2) Liệt kê top lý do hold và đề xuất trường hold reason tối thiểu. "
                "3) Viết 3 user stories cho màn hình Ops hiển thị lý do hold. "
                "4) Hoàn thành deliverable Beginner (không yêu cầu mô hình scoring hay anti-deepfake nâng cao)."
            ),
            "baRole": (
                "Bạn là Junior BA hỗ trợ đội Credit Operations của FinNova Starter, làm rõ thông tin hold eKYC trên UI nội bộ "
                "để giảm thời gian xử lý và cuộc gọi trùng."
            ),
            "assignment": (
                "Sprint 1: BAP ngắn + phạm vi eKYC + stakeholder map (Credit Ops, Onboarding, Compliance Junior). "
                "Sprint 2: Phỏng vấn 3 bên; ghi notes về lý do hold và SLA. "
                "Sprint 3: As-is 5 bước eKYC + bảng taxonomy hold reason. "
                "Sprint 4: 3 user stories + AC cho màn hình hold reason; phản ánh nhẹ sự kiện E1 nếu được kích hoạt."
            ),
            "inScope": (
                "- Bước eKYC CCCD + selfie + hold/reject lý do cơ bản trên UI Ops. "
                "- Taxonomy hold reason và trường dữ liệu tối thiểu. "
                "- Deliverable Beginner: BAP ngắn, stakeholder map, interview notes, as-is/to-be, 3 user stories + AC."
            ),
            "outOfScope": (
                "- Mô hình credit scoring, deepfake detection nâng cao, thay core banking. "
                "- BRD/SRS đầy đủ, ma trận tuân thủ pháp lý phức tạp cấp Intermediate/Advanced."
            ),
            "knownConstraints": (
                "- Tuân thủ lưu trữ ảnh KYC theo chính sách nội bộ; không đưa PII thật vào bài nộp. "
                "- Không đổi vendor eKYC trong phạm vi bài tập. "
                "- Số liệu hold là giả lập phục vụ đào tạo."
            ),
        },
        "personas": [
            {
                "file": "credit-ops.md",
                "code": "credit-ops",
                "name": "Ngô Minh Châu",
                "role": "Credit Ops Officer",
                "department": "Vận hành tín dụng",
                "order": 1,
                "responsibilities": "Xử lý hàng đợi hồ sơ eKYC bị hold, gọi khách hoặc chuyển Onboarding bổ sung giấy tờ, đóng case trong SLA 4 giờ.",
                "goals": "Muốn thấy lý do hold và gợi ý hành động ngay trên danh sách hồ sơ, không mở 3 màn hình.",
                "pain": "Phải đọc log thô để đoán nguyên nhân; dễ chọn nhầm lý do khi ghi chú tay.",
                "concerns": "Trễ SLA 4 giờ → khách bỏ cuộc; sợ taxonomy quá dài làm chậm thao tác.",
                "style": "Nhanh, thích ví dụ hồ sơ thật (đã ẩn PII), hỏi lại khi AC quá chung.",
                "personality": "Hướng kết quả, hợp tác, hơi thiếu kiên nhẫn với tài liệu dài.",
                "trust": 48,
                "patience": 50,
                "coop": 55,
                "note": "Muốn 5–7 lý do hold cố định; Compliance muốn thêm nhóm “Khác — cần rà soát” có bắt buộc ghi chú.",
            },
            {
                "file": "onboarding-specialist.md",
                "code": "onboarding-specialist",
                "name": "Hoàng Nhật Lê",
                "role": "Onboarding Specialist",
                "department": "Chăm sóc mở tài khoản vay",
                "order": 2,
                "responsibilities": "Gọi/nhắn khách bổ sung ảnh CCCD/selfie, hướng dẫn chụp lại, cập nhật trạng thái chờ khách trên CRM nhẹ.",
                "goals": "Muốn script theo từng mã hold reason để hướng dẫn khách một lần đúng.",
                "pain": "Ops chuyển case với ghi chú “KYC fail” chung chung; phải hỏi lại mất thêm 1 vòng.",
                "concerns": "Khách bực vì bị yêu cầu chụp lại nhiều lần; sợ sai script làm tăng reject oan.",
                "style": "Thân thiện, kể cuộc gọi khách, thích bảng “mã → hướng dẫn”. Ít dùng jargon pháp lý.",
                "personality": "Empathy cao, hợp tác, đôi khi bênh khách trước Ops.",
                "trust": 55,
                "patience": 62,
                "coop": 60,
                "note": "Căng nhẹ với Ops về chất lượng ghi chú chuyển case.",
            },
            {
                "file": "compliance-junior.md",
                "code": "compliance-junior",
                "name": "Đặng Thu Hà",
                "role": "Compliance Junior",
                "department": "Tuân thủ",
                "order": 3,
                "responsibilities": "Rà soát mẫu hồ sơ hold/reject tuần, đảm bảo lý do lưu trữ đủ để giải trình nội bộ, nhắc không lưu PII ra kênh chat cá nhân.",
                "goals": "Muốn taxonomy hold có mã chuẩn và bắt buộc ghi chú khi chọn “Khác”.",
                "pain": "Ops hay viết lý do tự do trên Excel; khó thống kê và audit nhẹ.",
                "concerns": "Sợ thiếu dấu vết khi bị hỏi lại; lo Onboarding gửi ảnh CCCD qua Zalo cá nhân.",
                "style": "Cẩn trọng, nói rõ ràng, thích danh mục mã. Hợp tác nếu learner tôn trọng ràng buộc tuân thủ cơ bản.",
                "personality": "Nguyên tắc, hơi khô, nhưng sẵn sàng giải thích “vì sao cần mã chuẩn”.",
                "trust": 50,
                "patience": 55,
                "coop": 52,
                "note": "Biết tỷ lệ nhóm “Khác/chưa phân loại” cao — fact learner cần elicit.",
            },
        ],
        "knowledge": [
            {
                "file": "K-BEG-LOS-01-hold-reasons.md",
                "code": "K-BEG-LOS-01",
                "title": "Top lý do hold eKYC tuần gần nhất",
                "description": "Phân bố lý do hold theo ước lượng Ops (giả lập).",
                "category": "BusinessFact",
                "importance": "High",
                "access": [
                    {"persona": "credit-ops", "accessType": "Knows", "isHidden": False},
                    {"persona": "compliance-junior", "accessType": "Partial", "isHidden": False},
                    {"persona": "onboarding-specialist", "accessType": "Partial", "isHidden": False},
                ],
                "body": """Top hold (ước lượng Ops, 1 tuần mẫu ~140 hồ sơ hold):

1. Ảnh CCCD mờ / lóa — **42%**
2. Selfie không khớp góc mặt — **27%**
3. CCCD hết hạn — **15%**
4. Khác / chưa phân loại — **16%** ← nhóm khiến Ops mất thời gian nhất vì phải đọc log

Mục tiêu Beginner: giảm nhóm “Khác” bằng taxonomy mã hóa + ghi chú bắt buộc.""",
            },
            {
                "file": "K-BEG-LOS-02-ekyc-steps.md",
                "code": "K-BEG-LOS-02",
                "title": "Năm bước eKYC as-is",
                "description": "Luồng eKYC từ mở hồ sơ đến pass/hold/reject.",
                "category": "Process",
                "importance": "High",
                "access": [
                    {"persona": "credit-ops", "accessType": "Knows", "isHidden": False},
                    {"persona": "onboarding-specialist", "accessType": "Knows", "isHidden": False},
                    {"persona": "compliance-junior", "accessType": "Partial", "isHidden": False},
                ],
                "body": """As-is eKYC (rút gọn Beginner):

1. **Mở hồ sơ** — khách nhập SĐT, đồng ý điều khoản.
2. **Chụp CCCD** — mặt trước/sau; kiểm tra cơ bản (có ảnh, không crop quá tay).
3. **Selfie + liveness đơn giản** — chớp mắt/quay đầu nhẹ (vendor sẵn có).
4. **Đối khớp tự động** — ~70% **Pass**; còn lại **Hold** đưa vào queue Ops.
5. **Đối soát thủ công** — Ops quyết định Pass / Reject / Yêu cầu chụp lại (về Onboarding).

Thời gian trung vị bước 4: **under 2 phút**. Bước 5 khi hold: phụ thuộc hàng đợi, mục tiêu **< 4 giờ**.""",
            },
            {
                "file": "K-BEG-LOS-03-ops-ui-gap.md",
                "code": "K-BEG-LOS-03",
                "title": "Khoảng trống UI hàng đợi hold",
                "description": "Những trường Ops cần nhưng màn hình hiện chưa có.",
                "category": "System",
                "importance": "Medium",
                "access": [
                    {"persona": "credit-ops", "accessType": "Knows", "isHidden": False},
                    {"persona": "onboarding-specialist", "accessType": "Partial", "isHidden": False},
                ],
                "body": """Màn hình queue hiện có: mã hồ sơ, giờ vào queue, trạng thái Hold, nút mở chi tiết ảnh.

**Thiếu:** mã hold reason, mô tả ngắn, nguồn (auto rule / vendor code), số lần chụp lại, nút chuyển Onboarding kèm lý do.

Ops ước tính mỗi case mất thêm **3–5 phút** chỉ để đoán lý do trước khi gọi khách.""",
            },
            {
                "file": "K-BEG-LOS-04-unclassified-hold.md",
                "code": "K-BEG-LOS-04",
                "title": "Nhóm hold chưa phân loại và rủi ro tuân thủ (cần elicit)",
                "description": "Fact bán ẩn: 16% “Khác” thường bị ghi chú tự do, khó audit.",
                "category": "Policy",
                "importance": "Medium",
                "access": [
                    {"persona": "compliance-junior", "accessType": "Knows", "isHidden": False},
                    {"persona": "credit-ops", "accessType": "Partial", "isHidden": True},
                ],
                "body": """Fact cần khơi gợi từ Compliance Junior:

Trong nhóm **16% “Khác”**, khoảng một nửa thực chất thuộc các mã đã có (mờ ảnh, selfie lệch) nhưng Ops chọn “Khác” cho nhanh. Phần còn lại gồm lỗi vendor tạm thời hoặc nghi ngờ bất thường nhẹ.

Compliance yêu cầu: chọn **OTHER** phải nhập ghi chú ≥ 20 ký tự; cấm gửi ảnh CCCD qua Zalo cá nhân — chỉ qua kênh CRM/LOS.

Đây là điểm learner Beginner cần elicit để viết AC cho trường hold reason.""",
            },
        ],
        "rules": [
            {
                "file": "BR-BEG-LOS-01-hold-taxonomy.md",
                "code": "BR-BEG-LOS-01",
                "title": "Taxonomy mã lý do hold eKYC",
                "description": "Mọi hồ sơ hold phải gắn một mã lý do thuộc danh mục chuẩn.",
                "importance": "High",
                "isHidden": False,
                "body": "Danh mục tối thiểu: BLURRED_ID, SELFIE_MISMATCH, EXPIRED_ID, VENDOR_TIMEOUT, OTHER. Không lưu lý do chỉ dưới dạng text tự do nếu đã có mã tương ứng.",
            },
            {
                "file": "BR-BEG-LOS-02-other-note-required.md",
                "code": "BR-BEG-LOS-02",
                "title": "Bắt buộc ghi chú khi chọn OTHER",
                "description": "Mã OTHER phải kèm ghi chú đủ dài để audit nội bộ.",
                "importance": "High",
                "isHidden": False,
                "body": "Khi Ops chọn OTHER, hệ thống bắt buộc nhập ghi chú tối thiểu 20 ký tự trước khi lưu hoặc chuyển Onboarding. Không cho phép OTHER trống.",
            },
            {
                "file": "BR-BEG-LOS-03-sla-hold.md",
                "code": "BR-BEG-LOS-03",
                "title": "SLA xử lý hồ sơ hold",
                "description": "Hồ sơ hold phải được xử lý hoặc chuyển Onboarding trong 4 giờ làm việc.",
                "importance": "Medium",
                "isHidden": False,
                "body": "Credit Ops phải đóng case (Pass/Reject) hoặc chuyển Onboarding kèm mã lý do trong vòng 4 giờ kể từ khi hồ sơ vào queue hold. Quá hạn phải đánh dấu escalated trên UI.",
            },
        ],
        "event": {
            "file": "E1-hold-reason-blank-spike.md",
            "title": "Hàng loạt hồ sơ hold thiếu lý do sau đợt cập nhật vendor",
            "description": "Vendor eKYC đổi mã lỗi nội bộ; LOS chưa map → ~30 hồ sơ vào queue với lý do trống trong buổi sáng.",
            "triggerType": "Manual",
            "displayOrder": 1,
            "isActive": True,
            "body": """Sau nửa đêm, vendor eKYC đổi một số mã lỗi. Sáng hôm sau khoảng **30 hồ sơ** vào queue Hold với trường lý do **trống**. Ops phải mở log thô; Onboarding nhận case “KYC fail” không rõ.

Chưa có sự cố gian lận; chỉ ảnh hưởng tốc độ xử lý và trải nghiệm khách.

Yêu cầu learner (Beginner): nêu tác động tới SLA và UX Ops, đề xuất **1 rule/AC** cho trường hợp mã vendor chưa map (ví dụ gán VENDOR_TIMEOUT hoặc OTHER + ghi chú), cập nhật ngắn taxonomy — không yêu cầu ứng phó khủng hoảng Intermediate.""",
        },
        "rubric": {
            "title": "Rubric Beginner — FinNova Starter (BEG-LOS-01)",
            "description": "Tiêu chí đánh giá deliverable Beginner về eKYC hold reason trên Digital LOS.",
            "passingScore": 60.0,
            "body": """# Rubric Beginner FinNova Starter

Chấm BAP ngắn, stakeholder map, interview notes, as-is 5 bước eKYC, taxonomy hold, 3 user stories + AC. Không chấm mô hình scoring nâng cao.

## Thang điểm
- **85–100**: Taxonomy rõ, AC OTHER/SLA cụ thể, có dẫn chứng phỏng vấn.
- **60–84**: Đủ phần bắt buộc.
- **<60**: Thiếu taxonomy hoặc không truy vết stakeholder.""",
            "criteria": [
                {
                    "code": "COMPLETENESS",
                    "name": "Tính đầy đủ deliverable Beginner (Completeness)",
                    "description": "Có BAP ngắn, stakeholder map 3 bên, interview notes, as-is eKYC, taxonomy hold, 3 user stories + AC.",
                },
                {
                    "code": "FEASIBILITY",
                    "name": "Tính khả thi trong phạm vi Beginner (Feasibility)",
                    "description": "Khớp BR-BEG-LOS-*, không đòi đổi vendor/scoring, tôn trọng ràng buộc PII.",
                },
                {
                    "code": "TRACEABILITY",
                    "name": "Truy vết từ phỏng vấn (Traceability)",
                    "description": "User story dẫn về notes Credit Ops / Onboarding / Compliance (kể cả fact nhóm OTHER).",
                },
                {
                    "code": "CLARITY",
                    "name": "Rõ ràng và nhất quán (Clarity)",
                    "description": "Mã hold reason và AC viết rõ, không mâu thuẫn giữa Ops script và Compliance.",
                },
            ],
        },
    },
    # =====================================================================
    # 4) MegaFashion Starter — LMS_PROMO — paid
    # =====================================================================
    {
        "folder": "megafashion-starter",
        "code": "BEG-PROMO-01",
        "free": False,
        "industryCode": "RETAIL",
        "industryName": "Bán lẻ & Thời trang",
        "applicationCode": "LMS_PROMO",
        "applicationName": "Customer Loyalty & Promotion Engine",
        "title": "MegaFashion Starter — Làm rõ điều kiện voucher cơ bản",
        "description": "Beginner (trả phí): khảo sát điều kiện voucher GIẢM50K, chỗ lệch banner vs hệ thống, và viết AC cơ bản.",
        "tags": "[beginner, starter, promo, voucher, babok-v3]",
        "context": {
            "summary": "Làm rõ điều kiện voucher GIẢM50K giữa banner Marketing, app và POS để giảm khiếu nại mã không dùng được.",
            "companyName": "MegaFashion Starter",
            "businessContext": (
                "MegaFashion Starter là chuỗi thời trang casual với app bán hàng và 6 cửa hàng. Tuần này chạy chiến dịch "
                "voucher **GIẢM50K**: giảm 50.000đ cho đơn từ 499.000đ. Banner quảng cáo ghi “mọi sản phẩm”, trong khi "
                "Promotion Engine chỉ áp dụng danh mục **Áo**. POS cửa hàng đôi khi báo lỗi khác app. CX Specialist nhận "
                "khoảng 40–60 inbox/ngày liên quan mã trong ngày flash nhẹ."
            ),
            "businessProblem": (
                "1) Điều kiện trên banner khác cấu hình hệ thống (danh mục Áo, không cộng dồn FREESHIP). "
                "2) Marketing Ops, POS Supervisor và CX hiểu rule khác nhau. "
                "3) Chưa có bảng rule đơn giản + AC để BA trainee luyện đặc tả voucher."
            ),
            "businessObjectives": (
                "1) Liệt kê điều kiện voucher as-is (min order, danh mục, stacking, giới hạn dùng). "
                "2) Chỉ ra chỗ lệch banner vs hệ thống vs POS. "
                "3) Viết 3 acceptance criteria / user stories cho voucher mẫu GIẢM50K. "
                "4) Hoàn thành deliverable Beginner (không thiết kế engine điểm thưởng phức tạp)."
            ),
            "baRole": (
                "Bạn là Junior BA hỗ trợ Marketing Ops của MegaFashion Starter, làm rõ điều kiện voucher và đồng bộ wording "
                "giữa banner, app và POS."
            ),
            "assignment": (
                "Sprint 1: BAP ngắn + stakeholder map (Marketing Ops, POS Supervisor, CX Specialist). "
                "Sprint 2: Phỏng vấn 3 bên; ghi notes về lệch banner/POS. "
                "Sprint 3: Bảng điều kiện voucher as-is / to-be và danh sách chỗ lệch. "
                "Sprint 4: 3 user stories + AC cho GIẢM50K; phản ánh nhẹ sự kiện E1 nếu có."
            ),
            "inScope": (
                "- Một voucher tiền mặt cố định GIẢM50K: min order, danh mục, không cộng dồn, giới hạn 1 mã/tài khoản/ngày. "
                "- Đồng bộ mô tả điều kiện trên banner/app/POS. "
                "- Deliverable Beginner: BAP ngắn, stakeholder map, interview notes, as-is/to-be, 3 user stories + AC."
            ),
            "outOfScope": (
                "- Engine điểm thưởng phức tạp, cá nhân hóa AI, đa voucher stacking nâng cao. "
                "- Đổi chính sách giá gốc SKU toàn chuỗi."
            ),
            "knownConstraints": (
                "- Không đổi chính sách giá gốc sản phẩm trong bài tập. "
                "- Giữ mã FREESHIP hoạt động song song nhưng không cộng dồn với GIẢM50K. "
                "- Số liệu khiếu nại là giả lập đào tạo."
            ),
        },
        "personas": [
            {
                "file": "marketing-ops.md",
                "code": "marketing-ops",
                "name": "Vũ Thanh Tâm",
                "role": "Marketing Ops",
                "department": "Marketing",
                "order": 1,
                "responsibilities": "Cấu hình chiến dịch voucher trên Promotion Engine, soạn banner, theo dõi redemption và trả lời khiếu nại mã không dùng được.",
                "goals": "Muốn điều kiện voucher một nguồn sự thật giữa banner, app và POS.",
                "pain": "Khách inbox liên tục; phải giải thích thủ công từng case lệch danh mục Áo.",
                "concerns": "Chiến dịch bị dừng sớm vì khiếu nại — lãng phí ngân sách ads.",
                "style": "Nhanh, thích bảng điều kiện, đôi khi bào chữa wording banner “cho dễ hiểu”.",
                "personality": "Hướng tăng trưởng, hợp tác nếu learner giúp giảm ticket, hơi phòng thủ về banner.",
                "trust": 46,
                "patience": 52,
                "coop": 55,
                "note": "Căng nhẹ với POS: Marketing nghĩ POS chưa refresh rule; POS nghĩ banner sai.",
            },
            {
                "file": "pos-supervisor.md",
                "code": "pos-supervisor",
                "name": "Lý Quang Dũng",
                "role": "POS Supervisor",
                "department": "Cửa hàng — Vận hành quầy",
                "order": 2,
                "responsibilities": "Đảm bảo thu ngân áp dụng đúng khuyến mãi tại quầy, xử lý mã lỗi, báo cáo lệch app vs POS cuối ngày.",
                "goals": "Muốn POS hiện cùng điều kiện với app; có thông báo ngắn khi rule đổi.",
                "pain": "Thu ngân bị khách tranh cãi khi mã chỉ áp dụng Áo trong khi banner nói mọi sản phẩm.",
                "concerns": "Sợ nhân viên tự override giảm giá để “giữ khách”; làm lệch doanh thu ca.",
                "style": "Thẳng, lấy ví dụ bill thật, ít kiên nhẫn với slogan Marketing.",
                "personality": "Thực tế, hơi cứng về quy trình quầy, hợp tác khi AC gắn với thao tác POS.",
                "trust": 44,
                "patience": 48,
                "coop": 50,
                "note": "Biết POS cache rule đến 30 phút — fact cần elicit.",
            },
            {
                "file": "cx-specialist.md",
                "code": "cx-specialist",
                "name": "Phan Ngọc Mai",
                "role": "CX Specialist",
                "department": "Chăm sóc khách hàng",
                "order": 3,
                "responsibilities": "Trả lời inbox/hotline về mã giảm giá, tạo ticket khiếu nại, đề xuất bồi hoàn voucher theo guideline nhẹ.",
                "goals": "Muốn FAQ/script khớp điều kiện thật; giảm vòng hỏi lại Marketing.",
                "pain": "Mỗi ngày 40–60 inbox hỏi mã; thiếu checklist điều kiện chuẩn.",
                "concerns": "Bồi hoàn quá nhiều làm Marketing phàn nàn; từ chối khách làm NPS giảm.",
                "style": "Empathy, kể verbatim khách, thích checklist. Hợp tác cao.",
                "personality": "Hòa nhã, cầu nối giữa Marketing và cửa hàng, đôi khi tránh đổ lỗi.",
                "trust": 58,
                "patience": 65,
                "coop": 62,
                "note": "Muốn banner sửa trước; Marketing muốn giữ banner và chỉ sửa FAQ — căng nhẹ ưu tiên.",
            },
        ],
        "knowledge": [
            {
                "file": "K-BEG-PROMO-01-voucher-rules.md",
                "code": "K-BEG-PROMO-01",
                "title": "Cấu hình voucher GIẢM50K (as-is)",
                "description": "Điều kiện thật trên Promotion Engine so với banner.",
                "category": "Policy",
                "importance": "High",
                "access": [
                    {"persona": "marketing-ops", "accessType": "Knows", "isHidden": False},
                    {"persona": "pos-supervisor", "accessType": "Partial", "isHidden": False},
                    {"persona": "cx-specialist", "accessType": "Partial", "isHidden": False},
                ],
                "body": """**Banner:** “Giảm 50K cho đơn từ 499K, mọi sản phẩm”.

**Hệ thống Promotion Engine (as-is):**
- Giảm cố định **50.000đ**
- Min order **499.000đ** (sau giá niêm yết, trước phí ship)
- Chỉ áp dụng danh mục **Áo** (không phải mọi sản phẩm)
- **Không cộng dồn** với mã FREESHIP
- Giới hạn **1 mã / 1 tài khoản / ngày**

Đây là nguồn lệch kỳ vọng khách hàng và script CX.""",
            },
            {
                "file": "K-BEG-PROMO-02-complaint-mix.md",
                "code": "K-BEG-PROMO-02",
                "title": "Cơ cấu khiếu nại mã GIẢM50K",
                "description": "Phân loại inbox CX liên quan voucher trong tuần chiến dịch.",
                "category": "BusinessFact",
                "importance": "High",
                "access": [
                    {"persona": "cx-specialist", "accessType": "Knows", "isHidden": False},
                    {"persona": "marketing-ops", "accessType": "Partial", "isHidden": False},
                ],
                "body": """Tuần mẫu (giả lập), ~280 inbox liên quan GIẢM50K:

| Nhóm khiếu nại | Tỷ lệ | Ghi chú |
| --- | ---: | --- |
| Banner nói mọi SP nhưng đơn có Quần/Váy | 48% | Lệch danh mục |
| Không cộng được với FREESHIP | 22% | Stacking |
| App nhận mã nhưng POS từ chối | 18% | Đồng bộ POS |
| Hết lượt 1 mã/ngày | 12% | Giới hạn tài khoản |

CX đang bồi hoàn soft (voucher 30K) cho ~15% case danh mục — Marketing muốn giảm tỷ lệ này.""",
            },
            {
                "file": "K-BEG-PROMO-03-stacking.md",
                "code": "K-BEG-PROMO-03",
                "title": "Quy ước không cộng dồn với FREESHIP",
                "description": "Hành vi hệ thống khi khách nhập hai mã.",
                "category": "Process",
                "importance": "Medium",
                "access": [
                    {"persona": "marketing-ops", "accessType": "Knows", "isHidden": False},
                    {"persona": "cx-specialist", "accessType": "Knows", "isHidden": False},
                    {"persona": "pos-supervisor", "accessType": "Partial", "isHidden": False},
                ],
                "body": """Khi khách nhập **GIẢM50K** trước, mã **FREESHIP** bị từ chối với thông báo chung “Mã không thể kết hợp”.

Khi nhập FREESHIP trước rồi GIẢM50K, app giữ FREESHIP và báo GIẢM50K không hợp lệ — thứ tự nhập làm khách bối rối.

CX muốn thông báo rõ: “GIẢM50K không dùng cùng FREESHIP; hãy chọn 1 mã.” Banner hiện **không** nhắc điều này.""",
            },
            {
                "file": "K-BEG-PROMO-04-pos-cache.md",
                "code": "K-BEG-PROMO-04",
                "title": "Độ trễ đồng bộ rule POS (cần elicit)",
                "description": "Fact bán ẩn: POS cache cấu hình khuyến mãi đến khoảng 30 phút.",
                "category": "System",
                "importance": "Medium",
                "access": [
                    {"persona": "pos-supervisor", "accessType": "Knows", "isHidden": False},
                    {"persona": "marketing-ops", "accessType": "Partial", "isHidden": True},
                ],
                "body": """Fact cần khơi gợi từ POS Supervisor:

POS cửa hàng **cache** cấu hình khuyến mãi khoảng **30 phút**. Khi Marketing sửa rule trên engine (ví dụ siết danh mục Áo), app online cập nhật gần như ngay, nhưng quầy có thể vẫn áp dụng rule cũ tới chu kỳ refresh tiếp theo.

Khoảng **18%** khiếu nại “app được / POS không” xảy ra trong **30 phút đầu** sau mỗi lần Marketing chỉnh campaign.

POS đề xuất: nút “Làm mới khuyến mãi” cho thu ngân + thông báo nội bộ khi rule đổi.""",
            },
        ],
        "rules": [
            {
                "file": "BR-BEG-PROMO-01-min-order.md",
                "code": "BR-BEG-PROMO-01",
                "title": "Đơn tối thiểu áp dụng GIẢM50K",
                "description": "Voucher chỉ áp dụng khi giá trị hàng đủ điều kiện tối thiểu.",
                "importance": "High",
                "isHidden": False,
                "body": "GIẢM50K chỉ được áp dụng khi tổng tiền hàng (trước phí vận chuyển, sau giá niêm yết) ≥ 499.000đ và các dòng hàng thuộc danh mục được phép.",
            },
            {
                "file": "BR-BEG-PROMO-02-no-stack.md",
                "code": "BR-BEG-PROMO-02",
                "title": "Không cộng dồn với FREESHIP",
                "description": "Một đơn chỉ được chọn GIẢM50K hoặc FREESHIP, không dùng đồng thời.",
                "importance": "High",
                "isHidden": False,
                "body": "Hệ thống phải từ chối kết hợp GIẢM50K với FREESHIP bất kể thứ tự nhập mã, kèm thông báo rõ ràng cho khách và thu ngân.",
            },
            {
                "file": "BR-BEG-PROMO-03-daily-cap.md",
                "code": "BR-BEG-PROMO-03",
                "title": "Giới hạn 1 mã mỗi tài khoản mỗi ngày",
                "description": "Mỗi tài khoản chỉ redeem GIẢM50K một lần trong ngày dương lịch.",
                "importance": "Medium",
                "isHidden": False,
                "body": "Mỗi accountId chỉ được áp dụng thành công GIẢM50K một lần/ngày (00:00–23:59 theo giờ Việt Nam). Lần thứ hai trong ngày phải bị từ chối với lý do hết lượt.",
            },
        ],
        "event": {
            "file": "E1-banner-vs-pos-mismatch.md",
            "title": "Lệch banner và POS trong 30 phút sau khi siết danh mục Áo",
            "description": "Marketing siết rule chỉ còn danh mục Áo lúc 10:00; POS cache cũ khiến một số bill Quần vẫn nhận mã đến ~10:25, trong khi app đã từ chối.",
            "triggerType": "Manual",
            "displayOrder": 1,
            "isActive": True,
            "body": """Lúc **10:00** Marketing cập nhật campaign: chỉ còn danh mục **Áo**. App online áp dụng ngay. Một số POS vẫn cache rule cũ đến khoảng **10:25**, khiến vài bill có Quần vẫn giảm 50K tại quầy.

CX nhận thêm inbox “tại sao app không được mà bạn tôi mua tại cửa hàng lại được?”. Chưa phải sự cố gian lận lớn; chủ yếu lệch trải nghiệm đa kênh.

Yêu cầu learner (Beginner): phân tích tác động nhẹ, cập nhật as-is/to-be về đồng bộ điều kiện, thêm **1 AC** liên quan refresh/thông báo POS — không yêu cầu crisis promo stacking Intermediate.""",
        },
        "rubric": {
            "title": "Rubric Beginner — MegaFashion Starter (BEG-PROMO-01)",
            "description": "Tiêu chí đánh giá deliverable Beginner về điều kiện voucher và đồng bộ banner/app/POS.",
            "passingScore": 60.0,
            "body": """# Rubric Beginner MegaFashion Starter

Chấm BAP ngắn, stakeholder map, interview notes, bảng điều kiện voucher, chỗ lệch banner/POS, 3 user stories + AC. Không chấm engine loyalty phức tạp.

## Thang điểm
- **85–100**: Điều kiện và AC định lượng rõ, có chỗ lệch đa kênh.
- **60–84**: Đủ phần bắt buộc.
- **<60**: Thiếu điều kiện cốt lõi hoặc không truy vết phỏng vấn.""",
            "criteria": [
                {
                    "code": "COMPLETENESS",
                    "name": "Tính đầy đủ deliverable Beginner (Completeness)",
                    "description": "Có BAP ngắn, stakeholder map 3 bên, interview notes, bảng điều kiện as-is/to-be, 3 user stories + AC.",
                },
                {
                    "code": "FEASIBILITY",
                    "name": "Tính khả thi trong phạm vi Beginner (Feasibility)",
                    "description": "Khớp BR-BEG-PROMO-*, không đổi giá gốc hàng loạt hay redesign engine điểm thưởng.",
                },
                {
                    "code": "TRACEABILITY",
                    "name": "Truy vết từ phỏng vấn (Traceability)",
                    "description": "AC/user story dẫn về notes Marketing / POS / CX (kể cả fact cache POS nếu đã elicit).",
                },
                {
                    "code": "CLARITY",
                    "name": "Rõ ràng và nhất quán (Clarity)",
                    "description": "Wording điều kiện thống nhất giữa banner đề xuất, app và POS; AC không mâu thuẫn stacking.",
                },
            ],
        },
    },
]


def render_index(b: dict[str, Any], base: Path) -> None:
    free_yaml = "true" if b["free"] else "false"
    personas_rows = "\n".join(
        f"| `{p['code']}` | {p['role']} | {p['name']} | [personas/{p['file']}](personas/{p['file']}) |"
        for p in b["personas"]
    )
    knowledge_rows = "\n".join(
        f"| `{k['code']}` | {k['title']} | {k['category']} | {k['importance']} | [knowledge/{k['file']}](knowledge/{k['file']}) |"
        for k in b["knowledge"]
    )
    rules_rows = "\n".join(
        f"| `{r['code']}` | {r['title']} | {r['importance']} | [rules/{r['file']}](rules/{r['file']}) |"
        for r in b["rules"]
    )
    ev = b["event"]
    rub = b["rubric"]

    write(
        base / "index.md",
        f"""---
type: index
title: {q(b['title'] + ' (v1)')}
description: {q(b['description'])}
tags: {b['tags']}
baves:
  bundleVersion: 1
  scenarioCode: {b['code']}
  industryCode: {b['industryCode']}
  industryName: {q(b['industryName'])}
  applicationCode: {b['applicationCode']}
  applicationName: {q(b['applicationName'])}
  isFreeAccess: {free_yaml}
---
# {b['folder']} v1

Bundle Beginner OKF đầy đủ — nguồn markdown để import vào database. Dữ liệu doanh nghiệp là giả lập phục vụ đào tạo BA.

## Scenario
- [Bối cảnh và nhiệm vụ](scenario.md)

## Stakeholders
| Code | Vai trò | Tên | File |
| --- | --- | --- | --- |
{personas_rows}

## Knowledge
| Code | Tiêu đề | Phân loại | Tầm quan trọng | File |
| --- | --- | --- | --- | --- |
{knowledge_rows}

## Rules
| Code | Tiêu đề | Tầm quan trọng | File |
| --- | --- | --- | --- |
{rules_rows}

## Events
| File | Tiêu đề |
| --- | --- |
| [events/{ev['file']}](events/{ev['file']}) | {ev['title']} |

## Rubric
- [{rub['title']}](rubric/rubric.md)

## Quy ước
- Frontmatter YAML: `type`, `title`, `description`, `tags`; trường BA-VES nằm dưới `baves:`.
- Persona: `baves.public` hiển thị cho Learner; `baves.private` chỉ gửi AI.
- Knowledge: quyền truy cập khai báo ở `baves.access[]`.
- Beginner scope: BAP ngắn, stakeholder map, interview notes, as-is/to-be, 3 user stories + AC — không yêu cầu BRD/SRS/RTM đầy đủ.
""",
    )


def render_scenario(b: dict[str, Any], base: Path) -> None:
    free_yaml = "true" if b["free"] else "false"
    c = b["context"]
    write(
        base / "scenario.md",
        f"""---
type: scenario
title: {q(b['title'])}
description: {q(b['description'])}
tags: {b['tags']}
baves:
  code: {b['code']}
  versionNo: 1
  difficulty: Beginner
  isFreeAccess: {free_yaml}
  status: Published
  summary: {q(c['summary'])}
  companyName: {q(c['companyName'])}
  businessContext: {q(fold(c['businessContext']))}
  businessProblem: {q(fold(c['businessProblem']))}
  businessObjectives: {q(fold(c['businessObjectives']))}
  baRole: {q(fold(c['baRole']))}
  assignment: {q(fold(c['assignment']))}
  inScope: {q(fold(c['inScope']))}
  outOfScope: {q(fold(c['outOfScope']))}
  knownConstraints: {q(fold(c['knownConstraints']))}
---
Ghi chú: Beginner starter scenario đầy đủ tính năng OKF (personas, knowledge, rules, events, rubric).
`isFreeAccess` chỉ có hiệu lực với Difficulty = Beginner. Không đặt đáp án chi tiết vào đây — nội dung `baves` hiển thị cho Learner qua context API.
""",
    )


def render_persona(b: dict[str, Any], p: dict[str, Any], base: Path) -> None:
    write(
        base / "personas" / p["file"],
        f"""---
type: persona
title: {q(p['role'])}
description: {q(f"Stakeholder Beginner cho kịch bản {b['code']} — {p['role']}.")}
tags: [beginner, starter]
baves:
  code: {p['code']}
  name: {q(p['name'])}
  stakeholderRole: {q(p['role'])}
  department: {q(p['department'])}
  displayOrder: {p['order']}
  isActive: true
  public:
    responsibilities: {q(p['responsibilities'])}
  private:
    goals: >
      {p['goals']}
    painPoints: >
      {p['pain']}
    businessConcerns: >
      {p['concerns']}
    communicationStyle: >
      {p['style']}
    personality: >
      {p['personality']}
  initialState:
    trustLevel: {p['trust']}
    patienceLevel: {p['patience']}
    cooperationLevel: {p['coop']}
---
Ghi chú soạn thảo:
- {p['note']}
- Mức trust/patience/cooperation Beginner-friendly (~40–60), hơi hợp tác để học viên luyện phỏng vấn.
""",
    )


def render_knowledge(k: dict[str, Any], base: Path) -> None:
    access_yaml = "\n".join(
        "    - persona: {p}\n      accessType: {t}\n      isHidden: {h}".format(
            p=a["persona"],
            t=a["accessType"],
            h="true" if a["isHidden"] else "false",
        )
        for a in k["access"]
    )
    write(
        base / "knowledge" / k["file"],
        f"""---
type: knowledge
title: {q(k['title'])}
description: {q(k['description'])}
tags: [beginner, {k['category'].lower()}]
baves:
  code: {k['code']}
  category: {k['category']}
  importance: {k['importance']}
  access:
{access_yaml}
---
{k['body'].strip()}
""",
    )


def render_rule(r: dict[str, Any], base: Path) -> None:
    write(
        base / "rules" / r["file"],
        f"""---
code: {r['code']}
title: {q(r['title'])}
description: {q(r['description'])}
importance: {r['importance']}
isHidden: {"true" if r['isHidden'] else "false"}
---
{r['body'].strip()}
""",
    )


def render_event(ev: dict[str, Any], base: Path) -> None:
    write(
        base / "events" / ev["file"],
        f"""---
title: {q(ev['title'])}
description: {q(ev['description'])}
triggerType: {ev['triggerType']}
displayOrder: {ev['displayOrder']}
isActive: {"true" if ev['isActive'] else "false"}
---
{ev['body'].strip()}
""",
    )


def render_rubric(rub: dict[str, Any], base: Path) -> None:
    crit_lines: list[str] = []
    for c in rub["criteria"]:
        crit_lines.append(f"  - code: {c['code']}")
        crit_lines.append(f"    name: {q(c['name'])}")
        crit_lines.append(f"    description: {q(c['description'])}")
        crit_lines.append("    weight: 1.0")
        crit_lines.append("    maxScore: 100.0")
    criteria_yaml = "\n".join(crit_lines)
    write(
        base / "rubric" / "rubric.md",
        f"""---
title: {q(rub['title'])}
description: {q(rub['description'])}
passingScore: {rub['passingScore']}
criteria:
{criteria_yaml}
---
{rub['body'].strip()}
""",
    )


def render_bundle(b: dict[str, Any]) -> Path:
    base = SCENARIOS / b["folder"] / "v1"
    base.mkdir(parents=True, exist_ok=True)
    clear_bundle_dirs(base)

    render_index(b, base)
    render_scenario(b, base)
    for p in b["personas"]:
        render_persona(b, p, base)
    for k in b["knowledge"]:
        render_knowledge(k, base)
    for r in b["rules"]:
        render_rule(r, base)
    render_event(b["event"], base)
    render_rubric(b["rubric"], base)
    return base


def main() -> int:
    bundles = [render_bundle(b) for b in BUNDLES]
    print(f"Created {len(bundles)} OKF bundles under {SCENARIOS}")

    import_okf = ROOT / "import_okf.py"
    out_sql = ROOT / "database" / "seed-beginner-from-okf.sql"
    parts: list[str] = [
        "-- Auto-generated beginner starter scenarios from OKF markdown",
        "SET NOCOUNT ON;",
        "GO",
    ]
    for bundle in bundles:
        single = ROOT / "database" / f"seed-{bundle.parent.name}-from-okf.sql"
        subprocess.check_call([sys.executable, str(import_okf), str(bundle), "-o", str(single)])
        parts.append(single.read_text(encoding="utf-8"))
        parts.append("GO")
    out_sql.write_text("\n".join(parts), encoding="utf-8")
    print(f"Wrote {out_sql}")
    for bundle in bundles:
        print(f"  - {bundle}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
