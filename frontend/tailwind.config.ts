import type { Config } from "tailwindcss";

// Design tokens for this project:
// - ink:   deep navy, primary text/headers, and the Admin role accent
// - paper: warm off-white background (not the AI-cliché cream/terracotta pairing)
// - gold:  brass/gold accent, the Student role accent
// - sage:  muted green accent, the Teacher role accent
// - brick: muted red, used only for deadlines/alerts/destructive actions
// - slate: muted secondary text
const config: Config = {
  content: ["./src/**/*.{js,ts,jsx,tsx,mdx}"],
  theme: {
    extend: {
      colors: {
        ink: { DEFAULT: "#14213D", light: "#2A3B63" },
        paper: { DEFAULT: "#FAF8F4", dim: "#F0ECE3" },
        gold: { DEFAULT: "#B8912B", light: "#F3E7C9" },
        sage: { DEFAULT: "#3F5D4C", light: "#DCE6DE" },
        brick: { DEFAULT: "#9B3E3E", light: "#F4DEDE" },
        slate: { DEFAULT: "#5B6472", light: "#E4E7EB" },
      },
      fontFamily: {
        serif: ["'Source Serif 4'", "Georgia", "serif"],
        sans: ["Inter", "system-ui", "sans-serif"],
        mono: ["'IBM Plex Mono'", "monospace"],
      },
    },
  },
  plugins: [],
};

export default config;
