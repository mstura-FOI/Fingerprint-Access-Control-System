import { useState } from "react";
import {
  Platform,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
} from "react-native";
import DateTimePicker from "@expo/ui/community/datetime-picker";
import { Picker } from "@expo/ui/community/picker";
import {
  ApiErrorNotice,
  getApiErrorMessage,
} from "@/features/shared/components/api-error-notice";
import { FeatureListPage } from "@/features/shared/components/feature-list-page";
import {
  shiftModelSchema,
  type ShiftModel,
} from "@/features/shifts/schemas/shift-model.schema";
import { useShiftsService } from "@/features/shifts/services/shifts.service";

const initialModel: ShiftModel = {
  name: "",
  startTime: "08:00:00",
  endTime: "16:00:00",
};
type Errors = Partial<Record<keyof ShiftModel, string>>;

export function ShiftsPage() {
  const service = useShiftsService();
  const [model, setModel] = useState<ShiftModel>(initialModel);
  const [errors, setErrors] = useState<Errors>({});
  const [submitting, setSubmitting] = useState(false);
  const [operationError, setOperationError] = useState<string>();

  function updateField(key: keyof ShiftModel, value: string) {
    setModel((current) => ({ ...current, [key]: value }));
    setErrors((current) => ({ ...current, [key]: undefined }));
  }
  async function create(refreshList: () => void) {
    const parsed = shiftModelSchema.safeParse(model);
    if (!parsed.success) {
      const nextErrors: Errors = {};
      for (const issue of parsed.error.issues) {
        const key = issue.path[0] as keyof ShiftModel;
        if (!nextErrors[key]) nextErrors[key] = issue.message;
      }
      setErrors(nextErrors);
      return;
    }
    setOperationError(undefined);
    setSubmitting(true);
    try {
      await service.create(parsed.data);
      setModel(initialModel);
      setErrors({});
      refreshList();
    } catch (error) {
      setOperationError(getApiErrorMessage(error));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <FeatureListPage
      title="Smjene"
      headers={["Naziv", "Pocetak", "Kraj"]}
      load={service.getList}
      remove={(shift) =>
        shift.id ? service.remove(shift.id) : Promise.resolve()
      }
      getItemName={(shift) => shift.name ?? "smjena"}
      topContent={(refreshList) => (
        <View style={styles.form}>
          <Text style={styles.formTitle}>Nova smjena</Text>
          <ApiErrorNotice
            message={operationError}
            onDismiss={() => setOperationError(undefined)}
          />
          <NameField
            value={model.name}
            error={errors.name}
            onChangeText={(value) => updateField("name", value)}
          />
          <TimeField
            label="Pocetak"
            value={model.startTime}
            error={errors.startTime}
            onChange={(value) => updateField("startTime", value)}
          />
          <TimeField
            label="Kraj"
            value={model.endTime}
            error={errors.endTime}
            onChange={(value) => updateField("endTime", value)}
          />
          <Pressable
            disabled={submitting}
            style={[styles.button, submitting && styles.disabled]}
            onPress={() => void create(refreshList)}
          >
            <Text style={styles.buttonText}>
              {submitting ? "Spremanje..." : "Kreiraj smjenu"}
            </Text>
          </Pressable>
        </View>
      )}
      map={(shift) => [
        shift.name ?? "",
        shift.startTime ?? "",
        shift.endTime ?? "",
      ]}
    />
  );
}

function NameField({
  value,
  error,
  onChangeText,
}: {
  value: string;
  error?: string;
  onChangeText: (value: string) => void;
}) {
  return (
    <View style={styles.field}>
      <Text style={styles.label}>Naziv smjene</Text>
      <TextInput
        value={value}
        onChangeText={onChangeText}
        style={[styles.input, error && styles.inputError]}
        placeholder="Naziv smjene"
      />
      <Text style={styles.error}>{error ?? " "}</Text>
    </View>
  );
}
function TimeField({
  label,
  value,
  error,
  onChange,
}: {
  label: string;
  value: string;
  error?: string;
  onChange: (value: string) => void;
}) {
  const [open, setOpen] = useState(false);
  const [hours, minutes] = value.split(":");
  if (Platform.OS === "web") {
    return (
      <View style={styles.field}>
        <Text style={styles.label}>{label}</Text>
        <View style={[styles.webTime, error && styles.inputError]}>
          <Picker
            selectedValue={hours}
            onValueChange={(nextHours) =>
              onChange(`${String(nextHours).padStart(2, "0")}:${minutes}:00`)
            }
          >
            {Array.from({ length: 24 }, (_, hour) => (
              <Picker.Item
                key={hour}
                label={String(hour).padStart(2, "0")}
                value={String(hour).padStart(2, "0")}
              />
            ))}
          </Picker>
          <Text style={styles.timeSeparator}>:</Text>
          <Picker
            selectedValue={minutes}
            onValueChange={(nextMinutes) =>
              onChange(`${hours}:${String(nextMinutes).padStart(2, "0")}:00`)
            }
          >
            {Array.from({ length: 60 }, (_, minute) => (
              <Picker.Item
                key={minute}
                label={String(minute).padStart(2, "0")}
                value={String(minute).padStart(2, "0")}
              />
            ))}
          </Picker>
        </View>
        <Text style={styles.error}>{error ?? " "}</Text>
      </View>
    );
  }
  return (
    <View style={styles.field}>
      <Text style={styles.label}>{label}</Text>
      <Pressable
        style={[styles.timeButton, error && styles.inputError]}
        onPress={() => setOpen(true)}
      >
        <Text style={styles.timeText}>{value}</Text>
      </Pressable>
      {open && (
        <DateTimePicker
          value={timeToDate(value)}
          mode="time"
          presentation="dialog"
          is24Hour
          onChange={(_event, selected) => {
            setOpen(false);
            if (selected) onChange(formatTime(selected));
          }}
        />
      )}
      <Text style={styles.error}>{error ?? " "}</Text>
    </View>
  );
}
function timeToDate(value: string) {
  const [hours, minutes, seconds] = value.split(":").map(Number);
  return new Date(2000, 0, 1, hours || 0, minutes || 0, seconds || 0);
}
function formatTime(value: Date) {
  return [value.getHours(), value.getMinutes(), value.getSeconds()]
    .map((part) => String(part).padStart(2, "0"))
    .join(":");
}

const styles = StyleSheet.create({
  form: { gap: 10, padding: 16, backgroundColor: "#fff", borderRadius: 12 },
  formTitle: { fontSize: 18, fontWeight: "700", color: "#102a43" },
  field: { gap: 4 },
  label: { color: "#334e68", fontWeight: "600" },
  input: {
    borderWidth: 1,
    borderColor: "#cbd5e1",
    borderRadius: 8,
    padding: 10,
    backgroundColor: "#fff",
  },
  timeButton: {
    borderWidth: 1,
    borderColor: "#cbd5e1",
    borderRadius: 8,
    padding: 11,
    backgroundColor: "#fff",
  },
  timeText: { color: "#102a43" },
  webTime: {
    flexDirection: "row",
    alignItems: "center",
    borderWidth: 1,
    borderColor: "#cbd5e1",
    borderRadius: 8,
    backgroundColor: "#fff",
  },
  timeSeparator: { fontSize: 18, fontWeight: "700" },
  inputError: { borderColor: "#dc2626" },
  error: { minHeight: 17, color: "#b91c1c", fontSize: 12 },
  button: {
    alignSelf: "flex-start",
    padding: 12,
    borderRadius: 8,
    backgroundColor: "#2563eb",
  },
  buttonText: { color: "#fff", fontWeight: "700" },
  disabled: { opacity: 0.45 },
});
