"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState } from "react";

// TanStack Query wymaga QueryClientProvider owijającego aplikację.
// QueryClient zarządza cache zapytań – przechowuje dane, śledzi stan loading/error,
// zarządza pollingiem (refetchInterval).
//
// Tworzymy QueryClient wewnątrz useState żeby każda instancja aplikacji
// (np. w testach) miała własny, izolowany cache.
export default function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(() => new QueryClient({
    defaultOptions: {
      queries: {
        retry: 2,           // 2 ponowne próby przy błędzie sieciowym
        staleTime: 1000,    // dane są "świeże" przez 1s po pobraniu
      },
    },
  }));

  return (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
}
