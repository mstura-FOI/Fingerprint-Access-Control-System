import { useState } from "react";
import {
  Modal,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
} from "react-native";
import {
  ApiErrorNotice,
  getApiErrorMessage,
} from "@/features/shared/components/api-error-notice";
import { FeatureListPage } from "@/features/shared/components/feature-list-page";
import { useAccountsService } from "@/features/accounts/services/accounts.service";

type CreateAccountModel = {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
};
const initialModel: CreateAccountModel = {
  firstName: "",
  lastName: "",
  email: "",
  password: "",
  confirmPassword: "",
};

export function AccountsPage() {
  const service = useAccountsService();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [model, setModel] = useState<CreateAccountModel>(initialModel);
  const [validationError, setValidationError] = useState<string>();
  const [operationError, setOperationError] = useState<string>();
  const [saving, setSaving] = useState(false);
  const [editingId, setEditingId] = useState<string>();

  function update(key: keyof CreateAccountModel, value: string) {
    setModel((current) => ({ ...current, [key]: value }));
    setValidationError(undefined);
  }
  function closeDialog(force = false) {
    if (saving && !force) return;
    setDialogOpen(false);
    setEditingId(undefined);
    setModel(initialModel);
    setValidationError(undefined);
    setOperationError(undefined);
  }
  async function create(refresh: () => void) {
    if (
      !model.firstName.trim() ||
      !model.lastName.trim() ||
      !model.email.trim() ||
      (!editingId && !model.password)
    ) {
      setValidationError("Popunite sva obavezna polja.");
      return;
    }
    if (!/^\S+@\S+\.\S+$/.test(model.email)) {
      setValidationError("Unesite ispravan e-mail.");
      return;
    }
    if (!editingId && model.password !== model.confirmPassword) {
      setValidationError("Lozinke se ne podudaraju.");
      return;
    }
    setSaving(true);
    setOperationError(undefined);
    try {
      if (editingId)
        await service.update({
          id: editingId,
          firstName: model.firstName.trim(),
          lastName: model.lastName.trim(),
          email: model.email.trim(),
        });
      else
        await service.create({
          firstName: model.firstName.trim(),
          lastName: model.lastName.trim(),
          email: model.email.trim(),
          password: model.password,
        });
      closeDialog(true);
      refresh();
    } catch (error) {
      setOperationError(getApiErrorMessage(error));
    } finally {
      setSaving(false);
    }
  }

  return (
    <FeatureListPage
      title="Korisnici"
      headers={["Ime", "E-mail", "Role", "Status"]}
      load={service.getList}
      actions={(x) => (
        <Pressable
          style={styles.edit}
          onPress={() => {
            setEditingId(x.id);
            setModel({
              firstName: x.firstName ?? "",
              lastName: x.lastName ?? "",
              email: x.email ?? "",
              password: "",
              confirmPassword: "",
            });
            setDialogOpen(true);
          }}
        >
          <Text style={styles.editText}>Uredi</Text>
        </Pressable>
      )}
      remove={(x) => (x.id ? service.remove(x.id) : Promise.resolve())}
      getItemName={(x) => x.email ?? "korisnik"}
      topContent={(refresh) => (
        <View>
          <Pressable
            style={styles.newButton}
            onPress={() => setDialogOpen(true)}
          >
            <Text style={styles.newButtonText}>Novi korisnik</Text>
          </Pressable>
          <Modal
            transparent
            visible={dialogOpen}
            animationType="fade"
            onRequestClose={() => closeDialog()}
          >
            <View style={styles.overlay}>
              <View style={styles.dialog}>
                <Text style={styles.title}>
                  {editingId ? "Uredi korisnika" : "Novi korisnik"}
                </Text>
                <ApiErrorNotice
                  message={operationError || validationError}
                  onDismiss={() => {
                    setOperationError(undefined);
                    setValidationError(undefined);
                  }}
                />
                <Field
                  label="Ime"
                  value={model.firstName}
                  onChangeText={(value) => update("firstName", value)}
                />
                <Field
                  label="Prezime"
                  value={model.lastName}
                  onChangeText={(value) => update("lastName", value)}
                />
                <Field
                  label="E-mail"
                  value={model.email}
                  keyboardType="email-address"
                  autoCapitalize="none"
                  onChangeText={(value) => update("email", value)}
                />
                {!editingId && (
                  <Field
                    label="Lozinka"
                    value={model.password}
                    secureTextEntry
                    onChangeText={(value) => update("password", value)}
                  />
                )}
                {!editingId && (
                  <Field
                    label="Potvrdi lozinku"
                    value={model.confirmPassword}
                    secureTextEntry
                    onChangeText={(value) => update("confirmPassword", value)}
                  />
                )}
                <View style={styles.actions}>
                  <Pressable
                    disabled={saving}
                    style={styles.cancel}
                    onPress={() => closeDialog()}
                  >
                    <Text style={styles.cancelText}>Odustani</Text>
                  </Pressable>
                  <Pressable
                    disabled={saving}
                    style={[styles.save, saving && styles.disabled]}
                    onPress={() => void create(refresh)}
                  >
                    <Text style={styles.saveText}>
                      {saving
                        ? "Spremanje..."
                        : editingId
                          ? "Spremi izmjene"
                          : "Kreiraj"}
                    </Text>
                  </Pressable>
                </View>
              </View>
            </View>
          </Modal>
        </View>
      )}
      map={(x) => [
        `${x.firstName ?? ""} ${x.lastName ?? ""}`,
        x.email ?? "",
        x.roles?.join(", ") ?? "",
        x.isActive ? "Aktivan" : "Neaktivan",
      ]}
    />
  );
}

function Field({
  label,
  value,
  onChangeText,
  secureTextEntry,
  keyboardType,
  autoCapitalize,
}: {
  label: string;
  value: string;
  onChangeText: (value: string) => void;
  secureTextEntry?: boolean;
  keyboardType?: "email-address";
  autoCapitalize?: "none";
}) {
  return (
    <View style={styles.field}>
      <Text style={styles.label}>{label}</Text>
      <TextInput
        value={value}
        onChangeText={onChangeText}
        secureTextEntry={secureTextEntry}
        keyboardType={keyboardType}
        autoCapitalize={autoCapitalize}
        style={styles.input}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  edit: {
    borderWidth: 1,
    borderColor: "#2563eb",
    borderRadius: 6,
    paddingHorizontal: 10,
    paddingVertical: 7,
  },
  editText: { color: "#1d4ed8", fontWeight: "700" },
  newButton: {
    alignSelf: "flex-start",
    borderRadius: 8,
    paddingHorizontal: 14,
    paddingVertical: 11,
    backgroundColor: "#2563eb",
  },
  newButtonText: { color: "#fff", fontWeight: "700" },
  overlay: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: "rgba(15, 23, 42, 0.45)",
    padding: 24,
  },
  dialog: {
    width: "100%",
    maxWidth: 460,
    backgroundColor: "#fff",
    borderRadius: 14,
    padding: 20,
    gap: 12,
  },
  title: { fontSize: 20, fontWeight: "800", color: "#102a43" },
  field: { gap: 4 },
  label: { color: "#334e68", fontWeight: "600" },
  input: {
    borderWidth: 1,
    borderColor: "#cbd5e1",
    borderRadius: 8,
    padding: 10,
  },
  actions: { flexDirection: "row", justifyContent: "flex-end", gap: 10 },
  cancel: {
    paddingHorizontal: 14,
    paddingVertical: 10,
    borderWidth: 1,
    borderColor: "#94a3b8",
    borderRadius: 8,
  },
  cancelText: { color: "#334e68", fontWeight: "700" },
  save: {
    paddingHorizontal: 14,
    paddingVertical: 10,
    backgroundColor: "#2563eb",
    borderRadius: 8,
  },
  saveText: { color: "#fff", fontWeight: "700" },
  disabled: { opacity: 0.45 },
});
