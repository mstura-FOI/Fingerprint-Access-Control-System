import { Client, LoginRequestDto, LoginTotpRequestDto, RefreshRequestDto, TokenResponseDto } from '../api/generated/api';

export type LoginCredentials = { email: string; password: string };

export function createAuthenticationService(authApi: Client) {
  async function login(credentials: LoginCredentials) {
    return authApi.login(new LoginRequestDto({
      email: credentials.email.trim(),
      password: credentials.password,
    }));
  }

  async function loginWithTotp(credentials: LoginCredentials, code: string): Promise<TokenResponseDto> {
    return authApi.totp(new LoginTotpRequestDto({
      email: credentials.email.trim(),
      password: credentials.password,
      code,
    }));
  }

  async function refresh(refreshToken: string): Promise<TokenResponseDto> {
    return authApi.refresh(new RefreshRequestDto({ refreshToken }));
  }

  async function logout(refreshToken: string): Promise<void> {
    await authApi.logout(new RefreshRequestDto({ refreshToken }));
  }

  return { login, loginWithTotp, refresh, logout };
}
