/**
 * Import FE doc templates via tsx, strip BABOK branding, write JSON + SQL seed.
 */
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { spawnSync } from "node:child_process";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const backendDb = path.resolve(__dirname, "..");
const root = path.resolve(backendDb, "..", "..");
const feRoot = path.join(root, "BA-VES");
const outDir = path.join(backendDb, "doc-templates");
const outJson = path.join(outDir, "del-01-08.json");
const rawJson = path.join(outDir, "_raw.json");

function sanitizeHtmlEntities(text) {
  if (!text) return text;
  return String(text)
    // Avoid sqlcmd code-page corruption of Unicode punctuation
    .replace(/\u2022/g, "&bull;")
    .replace(/\u2013/g, "&ndash;")
    .replace(/\u2014/g, "&mdash;")
    .replace(/\u2018|\u2019/g, "&#39;")
    .replace(/\u201C|\u201D/g, "&quot;")
    .replace(/\u00A0/g, "&nbsp;")
    // Already-corrupted sequences if re-imported
    .replace(/â€¢/g, "&bull;")
    .replace(/â€“/g, "&ndash;")
    .replace(/â€”/g, "&mdash;")
    .replace(/â€™/g, "&#39;")
    .replace(/â€œ|â€/g, "&quot;");
}

function stripBabok(text) {
  if (!text) return text;
  return sanitizeHtmlEntities(
    String(text)
      .replace(/BABOK®\s*Guide\s*v3\.0/gi, "")
      .replace(/BABOK®\s*v3/gi, "")
      .replace(/BABOK\s*Guide\s*v3(?:\.0)?/gi, "")
      .replace(/BABOK\s*v3/gi, "")
      .replace(/BABOK®/gi, "")
      .replace(/BABOK/gi, "")
      .replace(/Confluence\s+Plan/gi, "BA Plan Canvas")
      .replace(/Traceability Matrix Canvas/gi, "Traceability Matrix")
      .replace(/\s*®\s*v3\s*BRD Canvas/gi, "BRD Canvas")
      .replace(/\s+-\s*<\/h1>/gi, "</h1>")
      .replace(/\s+-\s*$/gm, "")
      .replace(/\s*-\s*-\s*/g, " - ")
      .replace(/\(\s*\)/g, "")
      .replace(/\s{2,}/g, " ")
      .replace(/\s+\|/g, " |")
      .replace(/Must cite\s+Planning and Monitoring and identify/gi, "Identify")
      .replace(/chuẩn\s+,/gi, "chuẩn phân tích nghiệp vụ,")
      .replace(/•\s*<strong>Standard:<\/strong>\s*-\s*/gi, "&bull; <strong>Document type:</strong> ")
      .replace(/•\s*<strong>Standard:<\/strong>\s+/gi, "&bull; <strong>Document type:</strong> ")
      .replace(/•\s*<strong>Tiêu chuẩn:<\/strong>\s*/gi, "&bull; <strong>Loại tài liệu:</strong> ")
      .replace(/•\s*<strong>Tiêu chuẩn quản lý:<\/strong>\s*/gi, "&bull; <strong>Quản trị:</strong> ")
      .replace(/^\s*Guide v3\s+/i, "")
      .replace(/Guide v3\s+Business Analysis/gi, "Business Analysis")
      .replace(/Guide v3\s+Strategy Analysis/gi, "Strategy Analysis")
      .replace(/\(\s*Planning &amp; Monitoring\)/gi, "(Planning &amp; Monitoring)")
      .replace(/\(\s+Planning/gi, "(Planning")
      .trim()
  );
}

function mapCategory(cat) {
  if (!cat) return "Business Analysis";
  if (/Requirements/i.test(cat)) return "Requirements & Specs";
  if (/Process|Traceability/i.test(cat)) return "Process & Traceability";
  return "Business Analysis";
}

function mapArtifactType(code) {
  switch (code) {
    case "DEL-01":
      return "ProblemStatement";
    case "DEL-02":
      return "BAP";
    case "DEL-03":
      return "StakeholderRegister";
    case "DEL-04":
      return "ElicitationNotes";
    case "DEL-05":
      return "BRD";
    case "DEL-06":
      return "ProcessModel";
    case "DEL-07":
      return "SRS";
    case "DEL-08":
      return "RTM";
    default:
      return "Generic";
  }
}

function badgeFor(code) {
  const map = {
    "DEL-01": "Problem Canvas",
    "DEL-02": "BA Plan Canvas",
    "DEL-03": "Stakeholder Canvas",
    "DEL-04": "Meeting Notes",
    "DEL-05": "BRD Canvas",
    "DEL-06": "Process Spec",
    "DEL-07": "SRS & Stories",
    "DEL-08": "Traceability Matrix",
  };
  return map[code] || "BA Template";
}

function loadTemplatesViaTsx() {
  fs.mkdirSync(outDir, { recursive: true });
  const bridge = path.join(feRoot, "scripts", "_export-jira-templates.ts");
  fs.mkdirSync(path.dirname(bridge), { recursive: true });
  const rawPosix = rawJson.replace(/\\/g, "/");
  fs.writeFileSync(
    bridge,
    `import { writeFileSync } from "node:fs";
import { JIRA_DOC_TEMPLATES } from "../src/features/docs/jiraTemplates";
writeFileSync(${JSON.stringify(rawPosix)}, JSON.stringify(JIRA_DOC_TEMPLATES, null, 2), "utf8");
console.log("exported", JIRA_DOC_TEMPLATES.length);
`,
    "utf8"
  );

  const result = spawnSync("npx", ["--yes", "tsx", bridge], {
    cwd: feRoot,
    encoding: "utf8",
    shell: true,
  });
  if (result.status !== 0) {
    console.error(result.stdout);
    console.error(result.stderr);
    throw new Error("tsx export failed");
  }
  console.log(result.stdout.trim());
  return JSON.parse(fs.readFileSync(rawJson, "utf8"));
}

function toSeedRows(templates) {
  return templates.map((t, index) => {
    const code = t.templateCode;
    const checklist = (t.checklist || []).map((c) => ({
      id: c.id,
      title: stripBabok(c.title),
      description: stripBabok(c.description),
      rubricHint: stripBabok(c.rubricHint),
      snippetHtml: stripBabok(c.snippetHtml),
    }));
    return {
      code,
      deliverableCode: code,
      name: stripBabok(t.name).replace(/\s*-\s*$/, "").trim(),
      artifactType: mapArtifactType(code),
      description: stripBabok(t.summary),
      category: mapCategory(t.category),
      defaultTitle: stripBabok(t.defaultTitle),
      issueKeyPrefix: t.issueKeyPrefix,
      displayOrder: index + 1,
      jiraBadge: badgeFor(code),
      skeletonHtmlContent: stripBabok(t.skeletonHtmlContent),
      sampleHtmlContent: stripBabok(t.htmlContent),
      checklist,
      sections: checklist.map((c, i) => ({
        sectionKey: c.id.toUpperCase().replace(/-/g, "_"),
        title: c.title,
        instructions: c.description,
        displayOrder: i + 1,
        isRequired: true,
      })),
    };
  });
}

function sqlEscape(value) {
  if (value == null) return "NULL";
  return "N'" + String(value).replace(/'/g, "''") + "'";
}

function buildSql(rows) {
  const lines = [];
  lines.push("SET QUOTED_IDENTIFIER ON;");
  lines.push("SET ANSI_NULLS ON;");
  lines.push("SET NOCOUNT ON;");
  lines.push("GO");
  lines.push(`
IF COL_LENGTH('artifact.ArtifactTemplates', 'DeliverableCode') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [DeliverableCode] nvarchar(20) NULL;
IF COL_LENGTH('artifact.ArtifactTemplates', 'Category') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [Category] nvarchar(80) NULL;
IF COL_LENGTH('artifact.ArtifactTemplates', 'DefaultTitle') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [DefaultTitle] nvarchar(300) NULL;
IF COL_LENGTH('artifact.ArtifactTemplates', 'IssueKeyPrefix') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [IssueKeyPrefix] nvarchar(40) NULL;
IF COL_LENGTH('artifact.ArtifactTemplates', 'DisplayOrder') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [DisplayOrder] int NOT NULL CONSTRAINT DF_ArtifactTemplates_DisplayOrder DEFAULT 0;
IF COL_LENGTH('artifact.ArtifactTemplates', 'SkeletonHtmlContent') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [SkeletonHtmlContent] nvarchar(max) NULL;
IF COL_LENGTH('artifact.ArtifactTemplates', 'SampleHtmlContent') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [SampleHtmlContent] nvarchar(max) NULL;
IF COL_LENGTH('artifact.ArtifactTemplates', 'ChecklistJson') IS NULL
  ALTER TABLE [artifact].[ArtifactTemplates] ADD [ChecklistJson] nvarchar(max) NULL;
`);
  lines.push("GO");
  lines.push("BEGIN TRANSACTION;");
  lines.push(`
DELETE FROM [artifact].[Submissions] WHERE [ArtifactVersionId] IN (
  SELECT v.[Id] FROM [artifact].[ArtifactVersions] v
  INNER JOIN [artifact].[Artifacts] a ON a.[Id] = v.[ArtifactId]
);
DELETE FROM [artifact].[ArtifactVersions] WHERE [ArtifactId] IN (SELECT [Id] FROM [artifact].[Artifacts]);
DELETE FROM [artifact].[Artifacts];
DELETE FROM [artifact].[ArtifactTemplateSections];
DELETE FROM [artifact].[ArtifactTemplates];
`);

  for (const row of rows) {
    const varName = `@T_${row.code.replace(/-/g, "_")}`;
    lines.push(`DECLARE ${varName} int;`);
    lines.push(`INSERT INTO [artifact].[ArtifactTemplates]
([Code],[Name],[ArtifactType],[Description],[Status],[DeliverableCode],[Category],[DefaultTitle],[IssueKeyPrefix],[DisplayOrder],[SkeletonHtmlContent],[SampleHtmlContent],[ChecklistJson],[CreatedAt])
VALUES (
  ${sqlEscape(row.code)},
  ${sqlEscape(row.name)},
  ${sqlEscape(row.artifactType)},
  ${sqlEscape(row.description)},
  N'Published',
  ${sqlEscape(row.deliverableCode)},
  ${sqlEscape(row.category)},
  ${sqlEscape(row.defaultTitle)},
  ${sqlEscape(row.issueKeyPrefix)},
  ${row.displayOrder},
  ${sqlEscape(row.skeletonHtmlContent)},
  ${sqlEscape(row.sampleHtmlContent)},
  ${sqlEscape(JSON.stringify(row.checklist))},
  SYSDATETIMEOFFSET()
);
SET ${varName} = SCOPE_IDENTITY();
`);
    for (const s of row.sections) {
      lines.push(`INSERT INTO [artifact].[ArtifactTemplateSections]
([ArtifactTemplateId],[SectionKey],[Title],[Instructions],[DisplayOrder],[IsRequired])
VALUES (${varName}, ${sqlEscape(s.sectionKey)}, ${sqlEscape(s.title)}, ${sqlEscape(s.instructions)}, ${s.displayOrder}, ${s.isRequired ? 1 : 0});`);
    }
    lines.push("");
  }

  lines.push("COMMIT TRANSACTION;");
  lines.push(
    "SELECT [Id],[Code],[DeliverableCode],[Name], LEN([SkeletonHtmlContent]) AS SkeletonLen, LEN([SampleHtmlContent]) AS SampleLen FROM [artifact].[ArtifactTemplates] ORDER BY [DisplayOrder];"
  );
  return lines.join("\n");
}

const templates = loadTemplatesViaTsx();
const rows = toSeedRows(templates);
fs.writeFileSync(outJson, JSON.stringify(rows, null, 2), "utf8");
const sqlPath = path.join(outDir, "seed-del-01-08.sql");
fs.writeFileSync(sqlPath, buildSql(rows), "utf8");
console.log(`Wrote ${rows.length} templates -> ${outJson}`);
console.log(`Wrote SQL -> ${sqlPath}`);
