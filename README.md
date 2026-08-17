# FingerPrintSystem

Biometric access control system built for a diploma thesis for that reason code comments are in Croatian. Connects an AS608
fingerprint sensor and an ESP32 microcontroller with a central server
(ASP.NET Core) and a mobile application (React Native).

> This is a public snapshot of the code. Active development repositories are private.

## Structure

- **Backend/** — ASP.NET Core (.NET) server, clean architecture, EF Core.
- **FrontEnd/** — React Native application (admin and user).
- **POE/** — ESP32 firmware (ESP-IDF, C) for the AS608 sensor and server communication.

## Note

Configuration files containing secrets (`appsettings.json`), private keys and
certificates, and device configuration are not included in this snapshot.
