import argparse
import json
import re
import sys
import unicodedata
from pathlib import Path
from typing import Any, Dict, List, Optional, Tuple

import yaml


# Danh mục ánh xạ mặc định cho các Industry / Application phổ biến khi file OKF chỉ khai báo tên ngắn
INDUSTRY_PRESETS: Dict[str, Tuple[str, str, str]] = {
    "retail": (
        "SUPPLY_CHAIN",
        "Chuỗi cung ứng & Bán lẻ",
        "Logistics, kho vận, bán lẻ đa kênh Omni-channel",
    ),
    "supply_chain": (
        "SUPPLY_CHAIN",
        "Chuỗi cung ứng & Bán lẻ",
        "Logistics, kho vận, bán lẻ đa kênh Omni-channel",
    ),
    "banking": (
        "BANKING",
        "Tài chính & Ngân hàng",
        "Ngân hàng số, tín dụng, thanh toán và quản trị rủi ro tài chính",
    ),
    "finance": (
        "FINANCE",
        "Tài chính & Đầu tư",
        "Dịch vụ tài chính, cổng thanh toán, kế toán và đối soát giao dịch",
    ),
    "healthcare": (
        "HEALTHCARE",
        "Y tế & Chăm sóc sức khỏe",
        "Bệnh viện số, hồ sơ bệnh án điện tử (EMR/EHR) và đặt lịch khám",
    ),
    "education": (
        "EDUCATION",
        "Giáo dục & Đào tạo",
        "Hệ thống quản lý học tập (LMS), tuyển sinh và đào tạo trực tuyến",
    ),
    "logistics": (
        "LOGISTICS",
        "Vận tải & Giao nhận",
        "Quản lý đội xe, điều phối giao hàng chặng cuối và kho vận",
    ),
    "ecommerce": (
        "ECOMMERCE",
        "Thương mại điện tử",
        "Sàn giao dịch thương mại điện tử, giỏ hàng và khuyến mãi đa kênh",
    ),
    "manufacturing": (
        "MANUFACTURING",
        "Sản xuất công nghiệp",
        "Hoạch định vật tư, quản lý dây chuyền sản xuất (MES) và kiểm soát chất lượng",
    ),
    "insurance": (
        "INSURANCE",
        "Bảo hiểm",
        "Thẩm định hồ sơ, cấp đơn và giải quyết quyền lợi bảo hiểm (Claims)",
    ),
}

APPLICATION_PRESETS: Dict[str, Tuple[str, str, str]] = {
    "order_management": (
        "OMS",
        "Order Management System",
        "Hệ thống quản lý đơn hàng đa kênh",
    ),
    "oms": (
        "OMS",
        "Order Management System",
        "Hệ thống quản lý đơn hàng đa kênh",
    ),
    "warehouse_management": (
        "WMS",
        "Warehouse Management System",
        "Hệ thống quản lý kho vận và kiểm soát tồn kho",
    ),
    "core_banking": (
        "CORE_BANKING",
        "Core Banking & Digital Lending",
        "Hệ thống ngân hàng lõi và phê duyệt khoản vay số",
    ),
    "hospital_information_system": (
        "HIS",
        "Hospital Information System",
        "Hệ thống thông tin quản lý bệnh viện và bệnh án điện tử",
    ),
    "learning_management_system": (
        "LMS",
        "Learning Management System",
        "Hệ thống quản lý học tập và đánh giá năng lực người học",
    ),
    "crm": (
        "CRM",
        "Customer Relationship Management",
        "Hệ thống quản trị quan hệ và chăm sóc khách hàng",
    ),
    "erp": (
        "ERP",
        "Enterprise Resource Planning",
        "Hệ thống hoạch định nguồn lực doanh nghiệp",
    ),
}

BABOK_AREA_PRESETS: Dict[str, str] = {
    "BAPL": "Business Analysis Planning and Monitoring",
    "ELIC": "Elicitation and Collaboration",
    "RLCM": "Requirements Life Cycle Management",
    "STRAT": "Strategy Analysis",
    "RADD": "Requirements Analysis and Design Definition",
    "SOLE": "Solution Evaluation",
}


def slugify_code(raw: str, fallback: str = "DEFAULT", max_len: int = 80) -> str:
    """Chuyển chuỗi bất kỳ thành mã CODE chuẩn IN_HOA_GACH_DUOI an toàn cho SQL."""
    if not raw or not str(raw).strip():
        return fallback
    normalized = unicodedata.normalize("NFKD", str(raw).strip())
    ascii_text = normalized.encode("ascii", "ignore").decode("ascii")
    cleaned = re.sub(r"[^A-Za-z0-9]+", "_", ascii_text).strip("_").upper()
    return (cleaned or fallback)[:max_len]


def safe_sql_ident(raw: str, max_len: int = 60) -> str:
    """Tạo hậu tố tên biến T-SQL an toàn (chỉ gồm chữ, số và dấu gạch dưới)."""
    slug = slugify_code(raw, fallback="VAR", max_len=max_len)
    if slug and slug[0].isdigit():
        slug = "V_" + slug
    return slug


def esc(value: Optional[Any]) -> str:
    """Escape chuỗi sang hằng Unicode N'...' trong SQL Server."""
    if value is None:
        return "NULL"
    text = str(value)
    return "N'" + text.replace("'", "''") + "'"


SPRINT_HEADER_RE = re.compile(
    r"\bSprint\s*(\d+)\s*"
    r"(?:\|\s*(?:Ends:\s*(\d{4}-\d{2}-\d{2}(?:[T ]\d{2}:\d{2}(?::\d{2})?)?)|Duration:\s*(\d+)\s*([dw]|days?|weeks?))?)?\s*"
    r":\s*",
    re.IGNORECASE,
)


def _normalize_ends_at(raw: Any) -> Optional[str]:
    if raw is None:
        return None
    text = str(raw).strip()
    if not text:
        return None
    return text[:10] if len(text) >= 10 else text


def _parse_assignment_legacy_text(text: str) -> List[Dict[str, Any]]:
    """Tách 'Sprint 1: ... Sprint 2: ...' (kể cả cùng một dòng) thành list sprint."""
    text = (text or "").strip()
    if not text:
        return [{"sprintNo": 1, "focus": "", "endsAt": None, "templateCodes": []}]

    headers = list(SPRINT_HEADER_RE.finditer(text))
    if not headers:
        return [{
            "sprintNo": 1,
            "focus": text,
            "durationValue": 1,
            "durationUnit": "weeks",
            "templateCodes": [],
        }]

    by_no: Dict[int, Dict[str, Any]] = {}
    for i, match in enumerate(headers):
        try:
            sprint_no = int(match.group(1))
        except (TypeError, ValueError):
            continue
        if sprint_no < 1:
            continue
        start = match.end()
        end = headers[i + 1].start() if i + 1 < len(headers) else len(text)
        body = text[start:end].strip().rstrip("-• \t\r\n")
        by_no[sprint_no] = {
            "sprintNo": sprint_no,
            "focus": body,
            "durationValue": 1,
            "durationUnit": "weeks",
            "templateCodes": [],
        }

    if not by_no:
        return [{
            "sprintNo": 1,
            "focus": text,
            "durationValue": 1,
            "durationUnit": "weeks",
            "templateCodes": [],
        }]
    return [by_no[k] for k in sorted(by_no.keys())]


def _sprints_from_yaml_list(raw_list: Any) -> List[Dict[str, Any]]:
    if not isinstance(raw_list, list) or not raw_list:
        return []
    out: List[Dict[str, Any]] = []
    for idx, item in enumerate(raw_list):
        if isinstance(item, str):
            out.append(
                {
                    "sprintNo": idx + 1,
                    "focus": item.strip(),
                    "endsAt": None,
                    "templateCodes": [],
                }
            )
            continue
        if not isinstance(item, dict):
            continue
        codes = item.get("templateCodes") or item.get("deliverables") or []
        if isinstance(codes, str):
            codes = [c.strip() for c in re.split(r"[,|]", codes) if c.strip()]
        elif not isinstance(codes, list):
            codes = []
        duration_value = item.get("durationValue") or item.get("duration") or 1
        try:
            duration_value = max(1, int(duration_value))
        except (TypeError, ValueError):
            duration_value = 1
        unit_raw = str(item.get("durationUnit") or item.get("unit") or "weeks").strip().lower()
        duration_unit = "days" if unit_raw.startswith("d") else "weeks"
        out.append(
            {
                "sprintNo": idx + 1,
                "focus": str(item.get("focus") or item.get("tasks") or item.get("description") or "").strip(),
                "durationValue": duration_value,
                "durationUnit": duration_unit,
                "templateCodes": [str(c).strip().upper() for c in codes if str(c).strip()],
            }
        )
    return out


def normalize_sprints_list(
    assignment_raw: Any,
    sprints_yaml: Any = None,
) -> List[Dict[str, Any]]:
    """
    Parse sprint từ YAML / assignment legacy → list chuẩn.
    Lưu vào bảng scenario.ScenarioSprints (không nhét JSON vào Assignment).
    """
    from_yaml = _sprints_from_yaml_list(sprints_yaml)
    if from_yaml:
        sprints = from_yaml
    else:
        text = str(assignment_raw or "").strip()
        if text.startswith("{") or text.startswith("["):
            try:
                parsed = json.loads(text)
                if isinstance(parsed, dict) and isinstance(parsed.get("sprints"), list):
                    sprints = _sprints_from_yaml_list(parsed["sprints"])
                elif isinstance(parsed, list):
                    sprints = _sprints_from_yaml_list(parsed)
                else:
                    sprints = _parse_assignment_legacy_text(text)
            except json.JSONDecodeError:
                sprints = _parse_assignment_legacy_text(text)
        else:
            sprints = _parse_assignment_legacy_text(text)

    if not sprints:
        sprints = [{
            "sprintNo": 1,
            "focus": str(assignment_raw or "").strip() if not (
                str(assignment_raw or "").strip().startswith("{")
                or str(assignment_raw or "").strip().startswith("[")
            ) else "",
            "durationValue": 1,
            "durationUnit": "weeks",
            "templateCodes": [],
        }]

    normalized = []
    for idx, s in enumerate(sprints):
        try:
            duration_value = max(1, int(s.get("durationValue") or 1))
        except (TypeError, ValueError):
            duration_value = 1
        unit_raw = str(s.get("durationUnit") or "weeks").strip().lower()
        duration_unit = "days" if unit_raw.startswith("d") else "weeks"
        codes = list(s.get("templateCodes") or [])
        normalized.append(
            {
                "sprintNo": idx + 1,
                "focus": str(s.get("focus") or "").strip(),
                "durationValue": duration_value,
                "durationUnit": duration_unit,
                "templateCodes": codes,
            }
        )
    return normalized


def build_assignment_brief(sprints: List[Dict[str, Any]], assignment_raw: Any = None) -> str:
    """Free-text Assignment brief — không echo danh sách sprint."""
    text = str(assignment_raw or "").strip()
    if not text or text.startswith("{") or text.startswith("["):
        return ""
    # Dump "Sprint N:" → để trống (nội dung đã vào ScenarioSprints).
    if re.search(r"(?i)\bSprint\s*\d+\s*(?:\([^)]*\))?\s*:", text):
        return ""
    return text


def normalize_assignment_json(
    assignment_raw: Any,
    sprints_yaml: Any = None,
) -> str:
    """Deprecated: giữ API cũ — trả brief free-text (không còn JSON sprint)."""
    sprints = normalize_sprints_list(assignment_raw, sprints_yaml)
    return build_assignment_brief(sprints, assignment_raw)


def parse_md(path: Path) -> Tuple[Dict[str, Any], str]:
    """Đọc file Markdown có YAML frontmatter (`--- ... ---`)."""
    content = path.read_text(encoding="utf-8")
    if content.startswith("---"):
        parts = content.split("---", 2)
        if len(parts) >= 3:
            meta = yaml.safe_load(parts[1]) or {}
            if not isinstance(meta, dict):
                meta = {}
            return meta, parts[2].strip()
    return {}, content.strip()


def resolve_industry(baves_idx: Dict[str, Any], baves_sc: Dict[str, Any]) -> Tuple[str, str, str]:
    """Trích xuất (IndustryCode, IndustryName, IndustryDescription) từ frontmatter."""
    raw_industry = (
        baves_idx.get("industry")
        or baves_sc.get("industry")
        or baves_idx.get("industryName")
        or baves_sc.get("industryName")
        or "General Business"
    )
    explicit_code = baves_idx.get("industryCode") or baves_sc.get("industryCode")
    explicit_name = baves_idx.get("industryName") or baves_sc.get("industryName")
    explicit_desc = baves_idx.get("industryDescription") or baves_sc.get("industryDescription")

    lookup_key = slugify_code(str(raw_industry)).lower()
    if lookup_key in INDUSTRY_PRESETS:
        preset_code, preset_name, preset_desc = INDUSTRY_PRESETS[lookup_key]
        return (
            str(explicit_code or preset_code).strip(),
            str(explicit_name or preset_name).strip(),
            str(explicit_desc or preset_desc).strip(),
        )

    code = str(explicit_code or slugify_code(str(raw_industry), "GENERAL")).strip()
    name = str(explicit_name or raw_industry).strip()
    desc = str(explicit_desc or f"Lĩnh vực {name}").strip()
    return code, name, desc


def resolve_application(
    baves_idx: Dict[str, Any],
    baves_sc: Dict[str, Any],
    scenario_title: str,
) -> Tuple[str, str, str]:
    """Trích xuất (ApplicationCode, ApplicationName, ApplicationDescription) từ frontmatter."""
    raw_app = (
        baves_idx.get("application")
        or baves_sc.get("application")
        or baves_idx.get("applicationName")
        or baves_sc.get("applicationName")
        or "Business Application"
    )
    explicit_code = baves_idx.get("applicationCode") or baves_sc.get("applicationCode")
    explicit_name = baves_idx.get("applicationName") or baves_sc.get("applicationName")
    explicit_desc = baves_idx.get("applicationDescription") or baves_sc.get("applicationDescription")

    lookup_key = slugify_code(str(raw_app)).lower()
    if lookup_key in APPLICATION_PRESETS:
        preset_code, preset_name, preset_desc = APPLICATION_PRESETS[lookup_key]
        company_name = baves_sc.get("companyName") or ""
        default_desc = f"{preset_desc} {company_name}".strip() if company_name else preset_desc
        return (
            str(explicit_code or preset_code).strip(),
            str(explicit_name or preset_name).strip(),
            str(explicit_desc or default_desc).strip(),
        )

    code = str(explicit_code or slugify_code(str(raw_app), "APP")).strip()
    name = str(explicit_name or raw_app).strip()
    desc = str(explicit_desc or f"Hệ thống {name} ({scenario_title})").strip()
    return code, name, desc


def infer_version_no(bundle_dir: Path, baves_idx: Dict[str, Any], baves_sc: Dict[str, Any]) -> int:
    """Xác định VersionNo từ frontmatter hoặc tên thư mục (vd: v1, v2)."""
    raw_ver = baves_sc.get("versionNo") or baves_idx.get("bundleVersion") or baves_idx.get("versionNo")
    if raw_ver is not None:
        try:
            return max(1, int(raw_ver))
        except (ValueError, TypeError):
            pass

    match = re.match(r"^[vV](\d+)$", bundle_dir.name.strip())
    if match:
        return max(1, int(match.group(1)))
    return 1


def generate_scenario_sql_block(bundle_dir: Path, include_transaction: bool = True) -> Tuple[str, Dict[str, Any]]:
    """
    Đọc một thư mục bundle OKF (chứa index.md, scenario.md, personas/, knowledge/, rules/, events/, rubric/)
    và sinh đoạn mã T-SQL idempotent cho kịch bản đó.
    Trả về (sql_text, summary_info).
    """
    index_file = bundle_dir / "index.md"
    scenario_file = bundle_dir / "scenario.md"

    if not index_file.exists() or not scenario_file.exists():
        raise FileNotFoundError(
            f"Bundle tại '{bundle_dir}' thiếu file bắt buộc 'index.md' hoặc 'scenario.md'."
        )

    index_meta, _ = parse_md(index_file)
    scenario_meta, scenario_body = parse_md(scenario_file)

    baves_idx = index_meta.get("baves", {}) if isinstance(index_meta.get("baves"), dict) else {}
    baves_sc = scenario_meta.get("baves", {}) if isinstance(scenario_meta.get("baves"), dict) else {}

    default_slug = bundle_dir.parent.name if re.match(r"^[vV]\d+$", bundle_dir.name) else bundle_dir.name
    scenario_code = str(
        baves_sc.get("code")
        or baves_idx.get("scenarioCode")
        or baves_idx.get("code")
        or slugify_code(default_slug, "SCENARIO-001")
    ).strip()

    version_no = infer_version_no(bundle_dir, baves_idx, baves_sc)
    var_suffix = f"{safe_sql_ident(scenario_code)}_V{version_no}"

    scenario_title = str(
        scenario_meta.get("title")
        or index_meta.get("title")
        or baves_sc.get("title")
        or default_slug
    ).strip()
    scenario_desc = str(
        scenario_meta.get("description")
        or index_meta.get("description")
        or ""
    ).strip()

    industry_code, industry_name, industry_desc = resolve_industry(baves_idx, baves_sc)
    app_code, app_name, app_desc = resolve_application(baves_idx, baves_sc, scenario_title)

    summary = str(baves_sc.get("summary") or scenario_desc or scenario_title).strip()
    business_context = str(baves_sc.get("businessContext") or scenario_body or "").strip()
    business_problem = str(baves_sc.get("businessProblem") or "").strip()
    business_objectives = str(baves_sc.get("businessObjectives") or "").strip()
    ba_role = str(baves_sc.get("baRole") or "Business Analyst").strip()
    # Sprint → bảng ScenarioSprints; cột Assignment scenario để trống.
    sprints_rows = normalize_sprints_list(
        baves_sc.get("assignment"),
        baves_sc.get("sprints") or baves_idx.get("sprints"),
    )
    assignment = ""
    in_scope = str(baves_sc.get("inScope") or "").strip() or None
    out_of_scope = str(baves_sc.get("outOfScope") or "").strip() or None
    known_constraints = str(baves_sc.get("knownConstraints") or "").strip() or None
    difficulty = str(baves_sc.get("difficulty") or "Intermediate").strip()
    status = str(baves_sc.get("status") or "Published").strip()
    is_published = 1 if status.lower() == "published" else int(bool(baves_sc.get("isPublished", True)))
    # Chỉ Beginner mới được gắn miễn phí; Intermediate/Advanced luôn false
    is_free_raw = baves_sc.get("isFreeAccess", baves_idx.get("isFreeAccess", False))
    is_free_access = 1 if (str(difficulty).lower() == "beginner" and bool(is_free_raw)) else 0

    ind_var = f"@IndustryId_{var_suffix}"
    app_var = f"@ApplicationId_{var_suffix}"
    scen_var = f"@ScenarioId_{var_suffix}"
    ver_var = f"@ScenarioVersionId_{var_suffix}"
    instr_var = f"@InstructorUserId_{var_suffix}"

    lines: List[str] = [
        "-- ====================================================",
        f"-- BA-VES SEED DATA: {scenario_title.upper()} ({scenario_code} - v{version_no})",
        f"-- Source OKF Bundle: {bundle_dir.as_posix()}",
        "-- Auto-generated by import_okf.py",
        "-- ====================================================",
    ]

    if include_transaction:
        lines.extend([
            "SET QUOTED_IDENTIFIER ON;",
            "SET ANSI_NULLS ON;",
            "SET NOCOUNT ON;",
            "BEGIN TRANSACTION;",
            "",
        ])

    lines.extend([
        "-- 0. Ensure Default Instructor User exists (Schema: identity)",
        f"DECLARE {instr_var} int;",
        f"SELECT TOP 1 {instr_var} = [Id] FROM [identity].[Users] ORDER BY [Id];",
        f"IF {instr_var} IS NULL",
        "BEGIN",
        "    INSERT INTO [identity].[Users] (",
        "        [FullName], [Status], [CreatedAt], [UserName], [NormalizedUserName],",
        "        [Email], [NormalizedEmail], [EmailConfirmed], [PhoneNumberConfirmed],",
        "        [TwoFactorEnabled], [LockoutEnabled], [AccessFailedCount]",
        "    ) VALUES (",
        "        N'System Instructor', N'Active', SYSDATETIMEOFFSET(), N'instructor@baves.local', N'INSTRUCTOR@BAVES.LOCAL',",
        "        N'instructor@baves.local', N'INSTRUCTOR@BAVES.LOCAL', 1, 0, 0, 1, 0",
        "    );",
        f"    SET {instr_var} = SCOPE_IDENTITY();",
        "END;",
        "",
        "-- 1. Seed or Get Industry & Application (Schema: content)",
        f"DECLARE {ind_var} int;",
        f"SELECT {ind_var} = [Id] FROM [content].[Industries] WHERE [Code] = {esc(industry_code)};",
        f"IF {ind_var} IS NULL",
        "BEGIN",
        "    INSERT INTO [content].[Industries] ([Code], [Name], [Description], [Status], [DisplayOrder], [CreatedAt])",
        f"    VALUES ({esc(industry_code)}, {esc(industry_name)}, {esc(industry_desc)}, N'Active', 1, SYSDATETIMEOFFSET());",
        f"    SET {ind_var} = SCOPE_IDENTITY();",
        "END;",
        "",
        f"DECLARE {app_var} int;",
        f"SELECT {app_var} = [Id] FROM [content].[Applications] WHERE [IndustryId] = {ind_var} AND [Code] = {esc(app_code)};",
        f"IF {app_var} IS NULL",
        "BEGIN",
        "    INSERT INTO [content].[Applications] ([IndustryId], [Code], [Name], [Description], [Status], [DisplayOrder], [CreatedAt])",
        f"    VALUES ({ind_var}, {esc(app_code)}, {esc(app_name)}, {esc(app_desc)}, N'Active', 1, SYSDATETIMEOFFSET());",
        f"    SET {app_var} = SCOPE_IDENTITY();",
        "END;",
        "",
        f"-- 2. Seed Scenario & Version {version_no} (Schema: scenario)",
        f"DECLARE {scen_var} int;",
        f"SELECT {scen_var} = [Id] FROM [scenario].[Scenarios] WHERE [Code] = {esc(scenario_code)};",
        f"IF {scen_var} IS NULL",
        "BEGIN",
        "    INSERT INTO [scenario].[Scenarios] ([ApplicationId], [CreatedByUserId], [Code], [Title], [Description], [Status], [CreatedAt])",
        f"    VALUES ({app_var}, {instr_var}, {esc(scenario_code)}, {esc(scenario_title)}, {esc(scenario_desc)}, {esc(status)}, SYSDATETIMEOFFSET());",
        f"    SET {scen_var} = SCOPE_IDENTITY();",
        "END;",
        "",
        f"DECLARE {ver_var} int;",
        f"SELECT {ver_var} = [Id] FROM [scenario].[ScenarioVersions] WHERE [ScenarioId] = {scen_var} AND [VersionNo] = {version_no};",
        f"IF {ver_var} IS NULL",
        "BEGIN",
        "    INSERT INTO [scenario].[ScenarioVersions] (",
        "        [ScenarioId], [VersionNo], [Summary], [BusinessContext], [BusinessProblem], [BusinessObjectives],",
        "        [BARole], [Assignment], [InScope], [OutOfScope], [KnownConstraints], [Difficulty], [IsFreeAccess],",
        "        [Status], [IsPublished], [PublishedAt], [CreatedAt]",
        "    ) VALUES (",
        f"        {scen_var}, {version_no}, {esc(summary)}, {esc(business_context)}, {esc(business_problem)}, {esc(business_objectives)},",
        f"        {esc(ba_role)}, {esc(assignment)}, {esc(in_scope)}, {esc(out_of_scope)}, {esc(known_constraints)}, {esc(difficulty)}, {is_free_access},",
        f"        {esc(status)}, {is_published}, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()",
        "    );",
        f"    SET {ver_var} = SCOPE_IDENTITY();",
        "END;",
        "ELSE",
        "BEGIN",
        f"    UPDATE [scenario].[ScenarioVersions] SET",
        f"        [Summary] = {esc(summary)},",
        f"        [BusinessContext] = {esc(business_context)},",
        f"        [BusinessProblem] = {esc(business_problem)},",
        f"        [BusinessObjectives] = {esc(business_objectives)},",
        f"        [BARole] = {esc(ba_role)},",
        f"        [Assignment] = {esc(assignment)},",
        f"        [InScope] = {esc(in_scope)},",
        f"        [OutOfScope] = {esc(out_of_scope)},",
        f"        [KnownConstraints] = {esc(known_constraints)},",
        f"        [Difficulty] = {esc(difficulty)}, [IsFreeAccess] = {is_free_access},",
        f"        [Status] = {esc(status)}, [IsPublished] = {is_published}",
        f"    WHERE [Id] = {ver_var};",
        "END;",
        "",
        "-- 2b. Seed ScenarioSprints (tách khỏi Assignment)",
        f"DELETE FROM [scenario].[ScenarioSprints] WHERE [ScenarioVersionId] = {ver_var};",
    ])

    for sp in sprints_rows:
        codes = ",".join(str(c).strip().upper() for c in (sp.get("templateCodes") or []) if str(c).strip())
        codes_sql = esc(codes) if codes else "NULL"
        lines.extend([
            "INSERT INTO [scenario].[ScenarioSprints] (",
            "    [ScenarioVersionId], [SprintNo], [Focus], [DurationValue], [DurationUnit], [TemplateCodes]",
            ") VALUES (",
            f"    {ver_var}, {int(sp['sprintNo'])}, {esc(sp.get('focus') or '')}, "
            f"{int(sp['durationValue'])}, {esc(sp['durationUnit'])}, {codes_sql}",
            ");",
        ])
    lines.append("")

    # 3. Seed Stakeholder Personas
    lines.append("-- 3. Seed Stakeholder Personas")
    personas_dir = bundle_dir / "personas"
    persona_lookup: Dict[str, str] = {}
    personas_count = 0
    if personas_dir.exists():
        display_order = 1
        for p_file in sorted(personas_dir.glob("*.md")):
            meta, body = parse_md(p_file)
            bv = meta.get("baves", {}) if isinstance(meta.get("baves"), dict) else {}
            pub = bv.get("public", {}) if isinstance(bv.get("public"), dict) else {}
            priv = bv.get("private", {}) if isinstance(bv.get("private"), dict) else {}

            stem_code = p_file.stem
            explicit_p_code = str(bv.get("code") or meta.get("code") or stem_code).strip()
            name = str(bv.get("name") or meta.get("name") or meta.get("title") or stem_code).strip()
            role = str(
                bv.get("stakeholderRole")
                or bv.get("role")
                or meta.get("stakeholderRole")
                or meta.get("title")
                or "Stakeholder"
            ).strip()
            dept = str(bv.get("department") or pub.get("department") or meta.get("department") or "").strip() or None
            resp = str(
                pub.get("responsibilities")
                or bv.get("responsibilities")
                or meta.get("responsibilities")
                or body
                or ""
            ).strip() or None
            goals = str(priv.get("goals") or bv.get("goals") or meta.get("goals") or "").strip() or None
            pains = str(priv.get("painPoints") or bv.get("painPoints") or meta.get("painPoints") or "").strip() or None
            concerns = str(
                priv.get("businessConcerns")
                or bv.get("businessConcerns")
                or meta.get("businessConcerns")
                or ""
            ).strip() or None
            style = str(
                priv.get("communicationStyle")
                or bv.get("communicationStyle")
                or meta.get("communicationStyle")
                or ""
            ).strip() or None
            traits = str(
                priv.get("personality")
                or bv.get("personalityTraits")
                or bv.get("personality")
                or meta.get("personality")
                or ""
            ).strip() or None
            p_order = int(bv.get("displayOrder") or meta.get("displayOrder") or display_order)
            is_active = 1 if bool(bv.get("isActive", meta.get("isActive", True))) else 0

            # Lưu ánh xạ theo cả file stem, code tường minh và tên để knowledge access tra cứu linh hoạt
            persona_lookup[stem_code] = name
            persona_lookup[explicit_p_code] = name
            persona_lookup[name] = name

            lines.extend([
                f"IF NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = {ver_var} AND [Name] = {esc(name)})",
                "BEGIN",
                "    INSERT INTO [scenario].[StakeholderPersonas] (",
                "        [ScenarioVersionId], [Name], [StakeholderRole], [Department], [Responsibilities],",
                "        [Goals], [PainPoints], [BusinessConcerns], [CommunicationStyle], [Personality], [DisplayOrder], [IsActive]",
                "    ) VALUES (",
                f"        {ver_var}, {esc(name)}, {esc(role)}, {esc(dept)}, {esc(resp)},",
                f"        {esc(goals)}, {esc(pains)}, {esc(concerns)}, {esc(style)}, {esc(traits)}, {p_order}, {is_active}",
                "    );",
                "END;",
            ])
            display_order += 1
            personas_count += 1

    lines.append("")

    # 4. Seed Knowledge Items & Stakeholder Access
    lines.append("-- 4. Seed Knowledge Items & Stakeholder Access")
    knowledge_dir = bundle_dir / "knowledge"
    knowledge_count = 0
    if knowledge_dir.exists():
        for k_file in sorted(knowledge_dir.glob("*.md")):
            meta, body = parse_md(k_file)
            bv = meta.get("baves", {}) if isinstance(meta.get("baves"), dict) else {}
            k_code = str(bv.get("code") or meta.get("code") or "").strip()
            if not k_code:
                continue
            k_title = str(meta.get("title") or bv.get("title") or k_code).strip()
            k_category = str(bv.get("category") or meta.get("category") or "Process").strip()
            k_importance = str(bv.get("importance") or meta.get("importance") or "Medium").strip()
            k_content = str(body or bv.get("content") or meta.get("description") or "").strip()
            k_var = f"@KId_{var_suffix}_{safe_sql_ident(k_code)}"

            lines.extend([
                f"IF NOT EXISTS (SELECT 1 FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = {ver_var} AND [Code] = {esc(k_code)})",
                "BEGIN",
                "    INSERT INTO [scenario].[KnowledgeItems] ([ScenarioVersionId], [Code], [Title], [Content], [Category], [Importance], [CreatedAt])",
                f"    VALUES ({ver_var}, {esc(k_code)}, {esc(k_title)}, {esc(k_content)}, {esc(k_category)}, {esc(k_importance)}, SYSDATETIMEOFFSET());",
                "END;",
                f"DECLARE {k_var} int = (SELECT [Id] FROM [scenario].[KnowledgeItems] WHERE [ScenarioVersionId] = {ver_var} AND [Code] = {esc(k_code)});",
            ])

            access_list = bv.get("access") or meta.get("access") or []
            if isinstance(access_list, list):
                for acc in access_list:
                    if not isinstance(acc, dict):
                        continue
                    p_ref = str(acc.get("persona") or acc.get("personaCode") or acc.get("name") or "").strip()
                    if not p_ref or p_ref not in persona_lookup:
                        continue
                    p_name = persona_lookup[p_ref]
                    acc_type = str(acc.get("accessType") or "Knows").strip()
                    is_hidden = 1 if bool(acc.get("isHidden", False)) else 0
                    rule = acc.get("disclosureRule")
                    p_var = f"@PId_{var_suffix}_{safe_sql_ident(k_code)}_{safe_sql_ident(p_ref)}"

                    lines.extend([
                        f"DECLARE {p_var} int = (SELECT [Id] FROM [scenario].[StakeholderPersonas] WHERE [ScenarioVersionId] = {ver_var} AND [Name] = {esc(p_name)});",
                        f"IF {p_var} IS NOT NULL AND {k_var} IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [scenario].[StakeholderKnowledgeAccess] WHERE [StakeholderPersonaId] = {p_var} AND [KnowledgeItemId] = {k_var})",
                        "BEGIN",
                        "    INSERT INTO [scenario].[StakeholderKnowledgeAccess] ([StakeholderPersonaId], [KnowledgeItemId], [AccessType], [DisclosureRule], [IsHidden])",
                        f"    VALUES ({p_var}, {k_var}, {esc(acc_type)}, {esc(rule)}, {is_hidden});",
                        "END;",
                    ])
            knowledge_count += 1

    lines.append("")

    # 5. Seed Business Rules
    lines.append("-- 5. Seed Business Rules")
    rules_dir = bundle_dir / "rules"
    rules_count = 0
    if rules_dir.exists():
        for r_file in sorted(rules_dir.glob("*.md")):
            meta, body = parse_md(r_file)
            bv = meta.get("baves", {}) if isinstance(meta.get("baves"), dict) else {}
            r_code = str(meta.get("code") or bv.get("code") or "").strip()
            if not r_code:
                continue
            r_title = str(meta.get("title") or bv.get("title") or r_code).strip()
            r_desc = str(meta.get("description") or bv.get("description") or body or "").strip()
            r_importance = str(meta.get("importance") or bv.get("importance") or "Medium").strip()
            r_hidden = 1 if bool(meta.get("isHidden", bv.get("isHidden", False))) else 0

            lines.extend([
                f"IF NOT EXISTS (SELECT 1 FROM [scenario].[BusinessRules] WHERE [ScenarioVersionId] = {ver_var} AND [Code] = {esc(r_code)})",
                "BEGIN",
                "    INSERT INTO [scenario].[BusinessRules] ([ScenarioVersionId], [Code], [Title], [Description], [Importance], [IsHidden])",
                f"    VALUES ({ver_var}, {esc(r_code)}, {esc(r_title)}, {esc(r_desc)}, {esc(r_importance)}, {r_hidden});",
                "END;",
            ])
            rules_count += 1

    lines.append("")

    # 6. Seed Scenario Events
    lines.append("-- 6. Seed Scenario Events")
    events_dir = bundle_dir / "events"
    events_count = 0
    if events_dir.exists():
        default_event_order = 1
        for e_file in sorted(events_dir.glob("*.md")):
            meta, body = parse_md(e_file)
            bv = meta.get("baves", {}) if isinstance(meta.get("baves"), dict) else {}
            e_title = str(meta.get("title") or bv.get("title") or "").strip()
            if not e_title:
                continue
            e_desc = str(meta.get("description") or bv.get("description") or "").strip()
            e_type = str(meta.get("triggerType") or bv.get("triggerType") or "Manual").strip()
            e_order = int(meta.get("displayOrder") or bv.get("displayOrder") or default_event_order)
            e_active = 1 if bool(meta.get("isActive", bv.get("isActive", True))) else 0

            raw_cond = meta.get("triggerConditionJson") or bv.get("triggerConditionJson") or meta.get("triggerCondition") or bv.get("triggerCondition")
            if isinstance(raw_cond, (dict, list)):
                cond_sql = esc(json.dumps(raw_cond, ensure_ascii=False))
            elif isinstance(raw_cond, str) and raw_cond.strip():
                cond_sql = esc(raw_cond.strip())
            else:
                cond_sql = "NULL"

            lines.extend([
                f"IF NOT EXISTS (SELECT 1 FROM [scenario].[ScenarioEvents] WHERE [ScenarioVersionId] = {ver_var} AND [Title] = {esc(e_title)})",
                "BEGIN",
                "    INSERT INTO [scenario].[ScenarioEvents] ([ScenarioVersionId], [Title], [Description], [TriggerType], [TriggerConditionJson], [EventContent], [DisplayOrder], [IsActive])",
                f"    VALUES ({ver_var}, {esc(e_title)}, {esc(e_desc)}, {esc(e_type)}, {cond_sql}, {esc(body)}, {e_order}, {e_active});",
                "END;",
            ])
            default_event_order += 1
            events_count += 1

    lines.append("")

    # 7. Seed Assessment Rubric & Criteria (Chuẩn hóa theo bảng [assessment].[BabokTasks] đã hợp nhất KnowledgeAreaCode/Name)
    lines.append("-- 7. Seed Assessment Rubric & Criteria (Schema: assessment)")
    rubric_files: List[Path] = []
    rubric_dir = bundle_dir / "rubric"
    if (rubric_dir / "rubric.md").exists():
        rubric_files.append(rubric_dir / "rubric.md")
    elif rubric_dir.exists():
        rubric_files.extend(sorted(rubric_dir.glob("*.md")))

    criteria_count = 0
    for r_idx, rubric_file in enumerate(rubric_files, start=1):
        r_meta, _ = parse_md(rubric_file)
        bv_r = r_meta.get("baves", {}) if isinstance(r_meta.get("baves"), dict) else {}
        rubric_name = str(
            r_meta.get("title")
            or bv_r.get("title")
            or f"Bảng tiêu chí đánh giá {scenario_title} v{version_no}"
        ).strip()
        rubric_desc = str(r_meta.get("description") or bv_r.get("description") or "").strip()
        rubric_ver = int(r_meta.get("version") or bv_r.get("version") or r_idx)
        criteria_list = r_meta.get("criteria") or bv_r.get("criteria") or []

        rub_var = f"@RubricId_{var_suffix}_{r_idx}"
        default_task_var = f"@DefaultBabokTaskId_{var_suffix}_{r_idx}"

        lines.extend([
            f"DECLARE {default_task_var} int;",
            f"SELECT TOP 1 {default_task_var} = [Id] FROM [assessment].[BabokTasks] WHERE [Code] = N'RADD-7.1';",
            f"IF {default_task_var} IS NULL",
            "BEGIN",
            "    INSERT INTO [assessment].[BabokTasks] ([KnowledgeAreaCode], [KnowledgeAreaName], [Code], [Name], [Purpose], [Description], [BabokVersion], [DisplayOrder])",
            "    VALUES (N'RADD', N'Requirements Analysis and Design Definition', N'RADD-7.1', N'Specify and Model Requirements', N'Đặc tả và mô hình hóa yêu cầu nghiệp vụ', N'Phân tích, tổng hợp và đặc tả yêu cầu từ kết quả khảo sát Stakeholder.', N'v3', 1);",
            f"    SET {default_task_var} = SCOPE_IDENTITY();",
            "END;",
            "",
            f"DECLARE {rub_var} int;",
            f"SELECT {rub_var} = [Id] FROM [assessment].[Rubrics] WHERE [ScenarioVersionId] = {ver_var} AND [Name] = {esc(rubric_name)};",
            f"IF {rub_var} IS NULL",
            "BEGIN",
            "    INSERT INTO [assessment].[Rubrics] ([ScenarioVersionId], [CreatedByUserId], [Name], [Version], [Description], [Status], [CreatedAt])",
            f"    VALUES ({ver_var}, {instr_var}, {esc(rubric_name)}, {rubric_ver}, {esc(rubric_desc)}, N'Published', SYSDATETIMEOFFSET());",
            f"    SET {rub_var} = SCOPE_IDENTITY();",
            "END;",
        ])

        disp_order = 1
        for crit in criteria_list:
            if not isinstance(crit, dict):
                continue
            c_code = str(crit.get("code") or "").strip()
            if not c_code:
                continue
            c_name = str(crit.get("name") or c_code).strip()
            c_desc = str(crit.get("description") or "").strip()
            c_weight = float(crit.get("weight", 1.0))
            c_max = float(crit.get("maxScore", 100.0))
            c_guidance = crit.get("evidenceGuidance")
            c_required = 1 if bool(crit.get("isRequired", True)) else 0

            custom_task_code = str(crit.get("babokTaskCode") or "").strip()
            task_var_to_use = default_task_var
            if custom_task_code and custom_task_code.upper() != "RADD-7.1":
                ka_prefix = custom_task_code.split("-", 1)[0].upper()
                ka_code = str(crit.get("knowledgeAreaCode") or ka_prefix or "RADD").strip()
                ka_name = str(
                    crit.get("knowledgeAreaName")
                    or BABOK_AREA_PRESETS.get(ka_code, "Requirements Analysis and Design Definition")
                ).strip()
                task_name = str(crit.get("babokTaskName") or f"BABOK Task {custom_task_code}").strip()
                task_var_to_use = f"@BabokTaskId_{var_suffix}_{r_idx}_{safe_sql_ident(c_code)}"
                lines.extend([
                    f"DECLARE {task_var_to_use} int = (SELECT TOP 1 [Id] FROM [assessment].[BabokTasks] WHERE [Code] = {esc(custom_task_code)});",
                    f"IF {task_var_to_use} IS NULL",
                    "BEGIN",
                    "    INSERT INTO [assessment].[BabokTasks] ([KnowledgeAreaCode], [KnowledgeAreaName], [Code], [Name], [Purpose], [Description], [BabokVersion], [DisplayOrder])",
                    f"    VALUES ({esc(ka_code)}, {esc(ka_name)}, {esc(custom_task_code)}, {esc(task_name)}, NULL, NULL, N'v3', {disp_order});",
                    f"    SET {task_var_to_use} = SCOPE_IDENTITY();",
                    "END;",
                ])

            if c_guidance:
                lines.extend([
                    f"IF NOT EXISTS (SELECT 1 FROM [assessment].[RubricCriteria] WHERE [RubricId] = {rub_var} AND [Code] = {esc(c_code)})",
                    "BEGIN",
                    "    INSERT INTO [assessment].[RubricCriteria] ([RubricId], [BabokTaskId], [Code], [Name], [Description], [Weight], [MaxScore], [EvidenceGuidance], [DisplayOrder], [IsRequired])",
                    f"    VALUES ({rub_var}, {task_var_to_use}, {esc(c_code)}, {esc(c_name)}, {esc(c_desc)}, {c_weight}, {c_max}, {esc(c_guidance)}, {disp_order}, {c_required});",
                    "END;",
                ])
            else:
                lines.extend([
                    f"IF NOT EXISTS (SELECT 1 FROM [assessment].[RubricCriteria] WHERE [RubricId] = {rub_var} AND [Code] = {esc(c_code)})",
                    "BEGIN",
                    "    INSERT INTO [assessment].[RubricCriteria] ([RubricId], [BabokTaskId], [Code], [Name], [Description], [Weight], [MaxScore], [DisplayOrder], [IsRequired])",
                    f"    VALUES ({rub_var}, {task_var_to_use}, {esc(c_code)}, {esc(c_name)}, {esc(c_desc)}, {c_weight}, {c_max}, {disp_order}, {c_required});",
                    "END;",
                ])
            disp_order += 1
            criteria_count += 1

    if include_transaction:
        lines.extend([
            "",
            "COMMIT TRANSACTION;",
            f"PRINT N'>> OKF Scenario {scenario_title} ({scenario_code} v{version_no}) successfully seeded into SQL Server!';",
        ])

    info = {
        "bundle_dir": str(bundle_dir),
        "scenario_code": scenario_code,
        "scenario_title": scenario_title,
        "version_no": version_no,
        "industry_code": industry_code,
        "application_code": app_code,
        "personas": personas_count,
        "knowledge_items": knowledge_count,
        "business_rules": rules_count,
        "events": events_count,
        "rubric_criteria": criteria_count,
        "lines_count": len(lines),
    }
    return "\n".join(lines), info


def generate_sql_from_okf(bundle_dir: Path, output_sql: Path) -> Dict[str, Any]:
    """Sinh file SQL độc lập cho một OKF bundle."""
    sql_text, info = generate_scenario_sql_block(bundle_dir, include_transaction=True)
    output_sql.parent.mkdir(parents=True, exist_ok=True)
    output_sql.write_text(sql_text, encoding="utf-8")
    print(
        f"[OK] Generated SQL for '{info['scenario_code']} v{info['version_no']}' "
        f"-> {output_sql} ({info['lines_count']} lines | "
        f"{info['personas']} personas, {info['knowledge_items']} knowledge items, "
        f"{info['business_rules']} rules, {info['events']} events, {info['rubric_criteria']} criteria)"
    )
    return info


def discover_okf_bundles(root_dir: Path) -> List[Path]:
    """
    Tìm tất cả thư mục OKF bundle hợp lệ (có chứa cả index.md và scenario.md)
    bên dưới root_dir (hỗ trợ truyền trực tiếp 1 bundle, 1 thư mục kịch bản, hoặc thư mục scenarios/).
    """
    if not root_dir.exists():
        return []

    if (root_dir / "index.md").exists() and (root_dir / "scenario.md").exists():
        return [root_dir]

    bundles: List[Path] = []
    for index_file in sorted(root_dir.rglob("index.md")):
        candidate = index_file.parent
        if (candidate / "scenario.md").exists():
            bundles.append(candidate)
    return bundles


def default_output_filename(bundle_dir: Path) -> str:
    """
    Quy ước đặt tên file SQL đầu ra từ đường dẫn bundle:
    - scenarios/omnimart/v1 -> seed-omnimart-from-okf.sql (v1 giữ tên gọn tương thích ngược)
    - scenarios/omnimart/v2 -> seed-omnimart-v2-from-okf.sql
    - scenarios/banking-loan/v1 -> seed-banking-loan-from-okf.sql
    """
    if re.match(r"^[vV](\d+)$", bundle_dir.name):
        scenario_slug = bundle_dir.parent.name.lower()
        ver_num = int(re.match(r"^[vV](\d+)$", bundle_dir.name).group(1))
        if ver_num == 1:
            return f"seed-{scenario_slug}-from-okf.sql"
        return f"seed-{scenario_slug}-v{ver_num}-from-okf.sql"
    return f"seed-{bundle_dir.name.lower()}-from-okf.sql"


def generate_combined_sql(bundles: List[Path], combined_output_sql: Path) -> None:
    """Gộp nhiều OKF bundle vào một file SQL duy nhất có chung 1 TRANSACTION."""
    header = [
        "-- =============================================================================",
        f"-- BA-VES COMBINED MULTI-SCENARIO SEED SCRIPT ({len(bundles)} OKF BUNDLE(S))",
        "-- Auto-generated by import_okf.py",
        "-- =============================================================================",
        "SET QUOTED_IDENTIFIER ON;",
        "SET ANSI_NULLS ON;",
        "SET NOCOUNT ON;",
        "BEGIN TRANSACTION;",
        "",
    ]
    blocks: List[str] = []
    for b_dir in bundles:
        block_sql, _ = generate_scenario_sql_block(b_dir, include_transaction=False)
        blocks.append(block_sql)

    footer = [
        "",
        "COMMIT TRANSACTION;",
        f"PRINT N'>> Successfully seeded {len(bundles)} OKF scenario bundle(s) into SQL Server!';",
    ]
    full_sql = "\n".join(header) + "\n\n".join(blocks) + "\n".join(footer)
    combined_output_sql.parent.mkdir(parents=True, exist_ok=True)
    combined_output_sql.write_text(full_sql, encoding="utf-8")
    print(f"[OK] Generated Combined SQL ({len(bundles)} scenarios) -> {combined_output_sql}")


def main(argv: Optional[List[str]] = None) -> int:
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    if hasattr(sys.stderr, "reconfigure"):
        sys.stderr.reconfigure(encoding="utf-8", errors="replace")

    base_dir = Path(__file__).resolve().parent
    parser = argparse.ArgumentParser(
        description="Công cụ biên dịch các gói kịch bản OKF (Markdown/YAML) sang SQL Server seed script cho BA-VES."
    )
    parser.add_argument(
        "target",
        nargs="?",
        default=None,
        help="Đường dẫn tới 1 thư mục bundle (vd: scenarios/omnimart/v1), 1 kịch bản (vd: scenarios/omnimart), hoặc bỏ trống để quét toàn bộ thư mục scenarios/.",
    )
    parser.add_argument(
        "-b",
        "--bundle",
        dest="bundle_opt",
        default=None,
        help="Đường dẫn tới thư mục OKF bundle cần import (tương đương tham số vị trí target).",
    )
    parser.add_argument(
        "-r",
        "--scenarios-root",
        default=str(base_dir / "scenarios"),
        help="Thư mục gốc chứa danh sách các kịch bản OKF (mặc định: ./scenarios).",
    )
    parser.add_argument(
        "-o",
        "--output",
        default=None,
        help="Đường dẫn file .sql đầu ra (khi import 1 bundle) hoặc thư mục chứa các file .sql (khi import nhiều bundle, mặc định: ./database).",
    )
    parser.add_argument(
        "-c",
        "--combined",
        default=None,
        help="Đường dẫn file .sql gộp chung toàn bộ kịch bản đã quét (tùy chọn, vd: database/seed-all-scenarios-from-okf.sql).",
    )

    args = parser.parse_args(argv)
    target_str = args.bundle_opt or args.target
    search_root = Path(target_str).resolve() if target_str else Path(args.scenarios_root).resolve()

    bundles = discover_okf_bundles(search_root)
    if not bundles:
        print(f"[ERROR] Không tìm thấy gói kịch bản OKF nào (cần có index.md và scenario.md) tại: {search_root}", file=sys.stderr)
        return 1

    output_arg = Path(args.output).resolve() if args.output else (base_dir / "database")

    if len(bundles) == 1 and output_arg.suffix.lower() == ".sql":
        generate_sql_from_okf(bundles[0], output_arg)
    else:
        out_dir = output_arg if output_arg.suffix.lower() != ".sql" else output_arg.parent
        for bundle_path in bundles:
            out_file = out_dir / default_output_filename(bundle_path)
            generate_sql_from_okf(bundle_path, out_file)

    if args.combined:
        generate_combined_sql(bundles, Path(args.combined).resolve())

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
