"use client";

import { createContext, useContext, useEffect, useState, ReactNode } from "react";
import { useRouter } from "next/navigation";
import { api } from "./api";
import { LoginResponse, UserResponse } from "./types";

interface AuthContextValue {
  user: UserResponse | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<UserResponse>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function setAuthCookie(role: string) {
  // Readable (non-HttpOnly) cookie used only for client-side route redirects in middleware.ts.
  // The actual API authorization is always enforced by the backend via the JWT bearer token.
  document.cookie = `role=${role}; path=/; max-age=${60 * 60 * 8}; SameSite=Lax`;
}

function clearAuthCookie() {
  document.cookie = "role=; path=/; max-age=0";
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    const stored = localStorage.getItem("user");
    if (stored) {
      try {
        setUser(JSON.parse(stored));
      } catch {
        localStorage.removeItem("user");
      }
    }
    setIsLoading(false);
  }, []);

  async function login(email: string, password: string): Promise<UserResponse> {
    const result = await api.post<LoginResponse>("/api/auth/login", { email, password });
    localStorage.setItem("token", result.token);
    localStorage.setItem("user", JSON.stringify(result.user));
    setAuthCookie(result.user.role);
    setUser(result.user);
    return result.user;
  }

  function logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    clearAuthCookie();
    setUser(null);
    router.push("/login");
  }

  return (
    <AuthContext.Provider value={{ user, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
  return ctx;
}
