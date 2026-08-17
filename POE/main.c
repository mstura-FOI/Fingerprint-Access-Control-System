#include <stdbool.h>
#include <stdint.h>
#include "config.h"
#include "net/wifi.h"
#include "net/time_sync.h"
#include "net/api_client.h"
#include "sensor/fingerprint.h"
#include "io/code_input.h"
#include "esp_log.h"

static const char *TAG = "APP";

void app_main(void)
{
    wifi_connect();
    time_sync_wait(15);
    api_init();
    if (!fp_init())
    {
        ESP_LOGE(TAG, "AS608 init fail");
        return;
    }
    code_input_init();

    ESP_LOGI(TAG, "Spreman. Cekam kod...");

    while (1)
    {
        char code[7];
        if (!code_input_read(code))
            continue;

        api_prepare_result_t pr;
        if (!api_prepare(code, &pr))
        {
            ESP_LOGE(TAG, "prepare fail");
            continue;
        }

        if (pr.mode == API_MODE_ENROLL)
        {
            ESP_LOGI(TAG, "== ENROLL ==");
            uint8_t tpl[TEMPLATE_MAX];
            uint16_t len = fp_enroll(tpl, sizeof(tpl));
            if (len == 0)
            {
                ESP_LOGE(TAG, "enroll capture fail");
                continue;
            }
            if (api_enroll_complete(code, tpl, len))
                ESP_LOGI(TAG, "Registriran otisak.");
            else
                ESP_LOGE(TAG, "enroll upload fail");
        }
        else if (pr.mode == API_MODE_VERIFY)
        {
            ESP_LOGI(TAG, "== VERIFY ==");
            bool matched = false;

            if (!fp_verify(
                    pr.template_buf,
                    pr.template_len,
                    &matched))
            {
                ESP_LOGE(TAG, "verify fail");
                continue;
            }

            ESP_LOGI(
                TAG,
                "matched=%s",
                matched ? "TRUE" : "FALSE");

            bool granted = false;

            if (api_access_result(
                    pr.session_id,
                    matched,
                    &granted))
            {
                ESP_LOGI(
                    TAG,
                    "%s",
                    granted
                        ? ">>> PRISTUP ODOBREN"
                        : ">>> ODBIJEN");
            }
        }
    }
}