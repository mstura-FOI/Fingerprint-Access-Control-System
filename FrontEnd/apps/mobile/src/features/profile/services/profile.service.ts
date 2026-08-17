import { useMemo } from "react";
import {
  AccountUpdateDto,
  ChangePasswordDto,
  useAccountApiService,
  usePasswordApiService,
} from "@/packages";
function userId(token: string) {
  const payload = token.split(".")[1];
  if (!payload) throw new Error("Neispravan pristupni token.");
  const json = globalThis.atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
  const data = JSON.parse(json) as Record<string, string>;
  const id =
    data.sub ||
    data[
      "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
    ];
  if (!id) throw new Error("Korisnik nije pronađen u tokenu.");
  return id;
}
export function useProfileService() {
  const accounts = useAccountApiService();
  const password = usePasswordApiService();
  return useMemo(
    () => ({
      get: (token: string) => accounts.accountGET(userId(token)),
      update: (
        token: string,
        input: { firstName: string; lastName: string; email: string },
      ) =>
        accounts.accountPUT(
          new AccountUpdateDto({ id: userId(token), ...input }),
        ),
      changePassword: (
        token: string,
        currentPassword: string,
        newPassword: string,
      ) =>
        password.changePassword(
          new ChangePasswordDto({
            id: userId(token),
            currentPassword,
            newPassword,
          }),
        ),
    }),
    [accounts, password],
  );
}
