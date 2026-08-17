import { API_URL } from "./client";
import { Client } from "./api/generated/api";
import { createAuthenticationService } from "./auth/authentication.service";

export * from "./client";
export * from "./api/generated/api";
export * from "./auth/authentication.service";
export * from "./ui/login-page";

export type AccessTokenProvider = () => string | undefined | null;
export type UnauthorizedHandler = () => Promise<string | undefined | null>;

let accessTokenProvider: AccessTokenProvider | undefined;
let unauthorizedHandler: UnauthorizedHandler | undefined;
let refreshInProgress: Promise<string | undefined | null> | undefined;

export function setAccessTokenProvider(provider: AccessTokenProvider | undefined): void {
  accessTokenProvider = provider;
}

export function setUnauthorizedHandler(handler: UnauthorizedHandler | undefined): void {
  unauthorizedHandler = handler;
}

function isAuthRequest(input: RequestInfo): boolean {
  const url = typeof input === "string" ? input : input.url;
  return /\/api\/Auth\/(?:login(?:\/totp)?|refresh)(?:[?#]|$)/i.test(url);
}

async function refreshAccessToken(): Promise<string | undefined | null> {
  if (!unauthorizedHandler) return undefined;
  refreshInProgress ??= unauthorizedHandler().finally(() => { refreshInProgress = undefined; });
  return refreshInProgress;
}

const http = {
  async fetch(input: RequestInfo, init?: RequestInit): Promise<Response> {
    const headers = new Headers(init?.headers);
    const accessToken = accessTokenProvider?.();
    if (accessToken && !headers.has("Authorization")) headers.set("Authorization", `Bearer ${accessToken}`);

    const response = await fetch(input, { ...init, headers, credentials: "include" });
    if (response.status !== 401 || isAuthRequest(input)) return response;

    const refreshedAccessToken = await refreshAccessToken();
    if (!refreshedAccessToken) return response;

    const retryHeaders = new Headers(init?.headers);
    retryHeaders.set("Authorization", `Bearer ${refreshedAccessToken}`);
    return fetch(input, { ...init, headers: retryHeaders, credentials: "include" });
  },
};

function defineComposableService<T>(service: T): () => T { return () => service; }
const createClient = () => new Client(API_URL, http);

export const useAccessCodeApiService = defineComposableService(createClient());
export const useAccessRightApiService = defineComposableService(createClient());
export const useAccountApiService = defineComposableService(createClient());
export const useAttendanceApiService = defineComposableService(createClient());
export const useAuthApiService = defineComposableService(createClient());
export const useAuthenticationService = defineComposableService(createAuthenticationService(useAuthApiService()));
export const useDeviceApiService = defineComposableService(createClient());
export const useFingerprintScanApiService = defineComposableService(createClient());
export const usePasswordApiService = defineComposableService(createClient());
export const useRoomApiService = defineComposableService(createClient());
export const useRoomShiftApiService = defineComposableService(createClient());
export const useShiftApiService = defineComposableService(createClient());
