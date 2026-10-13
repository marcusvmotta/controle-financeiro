import { AuthResponse, User } from './auth.models';

// Dados falsos reutilizados pelos testes de autenticação.

export const testUser: User = {
  id: '0192f3a4-0000-7000-8000-000000000001',
  name: 'Ana Souza',
  email: 'ana@email.com',
  isDemo: false,
};

export function authResponse(accessToken: string, user: User = testUser): AuthResponse {
  return { accessToken, expiresAt: '2026-10-13T12:15:00Z', user };
}
