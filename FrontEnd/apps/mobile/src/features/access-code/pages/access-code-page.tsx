import { useState } from "react";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { router } from "expo-router";
import { useAccessCodeService } from "@/features/access-code/services/access-code.service";
export function AccessCodePage() {
  const service = useAccessCodeService();
  const [code, setCode] = useState<string>();
  const [error, setError] = useState<string>();
  const [loading, setLoading] = useState(false);
  async function generate() {
    setLoading(true);
    setError(undefined);
    try {
      setCode((await service.generate()).code);
    } catch {
      setError("Kod nije bilo moguće generirati.");
    } finally {
      setLoading(false);
    }
  }
  return (
    <SafeAreaView style={s.page}>
      <Pressable onPress={() => router.back()}>
        <Text style={s.back}>Natrag</Text>
      </Pressable>
      <View style={s.card}>
        <Text style={s.title}>Pristupni kod</Text>
        <Text style={s.text}>Generirajte jednokratni kod za pristup.</Text>
        {code && <Text style={s.code}>{code}</Text>}
        {error && <Text style={s.error}>{error}</Text>}
        <Pressable style={s.button} onPress={() => void generate()}>
          <Text style={s.buttonText}>
            {loading ? "Generiranje..." : "Generiraj kod"}
          </Text>
        </Pressable>
      </View>
    </SafeAreaView>
  );
}
const s = StyleSheet.create({
  page: { flex: 1, padding: 24, gap: 20, backgroundColor: "#f5f7fb" },
  back: { color: "#047857", fontWeight: "700" },
  card: { gap: 16, padding: 24, borderRadius: 18, backgroundColor: "#fff" },
  title: { fontSize: 28, fontWeight: "800", color: "#172033" },
  text: { color: "#5e6b82" },
  code: {
    fontSize: 38,
    fontWeight: "800",
    letterSpacing: 5,
    textAlign: "center",
    color: "#047857",
    padding: 16,
  },
  error: { color: "#b91c1c" },
  button: {
    alignItems: "center",
    padding: 14,
    borderRadius: 10,
    backgroundColor: "#047857",
  },
  buttonText: { color: "#fff", fontWeight: "700" },
});
