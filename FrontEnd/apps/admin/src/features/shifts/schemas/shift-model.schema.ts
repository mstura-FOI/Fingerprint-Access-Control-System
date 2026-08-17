import { z } from "zod";

const timePattern = /^(?:[01]\d|2[0-3]):[0-5]\d:[0-5]\d$/;

export const shiftModelSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Naziv smjene je obavezan.")
    .max(100, "Naziv moze imati najvise 100 znakova."),
  startTime: z
    .string()
    .regex(timePattern, "Vrijeme mora biti u formatu HH:mm:ss."),
  endTime: z
    .string()
    .regex(timePattern, "Vrijeme mora biti u formatu HH:mm:ss."),
});

export type ShiftModel = z.infer<typeof shiftModelSchema>;
