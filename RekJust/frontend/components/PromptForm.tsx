"use client";

import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { createPrompt } from "@/lib/api";

export default function PromptForm() {
  const [text, setText] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const queryClient = useQueryClient();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (text.trim().length < 3) return;

    setLoading(true);
    setError(null);

    try {
      await createPrompt(text.trim());
      setText("");
      // Inwalidacja cache wymusza natychmiastowe odświeżenie listy zamiast czekania na kolejny polling.
      await queryClient.invalidateQueries({ queryKey: ["prompts"] });
    } catch {
      setError("Nie udało się wysłać promptu. Spróbuj ponownie.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} style={{ marginBottom: "2rem" }}>
      <div style={{ display: "flex", gap: "0.5rem" }}>
        <textarea
          value={text}
          onChange={(e) => setText(e.target.value)}
          placeholder="Wpisz swój prompt... (min. 3 znaki)"
          rows={3}
          style={{
            flex: 1,
            padding: "0.75rem",
            borderRadius: "6px",
            border: "1px solid #ccc",
            fontSize: "1rem",
            resize: "vertical",
          }}
          disabled={loading}
        />
        <button
          type="submit"
          disabled={loading || text.trim().length < 3}
          style={{
            padding: "0 1.5rem",
            borderRadius: "6px",
            border: "none",
            background: loading ? "#999" : "#0070f3",
            color: "white",
            fontSize: "1rem",
            cursor: loading ? "not-allowed" : "pointer",
          }}
        >
          {loading ? "Wysyłam..." : "Wyślij"}
        </button>
      </div>
      {error && <p style={{ color: "red", marginTop: "0.5rem" }}>{error}</p>}
    </form>
  );
}
