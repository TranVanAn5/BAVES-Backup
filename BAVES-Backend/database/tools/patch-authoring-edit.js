const fs = require("fs");
const p = "D:/BA-VES/BA-VES/src/features/instructor/components/AuthoringTab.tsx";
let t = fs.readFileSync(p, "utf8");

if (!t.includes("onUpdateKnowledge?:")) {
  t = t.replace(
    "  onAddKnowledge: (item: AgentKnowledgeItem) => void;\n  onAddEvent: (event: ScenarioEventConfig) => void;",
    "  onAddKnowledge: (item: AgentKnowledgeItem) => void;\n  onUpdateKnowledge?: (item: AgentKnowledgeItem) => void;\n  onAddEvent: (event: ScenarioEventConfig) => void;\n  onUpdateEvent?: (event: ScenarioEventConfig) => void;"
  );
}

if (!t.includes("onUpdateKnowledge,")) {
  t = t.replace(
    "  onAddKnowledge,\n  onAddEvent,",
    "  onAddKnowledge,\n  onUpdateKnowledge,\n  onAddEvent,\n  onUpdateEvent,"
  );
}

if (!t.includes("editingKnowledgeId")) {
  t = t.replace(
    "  const [showNewKnowledgeModal, setShowNewKnowledgeModal] = useState(false);\n  const [showNewEventModal, setShowNewEventModal] = useState(false);",
    `  const [showNewKnowledgeModal, setShowNewKnowledgeModal] = useState(false);
  const [editingKnowledgeId, setEditingKnowledgeId] = useState<string | null>(null);
  const [showNewEventModal, setShowNewEventModal] = useState(false);
  const [editingEventId, setEditingEventId] = useState<string | null>(null);
  const [editingRuleIndex, setEditingRuleIndex] = useState<number | null>(null);`
  );
}

// Improve create version flow
const oldCreate = `  const handleCreateVersion = async () => {
    const scenarioNumericId =
      parseInt(String(currentScenario?.id || "").replace(/\\D+/g, ""), 10) || activeDbId;
    // createVersion expects scenarioId, not versionId — resolve from listMy/create response best-effort
    try {
      // Fallback: use scenario id from definition load path via mine list is unreliable here.
      // Instructor flow typically uses scenario entity id; Authoring stores version id in \`id\`.
      // Prefer parsing from getDefinition.scenarioId after load.
      const def = await scenarioAuthoringService.getDefinition(activeDbId);
      const created = await scenarioAuthoringService.createVersion(def.scenarioId, true);
      alert(\`Đã tạo version mới #\${created.versionNo} (id \${created.id}).\`);
      onUpdateScenario?.(currentScenario.scenarioId, {
        id: \`SCV-\${created.id}\`,
        versionNumber: \`v\${created.versionNo}.0\`,
        status: "Bản nháp",
      });
      setSelectedScenarioId(currentScenario.scenarioId);
    } catch (err) {
      alert(err instanceof Error ? err.message : \`Không tạo được version mới (\${scenarioNumericId}).\`);
    }
  };`;

const newCreate = `  const handleCreateVersion = async () => {
    if (!currentScenario || !activeDbId) {
      alert("Chưa xác định được kịch bản hiện tại.");
      return;
    }
    try {
      const def = await scenarioAuthoringService.getDefinition(activeDbId);
      const created = await scenarioAuthoringService.createVersion(def.scenarioId, true);
      onUpdateScenario?.(currentScenario.scenarioId, {
        id: \`SCV-\${created.id}\`,
        versionNumber: \`v\${created.versionNo}.0\`,
        status: "Bản nháp",
        reviewComment: undefined,
      });
      setSelectedScenarioId(currentScenario.scenarioId);
      const fresh = await scenarioAuthoringService.getDefinition(created.id);
      applyDefinitionToUi(fresh);
      setActiveSubTab("blueprint");
      alert(
        \`Đã tạo version mới v\${created.versionNo}. Bạn có thể sửa blueprint, knowledge, rules, events rồi bấm Lưu định nghĩa.\`
      );
    } catch (err) {
      alert(err instanceof Error ? err.message : "Không tạo được version mới.");
    }
  };`;

if (t.includes("const handleCreateVersion") && !t.includes("const fresh = await scenarioAuthoringService.getDefinition(created.id)")) {
  if (t.includes(oldCreate)) {
    t = t.replace(oldCreate, newCreate);
  } else {
    console.log("WARN: create version block not exact-matched; skipping create-version patch");
  }
}

// Knowledge: open edit + update in submit
if (!t.includes("const openEditKnowledge")) {
  t = t.replace(
    "  const handleCreateKnowledge = (e: React.FormEvent) => {\n    e.preventDefault();\n    if (!currentScenario || !newKnoForm.title.trim() || !newKnoForm.content.trim()) return;\n    if (newKnoForm.personaCodes.length === 0) {\n      alert(\"Chọn ít nhất một persona được biết knowledge này (bắt buộc để gửi Expert duyệt).\");\n      return;\n    }\n    const newK: AgentKnowledgeItem = {\n      id: `kno-${Date.now()}`,\n      code: newKnoForm.code.trim() || slugCode(\"K\", newKnoForm.title, scenarioKnowledge.length),\n      scenarioId: currentScenario.scenarioId,\n      title: newKnoForm.title.trim(),\n      category: newKnoForm.category,\n      content: newKnoForm.content.trim(),\n      confidentiality: newKnoForm.confidentiality,\n      knownByStakeholders: newKnoForm.personaCodes,\n    };\n    onAddKnowledge(newK);\n    setShowNewKnowledgeModal(false);\n    setNewKnoForm({\n      code: \"\",\n      title: \"\",\n      category: \"Quy tắc nghiệp vụ\",\n      content: \"\",\n      confidentiality: \"Nội bộ\",\n      personaCodes: [],\n    });\n  };",
    `  const openCreateKnowledge = () => {
    setEditingKnowledgeId(null);
    setNewKnoForm({
      code: "",
      title: "",
      category: "Quy tắc nghiệp vụ",
      content: "",
      confidentiality: "Nội bộ",
      personaCodes: [],
    });
    setShowNewKnowledgeModal(true);
  };

  const openEditKnowledge = (k: AgentKnowledgeItem) => {
    setEditingKnowledgeId(k.id);
    setNewKnoForm({
      code: k.code || "",
      title: k.title,
      category: k.category,
      content: k.content,
      confidentiality: k.confidentiality,
      personaCodes: [...k.knownByStakeholders],
    });
    setShowNewKnowledgeModal(true);
  };

  const handleCreateKnowledge = (e: React.FormEvent) => {
    e.preventDefault();
    if (!currentScenario || !newKnoForm.title.trim() || !newKnoForm.content.trim()) return;
    if (newKnoForm.personaCodes.length === 0) {
      alert("Chọn ít nhất một persona được biết knowledge này (bắt buộc để gửi Expert duyệt).");
      return;
    }
    if (editingKnowledgeId) {
      const existing = scenarioKnowledge.find((k) => k.id === editingKnowledgeId);
      if (!existing) return;
      onUpdateKnowledge?.({
        ...existing,
        code: newKnoForm.code.trim() || existing.code,
        title: newKnoForm.title.trim(),
        category: newKnoForm.category,
        content: normalizeMultilineText(newKnoForm.content.trim()),
        confidentiality: newKnoForm.confidentiality,
        knownByStakeholders: newKnoForm.personaCodes,
      });
    } else {
      const newK: AgentKnowledgeItem = {
        id: \`kno-\${Date.now()}\`,
        code: newKnoForm.code.trim() || slugCode("K", newKnoForm.title, scenarioKnowledge.length),
        scenarioId: currentScenario.scenarioId,
        title: newKnoForm.title.trim(),
        category: newKnoForm.category,
        content: normalizeMultilineText(newKnoForm.content.trim()),
        confidentiality: newKnoForm.confidentiality,
        knownByStakeholders: newKnoForm.personaCodes,
      };
      onAddKnowledge(newK);
    }
    setShowNewKnowledgeModal(false);
    setEditingKnowledgeId(null);
    setNewKnoForm({
      code: "",
      title: "",
      category: "Quy tắc nghiệp vụ",
      content: "",
      confidentiality: "Nội bộ",
      personaCodes: [],
    });
  };`
  );
}

// Rule edit helpers
if (!t.includes("const openEditRule")) {
  t = t.replace(
    "  const handleCreateRule = (e: React.FormEvent) => {\n    e.preventDefault();\n    if (!newRuleForm.title.trim() || !newRuleForm.description.trim()) return;\n    const code =\n      newRuleForm.code.trim().toUpperCase() ||\n      slugCode(\"BR\", newRuleForm.title, businessRules.length);\n    setBusinessRules((prev) => [\n      ...prev,\n      {\n        code,\n        title: newRuleForm.title.trim(),\n        description: newRuleForm.description.trim(),\n        isHidden: newRuleForm.isHidden,\n        importance: newRuleForm.importance,\n      },\n    ]);\n    setNewRuleForm({ code: \"\", title: \"\", description: \"\", isHidden: true, importance: 2 });\n    setShowNewRuleModal(false);\n  };",
    `  const openCreateRule = () => {
    setEditingRuleIndex(null);
    setNewRuleForm({ code: "", title: "", description: "", isHidden: true, importance: 2 });
    setShowNewRuleModal(true);
  };

  const openEditRule = (idx: number) => {
    const rule = businessRules[idx];
    if (!rule) return;
    setEditingRuleIndex(idx);
    setNewRuleForm({
      code: rule.code,
      title: rule.title,
      description: rule.description,
      isHidden: rule.isHidden,
      importance: rule.importance,
    });
    setShowNewRuleModal(true);
  };

  const handleCreateRule = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newRuleForm.title.trim() || !newRuleForm.description.trim()) return;
    const code =
      newRuleForm.code.trim().toUpperCase() ||
      slugCode("BR", newRuleForm.title, businessRules.length);
    const next = {
      code,
      title: newRuleForm.title.trim(),
      description: normalizeMultilineText(newRuleForm.description.trim()),
      isHidden: newRuleForm.isHidden,
      importance: newRuleForm.importance,
    };
    if (editingRuleIndex != null) {
      setBusinessRules((prev) => prev.map((r, i) => (i === editingRuleIndex ? next : r)));
    } else {
      setBusinessRules((prev) => [...prev, next]);
    }
    setNewRuleForm({ code: "", title: "", description: "", isHidden: true, importance: 2 });
    setEditingRuleIndex(null);
    setShowNewRuleModal(false);
  };`
  );
}

// Event edit helpers
if (!t.includes("const openEditEvent")) {
  t = t.replace(
    "  const handleCreateEvent = (e: React.FormEvent) => {\n    e.preventDefault();\n    if (!currentScenario || !newEventForm.title.trim()) return;\n    const newEv: ScenarioEventConfig = {\n      id: `evt-${Date.now()}`,\n      scenarioId: currentScenario.scenarioId,\n      title: newEventForm.title.trim(),\n      triggerType: newEventForm.triggerType,\n      triggerCondition: newEventForm.triggerCondition,\n      description: newEventForm.description.trim(),\n      impactedRequirements: newEventForm.impactedRequirements\n        .split(\",\")\n        .map((s) => s.trim())\n        .filter(Boolean),\n      injectedFact: newEventForm.injectedFact.trim(),\n    };\n    onAddEvent(newEv);\n    setShowNewEventModal(false);\n    setNewEventForm({\n      title: \"\",\n      triggerType: \"manual_instructor\",\n      triggerCondition: \"Giảng viên bấm nút kích hoạt thủ công trong lúc học viên thực hành\",\n      description: \"\",\n      impactedRequirements: \"\",\n      injectedFact: \"\",\n    });\n  };",
    `  const openCreateEvent = () => {
    setEditingEventId(null);
    setNewEventForm({
      title: "",
      triggerType: "manual_instructor",
      triggerCondition: "Giảng viên bấm nút kích hoạt thủ công trong lúc học viên thực hành",
      description: "",
      impactedRequirements: "",
      injectedFact: "",
    });
    setShowNewEventModal(true);
  };

  const openEditEvent = (ev: ScenarioEventConfig) => {
    setEditingEventId(ev.id);
    setNewEventForm({
      title: ev.title,
      triggerType: ev.triggerType,
      triggerCondition: ev.triggerCondition,
      description: ev.description,
      impactedRequirements: ev.impactedRequirements.join(", "),
      injectedFact: ev.injectedFact,
    });
    setShowNewEventModal(true);
  };

  const handleCreateEvent = (e: React.FormEvent) => {
    e.preventDefault();
    if (!currentScenario || !newEventForm.title.trim()) return;
    const payload: ScenarioEventConfig = {
      id: editingEventId || \`evt-\${Date.now()}\`,
      scenarioId: currentScenario.scenarioId,
      title: newEventForm.title.trim(),
      triggerType: newEventForm.triggerType,
      triggerCondition: newEventForm.triggerCondition,
      description: normalizeMultilineText(newEventForm.description.trim()),
      impactedRequirements: newEventForm.impactedRequirements
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean),
      injectedFact: normalizeMultilineText(newEventForm.injectedFact.trim()),
    };
    if (editingEventId) onUpdateEvent?.(payload);
    else onAddEvent(payload);
    setShowNewEventModal(false);
    setEditingEventId(null);
    setNewEventForm({
      title: "",
      triggerType: "manual_instructor",
      triggerCondition: "Giảng viên bấm nút kích hoạt thủ công trong lúc học viên thực hành",
      description: "",
      impactedRequirements: "",
      injectedFact: "",
    });
  };`
  );
}

// Wire open buttons
t = t.replace(
  'onClick={() => setShowNewKnowledgeModal(true)}\n                  className="gap-1.5 text-xs"\n                >\n                  <Plus size={14} /> Thêm knowledge',
  'onClick={openCreateKnowledge}\n                  className="gap-1.5 text-xs"\n                >\n                  <Plus size={14} /> Thêm knowledge'
);
t = t.replace(
  'onClick={() => setShowNewRuleModal(true)}\n                  className="gap-1.5 text-xs"\n                >\n                  <Plus size={14} /> Thêm rule',
  'onClick={openCreateRule}\n                  className="gap-1.5 text-xs"\n                >\n                  <Plus size={14} /> Thêm rule'
);
t = t.replace(
  'onClick={() => setShowNewEventModal(true)}\n                  className="gap-1.5 text-xs"\n                >\n                  <Plus size={14} /> Thêm event',
  'onClick={openCreateEvent}\n                  className="gap-1.5 text-xs"\n                >\n                  <Plus size={14} /> Thêm event'
);

// Add Sửa on knowledge cards
if (!t.includes("openEditKnowledge(k)")) {
  t = t.replace(
    `                    detailText={k.content}
                  />
                ))}
              </div>
            </div>
          )}

          {activeSubTab === "rules" && (`,
    `                    actions={
                      !readOnly ? (
                        <button
                          type="button"
                          className="text-[11px] font-semibold text-navy underline-offset-2 hover:underline"
                          onClick={() => openEditKnowledge(k)}
                        >
                          Sửa
                        </button>
                      ) : undefined
                    }
                    detailText={k.content}
                  />
                ))}
              </div>
            </div>
          )}

          {activeSubTab === "rules" && (`
  );
}

// Add Sửa on rules
if (!t.includes("openEditRule(idx)")) {
  t = t.replace(
    `                      actions={
                        !readOnly ? (
                          <button
                            type="button"
                            className="text-[11px] font-semibold text-[#a03d3d] hover:underline"
                            onClick={() => setBusinessRules((prev) => prev.filter((_, i) => i !== idx))}
                          >
                            Xóa
                          </button>
                        ) : undefined
                      }`,
    `                      actions={
                        !readOnly ? (
                          <div className="flex items-center gap-2">
                            <button
                              type="button"
                              className="text-[11px] font-semibold text-navy underline-offset-2 hover:underline"
                              onClick={() => openEditRule(idx)}
                            >
                              Sửa
                            </button>
                            <button
                              type="button"
                              className="text-[11px] font-semibold text-[#a03d3d] hover:underline"
                              onClick={() => setBusinessRules((prev) => prev.filter((_, i) => i !== idx))}
                            >
                              Xóa
                            </button>
                          </div>
                        ) : undefined
                      }`
  );
}

// Add Sửa on events — find first events CollapsibleDetail without openEditEvent
if (!t.includes("openEditEvent(ev)")) {
  t = t.replace(
    `                {scenarioEvents.map((ev) => (
                  <CollapsibleDetail
                    key={ev.id}
                    title={ev.title}
                    meta={
                      ev.triggerCondition ? (
                        <>
                          Kích hoạt: <span className="text-navy/80">{ev.triggerCondition}</span>
                        </>
                      ) : (
                        "Biến cố mô phỏng"
                      )
                    }
                    detail={`,
    `                {scenarioEvents.map((ev) => (
                  <CollapsibleDetail
                    key={ev.id}
                    title={ev.title}
                    meta={
                      ev.triggerCondition ? (
                        <>
                          Kích hoạt: <span className="text-navy/80">{ev.triggerCondition}</span>
                        </>
                      ) : (
                        "Biến cố mô phỏng"
                      )
                    }
                    actions={
                      !readOnly ? (
                        <button
                          type="button"
                          className="text-[11px] font-semibold text-navy underline-offset-2 hover:underline"
                          onClick={() => openEditEvent(ev)}
                        >
                          Sửa
                        </button>
                      ) : undefined
                    }
                    detail={`
  );
}

// Modal titles
t = t.replace(
  '<h3 className="text-sm font-bold text-navy">Thêm knowledge</h3>',
  '<h3 className="text-sm font-bold text-navy">{editingKnowledgeId ? "Sửa knowledge" : "Thêm knowledge"}</h3>'
);
t = t.replace(
  `<Button size="sm" type="submit">
                  Thêm knowledge
                </Button>`,
  `<Button size="sm" type="submit">
                  {editingKnowledgeId ? "Cập nhật knowledge" : "Thêm knowledge"}
                </Button>`
);
t = t.replace(
  '<h3 className="text-sm font-bold text-navy">Thêm business rule</h3>',
  '<h3 className="text-sm font-bold text-navy">{editingRuleIndex != null ? "Sửa business rule" : "Thêm business rule"}</h3>'
);
t = t.replace(
  `<Button size="sm" type="submit">
                    Thêm rule
                  </Button>`,
  `<Button size="sm" type="submit">
                    {editingRuleIndex != null ? "Cập nhật rule" : "Thêm rule"}
                  </Button>`
);
t = t.replace(
  '<h3 className="text-sm font-bold text-navy">Thêm event</h3>',
  '<h3 className="text-sm font-bold text-navy">{editingEventId ? "Sửa event" : "Thêm event"}</h3>'
);
t = t.replace(
  `<Button size="sm" type="submit">
                  Thêm event
                </Button>`,
  `<Button size="sm" type="submit">
                  {editingEventId ? "Cập nhật event" : "Thêm event"}
                </Button>`
);

fs.writeFileSync(p, t, "utf8");
console.log(
  JSON.stringify({
    boi: t.includes("Bối cảnh"),
    editK: t.includes("openEditKnowledge"),
    editR: t.includes("openEditRule"),
    editE: t.includes("openEditEvent"),
    fresh: t.includes("getDefinition(created.id)"),
    auto: t.includes("AutoResizeTextarea"),
  })
);
