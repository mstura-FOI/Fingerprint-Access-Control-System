#pragma once
#include <stdint.h>
#include <stdbool.h>

typedef enum { API_MODE_ENROLL = 0, API_MODE_VERIFY = 1, API_MODE_ERROR = 2 } api_mode_t;

typedef struct {
    api_mode_t mode;
    char       session_id[40];
    uint8_t    template_buf[1024];
    uint16_t   template_len;
} api_prepare_result_t;

void api_init(void);
bool api_prepare(const char *code, api_prepare_result_t *out);
bool api_access_result(const char *session_id, bool matched, bool *granted);
bool api_enroll_complete(const char *code, const uint8_t *tpl, uint16_t len);