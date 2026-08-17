#include "hex.h"
#include <stdio.h>
#include <string.h>

void hex_encode(const uint8_t *in, uint16_t len, char *out)
{
    for (uint16_t i = 0; i < len; i++)
        sprintf(out + i * 2, "%02X", in[i]);
    out[len * 2] = '\0';
}

static int nib(char c)
{
    if (c >= '0' && c <= '9') return c - '0';
    if (c >= 'A' && c <= 'F') return c - 'A' + 10;
    if (c >= 'a' && c <= 'f') return c - 'a' + 10;
    return -1;
}

uint16_t hex_decode(const char *hex, uint8_t *out, uint16_t cap)
{
    uint16_t len = strlen(hex);
    if (len % 2 != 0) return 0;
    uint16_t bytes = len / 2;
    if (bytes > cap) return 0;

    for (uint16_t i = 0; i < bytes; i++) {
        int hi = nib(hex[i * 2]);
        int lo = nib(hex[i * 2 + 1]);
        if (hi < 0 || lo < 0) return 0;
        out[i] = (uint8_t)((hi << 4) | lo);
    }
    return bytes;
}