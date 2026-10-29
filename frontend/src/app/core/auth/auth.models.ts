// Tipos que espelham os DTOs da API (backend: Application/Features/Auth/AuthContracts.cs).

export interface User {
  id: string;
  name: string;
  email: string;
  isDemo: boolean;
}

export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  user: User;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface AuthConfig {
  demoEnabled: boolean;
}
