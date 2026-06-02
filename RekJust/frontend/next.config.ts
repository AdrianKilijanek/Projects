import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // W Dockerze Next.js uruchamia się na porcie 3000 wewnątrz kontenera.
  // Nginx proxy kieruje ruch /api/* do PromptApi, resztę do frontendu.
  output: "standalone", // optymalizuje build Dockera – kopiuje tylko potrzebne pliki
};

export default nextConfig;
