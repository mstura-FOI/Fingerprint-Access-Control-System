import { useEffect, useMemo, useState } from "react";
import { Alert, Pressable, StyleSheet, Text, View } from "react-native";
import { Picker } from "@expo/ui/community/picker";
import { FeatureListPage } from "@/features/shared/components/feature-list-page";
import {
  ApiErrorNotice,
  getApiErrorMessage,
} from "@/features/shared/components/api-error-notice";
import { useAccountsService } from "@/features/accounts/services/accounts.service";
import { useRoomsService } from "@/features/rooms/services/rooms.service";
import { useRoomShiftsService } from "@/features/room-shifts/services/room-shifts.service";
import { useShiftsService } from "@/features/shifts/services/shifts.service";

type Option = { id: string; label: string };
type RoomShiftModel = { roomId: string; shiftId: string; managerId: string };
const initialModel: RoomShiftModel = { roomId: "", shiftId: "", managerId: "" };

export function RoomShiftsPage() {
  const roomShifts = useRoomShiftsService();
  const rooms = useRoomsService();
  const shifts = useShiftsService();
  const accounts = useAccountsService();
  const [model, setModel] = useState<RoomShiftModel>(initialModel);
  const [roomOptions, setRoomOptions] = useState<Option[]>([]);
  const [shiftOptions, setShiftOptions] = useState<Option[]>([]);
  const [managerOptions, setManagerOptions] = useState<Option[]>([]);
  const [creating, setCreating] = useState(false);
  const [referencesReady, setReferencesReady] = useState(false);
  const [operationError, setOperationError] = useState<string>();
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    void Promise.all([rooms.getAll(), shifts.getAll(), accounts.getAll()])
      .then(([roomItems, shiftItems, accountItems]) => {
        setRoomOptions(
          roomItems
            .filter((x) => x.id)
            .map((x) => ({ id: x.id!, label: x.name ?? x.id! })),
        );
        setShiftOptions(
          shiftItems
            .filter((x) => x.id)
            .map((x) => ({
              id: x.id!,
              label: `${x.name ?? "Smjena"} (${x.startTime ?? ""}-${x.endTime ?? ""})`,
            })),
        );
        setManagerOptions(
          accountItems
            .filter((x) => x.id && x.isActive)
            .map((x) => ({ id: x.id!, label: x.email ?? x.userName ?? x.id! })),
        );
      })
      .catch((error) => setOperationError(getApiErrorMessage(error)))
      .finally(() => setReferencesReady(true));
  }, [accounts, rooms, shifts]);

  const managerEmails = useMemo(
    () => new Map(managerOptions.map((option) => [option.id, option.label])),
    [managerOptions],
  );
  async function create(refreshList: () => void) {
    if (!model.roomId || !model.shiftId || !model.managerId) {
      Alert.alert("Nedostaju podaci", "Odaberite sobu, smjenu i voditelja.");
      return;
    }
    setOperationError(undefined);
    setCreating(true);
    try {
      await roomShifts.create(model);
      setModel(initialModel);
      refreshList();
    } catch (error) {
      setOperationError(getApiErrorMessage(error));
    } finally {
      setCreating(false);
    }
  }

  return (
    <FeatureListPage
      title="Smjene u sobama"
      headers={["Soba", "Smjena", "Voditelj"]}
      load={roomShifts.getList}
      remove={(x: any) => roomShifts.remove(x.id)}
      getItemName={(x: any) => x.shiftName ?? "smjena u sobi"}
      topContent={(refreshList) => (
        <View style={styles.form}>
          <Text style={styles.formTitle}>Dodijeli smjenu sobi</Text>
          <ApiErrorNotice
            message={operationError}
            onDismiss={() => setOperationError(undefined)}
          />
          <SelectionField
            label="Soba"
            value={model.roomId}
            options={roomOptions}
            disabled={!referencesReady || creating}
            onSelect={(roomId) =>
              setModel((current) => ({ ...current, roomId }))
            }
          />
          <SelectionField
            label="Smjena"
            value={model.shiftId}
            options={shiftOptions}
            disabled={!referencesReady || creating}
            onSelect={(shiftId) =>
              setModel((current) => ({ ...current, shiftId }))
            }
          />
          <SelectionField
            label="Voditelj"
            value={model.managerId}
            options={managerOptions}
            disabled={!referencesReady || creating}
            onSelect={(managerId) =>
              setModel((current) => ({ ...current, managerId }))
            }
          />
          <Pressable
            disabled={!referencesReady || creating}
            style={[
              styles.button,
              (!referencesReady || creating) && styles.disabled,
            ]}
            onPress={() => void create(refreshList)}
          >
            <Text style={styles.buttonText}>
              {creating ? "Spremanje..." : "Dodijeli smjenu"}
            </Text>
          </Pressable>
        </View>
      )}
      map={(x: any) => [
        x.roomName ?? "",
        x.shiftName ?? "",
        managerEmails.get(x.managerId ?? "") ?? x.managerId ?? "",
      ]}
    />
  );
}

function SelectionField({
  label,
  value,
  options,
  disabled,
  onSelect,
}: {
  label: string;
  value: string;
  options: Option[];
  disabled: boolean;
  onSelect: (id: string) => void;
}) {
  return (
    <View style={styles.field}>
      <Text style={styles.label}>{label}</Text>
      <View style={[styles.select, disabled && styles.disabled]}>
        <Picker
          selectedValue={value}
          enabled={!disabled}
          onValueChange={(nextValue) => onSelect(String(nextValue))}
        >
          <Picker.Item
            label={
              disabled ? "Ucitavanje..." : `Odaberi ${label.toLowerCase()}`
            }
            value=""
          />
          {options.map((option) => (
            <Picker.Item
              key={option.id}
              label={option.label}
              value={option.id}
            />
          ))}
        </Picker>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  form: { gap: 12, padding: 16, borderRadius: 12, backgroundColor: "#fff" },
  formTitle: { fontSize: 18, fontWeight: "700", color: "#102a43" },
  field: { gap: 5 },
  label: { fontWeight: "600", color: "#334e68" },
  select: {
    borderWidth: 1,
    borderColor: "#cbd5e1",
    borderRadius: 8,
    padding: 11,
    backgroundColor: "#fff",
  },
  button: {
    alignSelf: "flex-start",
    borderRadius: 8,
    paddingHorizontal: 14,
    paddingVertical: 11,
    backgroundColor: "#2563eb",
  },
  buttonText: { color: "#fff", fontWeight: "700" },
  disabled: { opacity: 0.45 },
});
