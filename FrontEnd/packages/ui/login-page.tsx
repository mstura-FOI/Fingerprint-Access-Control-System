import { useState } from "react";
import {
  ActivityIndicator,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

export type LoginFormModel = { email: string; password: string };

type LoginPageProps = {
  accentColor: string;
  description: string;
  onLogin: (model: LoginFormModel) => Promise<boolean>;
  onVerifyTotp: (model: LoginFormModel, code: string) => Promise<void>;
  title: string;
};

const initialModel: LoginFormModel = { email: "", password: "" };

function validateLoginModel(model: LoginFormModel): string | undefined {
  if (!model.email.trim()) return "E-mail je obavezan.";
  if (!/^\S+@\S+\.\S+$/.test(model.email)) return "Unesite ispravan e-mail.";
  if (!model.password) return "Lozinka je obavezna.";
}

function getStatus(error: unknown) {
  return (error as { status?: number }).status;
}

export function SharedLoginPage({ accentColor, description, onLogin, onVerifyTotp, title }: LoginPageProps) {
  const [model, setModel] = useState(initialModel);
  const [requiresTotp, setRequiresTotp] = useState(false);
  const [code, setCode] = useState("");
  const [error, setError] = useState<string>();
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function submit() {
    const validationError = validateLoginModel(model);
    if (validationError) return setError(validationError);

    setError(undefined);
    setIsSubmitting(true);
    try {
      setRequiresTotp(await onLogin(model));
    } catch {
      setError("Prijava nije uspjela. Provjerite e-mail i lozinku.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function submitTotp() {
    if (!/^\d{6}$/.test(code)) return setError("Unesite 6-znamenkasti kod.");
    setError(undefined);
    setIsSubmitting(true);
    try {
      await onVerifyTotp(model, code);
    } catch (error) {
      const status = getStatus(error);
      setError(status === 401 ? "Neispravan kod" : status === 429 ? "Previše pokušaja, pričekaj minutu" : "Potvrda koda nije uspjela. Pokušajte ponovno.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.card}>
        <Text style={[styles.eyebrow, { color: accentColor }]}>BIOMETRIJSKI SUSTAV</Text>
        <Text style={styles.title}>{title}</Text>
        <Text style={styles.description}>{requiresTotp ? "Unesite kod iz authenticator aplikacije." : description}</Text>
        {requiresTotp ? <>
          <TextInput autoComplete="one-time-code" autoFocus keyboardType="number-pad" maxLength={6} placeholder="6-znamenkasti kod" style={styles.input} value={code} onChangeText={(value) => { setCode(value.replace(/\D/g, "").slice(0, 6)); setError(undefined); }} onSubmitEditing={submitTotp} />
          {error && <Text style={styles.error}>{error}</Text>}
          <Pressable disabled={isSubmitting} onPress={submitTotp} style={[styles.button, { backgroundColor: accentColor }, isSubmitting && styles.disabled]}>
            {isSubmitting ? <ActivityIndicator color="#fff" /> : <Text style={styles.buttonText}>Potvrdi kod</Text>}
          </Pressable>
        </> : <>
        <TextInput
          autoCapitalize="none"
          autoComplete="email"
          keyboardType="email-address"
          placeholder="E-mail"
          style={styles.input}
          value={model.email}
          onChangeText={(email) => setModel({ ...model, email })}
        />
        <TextInput
          autoComplete="password"
          placeholder="Lozinka"
          secureTextEntry
          style={styles.input}
          value={model.password}
          onChangeText={(password) => setModel({ ...model, password })}
          onSubmitEditing={submit}
        />
        {error && <Text style={styles.error}>{error}</Text>}
        <Pressable
          disabled={isSubmitting}
          onPress={submit}
          style={[styles.button, { backgroundColor: accentColor }, isSubmitting && styles.disabled]}
        >
          {isSubmitting ? <ActivityIndicator color="#fff" /> : <Text style={styles.buttonText}>Prijava</Text>}
        </Pressable>
        </>}
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, justifyContent: "center", padding: 24, backgroundColor: "#f5f7fb" },
  card: {
    width: "100%",
    maxWidth: 440,
    alignSelf: "center",
    gap: 14,
    padding: 28,
    borderRadius: 20,
    backgroundColor: "#fff",
    elevation: 3,
  },
  eyebrow: { fontSize: 12, fontWeight: "700", letterSpacing: 1.2 },
  title: { color: "#172033", fontSize: 30, fontWeight: "700" },
  description: { color: "#5e6b82", fontSize: 16, marginBottom: 8 },
  input: {
    borderWidth: 1,
    borderColor: "#d6dce8",
    borderRadius: 10,
    paddingHorizontal: 14,
    paddingVertical: 13,
    fontSize: 16,
  },
  button: { alignItems: "center", borderRadius: 10, paddingVertical: 14 },
  disabled: { opacity: 0.55 },
  buttonText: { color: "#fff", fontWeight: "700" },
  error: { color: "#b42318" },
});
