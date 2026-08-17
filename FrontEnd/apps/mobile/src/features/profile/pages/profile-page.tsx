import { useEffect, useState } from "react";
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from "react-native";
import { router } from "expo-router";
import { SafeAreaView } from "react-native-safe-area-context";
import { useAuthenticationStore } from "@/stores/authentication.store";
import { useProfileService } from "@/features/profile/services/profile.service";
type Profile = { firstName: string; lastName: string; email: string };
const empty: Profile = { firstName: "", lastName: "", email: "" };
export function ProfilePage() {
  const { tokens } = useAuthenticationStore();
  const service = useProfileService();
  const [profile, setProfile] = useState<Profile>(empty);
  const [current, setCurrent] = useState("");
  const [next, setNext] = useState("");
  const [confirm, setConfirm] = useState("");
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState<string>();
  const token = tokens?.accessToken;
  useEffect(() => {
    if (!token) return;
    void service
      .get(token)
      .then((x) =>
        setProfile({
          firstName: x.firstName ?? "",
          lastName: x.lastName ?? "",
          email: x.email ?? "",
        }),
      )
      .catch(() => setMessage("Profil nije bilo moguce ucitati."))
      .finally(() => setLoading(false));
  }, [service, token]);
  async function save() {
    if (!token) return;
    try {
      await service.update(token, profile);
      setMessage("Profil je spremljen.");
    } catch {
      setMessage("Profil nije bilo moguce spremiti.");
    }
  }
  async function password() {
    if (!token || !current || !next || next !== confirm) {
      setMessage("Provjerite podatke za novu lozinku.");
      return;
    }
    try {
      await service.changePassword(token, current, next);
      setCurrent("");
      setNext("");
      setConfirm("");
      setMessage("Lozinka je promijenjena.");
    } catch {
      setMessage("Lozinku nije bilo moguce promijeniti.");
    }
  }
  if (loading)
    return (
      <SafeAreaView style={s.page}>
        <ActivityIndicator />
      </SafeAreaView>
    );
  return (
    <SafeAreaView style={s.page}>
      <ScrollView contentContainerStyle={s.content}>
        <Pressable onPress={() => router.back()}>
          <Text style={s.back}>Natrag</Text>
        </Pressable>
        <Text style={s.title}>Profil</Text>
        {message && <Text style={s.message}>{message}</Text>}
        <View style={s.card}>
          <Text style={s.heading}>Osobni podaci</Text>
          <Field
            label="Ime"
            value={profile.firstName}
            onChangeText={(v) => setProfile((p) => ({ ...p, firstName: v }))}
          />
          <Field
            label="Prezime"
            value={profile.lastName}
            onChangeText={(v) => setProfile((p) => ({ ...p, lastName: v }))}
          />
          <Field
            label="E-mail"
            value={profile.email}
            onChangeText={(v) => setProfile((p) => ({ ...p, email: v }))}
          />
          <Button label="Spremi profil" onPress={() => void save()} />
        </View>
        <View style={s.card}>
          <Text style={s.heading}>Nova lozinka</Text>
          <Field
            label="Trenutna lozinka"
            value={current}
            secure
            onChangeText={setCurrent}
          />
          <Field
            label="Nova lozinka"
            value={next}
            secure
            onChangeText={setNext}
          />
          <Field
            label="Potvrdi novu lozinku"
            value={confirm}
            secure
            onChangeText={setConfirm}
          />
          <Button label="Promijeni lozinku" onPress={() => void password()} />
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}
function Field({
  label,
  value,
  onChangeText,
  secure,
}: {
  label: string;
  value: string;
  onChangeText: (v: string) => void;
  secure?: boolean;
}) {
  return (
    <View style={s.field}>
      <Text style={s.label}>{label}</Text>
      <TextInput
        value={value}
        onChangeText={onChangeText}
        secureTextEntry={secure}
        style={s.input}
      />
    </View>
  );
}
function Button({ label, onPress }: { label: string; onPress: () => void }) {
  return (
    <Pressable style={s.button} onPress={onPress}>
      <Text style={s.buttonText}>{label}</Text>
    </Pressable>
  );
}
const s = StyleSheet.create({
  page: { flex: 1, backgroundColor: "#f5f7fb" },
  content: { padding: 24, gap: 18 },
  back: { color: "#047857", fontWeight: "700" },
  title: { fontSize: 30, fontWeight: "800", color: "#172033" },
  message: { color: "#047857", fontWeight: "600" },
  card: { gap: 12, padding: 20, borderRadius: 16, backgroundColor: "#fff" },
  heading: { fontSize: 18, fontWeight: "800" },
  field: { gap: 4 },
  label: { fontWeight: "600", color: "#334155" },
  input: {
    borderWidth: 1,
    borderColor: "#cbd5e1",
    borderRadius: 8,
    padding: 10,
  },
  button: {
    alignItems: "center",
    borderRadius: 8,
    padding: 12,
    backgroundColor: "#047857",
  },
  buttonText: { color: "#fff", fontWeight: "700" },
});
