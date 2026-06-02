import type { Metadata } from "next";
import Providers from "./providers";

export const metadata: Metadata = {
  title: "RekJust — LLM Prompt Processor",
  description: "Mikroserwisowa aplikacja do przetwarzania promptów przez LLM",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="pl">
      <body style={{ margin: 0, fontFamily: "system-ui, sans-serif", background: "#f9fafb" }}>
        <Providers>
          {children}
        </Providers>
      </body>
    </html>
  );
}
