const fs = require("fs");
const p = "D:/BA-VES/BA-VES/src/features/instructor/components/AuthoringTab.tsx";
let t = fs.readFileSync(p, "utf8");
if (!t.includes("AutoResizeTextarea")) {
  t = t.replace(
    'import { Badge } from "../../../components/ui/Badge";',
    'import { Badge } from "../../../components/ui/Badge";\nimport { AutoResizeTextarea } from "../../../components/ui/AutoResizeTextarea";'
  );
}
const before = (t.match(/<textarea/g) || []).length;
t = t.replace(/<textarea/g, "<AutoResizeTextarea").replace(/<\/textarea>/g, "</AutoResizeTextarea>");
const after = (t.match(/<AutoResizeTextarea/g) || []).length;
fs.writeFileSync(p, t, "utf8");
console.log(JSON.stringify({ before, after, boi: t.includes("Bối cảnh doanh nghiệp"), ok: t.includes("AutoResizeTextarea") }));
