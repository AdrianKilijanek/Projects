"use client";

import { useQuery } from "@tanstack/react-query";
import { fetchPrompts, type Prompt } from "@/lib/api";

const STATUS_CONFIG = {
  pending:    { label: "Oczekuje",    color: "#888",   bg: "#f5f5f5" },
  processing: { label: "Przetwarza",  color: "#b45309", bg: "#fef3c7" },
  completed:  { label: "Gotowe",      color: "#065f46", bg: "#d1fae5" },
  failed:     { label: "Błąd",        color: "#991b1b", bg: "#fee2e2" },
};

export default function PromptList() {
  const { data: prompts, isLoading, isError } = useQuery({
    queryKey: ["prompts"],      // klucz cache używany też przez PromptForm do inwalidacji
    queryFn: fetchPrompts,
    refetchInterval: 3000,      // polling co 3 sekundy
    staleTime: 2000,
    refetchOnWindowFocus: true,
  });

  if (isLoading) return <p>Ładowanie...</p>;
  if (isError)   return <p style={{ color: "red" }}>Błąd ładowania danych.</p>;
  if (!prompts?.length) return <p style={{ color: "#888" }}>Brak promptów. Wyślij pierwszy!</p>;

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
      {prompts.map((prompt) => (
        <PromptCard key={prompt.id} prompt={prompt} />
      ))}
    </div>
  );
}

function PromptCard({ prompt }: { prompt: Prompt }) {
  const cfg = STATUS_CONFIG[prompt.status] ?? STATUS_CONFIG.pending;

  return (
    <div style={{
      border: "1px solid #e5e7eb",
      borderRadius: "8px",
      padding: "1rem",
      background: "white",
    }}>
      <div style={{ display: "flex", justifyContent: "space-between", marginBottom: "0.5rem" }}>
        <span style={{ fontSize: "0.85rem", color: "#888" }}>
          {new Date(prompt.createdAt).toLocaleString("pl-PL")}
        </span>
        <span style={{
          fontSize: "0.8rem",
          fontWeight: 600,
          padding: "2px 8px",
          borderRadius: "999px",
          color: cfg.color,
          background: cfg.bg,
        }}>
          {cfg.label}
        </span>
      </div>

      <p style={{ margin: "0 0 0.5rem", fontWeight: 500 }}>{prompt.text}</p>

      {prompt.status === "completed" && prompt.result && (
        <div style={{
          marginTop: "0.75rem",
          padding: "0.75rem",
          background: "#f0fdf4",
          borderRadius: "6px",
          borderLeft: "3px solid #22c55e",
          whiteSpace: "pre-wrap",
          fontSize: "0.95rem",
        }}>
          {prompt.result}
        </div>
      )}

      {prompt.status === "failed" && prompt.errorMessage && (
        <div style={{
          marginTop: "0.75rem",
          padding: "0.75rem",
          background: "#fef2f2",
          borderRadius: "6px",
          borderLeft: "3px solid #ef4444",
          fontSize: "0.9rem",
          color: "#991b1b",
        }}>
          Błąd: {prompt.errorMessage}
        </div>
      )}

      {prompt.status === "processing" && (
        <p style={{ marginTop: "0.5rem", color: "#b45309", fontSize: "0.9rem" }}>
          Przetwarzam przez LLM...
        </p>
      )}
    </div>
  );
}
