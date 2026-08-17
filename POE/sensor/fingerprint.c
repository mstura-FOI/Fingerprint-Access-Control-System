#include "fingerprint.h"
#include "config.h"
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "driver_as608.h"
#include "driver_as608_interface.h"
#include "esp_log.h"

static const char *TAG = "FP";
static as608_handle_t g;

bool fp_init(void)
{
    DRIVER_AS608_LINK_INIT(&g, as608_handle_t);
    DRIVER_AS608_LINK_UART_INIT(&g, as608_interface_uart_init);
    DRIVER_AS608_LINK_UART_DEINIT(&g, as608_interface_uart_deinit);
    DRIVER_AS608_LINK_UART_READ(&g, as608_interface_uart_read);
    DRIVER_AS608_LINK_UART_WRITE(&g, as608_interface_uart_write);
    DRIVER_AS608_LINK_UART_FLUSH(&g, as608_interface_uart_flush);
    DRIVER_AS608_LINK_DELAY_MS(&g, as608_interface_delay_ms);
    DRIVER_AS608_LINK_DEBUG_PRINT(&g, as608_interface_debug_print);

    if (as608_init(&g, AS608_ADDR) != 0)
    {
        ESP_LOGE(TAG, "as608 init fail (wiring/baud?)");
        return false;
    }
    ESP_LOGI(TAG, "as608 init ok");
    return true;
}

// Uhvati 2 prislona istog prsta → kombiniraj → izvezi template.
uint16_t fp_enroll(uint8_t *tpl_out, uint16_t cap)
{
    as608_status_t st;

    // --- prvi prislon → buffer 1 ---
    ESP_LOGI(TAG, "Prisloni prst (1/2)...");
    while (as608_get_image(&g, AS608_ADDR, &st) != 0 || st != AS608_STATUS_OK)
        vTaskDelay(pdMS_TO_TICKS(200));
    if (as608_generate_feature(&g, AS608_ADDR, AS608_BUFFER_NUMBER_1, &st) != 0 || st != AS608_STATUS_OK)
    {
        ESP_LOGE(TAG, "feature 1 fail (0x%02X)", st);
        return 0;
    }

    // --- makni prst ---
    ESP_LOGI(TAG, "Makni prst...");
    while (as608_get_image(&g, AS608_ADDR, &st) != 0 || st != AS608_STATUS_NO_FINGERPRINT)
        vTaskDelay(pdMS_TO_TICKS(200));

    // --- drugi prislon → buffer 2 ---
    ESP_LOGI(TAG, "Prisloni isti prst (2/2)...");
    while (as608_get_image(&g, AS608_ADDR, &st) != 0 || st != AS608_STATUS_OK)
        vTaskDelay(pdMS_TO_TICKS(200));
    if (as608_generate_feature(&g, AS608_ADDR, AS608_BUFFER_NUMBER_2, &st) != 0 || st != AS608_STATUS_OK)
    {
        ESP_LOGE(TAG, "feature 2 fail (0x%02X)", st);
        return 0;
    }

    // --- kombiniraj u template (buffer 1) ---
    if (as608_combine_feature(&g, AS608_ADDR, &st) != 0 || st != AS608_STATUS_OK)
    {
        ESP_LOGE(TAG, "combine fail (0x%02X)", st);
        return 0;
    }

    // --- izvezi template iz buffera 1 u ESP32 ---
    uint16_t len = cap;
    if (as608_upload_feature(&g, AS608_ADDR, AS608_BUFFER_NUMBER_1, tpl_out, &len, &st) != 0 || st != AS608_STATUS_OK)
    {
        ESP_LOGE(TAG, "upload fail (0x%02X)", st);
        return 0;
    }
    ESP_LOGI(TAG, "ENROLL first8: %02X %02X %02X %02X %02X %02X %02X %02X",
             tpl_out[0], tpl_out[1], tpl_out[2], tpl_out[3], tpl_out[4], tpl_out[5], tpl_out[6], tpl_out[7]);

    ESP_LOGI(TAG, "Enroll template len: %u", len);
    ESP_LOGI(TAG, "Template spreman: %u B", len);
    return len;
}

// TEST: preskače download, skenira ISTI prst dvaput (buffer 1 + buffer 2), matcha.
// Dokazuje radi li match uopće na senzoru kad OBA buffera nastanu lokalno.
// tpl_in/len se IGNORIRAJU (namjerno) — samo da potpis odgovara pozivu.
bool fp_verify_test(const uint8_t *tpl_in, uint16_t len, bool *matched)
{
    (void)tpl_in; // namjerno neiskorišteno
    (void)len;

    as608_status_t st;

    // Buffer 1: prvi prislon
    ESP_LOGI(TAG, "TEST: Prisloni prst (buffer 1)...");
    while (as608_get_image(&g, AS608_ADDR, &st) != 0 || st != AS608_STATUS_OK)
        vTaskDelay(pdMS_TO_TICKS(200));
    if (as608_generate_feature(&g, AS608_ADDR, AS608_BUFFER_NUMBER_1, &st) != 0 || st != AS608_STATUS_OK)
    {
        ESP_LOGE(TAG, "TEST feature 1 fail (0x%02X)", st);
        return false;
    }

    ESP_LOGI(TAG, "TEST: Makni prst...");
    while (as608_get_image(&g, AS608_ADDR, &st) != 0 || st != AS608_STATUS_NO_FINGERPRINT)
        vTaskDelay(pdMS_TO_TICKS(200));

    // Buffer 2: drugi prislon ISTOG prsta
    ESP_LOGI(TAG, "TEST: Prisloni ISTI prst (buffer 2)...");
    while (as608_get_image(&g, AS608_ADDR, &st) != 0 || st != AS608_STATUS_OK)
        vTaskDelay(pdMS_TO_TICKS(200));
    if (as608_generate_feature(&g, AS608_ADDR, AS608_BUFFER_NUMBER_2, &st) != 0 || st != AS608_STATUS_OK)
    {
        ESP_LOGE(TAG, "TEST feature 2 fail (0x%02X)", st);
        return false;
    }

    // Match buffer 1 vs 2
    uint16_t score = 0;
    uint8_t res = as608_match_feature(&g, AS608_ADDR, &score, &st);
    ESP_LOGI(TAG, "TEST MATCH: res=%d st=0x%02X score=%u", res, st, score);

    *matched = (st == AS608_STATUS_OK && score >= MATCH_THRESHOLD);
    return true;
}

// TEST 2: download template u buffer 1, store u flash slot 1, pa search
bool fp_test_store_search(const uint8_t *tpl_in, uint16_t len, bool *matched)
{
    as608_status_t st;

    // 1. Download template u buffer 1
    if (as608_download_feature(&g, AS608_ADDR, AS608_BUFFER_NUMBER_1,
                               (uint8_t *)tpl_in, len, &st) != 0 ||
        st != AS608_STATUS_OK)
    {
        ESP_LOGE(TAG, "download fail 0x%02X", st);
        return false;
    }

    // 2. Store buffer 1 u flash slot 1
    if (as608_store_feature(&g, AS608_ADDR, AS608_BUFFER_NUMBER_1, 1, &st) != 0 || st != AS608_STATUS_OK)
    {
        ESP_LOGE(TAG, "store fail 0x%02X", st);
        return false;
    }
    ESP_LOGI(TAG, "Stored u slot 1. Prisloni prst za search...");

    // 3. Skeniraj živi prst
    while (as608_get_image(&g, AS608_ADDR, &st) != 0 || st != AS608_STATUS_OK)
        vTaskDelay(pdMS_TO_TICKS(200));
    as608_generate_feature(&g, AS608_ADDR, AS608_BUFFER_NUMBER_1, &st);

    // 4. Search kroz flash
    uint16_t page = 0, score = 0;
    uint8_t res = as608_search_feature(&g, AS608_ADDR, AS608_BUFFER_NUMBER_1, 0, 10, &page, &score, &st);
    ESP_LOGI(TAG, "SEARCH: res=%d st=0x%02X page=%u score=%u", res, st, page, score);

    *matched =
        (res == 0) &&
        (st == AS608_STATUS_OK) &&
        (page == 1) &&
        (score >= 50);

    return true;
}

// Učitaj primljeni (pohranjeni) template u senzor, skeniraj živi prst, usporedi.
bool fp_verify(
    const uint8_t *tpl_in,
    uint16_t len,
    bool *matched)
{
    as608_status_t st;

    *matched = false;

    // 1. Backend template -> buffer 1
    if (as608_download_feature(
            &g,
            AS608_ADDR,
            AS608_BUFFER_NUMBER_1,
            (uint8_t *)tpl_in,
            len,
            &st) != 0 ||
        st != AS608_STATUS_OK)
    {
        ESP_LOGE(TAG, "download fail 0x%02X", st);
        return false;
    }

    // 2. Spremi očekivani template u rezervirani slot
    const uint16_t verify_slot = 1;

    if (as608_store_feature(
            &g,
            AS608_ADDR,
            AS608_BUFFER_NUMBER_1,
            verify_slot,
            &st) != 0 ||
        st != AS608_STATUS_OK)
    {
        ESP_LOGE(TAG, "store fail 0x%02X", st);
        return false;
    }

    ESP_LOGI(
        TAG,
        "Expected template stored in verify slot %u",
        verify_slot);

    // 3. Uzmi živi fingerprint
    ESP_LOGI(TAG, "Prisloni prst...");

    while (as608_get_image(
               &g,
               AS608_ADDR,
               &st) != 0 ||
           st != AS608_STATUS_OK)
    {
        vTaskDelay(pdMS_TO_TICKS(200));
    }

    // 4. Live finger -> buffer 1
    if (as608_generate_feature(
            &g,
            AS608_ADDR,
            AS608_BUFFER_NUMBER_1,
            &st) != 0 ||
        st != AS608_STATUS_OK)
    {
        ESP_LOGE(TAG, "generate feature fail 0x%02X", st);
        return false;
    }

    // 5. Search SAMO očekivani slot
    uint16_t page = 0;
    uint16_t score = 0;

    uint8_t res = as608_search_feature(
        &g,
        AS608_ADDR,
        AS608_BUFFER_NUMBER_1,
        verify_slot,
        1,
        &page,
        &score,
        &st);

    ESP_LOGI(
        TAG,
        "VERIFY SEARCH: res=%u st=0x%02X page=%u score=%u",
        res,
        st,
        page,
        score);

    *matched =
        res == 0 &&
        st == AS608_STATUS_OK &&
        page == verify_slot &&
        score >= MATCH_THRESHOLD;

    ESP_LOGI(
        TAG,
        "Fingerprint matched=%s",
        *matched ? "TRUE" : "FALSE");

    return true;
}