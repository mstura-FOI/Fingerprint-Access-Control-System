import DateTimePicker from "@expo/ui/community/datetime-picker";
import { Picker } from "@expo/ui/community/picker";
import { createElement, useEffect, useMemo, useState } from "react";
import { Platform, Pressable, StyleSheet, Text, View } from "react-native";
import {
  AttendanceType,
  type AttendanceGetDto,
  type RoomShiftGetDto,
  type ShiftGetDto,
} from "@/packages";
import {
  ApiErrorNotice,
  getApiErrorMessage,
} from "@/features/shared/components/api-error-notice";
import { FeatureListPage } from "@/features/shared/components/feature-list-page";
import { useAccountsService } from "@/features/accounts/services/accounts.service";
import { useAttendanceService } from "@/features/attendance/services/attendance.service";
import { useAuthenticationStore } from "@/stores/authentication.store";
import { useRoomShiftsService } from "@/features/room-shifts/services/room-shifts.service";
import { useRoomsService } from "@/features/rooms/services/rooms.service";
import { useShiftsService } from "@/features/shifts/services/shifts.service";

const attendanceKey = (attendance: AttendanceGetDto) =>
  `${attendance.applicationUserId ?? ""}:${attendance.roomId ?? ""}`;

function formatPerson(firstName?: string, lastName?: string, email?: string) {
  const fullName = [firstName, lastName].filter(Boolean).join(" ");
  return fullName || email || "Nepoznat korisnik";
}

function getUserId(token?: string) {
  try {
    const payload = token?.split(".")[1];
    if (!payload) return undefined;
    const json = globalThis.atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
    const claims = JSON.parse(json) as Record<string, string>;
    return (
      claims.sub ??
      claims[
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
      ]
    );
  } catch {
    return undefined;
  }
}

function timeToMinutes(value?: string) {
  const [hour, minute] = (value ?? "").split(":").map(Number);
  return Number.isFinite(hour) && Number.isFinite(minute)
    ? hour * 60 + minute
    : undefined;
}

function isActiveShift(shift: ShiftGetDto, now: Date) {
  const start = timeToMinutes(shift.startTime);
  const end = timeToMinutes(shift.endTime);
  if (start === undefined || end === undefined) return false;
  const current = now.getHours() * 60 + now.getMinutes();
  return start <= end
    ? current >= start && current < end
    : current >= start || current < end;
}

export function AttendancePage() {
  const attendanceService = useAttendanceService();
  const accountsService = useAccountsService();
  const roomsService = useRoomsService();
  const roomShiftsService = useRoomShiftsService();
  const shiftsService = useShiftsService();
  const { tokens } = useAuthenticationStore();
  const [attendances, setAttendances] = useState<AttendanceGetDto[]>([]);
  const [roomShifts, setRoomShifts] = useState<RoomShiftGetDto[]>([]);
  const [shifts, setShifts] = useState<ShiftGetDto[]>([]);
  const [personId, setPersonId] = useState("");
  const [roomId, setRoomId] = useState("");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");
  const [refreshKey, setRefreshKey] = useState(0);
  const [now, setNow] = useState(() => new Date());
  const [loadingReferences, setLoadingReferences] = useState(true);
  const [loadingError, setLoadingError] = useState<string>();
  const [operationError, setOperationError] = useState<string>();
  const [checkingOutId, setCheckingOutId] = useState<string>();
  const [people, setPeople] = useState<
    { id: string; name: string; email: string }[]
  >([]);
  const [rooms, setRooms] = useState<{ id: string; name: string }[]>([]);

  const currentUserId = useMemo(
    () => getUserId(tokens?.accessToken),
    [tokens?.accessToken],
  );

  useEffect(() => {
    const interval = setInterval(() => setNow(new Date()), 60_000);
    return () => clearInterval(interval);
  }, []);

  async function loadData() {
    setLoadingReferences(true);
    setLoadingError(undefined);
    try {
      const [attendance, accounts, roomList, assignments, shiftList] =
        await Promise.all([
          attendanceService.getAll(),
          accountsService.getAll(),
          roomsService.getAll(),
          roomShiftsService.getAll(),
          shiftsService.getAll(),
        ]);
      setAttendances(attendance);
      setRoomShifts(assignments);
      setShifts(shiftList);
      setPeople(
        accounts
          .filter((account) => Boolean(account.id))
          .map((account) => ({
            id: account.id!,
            name: formatPerson(
              account.firstName,
              account.lastName,
              account.email,
            ),
            email: account.email ?? "",
          }))
          .sort((left, right) => left.name.localeCompare(right.name)),
      );
      setRooms(
        roomList
          .filter((room) => Boolean(room.id))
          .map((room) => ({
            id: room.id!,
            name: room.name ?? "Nepoznata soba",
          }))
          .sort((left, right) => left.name.localeCompare(right.name)),
      );
      setRefreshKey((key) => key + 1);
    } catch (error) {
      setLoadingError(getApiErrorMessage(error));
    } finally {
      setLoadingReferences(false);
    }
  }

  useEffect(() => {
    void loadData();
  }, []);

  const personById = useMemo(
    () => new Map(people.map((person) => [person.id, person])),
    [people],
  );
  const roomById = useMemo(
    () => new Map(rooms.map((room) => [room.id, room.name])),
    [rooms],
  );
  const activeShifts = useMemo(() => {
    const shiftById = new Map(shifts.map((shift) => [shift.id, shift]));
    return roomShifts.filter((assignment) => {
      const shift = assignment.shiftId
        ? shiftById.get(assignment.shiftId)
        : undefined;
      return (
        assignment.managerId === currentUserId &&
        Boolean(shift && isActiveShift(shift, now))
      );
    });
  }, [currentUserId, now, roomShifts, shifts]);
  const activeRoomIds = useMemo(
    () =>
      new Set(
        activeShifts
          .map((assignment) => assignment.roomId)
          .filter((id): id is string => Boolean(id)),
      ),
    [activeShifts],
  );
  const activeRooms = useMemo(
    () => rooms.filter((room) => activeRoomIds.has(room.id)),
    [activeRoomIds, rooms],
  );
  const openAttendanceIds = useMemo(() => {
    const latestByPersonAndRoom = new Map<string, AttendanceGetDto>();
    for (const attendance of attendances) {
      const key = attendanceKey(attendance);
      const previous = latestByPersonAndRoom.get(key);
      if (
        !previous ||
        (attendance.attendanceDateTime?.getTime() ?? 0) >
          (previous.attendanceDateTime?.getTime() ?? 0)
      ) {
        latestByPersonAndRoom.set(key, attendance);
      }
    }
    return new Set(
      [...latestByPersonAndRoom.values()]
        .filter((attendance) => attendance.attendanceType === AttendanceType._1)
        .map((attendance) => attendance.id)
        .filter((id): id is string => Boolean(id)),
    );
  }, [attendances]);

  async function checkout(attendance: AttendanceGetDto) {
    if (!attendance.applicationUserId || !attendance.roomId || !attendance.id)
      return;
    setOperationError(undefined);
    setCheckingOutId(attendance.id);
    try {
      await attendanceService.checkout(
        attendance.applicationUserId,
        attendance.roomId,
      );
      await loadData();
    } catch (error) {
      setOperationError(getApiErrorMessage(error));
    } finally {
      setCheckingOutId(undefined);
    }
  }

  async function loadPage(page: number, pageSize: number) {
    const from = parseFilterDate(dateFrom);
    const to = parseFilterDate(dateTo, true);
    const filtered = attendances.filter((attendance) => {
      const attendanceDate = attendance.attendanceDateTime;
      return (
        activeRoomIds.has(attendance.roomId ?? "") &&
        (!personId || attendance.applicationUserId === personId) &&
        (!roomId || attendance.roomId === roomId) &&
        (!from || Boolean(attendanceDate && attendanceDate >= from)) &&
        (!to || Boolean(attendanceDate && attendanceDate <= to))
      );
    });
    const totalCount = filtered.length;
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    const actualPage = Math.min(page, totalPages);
    const start = (actualPage - 1) * pageSize;
    return {
      items: filtered.slice(start, start + pageSize),
      page: actualPage,
      totalCount,
      totalPages,
    };
  }

  const activeShiftSummary = activeShifts.map((assignment) => {
    const shift = shifts.find((item) => item.id === assignment.shiftId);
    const roomName =
      assignment.roomName ??
      (assignment.roomId && roomById.get(assignment.roomId)) ??
      "nepoznata soba";
    return `${shift?.name ?? assignment.shiftName ?? "Smjena"} (${shift?.startTime ?? ""}-${shift?.endTime ?? ""}) za sobu ${roomName}`;
  });

  return (
    <FeatureListPage
      title="Prisustvo"
      headers={["Ime i prezime", "E-mail", "Soba", "Vrijeme", "Tip"]}
      load={loadPage}
      refreshKey={refreshKey}
      getSortValue={(attendance, column) => {
        const person = attendance.applicationUserId
          ? personById.get(attendance.applicationUserId)
          : undefined;
        if (column === 0) return person?.name;
        if (column === 1) return person?.email;
        if (column === 2)
          return attendance.roomId
            ? roomById.get(attendance.roomId)
            : undefined;
        if (column === 3) return attendance.attendanceDateTime;
        return attendance.attendanceType === AttendanceType._1
          ? "Ulazak"
          : "Izlazak";
      }}
      topContent={
        <>
          <ApiErrorNotice
            message={loadingError ?? operationError}
            onDismiss={() => {
              setLoadingError(undefined);
              setOperationError(undefined);
            }}
          />
          <View style={styles.filters}>
            <Filter
              label="Osoba"
              value={personId}
              onChange={(value) => {
                setPersonId(value);
                setRefreshKey((key) => key + 1);
              }}
            >
              <Picker.Item label="Sve osobe" value="" />
              {people.map((person) => (
                <Picker.Item
                  key={person.id}
                  label={`${person.name}${person.email ? ` (${person.email})` : ""}`}
                  value={person.id}
                />
              ))}
            </Filter>
            <Filter
              label="Soba"
              value={roomId}
              onChange={(value) => {
                setRoomId(value);
                setRefreshKey((key) => key + 1);
              }}
            >
              <Picker.Item label="Sve aktivne sobe" value="" />
              {activeRooms.map((room) => (
                <Picker.Item key={room.id} label={room.name} value={room.id} />
              ))}
            </Filter>
            <DateFilter
              label="Datum od"
              value={dateFrom}
              onChange={(value) => {
                setDateFrom(value);
                setRefreshKey((key) => key + 1);
              }}
            />
            <DateFilter
              label="Datum do"
              value={dateTo}
              onChange={(value) => {
                setDateTo(value);
                setRefreshKey((key) => key + 1);
              }}
            />
          </View>
          <View style={styles.shiftNotice}>
            <Text style={styles.shiftNoticeTitle}>Trenutna smjena</Text>
            <Text style={styles.shiftNoticeText}>
              {activeShiftSummary.length
                ? `Trenutno ste u smjeni: ${activeShiftSummary.join("; ")}.`
                : "Trenutno nemate aktivnu dodijeljenu smjenu."}
            </Text>
          </View>
          {loadingReferences && (
            <Text style={styles.loading}>Ucitajavanje podataka...</Text>
          )}
        </>
      }
      map={(attendance) => {
        const person = attendance.applicationUserId
          ? personById.get(attendance.applicationUserId)
          : undefined;
        return [
          person?.name ?? "Nepoznat korisnik",
          person?.email ?? "",
          (attendance.roomId && roomById.get(attendance.roomId)) ??
            "Nepoznata soba",
          attendance.attendanceDateTime?.toLocaleString() ?? "",
          attendance.attendanceType === AttendanceType._1
            ? "Ulazak"
            : "Izlazak",
        ];
      }}
      actions={(attendance) =>
        attendance.id && openAttendanceIds.has(attendance.id) ? (
          <Pressable
            style={[
              styles.checkoutButton,
              checkingOutId === attendance.id && styles.disabled,
            ]}
            disabled={checkingOutId === attendance.id}
            onPress={() => void checkout(attendance)}
          >
            <Text style={styles.checkoutText}>
              {checkingOutId === attendance.id
                ? "Evidentiranje..."
                : "Evidentiraj izlazak"}
            </Text>
          </Pressable>
        ) : (
          <Text style={styles.finished}>Zavrseno</Text>
        )
      }
    />
  );
}

function parseFilterDate(value: string, endOfDay = false) {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  if (!match) return undefined;
  const [, year, month, day] = match.map(Number);
  const date = new Date(
    year,
    month - 1,
    day,
    endOfDay ? 23 : 0,
    endOfDay ? 59 : 0,
    endOfDay ? 59 : 0,
    endOfDay ? 999 : 0,
  );
  return date.getFullYear() === year &&
    date.getMonth() === month - 1 &&
    date.getDate() === day
    ? date
    : undefined;
}

function DateFilter({
  label,
  value,
  onChange,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
}) {
  const [open, setOpen] = useState(false);

  return (
    <View style={styles.filter}>
      <Text style={styles.filterLabel}>{label}</Text>
      {Platform.OS === "web" ? (
        createElement("input", {
          type: "date",
          value,
          onChange: (event: { currentTarget: { value: string } }) =>
            onChange(event.currentTarget.value),
          style: webDateInputStyle,
        })
      ) : (
        <>
          <View style={styles.datePickerRow}>
            <Pressable style={styles.dateButton} onPress={() => setOpen(true)}>
              <Text style={styles.dateText}>{value || "Odaberi datum"}</Text>
            </Pressable>
            {value && (
              <Pressable
                style={styles.clearDateButton}
                onPress={() => onChange("")}
              >
                <Text style={styles.clearDateText}>Očisti</Text>
              </Pressable>
            )}
          </View>
          {open && (
            <DateTimePicker
              value={parseFilterDate(value) ?? new Date()}
              mode="date"
              presentation="dialog"
              onChange={(_event, selectedDate) => {
                setOpen(false);
                if (selectedDate) onChange(formatFilterDate(selectedDate));
              }}
            />
          )}
        </>
      )}
    </View>
  );
}

function formatFilterDate(date: Date) {
  return [date.getFullYear(), date.getMonth() + 1, date.getDate()]
    .map((part, index) => String(part).padStart(index === 0 ? 4 : 2, "0"))
    .join("-");
}
function Filter({
  label,
  value,
  onChange,
  children,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  children: React.ReactNode;
}) {
  return (
    <View style={styles.filter}>
      <Text style={styles.filterLabel}>{label}</Text>
      <View style={styles.select}>
        <Picker
          style={styles.picker}
          selectedValue={value}
          onValueChange={(value) => onChange(String(value))}
        >
          {children}
        </Picker>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  filters: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 14,
    padding: 16,
    borderWidth: 1,
    borderColor: "#e2e8f0",
    borderRadius: 12,
    backgroundColor: "#f8fafc",
  },
  filter: { minWidth: 240, flexBasis: 240, flexGrow: 1, gap: 7 },
  filterLabel: { color: "#334e68", fontSize: 13, fontWeight: "700" },
  datePickerRow: { flexDirection: "row", alignItems: "center", gap: 8 },
  dateButton: {
    minHeight: 42,
    flexGrow: 1,
    justifyContent: "center",
    borderWidth: 1,
    borderColor: "#94a3b8",
    borderRadius: 8,
    paddingHorizontal: 12,
    backgroundColor: "#fff",
  },
  dateText: { color: "#102a43" },
  clearDateButton: { paddingHorizontal: 8, paddingVertical: 10 },
  clearDateText: { color: "#1d4ed8", fontWeight: "700" },
  select: {
    height: 44,
    justifyContent: "center",
    overflow: "hidden",
    borderWidth: 1,
    borderColor: "#cbd5e1",
    borderRadius: 9,
    backgroundColor: "#fff",
    shadowColor: "#0f172a",
    shadowOpacity: 0.05,
    shadowRadius: 3,
    shadowOffset: { width: 0, height: 1 },
    elevation: 1,
  },
  picker: {
    width: "100%",
    height: 44,
    borderWidth: 0,
    backgroundColor: "transparent",
  },
  shiftNotice: {
    gap: 4,
    padding: 14,
    borderRadius: 10,
    backgroundColor: "#eff6ff",
    borderWidth: 1,
    borderColor: "#bfdbfe",
  },
  shiftNoticeTitle: { color: "#1e3a8a", fontWeight: "800" },
  shiftNoticeText: { color: "#1e40af" },
  loading: { color: "#486581" },
  checkoutButton: {
    borderWidth: 1,
    borderColor: "#2563eb",
    borderRadius: 6,
    paddingHorizontal: 10,
    paddingVertical: 7,
  },
  checkoutText: { color: "#1d4ed8", fontWeight: "700" },
  finished: { color: "#64748b" },
  disabled: { opacity: 0.45 },
});

const webDateInputStyle = {
  minHeight: 42,
  borderWidth: 1,
  borderStyle: "solid",
  borderColor: "#94a3b8",
  borderRadius: 8,
  padding: "0 12px",
  backgroundColor: "#fff",
  color: "#102a43",
  fontSize: 14,
  boxSizing: "border-box",
  width: "100%",
} as const;
