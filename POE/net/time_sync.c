#include "time_sync.h"
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "esp_sntp.h"
#include "esp_log.h"
#include <time.h>

static const char *TAG = "TIME";

bool time_sync_wait(int timeout_seconds)
{
    esp_sntp_setoperatingmode(SNTP_OPMODE_POLL);
    esp_sntp_setservername(0, "pool.ntp.org");
    esp_sntp_init();

    time_t now = 0;
    struct tm ti = {0};
    for (int i = 0; i < timeout_seconds; i++) {
        time(&now);
        localtime_r(&now, &ti);
        if (ti.tm_year > (2020 - 1900)) {
            ESP_LOGI(TAG, "Vrijeme sinkronizirano.");
            return true;
        }
        vTaskDelay(pdMS_TO_TICKS(1000));
    }
    ESP_LOGE(TAG, "SNTP timeout — TLS ce pasti (krivi datum).");
    return false;
}