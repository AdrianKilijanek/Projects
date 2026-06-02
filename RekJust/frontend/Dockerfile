# Etap 1: Instalacja zależności (node_modules)
FROM node:20-alpine AS deps
WORKDIR /app
COPY package.json ./
RUN npm install

# Etap 2: Build Next.js
FROM node:20-alpine AS build
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
RUN npm run build

# Etap 3: Runtime – Next.js standalone mode
# next.config.ts ma output: "standalone" – kopiuje tylko potrzebne pliki
FROM node:20-alpine AS runtime
WORKDIR /app

ENV NODE_ENV=production
ENV PORT=3000

COPY --from=build /app/.next/standalone ./
COPY --from=build /app/.next/static ./.next/static
COPY --from=build /app/public ./public

EXPOSE 3000
CMD ["node", "server.js"]
