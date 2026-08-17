#include "api_client.h"
#include "config.h"
#include "util/hex.h"
#include "esp_http_client.h"
#include "esp_log.h"
#include <string.h>
#include <stdlib.h>

static const char *TAG = "API";

// Certs embedded in firmware.
extern const char server_ca_start[]  asm("_binary_server_ca_crt_start");
extern const char client_crt_start[] asm("_binary_sensor_crt_start");
extern const char client_key_start[] asm("_binary_sensor_key_start");

typedef struct { char *data; int len; } resp_t;

static esp_err_t on_event(esp_http_client_event_t *e)
{
    if (e->event_id == HTTP_EVENT_ON_DATA) {
        resp_t *r = (resp_t *)e->user_data;
        char *n = realloc(r->data, r->len + e->data_len + 1);
        if (!n) return ESP_FAIL;
        r->data = n;
        memcpy(r->data + r->len, e->data, e->data_len);
        r->len += e->data_len;
        r->data[r->len] = '\0';
    }
    return ESP_OK;
}

static int post_json(const char *url, const char *body, resp_t *resp)
{
    esp_http_client_config_t cfg = {
        .url             = url,
        .method          = HTTP_METHOD_POST,
        .timeout_ms      = 8000,
        .event_handler   = on_event,
        .user_data       = resp,
        .cert_pem        = server_ca_start,
        .client_cert_pem = client_crt_start,
        .client_key_pem  = client_key_start,
    };
    esp_http_client_handle_t c = esp_http_client_init(&cfg);
    esp_http_client_set_header(c, "Content-Type", "application/json");
    esp_http_client_set_post_field(c, body, strlen(body));

    int status = -1;
    esp_err_t err = esp_http_client_perform(c);
    if (err == ESP_OK) status = esp_http_client_get_status_code(c);
    else ESP_LOGE(TAG, "perform fail (%s): %s", url, esp_err_to_name(err));

    esp_http_client_cleanup(c);
    return status;
}


static bool json_str(const char *json, const char *key, char *dst, int cap)
{
    char pat[48];
    snprintf(pat, sizeof(pat), "\"%s\"", key);
    const char *p = strstr(json, pat);
    if (!p) return false;
    p = strchr(p + strlen(pat), ':');
    if (!p) return false;
    p++;
    while (*p == ' ' || *p == '\"') p++; 
    int i = 0;
    while (*p && *p != '\"' && *p != ',' && *p != '}' && i < cap - 1)
        dst[i++] = *p++;
    dst[i] = '\0';
    return i > 0;
}

static int json_int(const char *json, const char *key, int def)
{
    char pat[48];
    snprintf(pat, sizeof(pat), "\"%s\"", key);
    const char *p = strstr(json, pat);
    if (!p) return def;
    p = strchr(p + strlen(pat), ':');
    if (!p) return def;
    return atoi(p + 1);
}

void api_init(void) { }

bool api_prepare(const char *code, api_prepare_result_t *out)
{
    char body[64];
    snprintf(body, sizeof(body), "{\"code\":\"%s\"}", code);

    resp_t resp = {0};
    int status = post_json(EP_PREPARE, body, &resp);
    if (status != 200 || !resp.data) {
        ESP_LOGE(TAG, "prepare HTTP %d", status);
        free(resp.data);
        out->mode = API_MODE_ERROR;
        return false;
    }

    int mode = json_int(resp.data, "mode", API_MODE_ERROR);
    json_str(resp.data, "sessionId", out->session_id, sizeof(out->session_id));

    if (mode == API_MODE_VERIFY) {
        static char tpl_hex[TEMPLATE_MAX * 2 + 4];
        if (json_str(resp.data, "template", tpl_hex, sizeof(tpl_hex))) {
            out->template_len = hex_decode(tpl_hex, out->template_buf, sizeof(out->template_buf));
            out->mode = API_MODE_VERIFY;
        } else {
            out->mode = API_MODE_ERROR;
        }
    } else {
        out->mode = API_MODE_ENROLL;
    }

    free(resp.data);
    return out->mode != API_MODE_ERROR;
}

bool api_access_result(const char *session_id, bool matched, bool *granted)
{
    char body[96];
    snprintf(body, sizeof(body),
             "{\"sessionId\":\"%s\",\"matched\":%s}",
             session_id, matched ? "true" : "false");

    resp_t resp = {0};
    int status = post_json(EP_ACCESS_RES, body, &resp);
    *granted = (status == 200 && resp.data && strstr(resp.data, "true") != NULL);
    free(resp.data);
    return status == 200;
}

bool api_enroll_complete(const char *code, const uint8_t *tpl, uint16_t len)
{
    char *hex = malloc(len * 2 + 1);
    if (!hex) return false;
    hex_encode(tpl, len, hex);

    int cap = len * 2 + 64;
    char *body = malloc(cap);
    if (!body) { free(hex); return false; }
    snprintf(body, cap, "{\"code\":\"%s\",\"template\":\"%s\"}", code, hex);

    resp_t resp = {0};
    int status = post_json(EP_ENROLL_DONE, body, &resp);
    free(resp.data); free(body); free(hex);

    if (status != 200 && status != 204) {
        ESP_LOGE(TAG, "enroll HTTP %d", status);
        return false;
    }
    return true;
}