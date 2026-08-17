import { Pressable, StyleSheet, Text, View } from "react-native";
export function getApiErrorMessage(error: unknown): string {
  const apiError = error as {
    status?: number;
    response?: string;
    message?: string;
  };

  // 1. MREŽNA greška — zahtjev nije ni stigao do servera (SSL, port, offline, CORS).
  //    Nema status ni response, samo JS message.
  if (apiError.status === undefined && !apiError.response) {
    const msg = apiError.message ?? "";
    if (__DEV__)
      return `Mreža/veza: ${msg || "zahtjev nije stigao do servera"}`;
    return "Nije moguće spojiti se na server.";
  }

  // 2. Serverska greška s tijelom (ProblemDetails).
  const response = apiError.response?.trim();
  if (response) {
    try {
      const parsed = JSON.parse(response) as {
        title?: string;
        detail?: string;
        message?: string;
        errors?: Record<string, string[]>;
      };
      const validationMessage = parsed.errors
        ? Object.values(parsed.errors).flat()[0]
        : undefined;
      return (
        validationMessage ||
        parsed.detail ||
        parsed.title ||
        parsed.message ||
        "Zahtjev nije uspio."
      );
    } catch {
      // 3. Tijelo nije JSON (npr. plain-text 500 ili HTML stranica greške).
      //    U dev-u pokaži sirovo — tu je često prava greška.
      if (__DEV__) return response.slice(0, 300);
    }
  }

  if (apiError.status === 401)
    return "Sesija je istekla. Prijavite se ponovno.";
  if (apiError.status === 403) return "Nemate ovlast za ovu radnju.";
  if (apiError.status === 404) return "Traženi podatak više nije dostupan.";
  if (apiError.status && apiError.status >= 500) {
    // U dev-u, ako server vrati tekst uz 500, pokaži ga.
    if (__DEV__ && response) return `500: ${response.slice(0, 300)}`;
    return "Server trenutačno ne može obraditi zahtjev.";
  }
  return "Zahtjev nije uspio. Pokušajte ponovno.";
}
export function ApiErrorNotice({
  message,
  onDismiss,
}: {
  message?: string;
  onDismiss?: () => void;
}) {
  if (!message) return null;
  return (
    <View style={styles.box}>
      <View style={styles.text}>
        <Text style={styles.title}>Operacija nije uspjela</Text>
        <Text style={styles.message}>{message}</Text>
      </View>
      {onDismiss && (
        <Pressable style={styles.close} onPress={onDismiss}>
          <Text style={styles.closeText}>Zatvori</Text>
        </Pressable>
      )}
    </View>
  );
}
const styles = StyleSheet.create({
  box: {
    flexDirection: "row",
    alignItems: "flex-start",
    justifyContent: "space-between",
    gap: 12,
    padding: 14,
    borderRadius: 10,
    backgroundColor: "#fef2f2",
    borderWidth: 1,
    borderColor: "#fecaca",
  },
  text: { flex: 1, gap: 3 },
  title: { color: "#991b1b", fontWeight: "800" },
  message: { color: "#b91c1c" },
  close: { paddingHorizontal: 8, paddingVertical: 5 },
  closeText: { color: "#991b1b", fontWeight: "700" },
});
