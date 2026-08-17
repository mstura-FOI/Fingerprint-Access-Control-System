import { useState } from "react";
import * as Clipboard from "expo-clipboard";
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from "react-native";
import { MainLayout } from "@/layouts/main-layout";
import { useTotpService } from "@/features/security/services/totp.service";
import { getApiErrorMessage } from "@/features/shared/components/api-error-notice";
import { useAuthenticationStore } from "@/stores/authentication.store";

function statusOf(error: unknown) {
  return (error as { status?: number }).status;
}

export function SecurityPage() {
  const service = useTotpService();
  const { isTotpEnabled, markTotpEnabled } = useAuthenticationStore();
  const [secret, setSecret] = useState<string>();
  const [code, setCode] = useState("");
  const [message, setMessage] = useState<string>();
  const [error, setError] = useState<string>();
  const [busy, setBusy] = useState(false);

  async function beginSetup() {
    setBusy(true);
    setError(undefined);
    setMessage(undefined);
    try {
      const setup = await service.beginSetup();
      if (!setup.secret) throw new Error("API nije vratio TOTP ključ.");
      setSecret(setup.secret);
      setCode("");
    } catch (nextError) {
      setError(getApiErrorMessage(nextError));
    } finally {
      setBusy(false);
    }
  }

  async function verifySetup() {
    if (!/^\d{6}$/.test(code)) return setError("Unesite 6-znamenkasti kod.");
    setBusy(true);
    setError(undefined);
    try {
      await service.verifySetup(code);
      await markTotpEnabled(true);
      setMessage("2FA je uključen");
      setSecret(undefined);
      setCode("");
    } catch (nextError) {
      setError(
        statusOf(nextError) === 400
          ? "Neispravan kod, poku\u0161aj ponovno."
          : getApiErrorMessage(nextError),
      );
    } finally {
      setBusy(false);
    }
  }

  return (
    <MainLayout>
      <ScrollView contentContainerStyle={styles.page}>
        <Text style={styles.title}>Sigurnost / 2FA</Text>
        <Text style={styles.description}>
          {
            "Za\u0161titite administratorski račun kodom iz authenticator aplikacije."
          }
        </Text>
        <View style={styles.card}>
          {isTotpEnabled && !secret && (
            <View style={styles.enabledBox}>
              <Text style={styles.enabledTitle}>2FA je uključen</Text>
              <Text style={styles.enabledText}>Račun je zaštićen jednokratnim kodom.</Text>
            </View>
          )}
          {message && <Text style={styles.success}>{message}</Text>}
          {error && <Text style={styles.error}>{error}</Text>}
          {!secret ? (
            <Pressable
              disabled={busy}
              onPress={beginSetup}
              style={[styles.primaryButton, busy && styles.disabled]}
            >
              {busy ? (
                <ActivityIndicator color="#fff" />
              ) : (
                <Text style={styles.primaryButtonText}>
                  {isTotpEnabled ? "Promijeni 2FA ključ" : "Uključi dvofaktorsku autentikaciju"}
                </Text>
              )}
            </Pressable>
          ) : (
            <View style={styles.setup}>
              <Text style={styles.instruction}>
                {
                  "Otvori authenticator app (Google Authenticator / Authy), odaberi Enter setup key / Ručni unos, i upiši ovaj ključ."
                }
              </Text>
              <View style={styles.secretRow}>
                <Text selectable style={styles.secret}>
                  {secret}
                </Text>
                <Pressable
                  onPress={() => void Clipboard.setStringAsync(secret)}
                  style={styles.copyButton}
                >
                  <Text style={styles.copyButtonText}>Kopiraj</Text>
                </Pressable>
              </View>
              <TextInput
                autoComplete="one-time-code"
                keyboardType="number-pad"
                maxLength={6}
                placeholder="6-znamenkasti kod"
                style={styles.input}
                value={code}
                onChangeText={(value) => {
                  setCode(value.replace(/\D/g, "").slice(0, 6));
                  setError(undefined);
                }}
                onSubmitEditing={verifySetup}
              />
              <Pressable
                disabled={busy}
                onPress={verifySetup}
                style={[styles.primaryButton, busy && styles.disabled]}
              >
                {busy ? (
                  <ActivityIndicator color="#fff" />
                ) : (
                  <Text style={styles.primaryButtonText}>Potvrdi</Text>
                )}
              </Pressable>
            </View>
          )}
        </View>
      </ScrollView>
    </MainLayout>
  );
}

const styles = StyleSheet.create({
  page: { padding: 30, gap: 12 },
  title: { color: "#102a43", fontSize: 30, fontWeight: "800" },
  description: { color: "#64748b", fontSize: 16 },
  card: {
    width: "100%",
    maxWidth: 650,
    marginTop: 18,
    padding: 24,
    borderRadius: 14,
    backgroundColor: "#fff",
    gap: 14,
  },
  setup: { gap: 16 },
  instruction: { color: "#334e68", fontSize: 16, lineHeight: 24 },
  secretRow: {
    flexDirection: "row",
    alignItems: "center",
    gap: 12,
    padding: 14,
    borderRadius: 10,
    backgroundColor: "#f1f5f9",
  },
  secret: {
    flex: 1,
    color: "#0f172a",
    fontFamily: "monospace",
    fontSize: 17,
    letterSpacing: 1.5,
  },
  copyButton: {
    paddingHorizontal: 14,
    paddingVertical: 10,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#2563eb",
  },
  copyButtonText: { color: "#1d4ed8", fontWeight: "700" },
  input: {
    borderWidth: 1,
    borderColor: "#cbd5e1",
    borderRadius: 10,
    paddingHorizontal: 14,
    paddingVertical: 13,
    fontSize: 18,
    letterSpacing: 4,
  },
  primaryButton: {
    alignItems: "center",
    alignSelf: "flex-start",
    minWidth: 150,
    paddingHorizontal: 18,
    paddingVertical: 13,
    borderRadius: 10,
    backgroundColor: "#2563eb",
  },
  primaryButtonText: { color: "#fff", fontWeight: "700" },
  disabled: { opacity: 0.55 },
  success: {
    color: "#166534",
    padding: 12,
    borderRadius: 8,
    backgroundColor: "#dcfce7",
    fontWeight: "700",
  },
  enabledBox: { padding: 14, borderRadius: 10, borderWidth: 1, borderColor: "#86efac", backgroundColor: "#f0fdf4", gap: 4 },
  enabledTitle: { color: "#166534", fontSize: 17, fontWeight: "800" },
  enabledText: { color: "#15803d" },
  error: {
    color: "#b42318",
    padding: 12,
    borderRadius: 8,
    backgroundColor: "#fef2f2",
  },
});
