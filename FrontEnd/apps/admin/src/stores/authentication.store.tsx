import {
  createContext,
  PropsWithChildren,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import * as SecureStore from "expo-secure-store";
import { Platform } from "react-native";
import {
  setAccessTokenProvider,
  setUnauthorizedHandler,
  TokenResponseDto,
  useAuthApiService,
} from "@/packages";
import { createAuthenticationService } from "@/packages/auth/authentication.service";
import type { LoginModel } from "@/features/auth/schemas/login-model";

const refreshTokenKey = "fps.admin.refresh-token";
const totpEnabledKey = "fps.admin.totp-enabled";
const isWebDevelopment = Platform.OS === "web" && __DEV__;

type AuthenticationContextValue = {
  tokens?: TokenResponseDto;
  isAuthenticated: boolean;
  isReady: boolean;
  isTotpEnabled: boolean;
  login: (model: LoginModel) => Promise<boolean>;
  loginWithTotp: (model: LoginModel, code: string) => Promise<void>;
  markTotpEnabled: (enabled: boolean) => Promise<void>;
  logout: () => Promise<void>;
};
const AuthenticationContext = createContext<
  AuthenticationContextValue | undefined
>(undefined);

function accessTokenState(tokens: TokenResponseDto) {
  return new TokenResponseDto({
    accessToken: tokens.accessToken,
    accessTokenExpiresAtUtc: tokens.accessTokenExpiresAtUtc,
  });
}
async function getRefreshToken() {
  if (Platform.OS === "web")
    return isWebDevelopment
      ? (globalThis.sessionStorage?.getItem(refreshTokenKey) ?? null)
      : null;
  return SecureStore.getItemAsync(refreshTokenKey);
}
async function storeRefreshToken(tokens: TokenResponseDto) {
  if (!tokens.refreshToken) throw new Error("API nije vratio refresh token.");
  if (Platform.OS === "web") {
    if (isWebDevelopment)
      globalThis.sessionStorage?.setItem(refreshTokenKey, tokens.refreshToken);
    return;
  }
  await SecureStore.setItemAsync(refreshTokenKey, tokens.refreshToken, {
    keychainAccessible: SecureStore.AFTER_FIRST_UNLOCK,
  });
}
async function deleteRefreshToken() {
  if (Platform.OS === "web") {
    if (isWebDevelopment)
      globalThis.sessionStorage?.removeItem(refreshTokenKey);
    return;
  }
  await SecureStore.deleteItemAsync(refreshTokenKey);
}
async function getTotpEnabled() {
  if (Platform.OS === "web")
    return globalThis.sessionStorage?.getItem(totpEnabledKey) === "true";
  return (await SecureStore.getItemAsync(totpEnabledKey)) === "true";
}
async function storeTotpEnabled(enabled: boolean) {
  if (Platform.OS === "web") {
    if (enabled) globalThis.sessionStorage?.setItem(totpEnabledKey, "true");
    else globalThis.sessionStorage?.removeItem(totpEnabledKey);
    return;
  }
  if (enabled) await SecureStore.setItemAsync(totpEnabledKey, "true");
  else await SecureStore.deleteItemAsync(totpEnabledKey);
}

export function AuthenticationProvider({ children }: PropsWithChildren) {
  const [tokens, setTokens] = useState<TokenResponseDto>();
  const [isReady, setIsReady] = useState(false);
  const [isTotpEnabled, setIsTotpEnabled] = useState(false);
  const authenticationService =
    createAuthenticationService(useAuthApiService());

  useEffect(() => {
    let active = true;
    async function restoreSession() {
      try {
        const refreshToken = await getRefreshToken();
        if (!refreshToken) return;
        const nextTokens = await authenticationService.refresh(refreshToken);
        await storeRefreshToken(nextTokens);
        const totpEnabled = await getTotpEnabled();
        if (active) setIsTotpEnabled(totpEnabled);
        if (active) setTokens(accessTokenState(nextTokens));
      } catch {
        await deleteRefreshToken();
      } finally {
        if (active) setIsReady(true);
      }
    }
    void restoreSession();
    return () => {
      active = false;
    };
  }, []);

  useEffect(() => {
    setAccessTokenProvider(() => tokens?.accessToken);
    setUnauthorizedHandler(async () => {
      const refreshToken = await getRefreshToken();
      if (!refreshToken) {
        setTokens(undefined);
        return undefined;
      }

      try {
        const nextTokens = await authenticationService.refresh(refreshToken);
        await storeRefreshToken(nextTokens);
        setTokens(accessTokenState(nextTokens));
        return nextTokens.accessToken;
      } catch {
        setTokens(undefined);
        await deleteRefreshToken();
        return undefined;
      }
    });

    return () => {
      setAccessTokenProvider(undefined);
      setUnauthorizedHandler(undefined);
    };
  }, [tokens, authenticationService]);

  const value = useMemo<AuthenticationContextValue>(
    () => ({
      tokens,
      isAuthenticated: Boolean(tokens?.accessToken),
      isReady,
      isTotpEnabled,
      async login(model) {
        const result = await authenticationService.login(model);
        if (result.requiresTotp) return true;
        await storeTotpEnabled(false);
        setIsTotpEnabled(false);
        if (!result.tokens) throw new Error("API nije vratio tokene.");
        const nextTokens = result.tokens;
        await storeRefreshToken(nextTokens);
        setTokens(accessTokenState(nextTokens));
        return false;
      },
      async loginWithTotp(model, code) {
        const nextTokens = await authenticationService.loginWithTotp(
          model,
          code,
        );
        await storeRefreshToken(nextTokens);
        await storeTotpEnabled(true);
        setIsTotpEnabled(true);
        setTokens(accessTokenState(nextTokens));
      },
      async markTotpEnabled(enabled) {
        await storeTotpEnabled(enabled);
        setIsTotpEnabled(enabled);
      },
      async logout() {
        const refreshToken = await getRefreshToken();
        setTokens(undefined);
        setIsTotpEnabled(false);
        try {
          if (refreshToken) await authenticationService.logout(refreshToken);
        } finally {
          await deleteRefreshToken();
          await storeTotpEnabled(false);
        }
      },
    }),
    [tokens, isReady, isTotpEnabled],
  );

  return (
    <AuthenticationContext.Provider value={value}>
      {children}
    </AuthenticationContext.Provider>
  );
}
export function useAuthenticationStore() {
  const context = useContext(AuthenticationContext);
  if (!context)
    throw new Error(
      "useAuthenticationStore must be used within AuthenticationProvider.",
    );
  return context;
}
