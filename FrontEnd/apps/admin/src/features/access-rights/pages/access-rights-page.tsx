import { useEffect, useMemo, useState } from "react";
import { Modal, Pressable, StyleSheet, Text, View } from "react-native";
import { Picker } from "@expo/ui/community/picker";
import {
  ApiErrorNotice,
  getApiErrorMessage,
} from "@/features/shared/components/api-error-notice";
import { FeatureListPage } from "@/features/shared/components/feature-list-page";
import { useAccountsService } from "@/features/accounts/services/accounts.service";
import { useRoomsService } from "@/features/rooms/services/rooms.service";
import { useAccessRightsService } from "@/features/access-rights/services/access-rights.service";
type Model = { id?: string; applicationUserId: string; roomId: string };
const empty: Model = { applicationUserId: "", roomId: "" };
type Option = { id: string; label: string };
export function AccessRightsPage() {
  const service = useAccessRightsService();
  const accounts = useAccountsService();
  const rooms = useRoomsService();
  const [users, setUsers] = useState<Option[]>([]);
  const [roomOptions, setRoomOptions] = useState<Option[]>([]);
  const [model, setModel] = useState<Model>(empty);
  const [open, setOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string>();
  useEffect(() => {
    void Promise.all([accounts.getAll(), rooms.getAll()])
      .then(([a, r]) => {
        setUsers(
          a
            .filter((x) => x.id && x.isActive)
            .map((x) => ({ id: x.id!, label: x.email ?? x.id! })),
        );
        setRoomOptions(
          r
            .filter((x) => x.id)
            .map((x) => ({ id: x.id!, label: x.name ?? x.id! })),
        );
      })
      .catch((e) => setError(getApiErrorMessage(e)));
  }, [accounts, rooms]);
  const emails = useMemo(
    () => new Map(users.map((x) => [x.id, x.label])),
    [users],
  );
  const names = useMemo(
    () => new Map(roomOptions.map((x) => [x.id, x.label])),
    [roomOptions],
  );
  function close() {
    if (!saving) {
      setOpen(false);
      setModel(empty);
      setError(undefined);
    }
  }
  async function save(refresh: () => void) {
    if (!model.applicationUserId || !model.roomId) {
      setError("Odaberite korisnika i sobu.");
      return;
    }
    setSaving(true);
    setError(undefined);
    try {
      if (model.id)
        await service.update({
          id: model.id,
          applicationUserId: model.applicationUserId,
          roomId: model.roomId,
        });
      else await service.create(model);
      close();
      refresh();
    } catch (e) {
      setError(getApiErrorMessage(e));
    } finally {
      setSaving(false);
    }
  }
  return (
    <FeatureListPage
      title="Prava pristupa"
      headers={["Korisnik", "Soba"]}
      load={service.getList}
      remove={(x) => (x.id ? service.remove(x.id) : Promise.resolve())}
      getItemName={(x) =>
        emails.get(x.applicationUserId ?? "") ?? "pravo pristupa"
      }
      actions={(x, refresh) => (
        <Pressable
          style={s.edit}
          onPress={() => {
            setModel({
              id: x.id,
              applicationUserId: x.applicationUserId ?? "",
              roomId: x.roomId ?? "",
            });
            setOpen(true);
          }}
        >
          <Text style={s.editText}>Uredi</Text>
        </Pressable>
      )}
      topContent={(refresh) => (
        <View>
          <Pressable style={s.new} onPress={() => setOpen(true)}>
            <Text style={s.newText}>Novo pravo pristupa</Text>
          </Pressable>
          <Modal
            transparent
            visible={open}
            animationType="fade"
            onRequestClose={close}
          >
            <View style={s.overlay}>
              <View style={s.dialog}>
                <Text style={s.title}>
                  {model.id ? "Uredi pravo pristupa" : "Novo pravo pristupa"}
                </Text>
                <ApiErrorNotice
                  message={error}
                  onDismiss={() => setError(undefined)}
                />
                <Select
                  label="Korisnik"
                  value={model.applicationUserId}
                  options={users}
                  onChange={(v) =>
                    setModel((m) => ({ ...m, applicationUserId: v }))
                  }
                />
                <Select
                  label="Soba"
                  value={model.roomId}
                  options={roomOptions}
                  onChange={(v) => setModel((m) => ({ ...m, roomId: v }))}
                />
                <View style={s.actions}>
                  <Pressable style={s.cancel} onPress={close}>
                    <Text>Odustani</Text>
                  </Pressable>
                  <Pressable style={s.save} onPress={() => void save(refresh)}>
                    <Text style={s.saveText}>
                      {saving
                        ? "Spremanje..."
                        : model.id
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
        emails.get(x.applicationUserId ?? "") ?? x.applicationUserId ?? "",
        names.get(x.roomId ?? "") ?? x.roomId ?? "",
      ]}
    />
  );
}
function Select({
  label,
  value,
  options,
  onChange,
}: {
  label: string;
  value: string;
  options: Option[];
  onChange: (v: string) => void;
}) {
  return (
    <View style={s.field}>
      <Text style={s.label}>{label}</Text>
      <View style={s.select}>
        <Picker
          selectedValue={value}
          onValueChange={(v) => onChange(String(v))}
        >
          <Picker.Item label={`Odaberi ${label.toLowerCase()}`} value="" />
          {options.map((x) => (
            <Picker.Item key={x.id} label={x.label} value={x.id} />
          ))}
        </Picker>
      </View>
    </View>
  );
}
const s = StyleSheet.create({
  new: {
    alignSelf: "flex-start",
    backgroundColor: "#2563eb",
    borderRadius: 8,
    padding: 12,
  },
  newText: { color: "#fff", fontWeight: "700" },
  edit: {
    borderWidth: 1,
    borderColor: "#2563eb",
    borderRadius: 6,
    paddingHorizontal: 10,
    paddingVertical: 7,
  },
  editText: { color: "#1d4ed8", fontWeight: "700" },
  overlay: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: "rgba(15,23,42,.45)",
    padding: 24,
  },
  dialog: {
    width: "100%",
    maxWidth: 440,
    backgroundColor: "#fff",
    borderRadius: 14,
    padding: 20,
    gap: 12,
  },
  title: { fontSize: 20, fontWeight: "800" },
  field: { gap: 4 },
  label: { fontWeight: "600" },
  select: { borderWidth: 1, borderColor: "#cbd5e1", borderRadius: 8 },
  actions: { flexDirection: "row", justifyContent: "flex-end", gap: 10 },
  cancel: {
    padding: 10,
    borderWidth: 1,
    borderColor: "#94a3b8",
    borderRadius: 8,
  },
  save: { padding: 10, backgroundColor: "#2563eb", borderRadius: 8 },
  saveText: { color: "#fff", fontWeight: "700" },
});
