idf_component_register(
    SRCS
        "main.c"
        "net/wifi.c"
        "net/time_sync.c"
        "net/api_client.c"
        "sensor/fingerprint.c"
        "io/code_input.c"
        "util/hex.c"
        "driver_as608.c"
        "driver_as608_advance.c"
        "driver_as608_interface.c"
    INCLUDE_DIRS "." "net" "sensor" "io" "util"
    EMBED_TXTFILES
        "certs/server_ca.crt"
        "certs/sensor.crt"
        "certs/sensor.key"
    REQUIRES
        esp_wifi esp_event esp_netif nvs_flash
        esp_http_client esp-tls
        esp_driver_uart
        esp_driver_gpio          # ← AS608 interface treba driver/gpio.h
        driver                   # ← fallback, neki driver headeri su ovdje
)