// Nginx proxy kieruje /api/* do PromptApi, dzięki czemu unikamy problemów z CORS.
const API_BASE = "/api";

export interface Prompt {
  id: string;
  text: string;
  status: "pending" | "processing" | "completed" | "failed";
  result: string | null;
  errorMessage: string | null;
  createdAt: string;
  completedAt: string | null;
}

export async function fetchPrompts(): Promise<Prompt[]> {
  const res = await fetch(`${API_BASE}/prompts`);
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

export async function createPrompt(text: string): Promise<Prompt> {
  const res = await fetch(`${API_BASE}/prompts`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ text }),
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}
