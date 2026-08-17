export type LoginModel = { email: string; password: string };

export function validateLoginModel(model: LoginModel): string | undefined {
  if (!model.email.trim()) return "E-mail je obavezan.";
  if (!/^\S+@\S+\.\S+$/.test(model.email)) return "Unesite ispravan e-mail.";
  if (!model.password) return "Lozinka je obavezna.";
}
