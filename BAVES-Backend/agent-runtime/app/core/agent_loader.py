import os
import yaml
from pathlib import Path
from jinja2 import Environment, BaseLoader

class AgentDefinitionLoader:
    def __init__(self, agents_dir: Path):
        self.agents_dir = agents_dir
        self.jinja_env = Environment(loader=BaseLoader(), autoescape=False)

    def load_agent(self, agent_name: str) -> dict:
        """
        Đọc file app/agents/{agent_name}/AGENT.md
        Parse YAML frontmatter và gộp includes (ví dụ _shared/guardrails.md)
        """
        agent_path = self.agents_dir / agent_name / "AGENT.md"
        if not agent_path.exists():
            raise FileNotFoundError(f"Agent template not found: {agent_path}")

        content = agent_path.read_text(encoding="utf-8")
        frontmatter, body = self._split_frontmatter(content)
        metadata = yaml.safe_load(frontmatter) or {}

        # Xử lý includes
        includes_content = []
        if "includes" in metadata and isinstance(metadata["includes"], list):
            for inc in metadata["includes"]:
                inc_path = self.agents_dir / inc
                if inc_path.exists():
                    _, inc_body = self._split_frontmatter(inc_path.read_text(encoding="utf-8"))
                    includes_content.append(inc_body)

        full_template_str = body + "\n\n" + "\n\n".join(includes_content)
        template = self.jinja_env.from_string(full_template_str)

        return {
            "metadata": metadata,
            "template": template,
            "version": metadata.get("version", "1.0.0"),
            "model_config": metadata.get("model", {})
        }

    def render_prompt(self, agent_name: str, context: dict) -> tuple[str, str]:
        agent_data = self.load_agent(agent_name)
        rendered_prompt = agent_data["template"].render(**context)
        version_str = f"{agent_name}@{agent_data['version']}"
        return rendered_prompt, version_str

    def _split_frontmatter(self, text: str) -> tuple[str, str]:
        if text.startswith("---"):
            parts = text.split("---", 2)
            if len(parts) >= 3:
                return parts[1], parts[2].strip()
        return "", text.strip()
