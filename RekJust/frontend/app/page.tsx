import PromptForm from "@/components/PromptForm";
import PromptList from "@/components/PromptList";

// Główna strona – Server Component (domyślnie w App Router).
// PromptForm i PromptList mają "use client" bo używają useState/useQuery.
export default function HomePage() {
  return (
    <main style={{ maxWidth: "800px", margin: "0 auto", padding: "2rem 1rem" }}>
      <h1 style={{ fontSize: "1.75rem", fontWeight: 700, marginBottom: "0.5rem" }}>
        LLM Prompt Processor
      </h1>
      <p style={{ color: "#6b7280", marginBottom: "2rem" }}>
        Wpisz pytanie. Przetworzy je lokalny model Ollama (llama3.2).
      </p>

      <PromptForm />

      <h2 style={{ fontSize: "1.25rem", fontWeight: 600, marginBottom: "1rem" }}>
        Historia promptów
      </h2>
      <PromptList />
    </main>
  );
}
