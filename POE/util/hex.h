#pragma once
#include <stdint.h>

void hex_encode(const uint8_t *in, uint16_t len, char *out);

uint16_t hex_decode(const char *hex, uint8_t *out, uint16_t cap);